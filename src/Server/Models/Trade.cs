using System;
using MarketMakingGame.Shared.Models;

namespace MarketMakingGame.Server.Models
{
  public class Trade
  {
    public int TradeId { get; set; }

    public int GameStateId { get; set; }
    public virtual GameState GameState { get; set; }

    public int InitiatorPlayerStateId { get; set; }
    public virtual PlayerState Initiator { get; set; }

    public int TargetPlayerStateId { get; set; }
    public virtual PlayerState Target { get; set; }

    public bool IsBuy { get; set; }

    public double TradePrice { get; set; }

    public double TradeQty { get; set; }
  }
}