using System;
using System.Collections.Generic;
using MarketMakingGame.Shared.Lib;

namespace MarketMakingGame.Shared.Messages
{
  public class GameUpdateResponse : BaseResponse
  {
    public string GameId { get; set; }

    public List<PlayerPublicState> PlayerPublicStates { get; set; }

    public List<int> CommunityCardIds { get; set; }

    public List<TradeUpdate> TradeUpdates { get; set; }

    public double? BestCurrentBid { get; set; }

    public double? BestCurrentAsk { get; set; }

    public override string ToString()
    {
      return this.ToStringWithProperties();
    }
  }
}