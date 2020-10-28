using System;
using MarketMakingGame.Shared.Lib;

namespace MarketMakingGame.Shared.Messages
{
  public sealed class CreateGameResponse : BaseResponse
  {
    public CreateGameResponse() : base()
    { }

    public CreateGameResponse(CreateGameRequest request) : base(request)
    {
      GameId = Guid.NewGuid().ToBase62();
    }

    public string GameId { get; set; }

    public override string ToString()
    {
      return this.ToStringWithProperties();
    }
  }
}