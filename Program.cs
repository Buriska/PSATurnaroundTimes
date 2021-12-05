using Discord;
using Discord.Webhook;
using PuppeteerSharp;
using System;
using System.IO;
using System.Threading.Tasks;

namespace TurnaroundTimes
{
    class Program
    {
        static string psaPageUrl = "https://www.psacard.com/pricing";
        static string discordWebhookUrl = "";
        static string screenshotDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "screenshots");

        static async Task Main(string[] args)
        {
            await TakeScreenshot();
        }

        private static async Task TakeScreenshot()
        {
            try
            {
                Console.WriteLine("Launching browser...");
                await new BrowserFetcher().DownloadAsync();

                var browser = await Puppeteer.LaunchAsync(new LaunchOptions
                {
                    DefaultViewport = { Height = 1920, Width = 1080 },
                    Headless = true,
                    IgnoreHTTPSErrors = true
                });

                using (var page = await browser.NewPageAsync())
                {
                    Console.WriteLine("Opening page...");
                    await page.GoToAsync(psaPageUrl);

                    var tableElement = await page.WaitForSelectorAsync("table.table");
                    Console.WriteLine("Element found!");

                    string filename = String.Format(@"{0}_{1}.png", "PSA_TURNAROUND", DateTime.Now.ToString("yyyyMMdd_hhmmss"));

                    Console.WriteLine("Taking screenshot...");
                    Directory.CreateDirectory(screenshotDir);
                    string fullPath = Path.Combine(screenshotDir, filename);

                    await tableElement.ScreenshotAsync(fullPath);

                    Console.WriteLine("Sending Discord message...");
                    await SendDiscordMessage(fullPath);

                    Console.WriteLine("Done!");
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }
        public static async Task SendDiscordMessage(string imagePath)
        {
            using (var client = new DiscordWebhookClient(discordWebhookUrl))
            {
                var embed = new EmbedBuilder();
                embed
                    .WithFooter(DateTime.Now.ToString())
                    .WithColor(Color.Gold)
                    .WithTitle("PSA Complete Through Dates")
                    .WithUrl(psaPageUrl)
                    .WithImageUrl($"attachment://{Path.GetFileName(imagePath)}");

                await client.SendFileAsync(imagePath, "", embeds: new[] { embed.Build() });

                File.Delete(imagePath);
            }
        }
    }
}
