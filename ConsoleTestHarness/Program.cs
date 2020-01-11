using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using HueLib;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ConsoleTestHarness
{
    public class Program
    {

        static void Main(string[] args)
        { 
            JObject localStoreSettings;
            var bridgeIp = string.Empty;
            var bridgeUsername = string.Empty;

            // This is a suggested example of how to implement the HueLib Library.
            // Suggestions are welcome!
            try
            {
                using (var localStoreFile = new FileStream($"{Directory.GetCurrentDirectory()}/localSettings.json",
                    FileMode.OpenOrCreate, FileAccess.ReadWrite))
                {
                    using (var sr = new StreamReader(localStoreFile))
                    {
                        localStoreSettings = JObject.Parse(sr.ReadToEnd());
                    }
                }
            }
            catch (Exception e)
            {
                localStoreSettings = new JObject();
            }

            // Search for all Bridge IP addresses on the local network.
            // This value should be stored for later usage in some way.
            bridgeIp = GetOrCreateLocalSetting(localStoreSettings, "HueBridgeIp",
                async () => await GetFirstValidBridgeIp()).Result;

            // Prompt the user to tap the button on top of the hue bridge.
            // Once tapped this will return a username string.
            // This value should be stored to stop extraneous usernames being created.
            bridgeUsername = GetOrCreateLocalSetting(localStoreSettings, "HueBridgeUsername",
                async () => await GetHueBridgeUsername(bridgeIp)).Result;

            while (string.IsNullOrWhiteSpace(bridgeUsername))
            {
                Console.WriteLine("Please tap the button on top of the Hue bridge to authenticate this application");

                Task.Delay(TimeSpan.FromSeconds(10)).Wait();

                bridgeUsername = GetOrCreateLocalSetting(localStoreSettings, "HueBridgeUsername",
                    async () => await GetHueBridgeUsername(bridgeIp)).Result;
            }

            var client = new HueClient(bridgeIp, bridgeUsername);

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

        private static async Task<string> GetOrCreateLocalSetting(JObject localSettings, string key, Func<Task<string>> retrieveValueFunc)
        {
            try
            {
                if (!localSettings.ContainsKey(key))
                {
                    using (var localStoreFile = new FileStream($"{Directory.GetCurrentDirectory()}/localSettings.json",
                        FileMode.OpenOrCreate, FileAccess.ReadWrite))
                    {
                        using (var sr = new StreamWriter(localStoreFile))
                        {
                            var retrievedValue = await retrieveValueFunc.Invoke();

                            localSettings.Add(key, retrievedValue);

                            sr.Write(localSettings.ToString());

                            return retrievedValue;
                        }
                    }
                }
                else
                {
                    return (string) localSettings[key];
                }
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        private static async Task<string> GetFirstValidBridgeIp()
        {
            var ips = LocateHueBridgeIp.GetBridgeIpAddresses();

            return ips.First();
        }

        private static async Task<string> GetHueBridgeUsername(string ip)
        {
            var getUsernameHueClient = new GetUsernameHueClient(ip);
            var username = await getUsernameHueClient.GetUsername(
                "ExampleApplication",
                "ExampleDeviceName"
                );

            return username;
        }
    }
}
