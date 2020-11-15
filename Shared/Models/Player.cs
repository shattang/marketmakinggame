using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MarketMakingGame.Shared.Lib;

namespace MarketMakingGame.Shared.Models
{
  public class Player
  {
    [Required]
    [StringLength(36, MinimumLength = 1)]
    [Column(TypeName = "char")]
    public String PlayerId { get; set; }

    [Required]
    [StringLength(20, MinimumLength = 3)]
    [Column(TypeName = "char")]
    public String DisplayName { get; set; }

    [Required]
    [StringLength(100, MinimumLength = 1)]
    [Column(TypeName = "char")]
    public String AvatarSeed { get; set; }

    public override string ToString()
    {
      return this.ToStringWithProperties();
    }
  }
}