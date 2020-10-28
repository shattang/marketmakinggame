using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MarketMakingGame.Shared.Messages;
using MarketMakingGame.Shared.Lib;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace MarketMakingGame.Server.Hubs
{
  public class GameHub : Hub
  {
    private readonly ILogger _logger;

    public GameHub(ILogger<GameHub> logger)
    {
      _logger = logger;
    }

    public async Task<GetGameInfoResponse> GetGameInfo(GetGameInfoRequest request)
    {
      _logger.LogInformation("GetGameInfo {}", request);
      return await Task.FromResult(
       new GetGameInfoResponse()
       {
         RequestId = request.RequestId,
         IsSuccess = true,
         GameIds = request.GameIds,
         GameNames = Enumerable.Repeat("foo", request.GameIds.Count).ToList()
       });
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