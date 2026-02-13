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
            if (!p.CanWrite)
                continue;

            if (!values.TryGetValue(p.Name, out var val))
                continue;

            var targetType = p.PropertyType;
            var underlying = Nullable.GetUnderlyingType(targetType);
            var actualType = underlying ?? targetType;

            // null значение
            if (val == null || val.ToString() == "null")
            {
                if (!targetType.IsValueType || underlying != null)
                    p.SetValue(param, null);

                continue;
            }

            // если уже нужный тип
            if (actualType.IsInstanceOfType(val))
            {
                p.SetValue(param, val);
                continue;
            }

            try
            {
                object converted;

                if (actualType.IsEnum)
                {
                    converted = Enum.Parse(actualType, val.ToString(), true);
                }
                else if (actualType == typeof(Guid))
                {
                    converted = Guid.Parse(val.ToString());
                }
                else if (actualType == typeof(DateTime))
                {
                    converted = DateTime.Parse(val.ToString());
                }
                else if (actualType == typeof(bool))
                {
                    var s = val.ToString().ToLower();
                    converted = s switch
                    {
                        "1" => true,
                        "0" => false,
                        "yes" => true,
                        "no" => false,
                        _ => Convert.ChangeType(val, actualType)
                    };
                }
                else
                {
                    converted = Convert.ChangeType(val, actualType);
                }

                p.SetValue(param, converted);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error on build typed params: {@param}", param);
                continue;
            }
        }


        logger.LogDebug("Build typed instance params: {@param}", param);
        return param;
    }
}