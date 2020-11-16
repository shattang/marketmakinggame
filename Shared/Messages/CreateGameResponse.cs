using System;
using MarketMakingGame.Shared.Lib;

namespace MarketMakingGame.Shared.Messages
{
  public sealed class CreateGameResponse : BaseResponse
  {
    public string GameId { get; set; }

    public override string ToString()
    {
      return this.ToStringWithProperties();
    }
  }
}