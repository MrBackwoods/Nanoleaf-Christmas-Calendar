using System;
using System.Linq;
using DeviceDiscovery.Models;
using Nanoleaf.Client;
using Nanoleaf.Client.Discovery;
using System.Threading.Tasks;
using System.Drawing;
using Nanoleaf.Client.Models.Responses;
using System.Collections.Generic;

namespace NanoleafChristmasCalendar
{
    class Program
    {
        // token and hostname are needed to connect to the device
        static string token = "token";
        static string hostName = "host";

        // Main functionality for the christmas calendar
        static async Task Main(string[] args)
        {
            // Connection and streaming
            NanoleafClient client = new NanoleafClient(hostName, token);
            await client.StartExternalAsync();
            NanoleafStreamingClient nanoStream = new NanoleafStreamingClient(hostName);

            // Colors to be used
            Color redColor = Color.FromArgb(255, 0, 0);
            Color whiteColor = Color.FromArgb(255, 255, 255);

            // Get layout and create dictionary for panel colors
            Layout layout = await client.GetLayoutAsync();
            var paintedPanels = new Dictionary<int, Color>();

            // Calculate how many panels should be painted red today (fully red on christmas eve)
            double panelInDays = layout.NumPanels / 24.0f;
            int dayToday = DateTime.Now.Date.Day;
            int panelsToPaintRed = (int)(dayToday * panelInDays);
            Console.WriteLine("Today is " + dayToday + ".12. Therefore " + panelsToPaintRed + " panels are to be painted red and others to be set as white.");

            // Paint panels for the dictionary
            foreach (var position in layout.PositionData)
            {
                if (panelsToPaintRed > 0)
                {
                    paintedPanels[position.PanelId] = redColor;
                }
                else
                {
                    paintedPanels[position.PanelId] = whiteColor;
                }

                panelsToPaintRed -= 1;
            }

            // Sync to device
            await nanoStream.SetColorAsync(paintedPanels, 100);
            Console.WriteLine("Nanoleaf device updated!");
        }

        // You can use this to get IP and token for the device if you do not have that already. Remember to trigger pairing mode first on the device.
        static async Task GetTokenForFutureUse()
        {
            var nanoleafDiscovery = new NanoleafDiscovery();
            var request = new NanoleafDiscoveryRequest
            {
                ST = SearchTarget.Nanoleaf
            };
            var discoveredNanoleafs = nanoleafDiscovery.DiscoverNanoleafs(request);
            var nanoleaf = discoveredNanoleafs.FirstOrDefault();
            var newToken = await nanoleaf.CreateTokenAsync();
            
            Console.WriteLine("Token: " + newToken.Token);
            token = newToken.Token.ToString();
            Console.WriteLine("Hostname: " + nanoleaf.HostName);
            hostName = nanoleaf.HostName;

        }
    }
}
