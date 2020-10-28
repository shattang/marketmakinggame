using System;
using System.Collections.Generic;
using MarketMakingGame.Shared.Lib;

namespace MarketMakingGame.Shared.Messages
{
  public sealed class GetGameInfoResponse : BaseResponse
  {
    public List<string> GameIds { get; set; }

    public List<string> GameNames { get; set; }

    public override string ToString()
    {
      return this.ToStringWithProperties();
    }
  }
}