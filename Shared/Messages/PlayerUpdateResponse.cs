using System;
using MarketMakingGame.Shared.Lib;

namespace MarketMakingGame.Shared.Messages
{
  public class PlayerUpdateResponse : BaseResponse
  {
    public string GameId { get; set; }

    public string PlayerId { get; set; }

    public int PlayerPublicId { get; set; }

    public int CardId { get; set; }

    public override string ToString()
    {
      return this.ToStringWithProperties();
    }
  }
}