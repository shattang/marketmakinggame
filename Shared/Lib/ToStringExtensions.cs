using System;
using System.Text;
using System.Collections;
using System.Linq;

namespace MarketMakingGame.Shared.Lib
{
  public static class ToStringExtensions
  {
    public static string ToStringWithProperties(this object obj)
    {
      string elementToString(object o)
      {
        if (o == null)
          return "null";

        if (o is ICollection)
        {
          return $"[{String.Join(",", ((ICollection)o).Cast<object>().Select(elementToString))}]";
        }

        return o.ToString();
      }

      Type type = obj.GetType();
      var props = type.GetProperties();
      var sb = new StringBuilder();
      sb.Append(type.Name);
      sb.Append("{");
      for (int i = 0; i < props.Length; ++i)
      {
        var p = props[i];
        var val = p.GetValue(obj, null);
        sb.Append(p.Name + ": " + elementToString(val));
        if (i < props.Length - 1)
        {
          sb.Append(", ");
        }
      }
      sb.Append("}");
      return sb.ToString();
    }
  }
}