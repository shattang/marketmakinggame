using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MarketMakingGame.Shared.Messages;
using MarketMakingGame.Shared.Models;
using MarketMakingGame.Shared.Lib;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using MarketMakingGame.Server.Lib;

namespace MarketMakingGame.Server.Hubs
{
  public class GameHub : Hub
  {
    private readonly ILogger _logger;
    private readonly GameService _gameEngine;

    public GameHub(ILogger<GameHub> logger, GameService gameService)
    {
      _logger = logger;
      _gameEngine = gameService;
    }

    public async Task<GetGameInfoResponse> GetGameInfo(GetGameInfoRequest request)
    {
      _logger.LogInformation("Games: " + String.Join(",", _gameEngine.Games));
      var lists = request.GameIds.Select(x => _gameEngine.Games.GetValueOrDefault(x, null)).Where(x => x != null).ToList();
      _logger.LogInformation("GetGameInfo {} {}", request, String.Join("," , lists));
      
      return //await Task.FromResult(
       new GetGameInfoResponse()
       {
         RequestId = request.RequestId,
         IsSuccess = true,
         Games = lists
       }
       //)
       ;
    }

    public async Task CreateGame(CreateGameRequest request)
    {
      if (String.IsNullOrWhiteSpace(request.Game.GameName) || request.Game.GameName == "xxx")
        return;

      _logger.LogInformation("CreateGame {}", request);
      var resp = new CreateGameResponse()
      {
        RequestId = request.RequestId,
        IsSuccess = true,
        GameId = Guid.NewGuid().ToBase62()
      };
      var gameInfo = new Game() { GameId = resp.GameId, GameName = request.Game.GameName };
      _gameEngine.Games[gameInfo.GameId] = gameInfo;
      _logger.LogInformation("Games: " + String.Join(",", _gameEngine.Games));
      await Clients.Caller.SendAsync("OnCreateGameResponse", resp);
    }

    public async Task JoinGame(JoinGameRequest request)
    {
      if (String.IsNullOrWhiteSpace(request.GameId))
        return;

      _logger.LogInformation("JoinGame {}", request);
      var resp = new JoinGameResponse() { RequestId = request.RequestId, IsSuccess = true };

      await Clients.Caller.SendAsync("OnJoinGameResponse", resp);
    }
  }
}