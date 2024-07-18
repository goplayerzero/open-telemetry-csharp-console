using System;
using System.Threading;
using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace OtelPlayerzeroConsoleApp
{
    public class UserService
    {
        private readonly ILogger<UserService> _logger;

        public UserService(ILogger<UserService> logger)
        {
            _logger = logger;
        }

        public bool Authenticate(string username, string password)
        {
            using (var activity = Activity.Current)
            {
                if (activity != null)
                {
                    // Simulate authentication logic
                    if (username == "foo" && password == "bar")
                    {
                        // Set TraceState
                        activity.TraceStateString = "userid=5678";

                        _logger.LogInformation($"User '{username}' logged in successfully.");
                        return true;
                    }
                    else
                    {
                        _logger.LogError($"Failed login attempt for user '{username}'.");
                        return false;
                    }
                }
                else
                {
                    // Activity is null, handle if needed
                    _logger.LogWarning("Failed to start UserAuthentication activity.");
                    return false;
                }
            }
        }
    }
}
