using Newtonsoft.Json;

namespace NotifyAssistant.Models.LineNotify
{
    public class NotifyStatusUrlResponse
    {
        [JsonProperty("status")]
        public int Status { get; set; }
        
        [JsonProperty("message")]
        public string Message { get; set; }
        
        [JsonProperty("targetType")]
        public string TargetType { get; set; }
        
        [JsonProperty("target")]
        public string Target { get; set; }
    }
}
