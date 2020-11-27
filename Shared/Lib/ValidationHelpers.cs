using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.DataAnnotations;

namespace MarketMakingGame.Shared.Lib
{
  public static class ValidationHelpers
  {
    public static (bool Success, string ErrorMessages) ValidateObject<T>(T obj)
    {
      var validationResults = new List<ValidationResult>();
      var validationContext = new ValidationContext(obj, null, null);
      var b = Validator.TryValidateObject(obj, validationContext, validationResults, true);
      return (b, String.Join(Environment.NewLine, validationResults.Select(x => x.ErrorMessage)));
    }

    public static (bool Success, string ErrorMessages) ValidateProperty<T, P>(T obj, P propValue, string propertyName)
    {
      var validationResults = new List<ValidationResult>();
      var validationContext = new ValidationContext(obj, null, null) { MemberName = propertyName };
      var b = Validator.TryValidateProperty(propValue, validationContext, validationResults);
      return (b, String.Join(Environment.NewLine, validationResults.Select(x => x.ErrorMessage)));
    }
  }
}