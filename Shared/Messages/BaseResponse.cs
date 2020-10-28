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

    protected BaseResponse(BaseRequest request, bool success = true, string errorMessage = null)
    {
      IsSuccess = success;
      RequestId = request.RequestId;
      ErrorMessage = errorMessage;
    }

    public string RequestId { get; set; }

    public bool IsSuccess { get; set; }

    public String ErrorMessage { get; set; }

    public override string ToString()
    {
      return this.ToStringWithProperties();
    }
  }
}