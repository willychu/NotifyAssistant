using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace NotifyAssistant.Models.LineLogin
{
    public class LineCallbackRequestViewModel
    {
        [Required]
        [JsonProperty("code")]
        public string Code { get; set; }
        
        [Required]
        [JsonProperty("state")]
        public string State { get; set; }
        
        [JsonProperty("error")]
        public string Error { get; set; }
        
        [JsonProperty("error_description")]
        public string ErrorDescription { get; set; }
    }
}
