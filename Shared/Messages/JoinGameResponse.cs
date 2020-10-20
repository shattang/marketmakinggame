using System;

namespace MarketMakingGame.Shared.Messages
{
  public sealed class JoinGameResponse : BaseResponse
  {
    public JoinGameResponse(JoinGameRequest request) : base(request, true)
    {
    }

    public override string ToString()
    {
      return $"JoinGameResponse(" + base.ToString() + ")";
    }
  }
}