using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MarketMakingGame.Shared.Messages;
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

    public async Task CreateGame(CreateGameRequest request)
    {
      _logger.LogInformation("CreateGame {}", request);
      var resp = new CreateGameResponse(request);
      await Clients.All.SendAsync("GameCreated", resp);
    }
  }
}