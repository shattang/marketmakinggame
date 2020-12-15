release:
	dotnet publish -c Release -o bin src/MarketMakingGame.sln
stop:
	docker stop MarketMakingGame || true
docker: stop
	docker container rm MarketMakingGame || true
	docker build -t shattangdocker/marketmakinggame .
push: docker
	docker push shattangdocker/marketmakinggame:latest
run: stop
	docker run -d -p 8081:8081 --name MarketMakingGame -v /var/data/marketmakinggame:/var/data/marketmakinggame shattangdocker/marketmakinggame
dropin:
	docker exec -it MarketMakingGame /bin/sh
inspect:
	docker logs --follow MarketMakingGame
debug:
	dotnet build src/MarketMakingGame.sln && dotnet run -p src/Server
clean:
	dotnet restore src/MarketMakingGame.sln
	dotnet clean src/MarketMakingGame.sln
