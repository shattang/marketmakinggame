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

    public double? BestCurrentBid { get; set; }

    public double? BestCurrentAsk { get; set; }

    public bool IsFinished { get; set; }
    
    public bool IsTradingLocked { get; set; }

    public double? SettlementPrice { get; set; }

    public bool AllRoundsFinished { get; set; }

    public override string ToString()
    {
      return this.ToStringWithProperties();
    }
  }
}