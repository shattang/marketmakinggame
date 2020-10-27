using System;
using System.ComponentModel.DataAnnotations;
using MarketMakingGame.Shared.Lib;

namespace MarketMakingGame.Client.Models
{
  public class CreateGameData
  {
    [Required]
    [MinLength(3, ErrorMessage = "Min 3 characters.")]
    [MaxLength(20, ErrorMessage = "Max 20 characters.")]
    public String GameName { get; set; }

    public override string ToString()
    {
      return this.ToStringWithProperties();
    }
  }
}