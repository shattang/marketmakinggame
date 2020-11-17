using System;
using MarketMakingGame.Shared.Lib;
using MarketMakingGame.Shared.Models;

namespace MarketMakingGame.Shared.Messages
{
  public sealed class DealerRequest : BaseRequest
  {
    public enum DealerRequestType
    {
      DealPlayerCards,
      DealNextCommunityCard,
      LockTrading, 
      UnlockTrading
    }

    public string PlayerId { get; set; }

    public string GameId { get; set; }

    public DealerRequestType RequestType { get; set;}

    public override string ToString()
    {
      return this.ToStringWithProperties();
    }
  }
}