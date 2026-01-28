using Microsoft.Extensions.Logging;

namespace VoxCore.Runtime.Services;

public sealed class ParameterRefiner(
    ILogger<ParameterRefiner> logger
)
{
    internal async Task<bool> TryRefining(Type parametersType, object parameters, CurrentDialog dialog)
    {
        foreach (var prop in parametersType.GetProperties())
        {
            bool isRequired = !(
                Nullable.GetUnderlyingType(prop.PropertyType) != null ||
                !prop.PropertyType.IsValueType
            );
            if (isRequired)
            {
                try
                {
                    var value = prop.GetValue(parameters);
                    if (value == null)
                        value = await dialog.AskAsync($"Уточните свойство {prop.Name}");

                    prop.SetValue(parameters, value);
                    
                }
                catch (Exception ex)
                {
                    logger.LogCritical(ex, "Failed refining parameters");

                    return false;
                }
            }
        }

        return true;
    }
}