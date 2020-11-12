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
    private readonly GameService _gameService;

    public GameHub(ILogger<GameHub> logger, GameService gameService)
    {
      _logger = logger;
      _gameService = gameService;
    }

    public GetCardsResponse GetCards(GetCardsRequest request)
    {
      return _gameService.GetCards(request);
    }

    public GetGameInfoResponse GetGameInfo(GetGameInfoRequest request)
    {
      return _gameService.GetGameInfo(request);
    }

    public async Task CreateGame(CreateGameRequest request)
    {
      await Clients.Caller.SendAsync("OnCreateGameResponse", _gameService.CreateGame(request));
    }

    public async Task JoinGame(JoinGameRequest request)
    {
      await Clients.Caller.SendAsync("OnJoinGameResponse", _gameService.JoinGame(request));
    }
  }
}