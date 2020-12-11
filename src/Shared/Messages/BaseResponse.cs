using System;
using MarketMakingGame.Shared.Lib;

namespace MarketMakingGame.Shared.Messages
{
  public class BaseResponse : BaseMessage
  {
    public string RequestId { get; set; }

    public bool IsSuccess { get; set; }

    public String ErrorMessage { get; set; }

    public override string ToString()
    {
      return this.ToStringWithProperties();
    }
  }
}