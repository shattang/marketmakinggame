using System;

namespace MarketMakingGame.Shared.Messages
{
  public sealed class CreateGameRequest : BaseRequest
  {
    public override string RequestName => "CreateGame";

    public string UserName { get; set; }

    public string GameName { get; set; }

    public override string ToString()
    {
      return $"(UserName={UserName}, GameName={GameName}, " + base.ToString() + ")";
    }
  }
}