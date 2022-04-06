using System.ComponentModel.DataAnnotations;

namespace NotifyAssistant.Models.LineNotify
{
    public class NotifyContentViewModel
    {
        [Required]
        [MaxLength(1000)]
        public string NotifyContent { get; set; }
        
        public bool NotifySilent { get; set; }
    }
}
