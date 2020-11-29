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

        private static async Task<string> ParseStringFromUrl (Uri url, string xPath)
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.UserAgent.ParseAdd(@"Mozilla/5.0 (Macintosh; Intel Mac OS X 10_10_1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/39.0.2171.95 Safari/537.36");

            HtmlDocument pageDocument = new HtmlDocument();

            try
            {
                var response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();
                var pageContents = await response.Content.ReadAsStringAsync();
                pageDocument.LoadHtml(pageContents);
            }
            catch (Exception)
            {
                throw;
            }

            var singleNode = pageDocument.DocumentNode.SelectSingleNode(xPath);

            if (singleNode is not null)
                return singleNode.InnerText.Trim();
            else
                return string.Empty;
        }

        public void TimerCallback(object state)
        {
            CheckStockAsync();
        }

        private static (bool,string) CheckAvailability(string availability)
        {

            if (string.IsNullOrEmpty(availability))
                return (false, "Error parsing URL");

            if (availability.Contains("immediata") || availability.Contains("presso") || availability.Contains("solo"))
                return (true,availability);
            else
                return (false,availability);
        }

        private async void CheckProductStockAsync(AmazonProduct product)
        {

            string availabilityString;
            try
            {
                availabilityString = await ParseStringFromUrl(product.Url, Config.XPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{DateTime.Now:T} - [Amazon.{Config.AmazonDomain} {product.Description}] ERROR: {ex.Message}");
                return;
            }
            
            var isAvailable = CheckAvailability(availabilityString);

            if (isAvailable.Item1)
            {
                Console.WriteLine($"{DateTime.Now:T} - [Amazon.{Config.AmazonDomain} {product.Description}] {isAvailable.Item2}");
                SendTelegramMessage($"[Amazon.{Config.AmazonDomain} {product.Description}] {isAvailable.Item2} {Environment.NewLine}{product.Url}");
            }
           else
            {
                Console.WriteLine($"{DateTime.Now:T} - [Amazon.{Config.AmazonDomain} {product.Description}] {isAvailable.Item2}");
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