using Microsoft.AspNetCore.Identity.UI.Services;

namespace FieldLog.Services;

public class NoOpEmailSender : IEmailSender
{
    public Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        // Temporary no-op implementation for development / MVP
        return Task.CompletedTask;
    }
}
