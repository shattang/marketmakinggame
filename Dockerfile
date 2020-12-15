FROM mcr.microsoft.com/dotnet/core/sdk:3.1-alpine as builder
RUN apk add make
COPY src /build/src
COPY makefile /build/makefile
RUN make -C /build release
RUN mkdir -p /release
RUN cp -r /build/bin/* /release
RUN rm -rf /build

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-alpine
COPY --from=builder /release/ /apps/MarketMakingGame
RUN mkdir -p /var/data/marketmakinggame
WORKDIR /apps/MarketMakingGame
ENTRYPOINT ["dotnet", "MarketMakingGame.Server.dll"]

