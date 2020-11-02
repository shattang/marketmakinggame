using System;
using System.Collections.Generic;
using MarketMakingGame.Shared.Models;
using MarketMakingGame.Shared.Lib;

namespace MarketMakingGame.Shared.Messages
{
  public sealed class GetGameInfoResponse : BaseResponse
  {
    public List<GameInfo> GameInfos { get; set; }

    public override string ToString()
    {
      return this.ToStringWithProperties();
    }
  }
}