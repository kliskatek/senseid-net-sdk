using System.Text.RegularExpressions;

namespace Kliskatek.SenseId.Sdk.Parsers.Common
{
    public static partial class SharedLogic
    {
        public static bool IsHexString(string test) => IsHexStringRegHex().IsMatch(test);

        [GeneratedRegex(@"\A\b(0[xX])?[0-9a-fA-F]+\b\Z")]
        private static partial Regex IsHexStringRegHex();

        public static string RemoveHeadingText(string text, string headingText)
        {
            return text.StartsWith(headingText) ?
                text.Substring(headingText.Length, text.Length - headingText.Length) :
                text;
        }

        public static bool AreByteArrayHeadersEqual(byte[] arrayA, byte[] arrayB)
        {
            var shortestArray = arrayA;
            var longestArray = arrayB;
            if (arrayB.Length < arrayA.Length)
            {
                shortestArray = arrayB;
                longestArray = arrayA;
            }

            foreach (var item in shortestArray.Select((value, i) => new { i, value }))
            {
                var testByte = item.value;
                var index = item.i;

                if (testByte != longestArray[index])
                    return false;
            }

            return true;
        }

        public static T[] GetArraySlice<T>(T[] inputArray, int startIndex, int elementCount)
        {
            return (new ArraySegment<T>(inputArray)).Slice(startIndex, elementCount).ToArray();
        }

        public static T[] GetArraySlice<T>(T[] inputArray, int startIndex)
        {
            return (new ArraySegment<T>(inputArray)).Slice(startIndex, inputArray.Length - startIndex).ToArray();
        }

        public static string ByteArrayToHexString(byte[] inputArray)
        {
            return BitConverter.ToString(inputArray).Replace("-", string.Empty);
        }
    }
}