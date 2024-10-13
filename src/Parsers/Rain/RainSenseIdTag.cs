using Kliskatek.SenseId.Sdk.Parsers.Common;

namespace Kliskatek.SenseId.Sdk.Parsers.Rain
{
    public class RainSenseIdTag : SenseIdTag
    {
        public RainSenseIdTag()
        {
            Technology = Technologies.Rain;
        }

        public RainSenseIdTag(string epc)
        {
            UpdateTagData(EpcConverter.ParseEpc(epc));
        }

        public RainSenseIdTag(byte[] epc)
        {
            UpdateTagData(EpcConverter.ParseEpc(epc));
        }

        private void UpdateTagData(SenseIdTag newTagData)
        {
            Technology = Technologies.Rain;
            Name = newTagData.Name;
            Id = newTagData.Id;
            Description = newTagData.Description;
            Data = newTagData.Data;
            FirmwareVersion = newTagData.FirmwareVersion;
        }
    }
}