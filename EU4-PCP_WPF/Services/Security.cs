using EU4_PCP_WPF.Properties;

namespace EU4_PCP_WPF.Services
{
    class Security
    {
        public static string RetrieveValue(object tag) => RetrieveValue((string)tag)?.ToString();

        public static object RetrieveValue(string keyName)
        {
            return App.Current.Properties[keyName];
        }

        public static void StoreValue(string value, object tag) => StoreValue((object)value, (string)tag);

        public static void StoreValue(object value, string keyName)
        {
            if (value is null || 
                (value is string strVal && string.IsNullOrEmpty(strVal))) return;
            App.Current.Properties[keyName] = value;
        }
    }
}
