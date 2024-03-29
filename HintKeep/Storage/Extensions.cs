using System;
using System.Linq;
using System.Text;

namespace HintKeep.Storage
{
    public static class Extensions
    {
        public static string ToEncodedKeyProperty(this string value)
        {
            if (value is object && value.Any(IsReservedCharacter))
                return value
                    .Aggregate(
                        new StringBuilder(value.Sum(@char => IsReservedCharacter(@char) ? 3 : 1)),
                        (result, @char) => IsReservedCharacter(@char) ? result.AppendFormat("%{0:X2}", Convert.ToInt16(@char)) : result.Append(@char)
                    )
                    .ToString();
            else
                return value;

            static bool IsReservedCharacter(char @char)
                => @char == '%' || @char == '/' || @char == '\\' || @char == '#' || @char == '?' || (0x00 <= @char && @char <= 0x1F) || (0x7F <= @char && @char <= 0x9F) || char.IsWhiteSpace(@char);
        }
    }
}