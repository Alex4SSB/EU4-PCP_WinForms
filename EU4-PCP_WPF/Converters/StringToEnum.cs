using EU4_PCP_WPF.Models;
using System;

namespace EU4_PCP_WPF.Converters
{
    static class StringToEnum
    {
        public static Type ToEnum(this string enumName)
        {
            return enumName switch
            {
                "AutoLoad" => typeof(AutoLoad),
                "ProvinceNames" => typeof(ProvinceNames),
                _ => throw new NotImplementedException()
            };
        }
    }
}
