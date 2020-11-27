using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MarketMakingGame.Shared.Lib;

namespace MarketMakingGame.Shared.Models
{
  public class Game
  {
    [Required]
    [MaxLength(36)]
    [Column(TypeName = "char")]
    public String GameId { get; set; }

    [Required]
    [MaxLength(36)]
    [MinLength(3)]
    [Column(TypeName = "char")]
    public String GameName { get; set; }

    public int? NumberOfRounds { get; set; } = 5;

    public double? MinQuoteWidth { get; set; } = 1;

    public double? MaxQuoteWidth { get; set; } = 5;

    public double? TradeQty { get; set; } = 100;

    public override string ToString()
    {
      return this.ToStringWithProperties();
    }
  }
}