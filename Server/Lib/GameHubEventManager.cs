using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using MarketMakingGame.Server.Hubs;
using MarketMakingGame.Shared.Messages;
using Microsoft.AspNetCore.SignalR;

namespace MarketMakingGame.Server.Lib
{
  public class GameHubEventManager: IDisposable
  {
    private class GameMembership
    {
      public string PlayerId { get; set; }
      public string GameId { get; set; }
    }

    private ConcurrentDictionary<string, List<GameMembership>> _connectionToMembership;
    private IHubContext<GameHub> _hubContext;
    private GameService _gameService;

    public GameHubEventManager(GameService gameService, IHubContext<GameHub> hubContext)
    {
      _gameService = gameService;
      _connectionToMembership = new ConcurrentDictionary<string, List<GameMembership>>();
      _hubContext = hubContext;
      gameService.OnGameUpdate += HandleGameUpdate;
      gameService.OnPlayerUpdate += HandlePlayerUpdate;
    }

    public async Task RemoveConnection(string connectionId)
    {
      if (_connectionToMembership.TryRemove(connectionId, out var memberships))
      {
        foreach (var m in memberships)
        {
          await _hubContext.Groups.RemoveFromGroupAsync(connectionId, m.GameId);
          await _hubContext.Groups.RemoveFromGroupAsync(connectionId, TopicName(m.GameId, m.PlayerId));
          await _gameService.OnPlayerDisconnectedAsync(m.GameId, m.PlayerId);
        }
      }
    }

    private void HandleGameUpdate(GameUpdateResponse resp)
    {
      _ = _hubContext.Clients.Group(resp.GameId).SendAsync("OnGameUpdateResponse", resp);
    }

    private void HandlePlayerUpdate(PlayerUpdateResponse resp)
    {
      _ = _hubContext.Clients.Group(TopicName(resp.GameId, resp.PlayerId)).SendAsync("OnPlayerUpdateResponse", resp);
    }

    public async Task AddConnection(string gameId, string playerId, string connectionId)
    {
      _connectionToMembership.GetOrAdd(connectionId, x => new List<GameMembership>())
        .Add(new GameMembership() { GameId = gameId, PlayerId = playerId });
      await _hubContext.Groups.AddToGroupAsync(connectionId, gameId);
      await _hubContext.Groups.AddToGroupAsync(connectionId, TopicName(gameId, playerId));
    }

    private static string TopicName(string gameId, string playerId)
    {
      return $"{gameId}.{playerId}";
    }

    public void Dispose()
    {
      _gameService.OnGameUpdate -= HandleGameUpdate;
      _gameService.OnPlayerUpdate -= HandlePlayerUpdate;
    }
  }
}