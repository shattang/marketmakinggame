using System;
using System.Collections.Generic;
using MarketMakingGame.Shared.Models;
using MarketMakingGame.Shared.Lib;

namespace MarketMakingGame.Shared.Messages
{
  public sealed class GetCardsResponse : BaseResponse
  {
    public List<Card> Cards { get; set; }

    public Card UnopenedCard{ get; set; }

    public override string ToString()
    {
      return this.ToStringWithProperties();
    }
  }
}