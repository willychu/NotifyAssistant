using Newtonsoft.Json;

namespace NotifyAssistant.Models.LineLogin
{
    public class LineIdTokenPayload
    {
        [JsonProperty("iss")]
        public string Iss { get; set; }
        
        [JsonProperty("sub")]
        public string Sub { get; set; }
        
        [JsonProperty("aud")]
        public string Aud { get; set; }
        
        [JsonProperty("exp")]
        public string Exp { get; set; }
        
        [JsonProperty("iat")]
        public string Iat { get; set; }
        
        [JsonProperty("auth_time")]
        public string AuthTime { get; set; }
        
        [JsonProperty("nonce")]
        public string Nonce { get; set; }
        
        [JsonProperty("amr")]
        public List<string> Amr { get; set; }
        
        [JsonProperty("name")]
        public string Name { get; set; }
        
        [JsonProperty("picture")]
        public string Picture { get; set; }
        
        [JsonProperty("email")]
        public string Email { get; set; }
    }
}
