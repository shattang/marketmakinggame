using System;
using MarketMakingGame.Shared.Lib;

namespace MarketMakingGame.Shared.Messages
{
  public abstract class BaseRequest
  {
    protected BaseRequest()
    {
      RequestId = Guid.NewGuid().ToBase62();
    }

    public string RequestId { get; set; }

    public override string ToString()
    {
      return $"RequestId={RequestId}";
    }
  }
}