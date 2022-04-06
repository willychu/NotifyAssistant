using Newtonsoft.Json;

namespace NotifyAssistant.Models.LineLogin
{
    public class LineUserProfile
    {
        [JsonProperty("userId")]
        public string UserId { get; set; }
        
        [JsonProperty("displayName")]
        public string DisplayName { get; set; }
        
        [JsonProperty("pictureUrl")]
        public string PictureUrl { get; set; }
        
        [JsonProperty("statusMessage")]
        public string StatusMessage { get; set; }
    }
}
