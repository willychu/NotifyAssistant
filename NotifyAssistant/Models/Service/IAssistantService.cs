using NotifyAssistant.Models.Entity;

namespace NotifyAssistant.Models.Service;

public interface IAssistantService
{
    bool IsAuthenticated();
    int? GetUserId();
    Task<bool> SignInAsync(User user);
    Task<bool> SignOutAsync();
}