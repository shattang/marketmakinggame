using System;
using MarketMakingGame.Shared.Lib;
using MarketMakingGame.Shared.Models;

namespace MarketMakingGame.Shared.Messages
{
  public sealed class UpdateQuoteResponse : BaseResponse
  {
    public override string ToString()
    {
      return this.ToStringWithProperties();
    }
  }
}