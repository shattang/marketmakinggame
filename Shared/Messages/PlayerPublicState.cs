using System;
using System.Collections.Generic;
using MarketMakingGame.Shared.Lib;

namespace MarketMakingGame.Shared.Messages
{
  public class PlayerPublicState
  {
    public int PlayerPublicId { get; set; }

    public string DisplayName { get; set; }

    public string AvatarSeed { get; set; }

    public double? CurrentBid { get; set; }

    public double? CurrentAsk { get; set; }

    public double? PositionQty { get; set; }

    public double? PositionCashFlow { get; set; }

    public double? SettlementPnl { get; set; }

    public int? SettlementCardId { get; set; }

    public override string ToString()
    {
      return this.ToStringWithProperties();
    }
  }
}