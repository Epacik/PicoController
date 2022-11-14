using Splat;

namespace PicoController.Core.Extensions;

public static class Splat
{
    public static T GetRequiredService<T>(this IDependencyResolver res, string? contract = null)
    {
        return res.GetService<T>(contract) ?? 
            throw new InvalidOperationException($"Service of type {typeof(T)} was not found {(contract is not null ? $"contract: {contract}" : "")}");
    }

    public static T GetRequiredService<T>(this IReadonlyDependencyResolver res, string? contract = null)
    {
        return res.GetService<T>(contract) ??
            throw new InvalidOperationException($"Service of type {typeof(T)} was not found {(contract is not null ? $"contract: {contract}" : "")}");
    }


    public static IEnumerable<T> GetRequiredServices<T>(this IDependencyResolver res, string? contract = null)
    {
        return res.GetServices<T>(contract) ??
            throw new InvalidOperationException($"Service of type {typeof(T)} was not found {(contract is not null ? $"contract: {contract}" : "")}");
    }

    public static IEnumerable<T> GetRequiredServices<T>(this IReadonlyDependencyResolver res, string? contract = null)
    {
        return res.GetServices<T>(contract) ??
            throw new InvalidOperationException($"Service of type {typeof(T)} was not found {(contract is not null ? $"contract: {contract}" : "")}");
    }
}
