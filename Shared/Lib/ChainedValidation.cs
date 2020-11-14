
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Linq;

namespace MarketMakingGame.Shared.Lib
{
  public class ChainedValidation : ValidationAttribute
  {
    private List<ValidationAttribute> _attributes;

    public ChainedValidation(params ValidationAttribute[] validationAttributes)
    {
      _attributes = validationAttributes.ToList();
    }

    public override bool IsValid(object value)
    {
      return _attributes.All(x => x.IsValid(value));
    }
  }
}
