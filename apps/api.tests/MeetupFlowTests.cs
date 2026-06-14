using System.Data;
using System.Data.Common;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Dapper;
using MeetSettle.Api.Data;
using MeetSettle.Api.Features.Meetups;
using MeetSettle.Api.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace MeetSettle.Api.Tests;

public sealed class MeetupFlowTests
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    static MeetupFlowTests()
    {
        SqlMapper.AddTypeHandler(new GuidTypeHandler());
    }

    [Fact]
    public async Task MeetupFlow_CreatesParticipantsExpenseAndSettlement()
    {
        await using var factory = new MeetupApiFactory();
        await factory.ResetDatabaseAsync();

        var client = factory.CreateClient();

        var meetupResponse = await client.PostAsJsonAsync(
            "/api/meetups",
            new CreateMeetupRequest("Friday dinner", new DateOnly(2026, 6, 20), "Seoul", "KRW"),
            JsonOptions);

        Assert.Equal(HttpStatusCode.Created, meetupResponse.StatusCode);
        var meetup = await ReadJsonAsync<MeetupSummaryResponse>(meetupResponse);

        var alexResponse = await client.PostAsJsonAsync(
            $"/api/meetups/{meetup.Id}/participants",
            new CreateParticipantRequest("Alex"),
            JsonOptions);

        Assert.Equal(HttpStatusCode.Created, alexResponse.StatusCode);
        var alex = await ReadJsonAsync<ParticipantResponse>(alexResponse);

        var boraResponse = await client.PostAsJsonAsync(
            $"/api/meetups/{meetup.Id}/participants",
            new CreateParticipantRequest("Bora", IsAttending: false),
            JsonOptions);

        Assert.Equal(HttpStatusCode.Created, boraResponse.StatusCode);
        var bora = await ReadJsonAsync<ParticipantResponse>(boraResponse);
        Assert.False(bora.IsAttending);

        var attendanceResponse = await client.PatchAsJsonAsync(
            $"/api/meetups/{meetup.Id}/participants/{bora.Id}",
            new UpdateParticipantRequest(true),
            JsonOptions);

        Assert.Equal(HttpStatusCode.OK, attendanceResponse.StatusCode);
        var updatedBora = await ReadJsonAsync<ParticipantResponse>(attendanceResponse);
        Assert.True(updatedBora.IsAttending);

        var expenseResponse = await client.PostAsJsonAsync(
            $"/api/meetups/{meetup.Id}/expenses",
            new CreateExpenseRequest("Dinner", alex.Id, 60000m, [alex.Id, bora.Id]),
            JsonOptions);

        Assert.Equal(HttpStatusCode.Created, expenseResponse.StatusCode);

        var settlement = await client.GetFromJsonAsync<SettlementResponse>(
            $"/api/meetups/{meetup.Id}/settlement",
            JsonOptions);

        Assert.NotNull(settlement);
        Assert.Equal(meetup.Id, settlement.MeetupId);
        Assert.Equal("KRW", settlement.Currency);
        Assert.Collection(
            settlement.Balances.OrderBy(balance => balance.ParticipantName),
            balance =>
            {
                Assert.Equal(alex.Id, balance.ParticipantId);
                Assert.Equal("Alex", balance.ParticipantName);
                Assert.Equal(60000m, balance.PaidAmount);
                Assert.Equal(30000m, balance.OwedAmount);
                Assert.Equal(30000m, balance.NetAmount);
            },
            balance =>
            {
                Assert.Equal(bora.Id, balance.ParticipantId);
                Assert.Equal("Bora", balance.ParticipantName);
                Assert.Equal(0m, balance.PaidAmount);
                Assert.Equal(30000m, balance.OwedAmount);
                Assert.Equal(-30000m, balance.NetAmount);
            });

        var transfer = Assert.Single(settlement.Transfers);
        Assert.Equal(bora.Id, transfer.FromParticipantId);
        Assert.Equal("Bora", transfer.FromParticipantName);
        Assert.Equal(alex.Id, transfer.ToParticipantId);
        Assert.Equal("Alex", transfer.ToParticipantName);
        Assert.Equal(30000m, transfer.Amount);
        Assert.False(transfer.IsCompleted);
    }

    private static async Task<T> ReadJsonAsync<T>(HttpResponseMessage response)
    {
        var value = await response.Content.ReadFromJsonAsync<T>(JsonOptions);
        Assert.NotNull(value);
        return value;
    }

    private sealed class MeetupApiFactory : WebApplicationFactory<Program>, IAsyncDisposable
    {
        private readonly string _connectionString =
            $"Data Source=meetsettle-api-tests-{Guid.NewGuid():N};Mode=Memory;Cache=Shared";

        private readonly string? _previousApplyMigrations;
        private readonly string? _previousConnectionString;
        private readonly SqliteConnection _rootConnection;

        public MeetupApiFactory()
        {
            _previousApplyMigrations = Environment.GetEnvironmentVariable("APPLY_MIGRATIONS");
            _previousConnectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");
            Environment.SetEnvironmentVariable("APPLY_MIGRATIONS", "false");
            Environment.SetEnvironmentVariable("ConnectionStrings__DefaultConnection", "Host=localhost;Database=meetsettle_tests");

            _rootConnection = new SqliteConnection(_connectionString);
            _rootConnection.Open();
        }

        public async Task ResetDatabaseAsync()
        {
            await using var command = _rootConnection.CreateCommand();
            command.CommandText = """
                PRAGMA foreign_keys = OFF;
                DROP TABLE IF EXISTS expense_shares;
                DROP TABLE IF EXISTS settlement_transfers;
                DROP TABLE IF EXISTS expenses;
                DROP TABLE IF EXISTS invite_tokens;
                DROP TABLE IF EXISTS participants;
                DROP TABLE IF EXISTS meetups;
                PRAGMA foreign_keys = ON;

                CREATE TABLE meetups (
                    id TEXT PRIMARY KEY,
                    name TEXT NOT NULL,
                    date TEXT NOT NULL,
                    location TEXT NULL,
                    currency TEXT NOT NULL,
                    created_at TEXT NOT NULL,
                    updated_at TEXT NOT NULL
                );

                CREATE TABLE participants (
                    id TEXT PRIMARY KEY,
                    meetup_id TEXT NOT NULL,
                    name TEXT NOT NULL,
                    is_attending INTEGER NOT NULL DEFAULT 1,
                    created_at TEXT NOT NULL,
                    updated_at TEXT NOT NULL,
                    FOREIGN KEY (meetup_id) REFERENCES meetups(id) ON DELETE CASCADE
                );

                CREATE TABLE expenses (
                    id TEXT PRIMARY KEY,
                    meetup_id TEXT NOT NULL,
                    payer_participant_id TEXT NOT NULL,
                    title TEXT NOT NULL,
                    amount NUMERIC NOT NULL,
                    created_at TEXT NOT NULL,
                    updated_at TEXT NOT NULL,
                    FOREIGN KEY (meetup_id) REFERENCES meetups(id) ON DELETE CASCADE,
                    FOREIGN KEY (payer_participant_id) REFERENCES participants(id)
                );

                CREATE TABLE expense_shares (
                    id TEXT PRIMARY KEY,
                    expense_id TEXT NOT NULL,
                    participant_id TEXT NOT NULL,
                    weight NUMERIC NOT NULL DEFAULT 1,
                    FOREIGN KEY (expense_id) REFERENCES expenses(id) ON DELETE CASCADE,
                    FOREIGN KEY (participant_id) REFERENCES participants(id)
                );

                CREATE TABLE settlement_transfers (
                    id TEXT PRIMARY KEY,
                    meetup_id TEXT NOT NULL,
                    from_participant_id TEXT NOT NULL,
                    to_participant_id TEXT NOT NULL,
                    amount NUMERIC NOT NULL,
                    is_completed INTEGER NOT NULL DEFAULT 0,
                    created_at TEXT NOT NULL,
                    updated_at TEXT NOT NULL,
                    FOREIGN KEY (meetup_id) REFERENCES meetups(id) ON DELETE CASCADE,
                    FOREIGN KEY (from_participant_id) REFERENCES participants(id),
                    FOREIGN KEY (to_participant_id) REFERENCES participants(id)
                );

                CREATE TABLE invite_tokens (
                    id TEXT PRIMARY KEY,
                    meetup_id TEXT NOT NULL,
                    token TEXT NOT NULL,
                    expires_at TEXT NOT NULL,
                    created_at TEXT NOT NULL,
                    FOREIGN KEY (meetup_id) REFERENCES meetups(id) ON DELETE CASCADE
                );
                """;

            await command.ExecuteNonQueryAsync();
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureLogging(logging => logging.ClearProviders());
            builder.ConfigureAppConfiguration(configuration =>
            {
                configuration.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:DefaultConnection"] = "Host=localhost;Database=meetsettle_tests",
                    ["APPLY_MIGRATIONS"] = "false"
                });
            });

            builder.ConfigureTestServices(services =>
            {
                var sqliteProvider = new ServiceCollection()
                    .AddEntityFrameworkSqlite()
                    .BuildServiceProvider();

                services.RemoveAll<AppDbContext>();
                services.RemoveAll<DbContextOptions>();
                services.RemoveAll<DbContextOptions<AppDbContext>>();
                services.RemoveAll<IDbContextOptionsConfiguration<AppDbContext>>();
                services.RemoveAll<IDbConnectionFactory>();

                services.AddDbContext<AppDbContext>(options => options
                    .UseSqlite(_connectionString)
                    .UseInternalServiceProvider(sqliteProvider));
                services.AddSingleton<IDbConnectionFactory>(_ => new SqliteConnectionFactory(_connectionString));
            });
        }

        public new async ValueTask DisposeAsync()
        {
            await _rootConnection.DisposeAsync();
            await base.DisposeAsync();
            Environment.SetEnvironmentVariable("APPLY_MIGRATIONS", _previousApplyMigrations);
            Environment.SetEnvironmentVariable("ConnectionStrings__DefaultConnection", _previousConnectionString);
        }
    }

    private sealed class SqliteConnectionFactory(string connectionString) : IDbConnectionFactory
    {
        public DbConnection CreateConnection()
        {
            return new SqliteConnection(connectionString);
        }
    }

    private sealed class GuidTypeHandler : SqlMapper.TypeHandler<Guid>
    {
        public override Guid Parse(object value)
        {
            return value switch
            {
                Guid guid => guid,
                string text => Guid.Parse(text),
                byte[] bytes => new Guid(bytes),
                _ => throw new DataException($"Cannot convert {value.GetType()} to Guid.")
            };
        }

        public override void SetValue(System.Data.IDbDataParameter parameter, Guid value)
        {
            parameter.Value = value.ToString();
        }
    }
}
