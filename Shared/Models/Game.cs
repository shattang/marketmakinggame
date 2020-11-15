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
    [StringLength(36, MinimumLength = 1)]
    [Column(TypeName = "char")]
    public String GameId { get; set; }

    [Required]
    [StringLength(20, MinimumLength = 3)]
    [Column(TypeName = "char")]
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