using System;

namespace MarketMakingGame.Shared.Messages
{
  public sealed class JoinGameRequest : BaseRequest
  {
    public override string RequestName => "JoinGame";

    public string UserName { get; set; }

    public string GameId { get; set; }

    public override string ToString()
    {
      return $"(UserName={UserName}, GameId={GameId}, " + base.ToString() + ")";
    }
  }
}