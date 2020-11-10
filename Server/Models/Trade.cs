using System;

namespace MarketMakingGame.Server.Models
{
  public class Trade
  {
    public int TradeId { get; set; }

    public int GameStateId { get; set; }
    public GameState GameState { get; set; }

    public int InitiatingPlayerId { get; set; }

    public int TargetPlayerId { get; set; }

    public bool IsBuy { get; set; }

    public double TradePrice { get; set; }
  }
}