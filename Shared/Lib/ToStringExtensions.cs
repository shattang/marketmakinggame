using System;
using System.Text;

namespace MarketMakingGame.Shared.Lib
{
  public static class ToStringExtensions
  {
    public static string ToStringWithProperties(this object obj)
    {
      Type type = obj.GetType();
      var props = type.GetProperties();
      var sb = new StringBuilder();
      sb.Append(type.Name);
      sb.Append("{");
      for (int i = 0; i < props.Length; ++i)
      {
        var p = props[i];
        var val = p.GetValue(obj, null);
        sb.Append(p.Name + ": " + (val == null ? "null" : val));
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