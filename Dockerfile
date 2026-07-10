FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

COPY Wordix.slnx ./
COPY Wordix.Api/Wordix.Api.csproj Wordix.Api/
COPY Wordix.Application/Wordix.Application.csproj Wordix.Application/
COPY Wordix.Domain/Wordix.Domain.csproj Wordix.Domain/
COPY Wordix.Persistence/Wordix.Persistence.csproj Wordix.Persistence/
COPY Wordix.Infrastructure/Wordix.Infrastructure.csproj Wordix.Infrastructure/
COPY Wordix.Shared/Wordix.Shared.csproj Wordix.Shared/

RUN dotnet restore Wordix.Api/Wordix.Api.csproj

COPY . .

RUN dotnet publish Wordix.Api/Wordix.Api.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime

WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080

EXPOSE 8080

ENTRYPOINT ["dotnet", "Wordix.Api.dll"]