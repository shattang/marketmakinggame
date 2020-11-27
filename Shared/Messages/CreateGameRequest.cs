using System;
using MarketMakingGame.Shared.Lib;
using MarketMakingGame.Shared.Models;

namespace MarketMakingGame.Shared.Messages
{
  public sealed class CreateGameRequest : BaseRequest
  {
    public Player Player { get; set; }

    public Game Game { get; set; }

    public override string ToString()
    {
      return this.ToStringWithProperties();
    }
  }
}