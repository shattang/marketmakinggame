using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using MarketMakingGame.Shared.Lib;

namespace MarketMakingGame.Shared.Models
{
  public class Game
  {
    public class GameNameValidation : ChainedValidation
    {
      public GameNameValidation() : base(new RequiredAttribute(),
        new StringLengthAttribute(20) { ErrorMessage = "Max 20 Characters" },
        new MinLengthAttribute(3) { ErrorMessage = "Minimum 20 Characters" })
      { }
    }

    public String GameId { get; set; }

    [GameNameValidation]
    public String GameName { get; set; }

    public int? NumberOfRounds { get; set; } = 5;

    public double? MinQuoteWidth { get; set; } = 1;

    public double? MaxQuoteWidth { get; set; } = 5;

    public override string ToString()
    {
      return this.ToStringWithProperties();
    }
  }
}