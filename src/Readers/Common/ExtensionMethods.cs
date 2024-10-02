using System.IO.Ports;

namespace Kliskatek.SenseId.Sdk.Readers.Common
{
    public static class ExtensionsMethods
    {
        public static void WriteToSerialInterface(this List<byte> byteList, SerialPort serialPort)
        {
            serialPort.Write(byteList.ToArray(), 0, byteList.Count);
        }

        public static void WriteToSerialInterface(this byte[] byteArray, SerialPort serialPort)
        {
            serialPort.Write(byteArray, 0, byteArray.Length);
        }

        public static T[] GetArraySlice<T>(this T[] inputArray, int startIndex, int elementCount)
        {
            return (new ArraySegment<T>(inputArray)).Slice(startIndex, elementCount).ToArray();
        }

        public static T[] GetArraySlice<T>(this T[] inputArray, int startIndex)
        {
            return (new ArraySegment<T>(inputArray)).Slice(startIndex, inputArray.Length - startIndex).ToArray();
        }
    }
}
