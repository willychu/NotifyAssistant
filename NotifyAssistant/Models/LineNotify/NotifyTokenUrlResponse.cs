using Newtonsoft.Json;

namespace NotifyAssistant.Models.LineNotify
{
    public class NotifyTokenUrlResponse
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }
    }
}
