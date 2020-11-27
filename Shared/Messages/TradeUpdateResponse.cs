using System;
using MarketMakingGame.Shared.Lib;
using System.Collections.Generic;

namespace MarketMakingGame.Shared.Messages
{
  public class TradeUpdateResponse : BaseResponse
  {
    public string GameId { get; set; }

    public List<TradeUpdate> TradeUpdates { get; set; }

    public override string ToString()
    {
      return this.ToStringWithProperties();
    }
  }
}