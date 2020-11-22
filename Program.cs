using AmazonStockTracker.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading;

namespace AmazonStockTracker
{
    internal class Program
    {
        private static void Main()
        {
            StockConfig config;
            List<AmazonProduct> products;
            //Load config.json and products.json
            try
            {
                var jsonString = File.ReadAllText("config.json");
                config = JsonSerializer.Deserialize<StockConfig>(jsonString);

                jsonString = File.ReadAllText("products.json");
                products = JsonSerializer.Deserialize<List<AmazonProduct>>(jsonString);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading json files. {ex.Message}");
                config = null;
                products = null;
            }

            if (products != null && config != null)
            {
                var controller = new StockController(products, config);

                var timer = new Timer(controller.TimerCallback, null, 1000, config.CheckPeriodMs);
                Console.ReadLine();
            }
            else
            {
                if (products == null)
                    Console.WriteLine("Error loading products.");
                else if (config == null)
                    Console.WriteLine("Error loading configuration");
                else
                    Console.WriteLine("Error loading products and configuration");
            }
        }
    }
}