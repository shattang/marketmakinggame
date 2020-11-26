using System;
using MarketMakingGame.Shared.Lib;
using MarketMakingGame.Shared.Models;

namespace MarketMakingGame.Shared.Messages
{
  public sealed class UpdateQuoteResponse : BaseResponse
  {
    public double BidPrice { get; set; }

    public double AskPrice { get; set; }
    
    public override string ToString()
    {
      return this.ToStringWithProperties();
    }
  }
}