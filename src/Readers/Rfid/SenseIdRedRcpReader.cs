using Kliskatek.Driver.Rain.REDRCP;
using Kliskatek.Driver.Rain.REDRCP.Transports;
using Kliskatek.SenseId.Sdk.Readers.Common;
using Newtonsoft.Json;

namespace Kliskatek.SenseId.Sdk.Readers.Rfid
{
    public class SenseIdRedRcpReader : SenseIdReaderBase
    {
        private REDRCP _reader;
        private SenseIdReaderCallback _callback;

        protected override bool ConnectLowLevel(string connectionString)
        {
            _reader = new REDRCP();
            SerialPortConnectionParameters parameters = new SerialPortConnectionParameters
            {
                PortName = connectionString
            };
            if (!_reader.Connect(JsonConvert.SerializeObject(parameters)))
                return false;
            _reader.NewNotificationReceived += OnNewNotificationReceived;
            if (!SetAntiCollisionMode())
                return false;
            if (!SetQueryDefaultParameters())
                return false;

            return true;
        }

        private bool SetAntiCollisionMode()
        {
            return (_reader.SetAntiCollisionMode(new AntiCollisionModeParameters
            {
                Mode = AntiCollisionMode.Manual,
                QStart = 4,
                QMin = 0,
                QMax = 7
            }) == RcpResultType.Success);
        }

        private bool SetQueryDefaultParameters()
        {
            return _reader.SetTypeCaiQueryParameters(new TypeCaiQueryParameters
            {
                Dr = ParamDr.Dr64Div3,
                Modulation = ParamModulation.Miller4,
                TRext = false,
                Sel = ParamSel.All0,
                Session = ParamSession.S1,
                Target = ParamTarget.A,
                Toggle = ParamToggle.EveryInventoryRound,
                Q = 4
            }) == RcpResultType.Success;
        }

        protected override bool DisconnectLowLevel()
        {
            var disconnectionSuccessful = _reader.Disconnect();
            if (disconnectionSuccessful)
                _reader.NewNotificationReceived -= OnNewNotificationReceived;
            return disconnectionSuccessful;
        }

        protected override bool GetReaderInfo()
        {
            ReaderInfo = new SenseIdReaderInfo();
            ReaderInfo.AntennaCount = 1;
            if (_reader.GetReaderInformationFirmwareVersion(out var firmwareVersion) != RcpResultType.Success)
                return false;
            ReaderInfo.FirmwareVersion = firmwareVersion;
            if (_reader.GetReaderInformationDetails(out var details) != RcpResultType.Success)
                return false;
            ReaderInfo.MaxTxPower = (float)details.MaxTxPower;
            ReaderInfo.MinTxPower = (float)details.MinTxPower;
            ReaderInfo.Region = details.Region.ToString();
            if (_reader.GetReaderInformationReaderModel(out var model) != RcpResultType.Success)
                return false;
            ReaderInfo.ModelName = model;
            return true;
        }

        protected override bool SetTxPowerLowLevel(float txPower)
        {
            return _reader.SetTxPowerLevel(txPower) == RcpResultType.Success;
        }

        protected override bool SetEnabledAntennasLowLevel(bool[] antennaConfigArray)
        {
            var antennaSequence = new List<byte>();
            byte antennaEnableMask = 0x01;
            int antennaIndex = 0;
            foreach (var antenna in antennaConfigArray)
            {
                if (antenna)
                    antennaSequence.Add((byte)(antennaEnableMask << antennaIndex));
                antennaIndex++;
            }
            return _reader.SetMultiAntennaSequence(antennaSequence) == RcpResultType.Success;
        }

        protected override bool StartDataAcquisitionAsyncLowLevel(SenseIdReaderCallback callback)
        {
            _callback = callback;
            return _reader.StartAutoRead2() == RcpResultType.Success;
        }

        protected override bool StopDataAcquisitionAsyncLowLevel()
        {
            return _reader.StopAutoRead2() == RcpResultType.Success;
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
