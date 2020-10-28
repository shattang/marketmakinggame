using System;
using MarketMakingGame.Shared.Lib;

namespace MarketMakingGame.Shared.Messages
{
  public sealed class JoinGameResponse : BaseResponse
  {
    public override string ToString()
    {
      return this.ToStringWithProperties();
    }
  }
}