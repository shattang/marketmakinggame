using System;
using MarketMakingGame.Shared.Lib;
using MarketMakingGame.Shared.Models;

namespace MarketMakingGame.Shared.Messages
{
  public sealed class TradeRequest : BaseRequest
  {
    public string PlayerId { get; set; }

    public string GameId { get; set; }

    public bool IsBuy { get; set; }

    public double? OrderPrice { get; set; }

    public override string ToString()
    {
      return this.ToStringWithProperties();
    }
  }
}