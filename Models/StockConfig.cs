namespace AmazonStockTracker.Models
{
    public class StockConfig
    {
        public StockConfig(int checkPeriodMs, string amazonDomain, string xPath, string telegramBotToken, string telegramChannelId)
        {
            CheckPeriodMs = checkPeriodMs;
            AmazonDomain = amazonDomain;
            XPath = xPath;
            TelegramBotToken = telegramBotToken;
            TelegramChannelId = telegramChannelId;
        }

        public int CheckPeriodMs { get; }
        public string AmazonDomain { get; }
        public string XPath { get; }
        public string TelegramBotToken { get; }
        public string TelegramChannelId { get; }
    }
}