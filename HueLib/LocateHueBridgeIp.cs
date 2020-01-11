using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using HueLib.ResponseModels;
using Newtonsoft.Json;

namespace HueLib
{
    public static class LocateHueBridgeIp
    {
        public static List<string> GetBridgeIpAddresses()
        {
            // Attempt to check meethue.com first as this will be a much faster check.
            var discoveryCheck = DiscoveryMeetHueCheck();

            if (discoveryCheck.Any())
                return discoveryCheck;

            // Check failed, use IPScan instead
            return HueBridgeIps();
        }

        private static List<string> DiscoveryMeetHueCheck()
        {
            var client = new HttpClient();

            var response = client.GetAsync("https://discovery.meethue.com");
            response.Wait();

            var jsonData = response.Result.Content.ReadAsStringAsync();
            jsonData.Wait();

            var result = JsonConvert.DeserializeObject<List<DiscoveryMeetHueResponse>>(jsonData.Result);

            return result == null ? new List<string>() : result.Select(r => r.internalipaddress).ToList();
        }

        private static List<string> HueBridgeIps()
        {
            var localIp = GetLocalIpAddress();
            var ipComponents = localIp.Split('.');
            var ipBase = $"{ipComponents[0]}.{ipComponents[1]}.{ipComponents[2]}";

            var xmlResponseRegex = new Regex(@"Philips hue bridge", RegexOptions.IgnoreCase);

            var client = new HttpClient();

            var hueBridgeIps = new List<string>();

            Task[] tasks = Enumerable.Range(1, 254).Select(deviceNumber =>
                Task.Factory.StartNew(async () =>
                {
                    var ping = new Ping();
                    var ipAddress = $"{ipBase}.{deviceNumber}";
                    var response = ping.Send(ipAddress, 100, Encoding.ASCII.GetBytes(ipAddress));

                    if (response == null || response.Status != IPStatus.Success) return;
                    try
                    {
                        var ip = Encoding.ASCII.GetString(response.Buffer);

                        using (var clientResponse = await client.GetAsync($"http://{ip}/description.xml",
                            new CancellationTokenSource(TimeSpan.FromSeconds(1)).Token))
                        {
                            if (!clientResponse.IsSuccessStatusCode) return;

                            var ipResponse = await clientResponse.Content.ReadAsStringAsync();
                            if (xmlResponseRegex.IsMatch(ipResponse))
                            {
                                Console.WriteLine($"Hue Bridge found! Ip: {ip}");
                                hueBridgeIps.Add(ip);
                            }
                        }
                    }
                    catch
                    {
                    }
                })).ToArray();

            Task.WaitAll(tasks);

            return hueBridgeIps;
        }

        private static string GetLocalIpAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                    return ip.ToString();
            }

            throw new NetworkInformationException();
        }
    }
}