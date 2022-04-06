namespace NotifyAssistant.Models.Entity
{
    public class User
    {
        public int UserId { get; set; }

        public string Nickname { get; set; }

        public string AvatarUrl { get; set; }

        public string Role { get; set; }

        public string LineUserId { get; set; }

        public string LineLoginAccessToken { get; set; }

        public string LineLoginIdToken { get; set; }

        public string LineNotifyAccessToken { get; set; }
    }
}