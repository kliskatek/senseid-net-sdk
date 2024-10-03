using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kliskatek.Driver.Rain.REDRCP;
using Kliskatek.Driver.Rain.REDRCP.CommunicationBuses;
using Kliskatek.SenseId.Sdk.Readers.Common;
using Newtonsoft.Json;

namespace Kliskatek.SenseId.Sdk.Readers.Rfid.Phychips
{
    public class Red4SReader : SenseIdReaderBase
    {
        private REDRCP _rcp;
        private SenseIdReaderCallback _callback;

        protected override bool ConnectLowLevel(string connectionString)
        {
            _rcp = new REDRCP();
            SerialPortConnectionParameters parameters = new SerialPortConnectionParameters
            {
                PortName = connectionString
            };
            return _rcp.Connect(JsonConvert.SerializeObject(parameters));
        }

        protected override bool DisconnectLowLevel()
        {
            return _rcp.Disconnect();
        }

        protected override bool GetReaderInfo()
        {
            ReaderInfo = new SenseIdReaderInfo();
            ReaderInfo.AntennaCount = 1;
            return true;
            throw new NotImplementedException();
        }

        protected override bool SetTxPowerLowLevel(float txPower)
        {
            return true;
            throw new NotImplementedException();
        }

        protected override bool SetAntennaConfigLowLevel(bool[] antennaConfigArray)
        {
            return true;
            throw new NotImplementedException();
        }

        protected override bool StartDataAcquisitionAsyncLowLevel(SenseIdReaderCallback callback)
        {
            _callback = callback;
            return _rcp.StartAutoRead2(AutoRead2NotificationCallback);
        }

        protected override bool StopDataAcquisitionAsyncLowLevel()
        {
            return _rcp.StopAutoRead2();
        }

        private void AutoRead2NotificationCallback(string pc, string epc)
        {
            _callback(epc);
        }
    }
}
