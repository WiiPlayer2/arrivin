using Arrivin.Client.Application;
using LanguageExt.Effects.Traits;
using Microsoft.Extensions.DependencyInjection;

namespace Arrivin.Client.World;

public static class DI
{
    public static void AddWorldServices<RT>(this IServiceCollection services) where RT : struct, HasCancel<RT>
    {
        services.AddSingleton<IFileSystem<RT>, FileSystem<RT>>();
    }
}
