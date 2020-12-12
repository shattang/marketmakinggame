FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-alpine
COPY bin/ apps/MarketMakingGame
RUN mkdir -p apps/MarketMakingGame/data
WORKDIR /apps/MarketMakingGame
ENTRYPOINT ["dotnet", "MarketMakingGame.Server.dll"]