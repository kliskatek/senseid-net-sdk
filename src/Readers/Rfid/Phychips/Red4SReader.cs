using Kliskatek.Driver.Rain.REDRCP;
using Kliskatek.Driver.Rain.REDRCP.Transports;
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
            var connectionSuccessful = _rcp.Connect(JsonConvert.SerializeObject(parameters));
            if (connectionSuccessful)
                _rcp.NewNotificationReceived += OnNewNotificationReceived;
            return connectionSuccessful;
        }

        protected override bool DisconnectLowLevel()
        {
            var disconnectionSuccessful = _rcp.Disconnect();
            if (disconnectionSuccessful)
                _rcp.NewNotificationReceived -= OnNewNotificationReceived;
            return disconnectionSuccessful;
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
            return (_rcp.StartAutoRead2() == RcpResultType.Success);
        }

        protected override bool StopDataAcquisitionAsyncLowLevel()
        {
            return (_rcp.StopAutoRead2() == RcpResultType.Success);
        }

        private void OnNewNotificationReceived(object? sender, NotificationEventArgs e)
        {
            switch (e.NotificationType)
            {
                case SupportedNotifications.ReadTypeCUii:
                    OnReadTypeCUiiNotification((ReadTypeCUiiNotificationParameters)e.NotificationParameters);
                    break;
                default:
                    break;
            }
        }

        private void OnReadTypeCUiiNotification(ReadTypeCUiiNotificationParameters parameters)
        {
            _callback(parameters.Epc);
        }
    }
}
