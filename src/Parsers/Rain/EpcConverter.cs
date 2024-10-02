using Kliskatek.SenseId.Sdk.Parsers.Common;

namespace Kliskatek.SenseId.Sdk.Parsers.Rain
{
    public static class EpcConverter
    {
        private static bool _isEpcPenValid(byte[] epc)
        {
            var penHeader = RainSenseIdDefinitions.Definitions.pen_header;
            var penHeaderLength = penHeader.Length;
            if (epc.Length < penHeaderLength)
                return false;

            return SharedLogic.AreByteArrayHeadersEqual(penHeader, epc);
        }

        private static bool _isSenseIdTypeValid(byte[] epcSlice, out byte[] senseIdType)
        {
            var senseIdTypeDefinitionDictionary = RainSenseIdDefinitions.Definitions.types;
            senseIdType = new byte[] { 10, 10, 13 };
            foreach (var item in senseIdTypeDefinitionDictionary)
            {
                var testType = item.Key;
                if (SharedLogic.AreByteArrayHeadersEqual(epcSlice, testType))
                {
                    senseIdType = testType;
                    return true;
                }
            }
            return false;
        }

        private static bool _tryGetSerialNumber(byte[] epcSlice, out byte[] sn)
        {
            sn = new byte[Constants.SerialNumberByteLength];
            if (epcSlice.Length < Constants.SerialNumberByteLength)
                return false;
            sn = SharedLogic.GetArraySlice(epcSlice, 0, Constants.SerialNumberByteLength);
            return true;
        }

        private static SenseIdTag _getDefaultIdTag(byte[] epc)
        {
            return new SenseIdTag
            {
                Id = SharedLogic.ByteArrayToHexString(epc),
                Name = Constants.RegularTagName,
                Description = Constants.RegularTagDescription
            };
        }

        public static SenseIdTag ParseEpc(string epc)
        {
            if (!SharedLogic.IsHexString(epc))
                throw new ArgumentException("EPC " + epc + " is not a valid hexadecimal string");
            var epcTest = SharedLogic.RemoveHeadingText(epc, "0x").ToUpper();
            return ParseEpc(Convert.FromHexString(epcTest));
        }

        public static SenseIdTag ParseEpc(byte[] epc)
        {
            var senseIdDefinitions = RainSenseIdDefinitions.Definitions;

            if (senseIdDefinitions == null)
                throw new InvalidOperationException("No SenseID definitions available");

            if (!_isEpcPenValid(epc))
                return _getDefaultIdTag(epc);

            var epcNoPen = SharedLogic.GetArraySlice(epc, senseIdDefinitions.pen_header.Length);

            if (!_isSenseIdTypeValid(epcNoPen, out byte[] senseIdTypeKey))
                return _getDefaultIdTag(epc);

            var epcNoPenNoType = SharedLogic.GetArraySlice(epcNoPen, senseIdTypeKey.Length);

            if (!_tryGetSerialNumber(epcNoPenNoType, out byte[] sn))
                return _getDefaultIdTag(epc);

            var senseIdEpcData = SharedLogic.GetArraySlice(epcNoPenNoType, sn.Length);
            var senseIdTagType = senseIdDefinitions.types[senseIdTypeKey];
            return new SenseIdTag
            {
                Id = SharedLogic.ByteArrayToHexString(senseIdTypeKey.Concat(sn).ToArray()),
                Name = senseIdTagType.name,
                Description = senseIdTagType.description,
                Data = _getSenseIdData(senseIdEpcData, senseIdTagType.data_def)
            };
        }

        private static SenseIdData[] _getSenseIdData(byte[] epcData, SenseIdDataDefinitions[] dataDefinitions)
        {
            var returnArray = new SenseIdData[dataDefinitions.Length];
            int counter = 0;

            foreach (var dataConversion in dataDefinitions)
            {
                returnArray[counter++] = new SenseIdData
                {
                    Magnitude = dataConversion.magnitude,
                    UnitLong = dataConversion.unit_long,
                    UnitShort = dataConversion.unit_short,
                    Value = (float)SharedLogic.GetScalarTypeTransformed(ref epcData, dataConversion)
                };
            }

            return returnArray;
        }

    }
}
