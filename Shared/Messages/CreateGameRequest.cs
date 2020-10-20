using System;

namespace MarketMakingGame.Shared.Messages
{
  public sealed class CreateGameRequest : BaseRequest
  {
    public string UserName { get; set; }

    public string GameName { get; set; }

    public override string ToString()
    {
      return $"CreateGameRequest(UserName={UserName}, GameName={GameName}, " + base.ToString() + ")";
    }
  }
}