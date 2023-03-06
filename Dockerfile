#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["tic-tac-toe.csproj", "."]
RUN dotnet restore "./tic-tac-toe.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "tic-tac-toe.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "tic-tac-toe.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "tic-tac-toe.dll"]
