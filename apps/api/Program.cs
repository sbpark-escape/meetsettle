// 파일 용도: MeetSettle API 애플리케이션의 진입점과 의존성 구성을 담당한다.
// 파일 목적: 공개용 예제가 로컬 PostgreSQL과 Swagger로 빠르게 실행되도록 기본 구성을 제공한다.
using MeetSettle.Api.Data;
using MeetSettle.Api.Features.Meetups;
using MeetSettle.Api.Infrastructure;
using MeetSettle.SettlementCore;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("ConnectionStrings:DefaultConnection is required.");

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException("ConnectionStrings:DefaultConnection is required.");
}

builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// Health endpoint
builder.Services.AddHealthChecks().AddNpgSql(connectionString);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddSingleton<IDbConnectionFactory>(new NpgsqlConnectionFactory(connectionString));
builder.Services.AddScoped<SettlementCalculator>();
builder.Services.AddScoped<SettlementProjectionService>();

var allowedOrigins = builder.Configuration["ALLOWED_ORIGINS"]
    ?? Environment.GetEnvironmentVariable("ALLOWED_ORIGINS");

builder.Services.AddCors(options =>
{
    options.AddPolicy("ConfiguredOrigins", policy =>
    {
        if (string.IsNullOrWhiteSpace(allowedOrigins))
        {
            policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
            return;
        }

        policy
            .WithOrigins(allowedOrigins.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

var applyMigrations = app.Environment.IsDevelopment()
    || string.Equals(builder.Configuration["APPLY_MIGRATIONS"], "true", StringComparison.OrdinalIgnoreCase);

if (applyMigrations)
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await dbContext.Database.MigrateAsync();
}

var enableSwagger = app.Environment.IsDevelopment()
    || string.Equals(builder.Configuration["ENABLE_SWAGGER"], "true", StringComparison.OrdinalIgnoreCase);

if (enableSwagger)
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("ConfiguredOrigins");
app.MapGet("/health", static () => Results.Ok(new { status = "ok" }));
app.MapControllers();
// Health endpoint
app.MapHealthChecks("/health");

app.Run();

public partial class Program;
