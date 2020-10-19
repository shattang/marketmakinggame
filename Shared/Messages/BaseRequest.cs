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

    public abstract string RequestName { get; }

    public string RequestId { get; set; }

    public override string ToString()
    {
      return $"RequestId={RequestId}";
    }
  }
}