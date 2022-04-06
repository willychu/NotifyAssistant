using NotifyAssistant.Models.LineLogin;

namespace NotifyAssistant.Models.Service;

public interface ILineLoginService
{
    string GetAuthorizeEndpoint(string state);
    Task<HttpResponseMessage> GetTokenAsync(string code);
    Task<HttpResponseMessage> GetProfileAsync(string accessToken);
    Task<HttpResponseMessage> GetVerifyAsync(string accessToken);
    Task<HttpResponseMessage> RevokeTokenAsync(string accessToken);
    Task<T> ParseJsonResponseAsync<T>(HttpResponseMessage responseMessage);
    LineIdTokenPayload DecodeIdToken(string idToken);
}