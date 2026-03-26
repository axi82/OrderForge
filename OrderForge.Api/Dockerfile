# syntax=docker/dockerfile:1

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY OrderForge.sln ./
COPY OrderForge.Domain/OrderForge.Domain.csproj OrderForge.Domain/
COPY OrderForge.Application/OrderForge.Application.csproj OrderForge.Application/
COPY OrderForge.Infrastructure/OrderForge.Infrastructure.csproj OrderForge.Infrastructure/
COPY OrderForge.Api/OrderForge.Api.csproj OrderForge.Api/

RUN dotnet restore OrderForge.Api/OrderForge.Api.csproj

COPY OrderForge.Domain/ OrderForge.Domain/
COPY OrderForge.Application/ OrderForge.Application/
COPY OrderForge.Infrastructure/ OrderForge.Infrastructure/
COPY OrderForge.Api/ OrderForge.Api/

RUN dotnet publish OrderForge.Api/OrderForge.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "OrderForge.Api.dll"]
