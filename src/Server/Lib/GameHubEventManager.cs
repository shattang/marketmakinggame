using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MarketMakingGame.Server.Hubs;
using MarketMakingGame.Shared.Messages;
using Microsoft.AspNetCore.SignalR;

namespace MarketMakingGame.Server.Lib
{
  public class GameHubEventManager
  {
    public class GameMembership
    {
      public string PlayerId { get; set; }
      public string GameId { get; set; }
    }

    private ConcurrentDictionary<string, List<GameMembership>> _connectionToMembership;
    private IHubContext<GameHub> _hubContext;

    public GameHubEventManager(IHubContext<GameHub> hubContext)
    {
      _connectionToMembership = new ConcurrentDictionary<string, List<GameMembership>>();
      _hubContext = hubContext;
    }

    public async Task<IEnumerable<GameMembership>> RemoveConnection(string connectionId)
    {
      if (_connectionToMembership.TryRemove(connectionId, out var memberships))
      {
        foreach (var m in memberships)
        {
          await _hubContext.Groups.RemoveFromGroupAsync(connectionId, m.GameId);
          await _hubContext.Groups.RemoveFromGroupAsync(connectionId, TopicName(m.GameId, m.PlayerId));
        }
        return memberships;
      }
      return Enumerable.Empty<GameMembership>();
    }

    public void HandleGameUpdate(GameUpdateResponse resp)
    {
      _ = _hubContext.Clients.Group(resp.GameId).SendAsync("OnGameUpdateResponse", resp);
    }

    public void HandlePlayerUpdate(PlayerUpdateResponse resp)
    {
      _ = _hubContext.Clients.Group(TopicName(resp.GameId, resp.PlayerId)).SendAsync("OnPlayerUpdateResponse", resp);
    }

    public void HandleTradeUpdate(string playerId, TradeUpdateResponse resp)
    {
      _ = _hubContext.Clients.Group(TopicName(resp.GameId, playerId)).SendAsync("OnTradeUpdateResponse", resp);
    }

    public async Task AddConnection(string gameId, string playerId, string connectionId)
    {
      _connectionToMembership.GetOrAdd(connectionId, x => new List<GameMembership>())
        .Add(new GameMembership() { GameId = gameId, PlayerId = playerId });
      await _hubContext.Groups.AddToGroupAsync(connectionId, gameId);
      await _hubContext.Groups.AddToGroupAsync(connectionId, TopicName(gameId, playerId));
    }

    private static string TopicName(string gameId, string playerId = null)
    {
      return playerId == null ? gameId : $"{gameId}.{playerId}";
    }
  }
}