namespace NoxiousWeedsWebApp
{
    public class AppSettings
    {
        public string PredictionEndpoint { get; set; }
        public string PredictionEndpointApiKey { get; set; }
        public double ProbabilityThreshold { get; set; }
        public string StorageAccountConnectionString { get; set; }
    }
}