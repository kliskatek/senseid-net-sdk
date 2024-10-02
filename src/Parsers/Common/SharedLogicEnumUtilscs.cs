using System.ComponentModel.DataAnnotations;

namespace Kliskatek.SenseId.Sdk.Parsers.Common
{
    public static partial class SharedLogic
    {
        public static IEnumerable<T> GetValues<T>()
        {
            return Enum.GetValues(typeof(T)).Cast<T>();
        }

        private static DisplayAttribute GetDisplayAttributes(object value, Type objType)
        {
            var att = objType.GetField(value.ToString()).GetCustomAttributes(false).FirstOrDefault();
            return (DisplayAttribute)(att is DisplayAttribute ? att : null);
        }

        public static string GetDisplayShortName(this object value)
        {
            var da = GetDisplayAttributes(value, value.GetType());
            return da?.ShortName;
        }

        public static bool EnumShortNameMatch<T>(string test, out T returnValue)
        {
            T foundValue = GetValues<T>().FirstOrDefault();
            foreach (var enumValue in GetValues<T>())
            {

                if (test.Equals(enumValue.GetDisplayShortName()))
                {
                    foundValue = enumValue;
                    returnValue = foundValue;
                    return true;
                }
            }
            returnValue = foundValue;
            return false;
        }
    }
}