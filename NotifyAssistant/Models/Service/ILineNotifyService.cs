namespace NotifyAssistant.Models.Service;

public interface ILineNotifyService
{
    string GetAuthorizeEndpoint(string state);
    Task<HttpResponseMessage> GetTokenAsync(string code);
    Task<HttpResponseMessage> GetStatusAsync(string accessToken);
    Task<HttpResponseMessage> NotifyAsync(string accessToken, string message, bool silent);
    Task<HttpResponseMessage> RevokeTokenAsync(string accessToken);
    Task<T> ParseJsonResponseAsync<T>(HttpResponseMessage responseMessage);
}