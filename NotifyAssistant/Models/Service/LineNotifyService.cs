using Microsoft.AspNetCore.Http.Extensions;
using Newtonsoft.Json;

namespace NotifyAssistant.Models.Service
{
    public class LineNotifyService : ILineNotifyService
    {
        private readonly IConfiguration _config;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IHttpClientFactory _httpClientFactory;

        public LineNotifyService(
            IConfiguration config,
            IHttpContextAccessor httpContextAccessor,
            IHttpClientFactory httpClientFactory)
        {
            _config = config;
            _httpContextAccessor = httpContextAccessor;
            _httpClientFactory = httpClientFactory;
        }

        public string GetAuthorizeEndpoint(string state)
        {
            var builder = new QueryBuilder
            {
                { "response_type", "code" },
                { "client_id", _config["LineNotify:ClientId"] },
                { "redirect_uri", $"{GetHost()}{_config["LineNotify:RedirectUri"]}" },
                { "scope", _config["LineNotify:Scope"] },
                { "state", state },
                // { "response_mode", "form_post" }
            };
            
            return $"{_config["LineNotify:AuthUrl"]}{builder.ToQueryString().Value}";
        }

        public async Task<HttpResponseMessage> GetTokenAsync(string code)
        {
            var content = new FormUrlEncodedContent(new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("grant_type", "authorization_code"),
                new KeyValuePair<string, string>("code", code),
                new KeyValuePair<string, string>("redirect_uri", $"{GetHost()}{_config["LineNotify:RedirectUri"]}"),
                new KeyValuePair<string, string>("client_id", _config["LineNotify:ClientId"]),
                new KeyValuePair<string, string>("client_secret", _config["LineNotify:ClientSecret"])
            });
            
            var httpClient = _httpClientFactory.CreateClient();
            return await httpClient.PostAsync(_config["LineNotify:TokenUrl"], content);
        }

        public async Task<HttpResponseMessage> GetStatusAsync(string accessToken)
        {
            var httpClient = _httpClientFactory.CreateClient();
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");
            return await httpClient.GetAsync(_config["LineNotify:StatusUrl"]);
        }

        public async Task<HttpResponseMessage> NotifyAsync(string accessToken, string message, bool silent)
        {
            var list = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("message", message),
                new KeyValuePair<string, string>("notificationDisabled", silent.ToString())
            };
            var content = new FormUrlEncodedContent(list);
            
            var httpClient = _httpClientFactory.CreateClient();
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");
            return await httpClient.PostAsync(_config["LineNotify:NotifyUrl"], content);
        }

        public async Task<HttpResponseMessage> RevokeTokenAsync(string accessToken)
        {
            var httpClient = _httpClientFactory.CreateClient();
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");
            return await httpClient.PostAsync(_config["LineNotify:RevokeUrl"], null);
        }

        public async Task<T> ParseJsonResponseAsync<T>(HttpResponseMessage responseMessage)
        {
            var tokenJson = await responseMessage.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(tokenJson);
        }
        private string GetHost()
        {
            return $"{_httpContextAccessor.HttpContext.Request.Scheme}://{_httpContextAccessor.HttpContext.Request.Host.Value}";
        }
    }
}
