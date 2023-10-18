using System.Drawing;
using Nanoleaf.Client;
using Nanoleaf.Client.Models.Responses;
using System.Configuration;

namespace NanoleafXmas
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            try
            {
                // Get configurations
                var config = LoadConfiguration();

                // Connect to nanoleaf panels and get panel information
                var client = new NanoleafClient(config.Host, config.Token);
                var layout = await client.GetLayoutAsync();
                Console.WriteLine($"Connected to Nanoleaf device at {config.Host}. It has {layout.NumPanels} panels.");

                // Calculate amount of panels that should be red
                var redPanelCount = CalculateHowManyPanelsShouldBeRed(layout.NumPanels, int.Parse(config.DayWhenAllPanelsRed), int.Parse(config.MonthWhenAllPanelsRed));
                var whitePanelCount = layout.NumPanels - redPanelCount;
                var formatedPanels = FormUpdatedPanels(layout, redPanelCount);

                // Update the panels
                NanoleafStreamingClient nanoStream = new NanoleafStreamingClient(client.HostName);
                await nanoStream.SetColorAsync(formatedPanels, 100);
                Console.WriteLine($"Nanoleaf device updated. {redPanelCount} panels set to red and {layout.NumPanels - redPanelCount} set to white.");
            }

            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        // This function calculates how many panels should be red based on the date
        static int CalculateHowManyPanelsShouldBeRed(int totalAmountOfPanels, int dayWhenAllPanelsRed, int monthWhenAllPanelsRed)
        {
            var today = DateTime.Today;
            int panelsToPaintRed = 0;

            if (today.Month == monthWhenAllPanelsRed)
            {
                double panelInDays = (double)totalAmountOfPanels / (double)dayWhenAllPanelsRed;
                int dayToday = today.Day;
                panelsToPaintRed = (int)(dayToday * panelInDays);
            }

            return panelsToPaintRed;
        }

        // This function loads the configuration from the app.config xml
        static NanoleafConfiguration LoadConfiguration()
        {
            return new NanoleafConfiguration
            {
                Host = ConfigurationManager.AppSettings.Get("host") ?? "UNKNOWN",
                Token = ConfigurationManager.AppSettings.Get("token") ?? "UNKNOWN",
                DayWhenAllPanelsRed = ConfigurationManager.AppSettings.Get("DayWhenAllPanelsRed") ?? "24",
                MonthWhenAllPanelsRed = ConfigurationManager.AppSettings.Get("MonthWhenAllPanelsRed") ?? "12"
            };
        }

        // This function forms the updated panels
        static Dictionary<int, Color> FormUpdatedPanels(Layout layout, int redPanelCount)
        {
            Color redColor = Color.FromArgb(255, 0, 0);
            Color whiteColor = Color.FromArgb(255, 255, 255);

            // Set random panels white and red right amount
            var random = new Random();
            var formedPanels = new Dictionary<int, Color>();
            var shuffledPositions = layout.PositionData.OrderBy(p => random.Next()).ToList();

            for (int i = 0; i < redPanelCount; i++)
            {
                formedPanels[shuffledPositions[i].PanelId] = redColor;
            }

            foreach (var position in layout.PositionData)
            {
                if (!formedPanels.ContainsKey(position.PanelId))
                {
                    formedPanels[position.PanelId] = whiteColor;
                }
            }

            return formedPanels;
        }
    }
}