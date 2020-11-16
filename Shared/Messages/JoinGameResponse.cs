using System;
using MarketMakingGame.Shared.Lib;
using MarketMakingGame.Shared.Models;

namespace MarketMakingGame.Shared.Messages
{
  public sealed class JoinGameResponse : BaseResponse
  {
    public Game Game { get; set; }

    public int PlayerPublicId { get; set; }

    public override string ToString()
    {
      return this.ToStringWithProperties();
    }
  }
}