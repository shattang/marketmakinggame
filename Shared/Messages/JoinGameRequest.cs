using System;
using MarketMakingGame.Shared.Lib;

namespace MarketMakingGame.Shared.Messages
{
  public sealed class JoinGameRequest : BaseRequest
  {
    public string UserId { get; set; }

    public string GameId { get; set; }

    public string UserName { get; set; }

    public string UserAvatar { get; set; }

    public override string ToString()
    {
      return this.ToStringWithProperties();
    }
  }
}