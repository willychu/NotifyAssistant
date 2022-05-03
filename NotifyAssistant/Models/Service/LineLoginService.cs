using JWT.Algorithms;
using JWT.Builder;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using NotifyAssistant.Models.Config;
using NotifyAssistant.Models.LineLogin;

namespace NotifyAssistant.Models.Service
{
    public class LineLoginService : ILineLoginService
    {
        private readonly IOptions<LineLoginConfig> _options;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IHttpClientFactory _httpClientFactory;

        public LineLoginService(
            IOptions<LineLoginConfig> options,
            IHttpContextAccessor httpContextAccessor,
            IHttpClientFactory httpClientFactory)
        {
            _options = options;
            _httpContextAccessor = httpContextAccessor;
            _httpClientFactory = httpClientFactory;
        }
        
        public string GetAuthorizeEndpoint(string state)
        {
            var builder = new QueryBuilder
            {
                { "response_type", "code" },
                { "client_id", _options.Value.ClientId },
                { "redirect_uri", $"{GetHost()}{_options.Value.RedirectUri}" },
                { "scope", _options.Value.Scope },
                { "state", state }
            };
            
            return $"{_options.Value.AuthUrl}{builder.ToQueryString().Value}";
        }

        public async Task<HttpResponseMessage> GetTokenAsync(string code)
        {
            var content = new FormUrlEncodedContent(new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("grant_type", "authorization_code"),
                new KeyValuePair<string, string>("code", code),
                new KeyValuePair<string, string>("redirect_uri", $"{GetHost()}{_options.Value.RedirectUri}"),
                new KeyValuePair<string, string>("client_id", _options.Value.ClientId),
                new KeyValuePair<string, string>("client_secret", _options.Value.ClientSecret)
            });
            
            var httpClient = _httpClientFactory.CreateClient();
            return await httpClient.PostAsync(_options.Value.TokenUrl, content);
        }

        public async Task<HttpResponseMessage> GetProfileAsync(string accessToken)
        {
            var httpClient = _httpClientFactory.CreateClient();
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");
            return await httpClient.PostAsync(_options.Value.ProfileUrl, null);
        }

        public async Task<HttpResponseMessage> GetVerifyAsync(string accessToken)
        {
            var builder = new QueryBuilder
            {
                { "access_token", accessToken }
            };
            
            var httpClient = _httpClientFactory.CreateClient();
            return await httpClient.GetAsync($"{_options.Value.VerifyUrl}{builder.ToQueryString().Value}");
        }

        public async Task<HttpResponseMessage> RevokeTokenAsync(string accessToken)
        {
            var content = new FormUrlEncodedContent(new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("access_token", accessToken),
                new KeyValuePair<string, string>("client_id", _options.Value.ClientId),
                new KeyValuePair<string, string>("client_secret", _options.Value.ClientSecret)
            });
            
            var httpClient = _httpClientFactory.CreateClient();
            return await httpClient.PostAsync(_options.Value.RevokeUrl, content);
        }

        public async Task<T> ParseJsonResponseAsync<T>(HttpResponseMessage responseMessage)
        {
            var tokenJson = await responseMessage.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(tokenJson);
        }

        public LineIdTokenPayload DecodeIdToken(string idToken)
        {
            var payloadJson = JwtBuilder.Create()
                .WithAlgorithm(new HMACSHA256Algorithm())
                .WithSecret(_options.Value.ClientSecret)
                .MustVerifySignature()
                .Decode(idToken);
            
            return JsonConvert.DeserializeObject<LineIdTokenPayload>(payloadJson);
        }

        private string GetHost()
        {
            return $"{_httpContextAccessor.HttpContext.Request.Scheme}://{_httpContextAccessor.HttpContext.Request.Host.Value}";
        }
    }
}
