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

        private static bool _isSenseIdTypeValid(byte testSenseIdType, out byte senseIdType)
        {
            var senseIdTypeDefinitionDictionary = RainSenseIdDefinitions.Definitions.types;
            senseIdType = 0x00;
            foreach (var item in senseIdTypeDefinitionDictionary)
            {
                var testType = item.Key;
                if (testSenseIdType == testType)
                {
                    senseIdType = testType;
                    return true;
                }
            }
            return false;
        }

        private static bool _isFirmwareVersionValid(byte testFirmwareVersion, byte senseIdType,
            out byte firmwareVersion)
        {
            firmwareVersion = 0x00;
            var availableFirmwares = RainSenseIdDefinitions.Definitions.types[senseIdType].fw_versions;
            foreach (var senseIdFwVersion in availableFirmwares)
            {
                if (testFirmwareVersion == senseIdFwVersion)
                {
                    firmwareVersion = senseIdFwVersion;
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

            if (!_isSenseIdTypeValid(epcNoPen.First(), out var senseIdTypeKey))
                return _getDefaultIdTag(epc);

            var epcNoPenNoType = SharedLogic.GetArraySlice(epcNoPen, sizeof(byte));

            if (!_isFirmwareVersionValid(epcNoPenNoType.First(), senseIdTypeKey, out var firmwareVersion))
                return _getDefaultIdTag(epc);

            var epcNoPenNoTypeNoFirmware = SharedLogic.GetArraySlice(epcNoPenNoType, sizeof(byte));


            if (!_tryGetSerialNumber(epcNoPenNoTypeNoFirmware, out byte[] sn))
                return _getDefaultIdTag(epc);

            var senseIdEpcData = SharedLogic.GetArraySlice(epcNoPenNoTypeNoFirmware, sn.Length);
            var senseIdTagType = senseIdDefinitions.types[senseIdTypeKey];

            return new SenseIdTag
            {
                Technology = Technologies.Rain,
                Id = SharedLogic.ByteArrayToHexString((new[] { senseIdTypeKey, firmwareVersion }).Concat(sn).ToArray()),
                Name = senseIdTagType.name,
                Description = senseIdTagType.description,
                Data = _getSenseIdData(senseIdEpcData, senseIdTagType.data_def),
                FirmwareVersion = firmwareVersion
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
