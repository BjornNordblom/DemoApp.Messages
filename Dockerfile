#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["DemoApp.Messages.csproj", "."]
RUN dotnet restore "DemoApp.Messages.csproj"
COPY . .
WORKDIR "/src"
RUN dotnet build "DemoApp.Messages.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "DemoApp.Messages.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:7.0 
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DemoApp.Messages.dll"]
