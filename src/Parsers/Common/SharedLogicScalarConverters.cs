using System.Buffers.Binary;
using System.Text;

namespace Kliskatek.SenseId.Sdk.Parsers.Common
{
    public static partial class SharedLogic
    {
        public static double GetScalarTypeTransformed(ref byte[] byteArray,
            SenseIdDataDefinitions dataDefinition,
            Endianness endianness = Endianness.LittleEndian)
        {
            switch (dataDefinition.type)
            {
                case TypeEnum.Int16:
                    return TransformFromDouble(GetInt16FromByteArray(ref byteArray, endianness), dataDefinition);
                case TypeEnum.Uint16:
                    return TransformFromDouble(GetUint16FromByteArray(ref byteArray, endianness), dataDefinition);
                default:
                    throw new ArgumentException("Unsupported data type " + dataDefinition.type);
            }
        }

        private static Int16 GetInt16FromByteArray(ref byte[] byteArray, Endianness endianness = Endianness.LittleEndian)
        {
            if (byteArray.Length < 2)
                throw new ArgumentException($"Binary array {Encoding.UTF8.GetString(byteArray)} is too short to contain a Int16");
            var int16Bytes = GetArraySlice(byteArray, 0, 2);
            byteArray = GetArraySlice(byteArray, 2);
            return (endianness == Endianness.LittleEndian)
                ? BinaryPrimitives.ReadInt16LittleEndian(int16Bytes)
                : BinaryPrimitives.ReadInt16BigEndian(int16Bytes);
        }

        private static UInt16 GetUint16FromByteArray(ref byte[] byteArray, Endianness endianness = Endianness.LittleEndian)
        {
            if (byteArray.Length < 2)
                throw new ArgumentException($"Binary array {Encoding.UTF8.GetString(byteArray)} is too short to contain a UInt16");
            var uint16Bytes = GetArraySlice(byteArray, 0, 2);
            byteArray = GetArraySlice(byteArray, 2);
            return (endianness == Endianness.LittleEndian)
                ? BinaryPrimitives.ReadUInt16LittleEndian(uint16Bytes)
                : BinaryPrimitives.ReadUInt16BigEndian(byteArray);
        }
    }
}
