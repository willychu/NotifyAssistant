using Newtonsoft.Json;

namespace NotifyAssistant.Models.LineNotify
{
    public class NotifyRevokeUrlResponse
    {
        [JsonProperty("status")]
        public int Status { get; set; }
        
        [JsonProperty("message")]
        public string Message { get; set; }
    }
}
