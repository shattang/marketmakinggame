using System;
using MarketMakingGame.Shared.Lib;
using MarketMakingGame.Shared.Models;

namespace MarketMakingGame.Shared.Messages
{
  public sealed class DealPlayerCardsRequest : BaseRequest
  {
    public string PlayerId { get; set; }

    public string GameId { get; set; }

    public override string ToString()
    {
      return this.ToStringWithProperties();
    }
  }
}