using PicoController.Core.Config;
using PicoController.Core.Devices;
using PicoController.Core.Extensions;
using Splat;

namespace PicoController.Core.DependencyInjection;

public static class Bootstrapper
{
    public static void Register(IMutableDependencyResolver services, IReadonlyDependencyResolver resolver)
    {
        services.RegisterLazySingleton<IFileSystem>(() => new FileSystem());
        services.RegisterLazySingleton<ILocationProvider>(() => new LocationProvider());

        services.RegisterLazySingleton<IConfigRepository>(() => new ConfigRepository(
            resolver.GetRequiredService<ILocationProvider>(),
            resolver.GetRequiredService<IFileSystem>(),
            resolver.GetService<Serilog.ILogger>()
        ));

        services.RegisterLazySingleton<IPluginManager>(() => new PluginManager(
            resolver.GetRequiredService<ILocationProvider>(),
            resolver.GetRequiredService<IFileSystem>(),
            resolver,
            resolver.GetService<Serilog.ILogger>()
        ));

        services.RegisterLazySingleton<IDeviceManager>(() => new DeviceManager(
            resolver.GetRequiredService<IPluginManager>(),
            resolver.GetRequiredService<IConfigRepository>(),
            resolver.GetRequiredService<Serilog.ILogger>()
        ));
    }
}
