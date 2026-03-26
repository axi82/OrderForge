# syntax=docker/dockerfile:1

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY OrderForge.sln ./
COPY OrderForge.Client/OrderForge.Client.csproj OrderForge.Client/

RUN dotnet restore OrderForge.Client/OrderForge.Client.csproj

COPY OrderForge.Client/ OrderForge.Client/

RUN dotnet publish OrderForge.Client/OrderForge.Client.csproj -c Release -o /app/publish --no-restore

FROM nginx:1.27-alpine AS final
COPY OrderForge.Client/nginx.conf /etc/nginx/nginx.conf
COPY --from=build /app/publish/wwwroot /usr/share/nginx/html
EXPOSE 80
