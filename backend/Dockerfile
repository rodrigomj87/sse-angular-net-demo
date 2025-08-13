# Task 4.2 Backend Dockerfile (.NET 8)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5000

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY backend/SseDemo.sln ./
COPY backend/src ./src
RUN dotnet restore ./src/SseDemo.OrdersService/SseDemo.OrdersService.csproj
RUN dotnet publish ./src/SseDemo.OrdersService/SseDemo.OrdersService.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://0.0.0.0:5000
ENTRYPOINT ["dotnet", "SseDemo.OrdersService.dll"]
