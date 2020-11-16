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

    private void HandleGameUpdate(GameUpdateResponse resp)
    {
      _ = Clients.Group(resp.GameId).SendAsync("OnGameUpdate", resp);
    }

    private void HandlePlayerUpdate(PlayerUpdateResponse resp)
    {
      _ = Clients.Group($"{resp.GameId}.{resp.PlayerId}").SendAsync("OnPlayerUpdate", resp);
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
      CreateGameResponse resp;
      try
      {
        resp = await _gameService.CreateGameAsync(request);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, nameof(CreateGame));
        resp = new CreateGameResponse() { ErrorMessage = $"{ex.GetType()}: {ex.Message}" };
      }

      if (resp.IsSuccess)
      {
        await AddMembership(resp.GameId, request.Player.PlayerId, Context.ConnectionId);
      }

      _logger.LogInformation("Sending Response: {}", resp);
      await Clients.Caller.SendAsync("OnCreateGameResponse", resp);
    }

    public async Task JoinGame(JoinGameRequest request)
    {
      JoinGameResponse resp;
      try
      {
        resp = await _gameService.JoinGame(request);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, nameof(JoinGame));
        resp = new JoinGameResponse() { ErrorMessage = $"{ex.GetType()}: {ex.Message}" };
      }

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