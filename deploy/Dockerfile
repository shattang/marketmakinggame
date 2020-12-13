FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-alpine
COPY bin/ apps/MarketMakingGame
WORKDIR /apps/MarketMakingGame
ENTRYPOINT ["dotnet", "MarketMakingGame.Server.dll"]