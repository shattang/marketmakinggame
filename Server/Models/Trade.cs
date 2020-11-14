using System;
using MarketMakingGame.Shared.Models;

namespace MarketMakingGame.Server.Models
{
  public class Trade
  {
    public int TradeId { get; set; }

    public int GameStateId { get; set; }
    public GameState GameState { get; set; }

    public string InitiatingPlayerPlayerId { get; set; }
    public Player InitiatingPlayer { get; set; }

    public string TargetPlayerPlayerId { get; set; }
    public Player TargetPlayer { get; set; }

    public bool IsBuy { get; set; }

    public double TradePrice { get; set; }
  }
}