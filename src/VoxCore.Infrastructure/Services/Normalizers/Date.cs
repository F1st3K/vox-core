using System.Text.RegularExpressions;
using VoxCore.Runtime.Contracts;

namespace VoxCore.Infrastructure.Services.Normalizers;

public class Date : ITextNormalaizer
{
    static readonly Dictionary<string,int> Months = new(StringComparer.OrdinalIgnoreCase)
    {
        ["январ"]=1,["феврал"]=2,["март"]=3,["апрел"]=4,
        ["ма"]=5,["июн"]=6,["июл"]=7,["август"]=8,
        ["сентябр"]=9,["октябр"]=10,["ноябр"]=11,["декабр"]=12
    };

    public Task<string> NormalizeAsync(string text, CancellationToken ct)
    {
        var now = DateTime.Now;
        var today = now.Date;

        text = Regex.Replace(text, @"\b(сегодня|завтра|послезавтра|вчера|позавчера)\b", 
            m => m.Value.ToLower() switch
            {
                "сегодня" => today.ToString("yyyy-MM-dd"),
                "завтра" => today.AddDays(1).ToString("yyyy-MM-dd"),
                "послезавтра" => today.AddDays(2).ToString("yyyy-MM-dd"),
                "вчера" => today.AddDays(-1).ToString("yyyy-MM-dd"),
                "позавчера" => today.AddDays(-2).ToString("yyyy-MM-dd"),
                _ => m.Value
            }, RegexOptions.IgnoreCase);

        text = Regex.Replace(text, @"\b(\d{1,2})\s+([а-я]+)(?:\s+(\d{4}))?\b",
            m =>
            {
                int day = int.Parse(m.Groups[1].Value);
                string monthWord = m.Groups[2].Value.ToLower();

                var monthKey = Months.Keys.FirstOrDefault(k => monthWord.StartsWith(k));
                if (monthKey == null) return m.Value;

                int month = Months[monthKey];
                int year = m.Groups[3].Success ? int.Parse(m.Groups[3].Value) : today.Year;

                return new DateTime(year, month, day).ToString("yyyy-MM-dd");
            }, RegexOptions.IgnoreCase);

        return Task.FromResult(text);
    }
}
