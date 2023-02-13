FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["CringeBot/CringeBot.csproj", "CringeBot/"]
RUN dotnet restore "CringeBot/CringeBot.csproj"
COPY . .
WORKDIR "/src/CringeBot"
RUN dotnet build "CringeBot.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CringeBot.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CringeBot.dll"]
