FROM microsoft/dotnet:2.1-runtime AS base
WORKDIR /app

FROM microsoft/dotnet:2.1-sdk AS build
WORKDIR /src
COPY test/IntegrationTest/IntegrationTest.csproj test/IntegrationTest/
COPY src/Rabble.EventBus.RabbitMQ/Rabble.EventBus.RabbitMQ.csproj src/Rabble.EventBus.RabbitMQ/
COPY src/Rabble.EventBus.Abstractions/Rabble.EventBus.Abstractions.csproj src/Rabble.EventBus.Abstractions/
RUN dotnet restore test/IntegrationTest/IntegrationTest.csproj
COPY . .
WORKDIR /src/test/IntegrationTest
RUN dotnet build IntegrationTest.csproj -c Release -o /app

FROM build AS publish
RUN dotnet publish IntegrationTest.csproj -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "IntegrationTest.dll"]
