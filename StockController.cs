using AmazonStockTracker.Models;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Telegram.Bot;

namespace AmazonStockTracker
{
    public class StockController
    {
        public StockController(List<AmazonProduct> products, StockConfig config)
        {
            Products = products;
            Config = config;
        }

        public StockConfig Config { get; }
        public List<AmazonProduct> Products { get; }

        private static async Task<string> GetString(Uri url, string xPath)
        {
            HttpClient client = new HttpClient();
            var response = await client.GetAsync(url);
            var pageContents = await response.Content.ReadAsStringAsync();

            HtmlDocument pageDocument = new HtmlDocument();
            pageDocument.LoadHtml(pageContents);

            var text = pageDocument.DocumentNode.SelectSingleNode(xPath).InnerText;

            return text.Trim();
        }

        public void TimerCallback(object state)
        {
            CheckStockAsync();
        }

        private static (bool,string) CheckAvailability(string availability)
        {
            //TODO Check Domain
            if (availability.Contains("immediata") || availability.Contains("presso") || availability.Contains("solo"))
                return (true,availability);
            else
                return (false,availability);
        }

        private async void CheckProductStockAsync(AmazonProduct product)
        {
            var availabilityString = await GetString(product.Url, Config.XPath);

            var isAvailable = CheckAvailability(availabilityString);

            if (isAvailable.Item1)
            {
                Console.WriteLine($"{DateTime.Now.TimeOfDay} - [Amazon.{Config.AmazonDomain} {product.Description}] {isAvailable.Item2}");
                SendTelegramMessage($"[Amazon.{Config.AmazonDomain} {product.Description}] {isAvailable.Item2} {Environment.NewLine}{product.Url}");
            }
           else
            {
                Console.WriteLine($"{DateTime.Now.TimeOfDay} - [Amazon.{Config.AmazonDomain} {product.Description}] {isAvailable.Item2}");
            }     
        }

        private void CheckStockAsync()
        {
            foreach (var product in Products)
                CheckProductStockAsync(product);

            Console.Clear();
        }

        private void SendTelegramMessage(string message)
        {
            if (!string.IsNullOrEmpty(Config.TelegramBotToken) && !string.IsNullOrEmpty(Config.TelegramChannelId))
            {
                try
                {
                    var botClient = new TelegramBotClient(Config.TelegramBotToken);
                    botClient.SendTextMessageAsync(Config.TelegramChannelId, message);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error sending telegram notification. {ex.Message}");
                }
            }
        }
    }
}