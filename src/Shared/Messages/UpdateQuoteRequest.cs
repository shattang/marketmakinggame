using System;
using MarketMakingGame.Shared.Lib;
using MarketMakingGame.Shared.Models;

namespace MarketMakingGame.Shared.Messages
{
  public sealed class UpdateQuoteRequest : BaseRequest
  {
    public string PlayerId { get; set; }

    public string GameId { get; set; }

    public double? CurrentBid { get; set; }

    public double? CurrentAsk { get; set; }

    public override string ToString()
    {
      return this.ToStringWithProperties();
    }
  }
}