using System;

namespace MarketMakingGame.Shared.Messages
{
  public sealed class JoinGameRequest : BaseRequest
  {
    public string UserName { get; set; }

    public string GameId { get; set; }

    public override string ToString()
    {
      return $"JoinGameRequest(UserName={UserName}, GameId={GameId}, " + base.ToString() + ")";
    }
  }
}