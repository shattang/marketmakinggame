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
    private readonly GameEngine _gameEngine;

    public GameHub(ILogger<GameHub> logger, GameEngine gameEngine)
    {
      _logger = logger;
      _gameEngine = gameEngine;
    }

    public async Task<GetGameInfoResponse> GetGameInfo(GetGameInfoRequest request)
    {
      _logger.LogInformation("GameInfos: " + String.Join(",", _gameEngine._gameInfos));
      var lists = request.GameIds.Select(x => _gameEngine._gameInfos.GetValueOrDefault(x, null)).Where(x => x != null).ToList();
      _logger.LogInformation("GetGameInfo {} {}", request, String.Join("," , lists));
      
      return //await Task.FromResult(
       new GetGameInfoResponse()
       {
         RequestId = request.RequestId,
         IsSuccess = true,
         GameInfos = lists
       }
       //)
       ;
    }

    public async Task CreateGame(CreateGameRequest request)
    {
      if (String.IsNullOrWhiteSpace(request.GameName) || request.GameName == "xxx")
        return;

      _logger.LogInformation("CreateGame {}", request);
      var resp = new CreateGameResponse()
      {
        RequestId = request.RequestId,
        IsSuccess = true,
        GameId = Guid.NewGuid().ToBase62()
      };
      var gameInfo = new GameInfo() { GameId = resp.GameId, GameName = request.GameName };
      _gameEngine._gameInfos[gameInfo.GameId] = gameInfo;
      _logger.LogInformation("GameInfos: " + String.Join(",", _gameEngine._gameInfos));
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