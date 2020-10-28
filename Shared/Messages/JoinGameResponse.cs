using System;
using MarketMakingGame.Shared.Lib;

namespace MarketMakingGame.Shared.Messages
{
  public sealed class JoinGameResponse : BaseResponse
  {
    public JoinGameResponse()
    { }

    public JoinGameResponse(JoinGameRequest request) : base(request)
    {
    }

    public override string ToString()
    {
      return this.ToStringWithProperties();
    }
  }
}