using JWT.Algorithms;
using JWT.Builder;
using Microsoft.AspNetCore.Http.Extensions;
using Newtonsoft.Json;
using NotifyAssistant.Models.LineLogin;

namespace NotifyAssistant.Models.Service
{
    public class LineLoginService : ILineLoginService
    {
        private readonly IConfiguration _config;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IHttpClientFactory _httpClientFactory;

        public LineLoginService(
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
                { "client_id", _config["LineLogin:ClientId"] },
                { "redirect_uri", $"{GetHost()}{_config["LineLogin:RedirectUri"]}" },
                { "scope", _config["LineLogin:Scope"] },
                { "state", state }
            };
            
            return $"{_config["LineLogin:AuthUrl"]}{builder.ToQueryString().Value}";
        }

        public async Task<HttpResponseMessage> GetTokenAsync(string code)
        {
            var content = new FormUrlEncodedContent(new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("grant_type", "authorization_code"),
                new KeyValuePair<string, string>("code", code),
                new KeyValuePair<string, string>("redirect_uri", $"{GetHost()}{_config["LineLogin:RedirectUri"]}"),
                new KeyValuePair<string, string>("client_id", _config["LineLogin:ClientId"]),
                new KeyValuePair<string, string>("client_secret", _config["LineLogin:ClientSecret"])
            });
            
            var httpClient = _httpClientFactory.CreateClient();
            return await httpClient.PostAsync(_config["LineLogin:TokenUrl"], content);
        }

        public async Task<HttpResponseMessage> GetProfileAsync(string accessToken)
        {
            var httpClient = _httpClientFactory.CreateClient();
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");
            return await httpClient.PostAsync(_config["LineLogin:ProfileUrl"], null);
        }

        public async Task<HttpResponseMessage> GetVerifyAsync(string accessToken)
        {
            var builder = new QueryBuilder
            {
                { "access_token", accessToken }
            };
            
            var httpClient = _httpClientFactory.CreateClient();
            return await httpClient.GetAsync($"{_config["LineLogin:VerifyUrl"]}{builder.ToQueryString().Value}");
        }

        public async Task<HttpResponseMessage> RevokeTokenAsync(string accessToken)
        {
            var content = new FormUrlEncodedContent(new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("access_token", accessToken),
                new KeyValuePair<string, string>("client_id", _config["LineLogin:ClientId"]),
                new KeyValuePair<string, string>("client_secret", _config["LineLogin:ClientSecret"])
            });
            
            var httpClient = _httpClientFactory.CreateClient();
            return await httpClient.PostAsync(_config["LineLogin:RevokeUrl"], content);
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
                .WithSecret(_config["LineLogin:ClientSecret"])
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
