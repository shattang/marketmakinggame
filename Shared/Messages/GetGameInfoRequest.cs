using System;
using System.Collections.Generic;
using MarketMakingGame.Shared.Lib;

namespace MarketMakingGame.Shared.Messages
{
  public sealed class GetGameInfoRequest : BaseRequest
  {
    public List<string> GameIds { get; set; }

    public override string ToString()
    {
      return this.ToStringWithProperties();
    }
  }
}