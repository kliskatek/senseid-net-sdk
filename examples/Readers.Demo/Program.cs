using Kliskatek.SenseId.Sdk.Parsers.Rain;
using Kliskatek.SenseId.Sdk.Readers.Common;
using Kliskatek.SenseId.Sdk.Readers.Rfid.Nordic;
using Kliskatek.SenseId.Sdk.Readers.Rfid.Impinj;
using Kliskatek.SenseId.Sdk.Readers.Rfid.Phychips;

namespace Kliskatek.SenseId.Sdk.Readers.Demo
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Kliskatek SenseID.SDK.Readers demo");

            //var reader = (ISenseIdReader)new OctaneReader();
            //if (!reader.Connect("192.168.17.246"))
            //    return;
            //var reader = (ISenseIdReader)new NurApiReader();
            //if (!reader.Connect("ser://com9"))
            //    return;
            var reader = (ISenseIdReader)new Red4SReader();
            if (!reader.Connect("COM4"))
                return;

            reader.StartDataAcquisitionAsync(DelegateMethod);

            Thread.Sleep(4000);

            reader.StopDataAcquisitionAsync();

            reader.Disconnect();
        }

        public static void DelegateMethod(string epc)
        {
            var parsedEpc = epc.ToRainSenseIdTag();
            Console.WriteLine($"New EPC received : {epc} [{parsedEpc.Name}]");

            if (!String.Equals(parsedEpc.Name, Kliskatek.SenseId.Sdk.Parsers.Rain.Constants.RegularTagName))
            {
                Console.WriteLine($"SenseID data report");
                Console.WriteLine($"  * Name = {parsedEpc.Name}");
                Console.WriteLine($"  * Description = {parsedEpc.Description}");
                Console.WriteLine($"  * ID = 0x{parsedEpc.Id}");
                foreach (var data in parsedEpc.Data)
                    Console.WriteLine($"    - [{data.Magnitude}] : {data.Value} {data.UnitLong}");
            }
        }
    }
}