using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NotifyAssistant.Models.Config
{
    public class LineLoginConfig
    {
        public string ClientId { get; set; }

        public string ClientSecret { get; set; }

        public string Scope { get; set; }

        public string RedirectUri { get; set; }

        public string AuthUrl { get; set; }

        public string TokenUrl { get; set; }

        public string RevokeUrl { get; set; }

        public string ProfileUrl { get; set; }

        public string VerifyUrl { get; set; }
    }
}