// See https://aka.ms/new-console-template for more information

using Kliskatek.SenseId.Sdk.Parsers.Common;
using Kliskatek.SenseId.Sdk.Parsers.Rain;

Console.WriteLine("Kliskatek SenseId.SDK.Parsers");

List<Tuple<SenseIdTag, string>> resultList = [];
var rainEpcByteArray = new byte[] { 0x00, 0x00, 0x00, 0xF1, 0xD3, 0x01, 0x01, 0x00, 0x00, 0x01, 0x23, 0x01 };
var rainEpcHexString = SharedLogic.ByteArrayToHexString(rainEpcByteArray);

// Data conversion methods for EPCs in byte array format
resultList.Add(new Tuple<SenseIdTag, string>(EpcConverter.ParseEpc(rainEpcByteArray), "Rain.EpcConverter.ParseEpc(), binary data"));
resultList.Add(new Tuple<SenseIdTag, string>(new RainSenseIdTag(rainEpcByteArray), "RainSenseIdTag constructor, binary data"));
resultList.Add(new Tuple<SenseIdTag, string>(rainEpcByteArray.ToRainSenseIdTag(), "ToRainSenseIdTag() extension method of byte array type"));

// Data conversion methods for EPCs in hexadecimal string format
resultList.Add(new Tuple<SenseIdTag, string>(EpcConverter.ParseEpc(rainEpcHexString), "Rain.EpcConverter.ParseEpc(), hexadecimal string data"));
resultList.Add(new Tuple<SenseIdTag, string>(new RainSenseIdTag(rainEpcHexString), "RainSenseIdTag constructor, hexadecimal string data"));
resultList.Add(new Tuple<SenseIdTag, string>(rainEpcHexString.ToRainSenseIdTag(), "ToSenseIdTag() extension method of string type"));

// Display all parsed results
foreach (var result in resultList)
{
    Console.WriteLine("\n" + result.Item2);
    Console.WriteLine($"ID = {result.Item1.Id} [{result.Item1.Technology} : {result.Item1.Name} , {result.Item1.Description}]");
    foreach (var dataResult in result.Item1.Data)
        Console.WriteLine($"   * {dataResult.Magnitude} = {dataResult.Value} {dataResult.UnitShort}");
}