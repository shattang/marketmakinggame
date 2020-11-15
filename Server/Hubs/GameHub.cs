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
using System.Collections.Concurrent;

namespace MarketMakingGame.Server.Hubs
{
  public class GameHub : Hub, IDisposable
  {
    private class GameMembership
    {
      public string PlayerId { get; set; }
      public string GameId { get; set; }
    }

    private readonly ILogger _logger;
    private readonly GameService _gameService;
    private ConcurrentDictionary<string, List<GameMembership>> _connectionToMembership;

    public GameHub(ILogger<GameHub> logger, GameService gameService)
    {
      _connectionToMembership = new ConcurrentDictionary<string, List<GameMembership>>();
      _logger = logger;
      _gameService = gameService;
      _gameService.OnGameUpdate += HandleGameUpdate;
      _gameService.OnPlayerUpdate += HandlePlayerUpdate;
    }

    public override async Task OnDisconnectedAsync(Exception exception)
    {
      if (_connectionToMembership.TryRemove(Context.ConnectionId, out var memberships))
      {
        foreach (var m in memberships)
        {
          await Groups.RemoveFromGroupAsync(Context.ConnectionId, m.GameId);
          await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"{m.GameId}.{m.PlayerId}");
        }
      }
    }

    private void HandleGameUpdate(string gameId, BaseResponse resp)
    {
      try
      {
        _ = Clients.Group(gameId).SendAsync("OnGameUpdate", resp);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, nameof(HandleGameUpdate));
      }
    }

    private void HandlePlayerUpdate(string gameId, string playerId, BaseResponse resp)
    {
      try
      {
        _ = Clients.Group($"{gameId}.{playerId}").SendAsync("OnPlayerUpdate", resp);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, nameof(HandleGameUpdate));
      }
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
      var resp = await _gameService.CreateGame(request);
      if (resp.IsSuccess)
      {
        await AddMembership(resp.GameId, request.Player.PlayerId, Context.ConnectionId);
      }

      _logger.LogInformation("Sending Response: {}", resp);
      await Clients.Caller.SendAsync("OnCreateGameResponse", resp);
    }

    public async Task JoinGame(JoinGameRequest request)
    {
      var resp = _gameService.JoinGame(request);
      if (resp.IsSuccess)
      {
        await AddMembership(request.GameId, request.Player.PlayerId, Context.ConnectionId);
      }

      _logger.LogInformation("Sending Response: {}", resp);
      await Clients.Caller.SendAsync("OnJoinGameResponse", resp);
    }

    private async Task AddMembership(string gameId, string playerId, string connectionId)
    {
      var memberships = _connectionToMembership.GetOrAdd(Context.ConnectionId, x => new List<GameMembership>());
      GameMembership membership = new GameMembership() { GameId = gameId, PlayerId = playerId };
      memberships.Add(membership);
      await Groups.AddToGroupAsync(connectionId, gameId);
      await Groups.AddToGroupAsync(connectionId, $"{gameId}.{playerId}");
    }
  }
}