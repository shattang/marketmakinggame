using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using MarketMakingGame.Shared.Lib;

namespace MarketMakingGame.Shared.Models
{
  public class Card
  {
    public int CardId { get; set; }
 
    [Required]
    public string CardImageUrl { get; set; }

    [Required]
    public string CardDescription { get; set; }

    public double CardValue { get; set; }

    public override string ToString()
    {
      return this.ToStringWithProperties();
    }
  }
}