using System;
using System.Linq;
using System.Threading;
using HueLib;

namespace HueConsole
{
    public class Program
    {
        static void Main(string[] args)
        {
            var ips = LocateHueBridgeIp.GetBridgeIp();


            Console.WriteLine(string.Join(", ", ips));
            Console.WriteLine("Finished searching");

            Console.WriteLine("Requesting Username");
            
            //TODO: Access this from config
            var client = new HueClient(ips.First(), "username");

            switch (Console.ReadLine())
            {
                case "1":
                    var turnOffResult = client.TurnOffAllLights();
                    turnOffResult.Wait();
                    break;
                case "2":
                    var turnOnResult = client.TurnOnAllLights();
                    turnOnResult.Wait();
                    break;
            }

            Console.ReadLine();
        }
    }
}
