using System;
using MarketMakingGame.Shared.Lib;

namespace MarketMakingGame.Shared.Messages
{
  public class BaseResponse : BaseMessage
  {
    protected BaseResponse()
    {
      IsSuccess = false;
    }

    protected BaseResponse(BaseRequest request, bool success)
    {
      IsSuccess = success;
      RequestId = request.RequestId;
    }

    public string RequestId { get; set; }

    public bool IsSuccess { get; set; }

    public override string ToString()
    {
      return this.ToStringWithProperties();
    }
  }
}