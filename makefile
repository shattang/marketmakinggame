release:
	dotnet publish -c Release -o deploy/bin src/MarketMakingGame.sln
stop:
	docker stop MarketMakingGame || true
deploy: release stop
	docker container rm MarketMakingGame || true
	docker build -t marketmakinggame deploy
run: deploy
	docker run -d -p 8081:8081 --name MarketMakingGame -v /var/data/marketmakinggame:/var/data/marketmakinggame marketmakinggame
inspect:
	docker logs --follow MarketMakingGame
debug:
	dotnet build src/MarketMakingGame.sln && dotnet run -p src/Server
clean:
	dotnet restore src/MarketMakingGame.sln
	dotnet clean src/MarketMakingGame.sln
