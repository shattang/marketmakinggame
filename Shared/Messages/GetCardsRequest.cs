using System;
using System.Collections.Generic;
using MarketMakingGame.Shared.Lib;

namespace MarketMakingGame.Shared.Messages
{
  public sealed class GetCardsRequest : BaseRequest
  {
    public override string ToString()
    {
      return this.ToStringWithProperties();
    }
  }
}