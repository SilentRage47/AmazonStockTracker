using System;

namespace AmazonStockTracker.Models
{
    public class AmazonProduct
    {
        public AmazonProduct(string description, Uri url)
        {
            Description = description;
            Url = url;
        }

        public string Description { get; set; }
        public Uri Url { get; set; }
    }
}