using System.Text.RegularExpressions;
using VoxCore.Runtime.Contracts;

namespace VoxCore.Infrastructure.Services.Normalizers;

public class Time : ITextNormalaizer
{
    static readonly Dictionary<string, string> Words = new(StringComparer.OrdinalIgnoreCase)
    {
        ["утром"] = "08:00",
        ["полдень"] = "12:00",
        ["днём"] = "14:00",
        ["вечером"] = "19:00",
        ["полночь"] = "00:00",
        ["ночью"] = "02:00",
    };

    public Task<string> NormalizeAsync(string text, CancellationToken ct)
    {
        // 1️⃣ ищем "в <число>[:<мин>] [утро/утром/вечер/вечером/вечера/ночь/ночью]"
        text = Regex.Replace(text,
            @"\bв\s+(\d{1,2})(?::(\d{2}))?\s*(утр(ом|а)?|вечер(ом|а)?|ноч(ью|и)?)?\b",
            m =>
            {
                int hour = int.Parse(m.Groups[1].Value);
                int min = m.Groups[2].Success ? int.Parse(m.Groups[2].Value) : 0;
                string period = m.Groups[3].Success ? m.Groups[3].Value.ToLower() : "";

                if (!string.IsNullOrEmpty(period))
                {
                    // корректировка по слову
                    if (period.StartsWith("веч")) hour += 12;   // вечер 17-23
                    if (period.StartsWith("ноч")) hour = hour % 12; // ночь 00-04
                    if (period.StartsWith("утр") && hour >= 12) hour -= 12;
                }
                else
                {
                    // по умолчанию считаем дневным рабочим временем
                    if (hour >= 1 && hour <= 12)
                        hour += 12;  // 5 → 17:00
                }

                return $"{hour:D2}:{min:D2}";
            },
            RegexOptions.IgnoreCase);

        // 2️⃣ отдельно заменяем одиночные слова
        foreach (var kv in Words)
        {
            text = Regex.Replace(
                text,
                $@"(?<!\bс\s)\b{Regex.Escape(kv.Key)}\b",
                kv.Value,
                RegexOptions.IgnoreCase);
        }

        return Task.FromResult(text);
    }
}
