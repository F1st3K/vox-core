using System.Text.RegularExpressions;
using VoxCore.Runtime.Contracts;

namespace VoxCore.Infrastructure.Services.Normalizers;

public class Period : ITextNormalaizer
{
    static readonly Dictionary<string,int> PeriodUnits = new(StringComparer.OrdinalIgnoreCase)
    {
        ["д"]=1,["дн"]=1,["дня"]=1,["дней"]=1,
        ["нед"]=7, ["неделя"]=7, ["недели"]=7, ["недель"]=7,
        ["меся"]=30, ["месяц"]=30, ["месяца"]=30, ["месяцев"]=30,
        ["год"]=365, ["года"]=365, ["лет"]=365
    };

    public Task<string> NormalizeAsync(string text, CancellationToken ct)
    {
        var now = DateTime.Now;

        // пример: "через 2 дня", "через 3 недели", "через 1 месяц"
        text = Regex.Replace(text, @"\bчерез\s+(\d+)\s+([а-я]+)\b", m =>
        {
            int value = int.Parse(m.Groups[1].Value);

            string unitWord = m.Groups[2].Value.ToLower();
            var unitKey = PeriodUnits.Keys.FirstOrDefault(k => unitWord.StartsWith(k));
            if (unitKey == null) return m.Value;

            int days = PeriodUnits[unitKey] * value;
            return now.AddDays(days).ToString("yyyy-MM-dd");
        }, RegexOptions.IgnoreCase);

        // пример: "на 3 дня", "на 1 неделю" — можно оставить как ISO P-формат (по желанию)
        text = Regex.Replace(text, @"\bна\s+(\d+)\s+([а-я]+)\b", m =>
        {
            int value = int.Parse(m.Groups[1].Value);

            string unitWord = m.Groups[2].Value.ToLower();
            var unitKey = PeriodUnits.Keys.FirstOrDefault(k => unitWord.StartsWith(k));
            if (unitKey == null) return m.Value;

            int days = PeriodUnits[unitKey] * value;
            return now.AddDays(days).ToString("yyyy-MM-dd");
        }, RegexOptions.IgnoreCase);

        return Task.FromResult(text);
    }
}
