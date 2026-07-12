using Microsoft.Extensions.DependencyInjection;

namespace BookBase.Helpers;

public static class ServiceHelper
{
    public static T GetService<T>() where T : notnull
    {
        return IPlatformApplication.Current!.Services.GetRequiredService<T>();
    }
}
