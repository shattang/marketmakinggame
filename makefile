release:
	dotnet publish -c Release -o deploy/bin src/MarketMakingGame.sln
deploy: release
	docker stop MarketMakingGame || true
	docker container rm MarketMakingGame || true
	docker build -t marketmakinggame deploy
run: deploy
	docker run -d -p 8081:80 --name MarketMakingGame marketmakinggame
	docker logs --follow MarketMakingGame
debug:
	dotnet build src/MarketMakingGame.sln && dotnet run -p src/Server
clean:
	dotnet restore src/MarketMakingGame.sln
	dotnet clean src/MarketMakingGame.sln
