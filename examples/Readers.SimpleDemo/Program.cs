using Kliskatek.SenseId.Sdk.Readers.Common;
using Kliskatek.SenseId.Sdk.Readers.Rfid;

namespace Kliskatek.SenseId.Sdk.Readers.SimpleDemo
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Kliskatek SenseID.SDK.Readers simple demo");

            //var reader = (ISenseIdReader)new SenseIdOctaneReader();
            //if (!reader.Connect("192.168.17.246"))
            //    return;
            //var reader = (ISenseIdReader)new SenseIdNurApiReader();
            //if (!reader.Connect("ser://com9"))
            //    return;
            var reader = (ISenseIdReader)new SenseIdRedRcpReader();
            if (!reader.Connect("COM4"))
                return;

            reader.StartDataAcquisitionAsync(DelegateMethod);

            Thread.Sleep(4000);

            reader.StopDataAcquisitionAsync();

            reader.Disconnect();
        }

        public static void DelegateMethod(string epc)
        {
            Console.WriteLine($"New EPC received : {epc}");
        }
    }
}