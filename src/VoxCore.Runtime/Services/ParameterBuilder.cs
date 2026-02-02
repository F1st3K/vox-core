using System.Reflection;
using VoxCore.Plugins.Contracts;

namespace VoxCore.Runtime.Services;

public sealed class ParameterBuilder
{
    internal async Task<object?> TryBuild(Type parametersType, IDictionary<string, object> values)
    {
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

        return param;
    }
}