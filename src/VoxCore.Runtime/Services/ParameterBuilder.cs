using System.Reflection;
using Microsoft.Extensions.Logging;
using VoxCore.Plugins.Contracts;

namespace VoxCore.Runtime.Services;

public sealed class ParameterBuilder(
    ILogger<ParameterBuilder> logger
)
{
    internal async Task<object?> TryBuild(Type parametersType, IDictionary<string, object> values)
    {
        logger.LogDebug($"Type for build params: {parametersType}");
        if (parametersType == typeof(NoParameters))
            return new NoParameters();

        var param = Activator.CreateInstance(parametersType);

        if (param == null)
            return null;

        foreach (var p in parametersType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (!p.CanWrite) continue;
            if (!values.TryGetValue(p.Name, out var val)) continue;
            if (val is null && p.PropertyType.IsValueType) continue;

            if (p.PropertyType.IsInstanceOfType(val))
                p.SetValue(param, val);
            else if (val is IConvertible)
                p.SetValue(param, Convert.ChangeType(val, p.PropertyType));
        }

        logger.LogDebug("Build typed instance params: {@param}", param);
        return param;
    }
}