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
    [MaxLength(36)]
    [Column(TypeName = "char")]
    public String PlayerId { get; set; }

    [Required]
    [MaxLength(36)]
    [MinLength(3)]
    [Column(TypeName = "char")]
    public String DisplayName { get; set; }

    [Required]
    [MaxLength(36)]
    [MinLength(3)]
    [Column(TypeName = "char")]
    public String AvatarSeed { get; set; }

    public override string ToString()
    {
      return this.ToStringWithProperties();
    }
  }
}