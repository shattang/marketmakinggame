using System;
using MarketMakingGame.Shared.Lib;
using MarketMakingGame.Shared.Models;

namespace MarketMakingGame.Shared.Messages
{
  public sealed class DealGameRequest : BaseRequest
  {
    public enum RequestTypes
    {
      DealPlayerCards,
      DealNextCommunityCard,
      LockTrading, 
      UnlockTrading,
      FinishGame,
      DeleteGame
    }

    public string PlayerId { get; set; }

    public string GameId { get; set; }

    public RequestTypes RequestType { get; set;}

    public override string ToString()
    {
      return this.ToStringWithProperties();
    }
  }
}