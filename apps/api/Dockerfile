FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY MeetSettle.sln ./
COPY packages/settlement-core/MeetSettle.SettlementCore.csproj packages/settlement-core/
COPY apps/api/MeetSettle.Api.csproj apps/api/
RUN dotnet restore apps/api/MeetSettle.Api.csproj

COPY packages/settlement-core packages/settlement-core
COPY apps/api apps/api
RUN dotnet publish apps/api/MeetSettle.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "MeetSettle.Api.dll"]
