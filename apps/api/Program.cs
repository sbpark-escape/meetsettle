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

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddSingleton(new NpgsqlConnectionFactory(connectionString));
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

if (app.Environment.IsDevelopment())
{
    // 개발 환경에서는 Docker Compose로 처음 실행해도 DB 스키마가 준비되도록 migration을 자동 적용한다.
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await dbContext.Database.MigrateAsync();

    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("ConfiguredOrigins");
app.MapControllers();

app.Run();
