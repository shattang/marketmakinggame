using System;
using System.Collections.Generic;
using MarketMakingGame.Shared.Lib;

namespace MarketMakingGame.Shared.Messages
{
  public class TradeUpdate
  {
    public int TradeId { get; set; }
  
    public int InitiatorPlayerPublicId { get; set; }

    public int TargetPlayerPublicId { get; set; }

    public bool IsBuy { get; set; }

    public double TradePrice { get; set; }

    public double TradeQty { get; set; }

    public override string ToString()
    {
      return this.ToStringWithProperties();
    }
  }
}