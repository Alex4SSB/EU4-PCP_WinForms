using System.Windows;

namespace EU4_PCP_WPF.Converters
{
    static class AddConst
    {
        public static long GetDefault(this string key)
        {
            return long.Parse(Names.GlobalNames[key + "Default"]);
        }

        public static string GetPlaceholder(this FrameworkElement control) => GetPlaceholder(control.Tag.ToString());

        public static string GetPlaceholder(this string key)
        {
            return Names.GlobalNames[key + "Placeholder"];
        }
    }
}
