using System.Text.RegularExpressions;
using VoxCore.Runtime.Contracts;

namespace VoxCore.Infrastructure.Services.Normalizers;

public class Weekday : ITextNormalaizer
{
    static readonly Dictionary<string,int> Weekdays = new(StringComparer.OrdinalIgnoreCase)
    {
        ["понедельник"]=1,["вторник"]=2,["сред"]=3,["четверг"]=4,
        ["пятниц"]=5,["суббот"]=6,["воскресень"]=0
    };

    public Task<string> NormalizeAsync(string text, CancellationToken ct)
    {
        var today = DateTime.Now.Date;

        text = Regex.Replace(text,
            @"\b(в\s+)?(следующ(ий|ую|ем)?\s+)?(понедельник|вторник|среда|среду|четверг|пятница|пятницу|суббота|субботу|воскресенье)\b",
            m =>
            {
                var word = m.Groups[4].Value.ToLower();
                var key = Weekdays.Keys.First(k => word.StartsWith(k));
                int target = Weekdays[key];

                int diff = ((target - (int)today.DayOfWeek + 7) % 7);
                if (diff == 0) diff = 7;

                if (m.Value.Contains("следующ"))
                    diff += 7;

                return today.AddDays(diff).ToString("yyyy-MM-dd");
            }, RegexOptions.IgnoreCase);

        return Task.FromResult(text);
    }
}
