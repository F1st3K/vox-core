using System.Globalization;
using System.Text.Json;
using VoxCore.Plugins.Contracts;
using VoxCore.Plugins.Contracts.Services;

namespace VoxCore.Plugins.ExamplePack;

public class WeatherPlugin(
    ICurrentDialog dialog
) : PluginBase<WeatherPlugin.IntentDeclaration, WeatherPlugin.Params>
{
    public override IntentDeclaration Intent { get; } = new IntentDeclaration();

    public class IntentDeclaration() : IIntentDeclaration
    {
        public string Name => "weather";

        public IEnumerable<string> Examples =>
        [
            "Расскажи прогноз погоды",
            "Какая погода на улице?",
            "Погода сегодня",
            "Что будет с погодой завтра?",
            "Погода на послезавтра",
            $"Скажи прогноз на [3]({nameof(Params.CountDays)}) дня",
            "Какая температура сейчас?",
            "Будет ли дождь сегодня?",
            "Будет ли снег завтра?",
            "Какая погода в выходные?",
            $"Расскажи про осадки на [3]({nameof(Params.CountDays)}) дня",
            $"Какой будет температура воздуха на [1]({nameof(Params.CountDays)}) день?",
            $"Погода на ближайшие [5]({nameof(Params.CountDays)}) дней",
            $"Прогноз температуры на [7]({nameof(Params.CountDays)}) дней",
            "Погода на сегодня утром",
            "Погода на сегодня вечером",
            $"Будет ли солнце на [2]({nameof(Params.CountDays)}) день?",
            $"Скажи прогноз на [2]({nameof(Params.CountDays)}) дня",
            "Какая погода будет в пятницу?",
            $"Прогноз погоды на [10]({nameof(Params.CountDays)}) дней",
            "Че по погоде?",
            "Как там на улице?",
            "Стоит ли брать зонт?",
            "Холодно ли сегодня?",
            "Тепло ли на улице?",
            "Погода сегодня какая?",
            "Будет ли дождь сейчас?",
            "Солнечно ли сегодня?",
            "Похолодание ожидается?",
            "Потеплеет ли завтра?",
            "Снег идет?",
            "Дождь льет?",
            "Какая температура на улице?",
            "Как погода утром?",
            "Как погода вечером?",
            "Прогноз на день?",
            "Что за погода сейчас?",
            "Погода на ближайшие дни?",
            "Стоит ли брать куртку?",
            "Будет ли ветер?"
        ];
    }

    public class Params()
    {
        public int? CountDays { get; set; }
    }

    public override async Task ExecuteAsync(Params parameters, CancellationToken ct)
    {
        using var http = new HttpClient();

        using var docCords = JsonDocument.Parse(
            await http.GetStringAsync("https://ipapi.co/json/"));

        var lat = docCords.RootElement.GetProperty("latitude").GetDouble();
        var lon = docCords.RootElement.GetProperty("longitude").GetDouble();

        using var weather = JsonDocument.Parse(
            await http.GetStringAsync("https://api.open-meteo.com/v1/forecast"
                + $"?latitude={lat}"
                + $"&longitude={lon}"
                + "&daily=temperature_2m_mean,apparent_temperature_mean,weather_code"
                + "&current=temperature_2m,apparent_temperature,weather_code"
                + "&timezone=auto"
                + (parameters.CountDays.HasValue
                    ? $"&start_date={DateTime.Today.AddDays(1):yyyy-MM-dd}"
                    + $"&end_date={DateTime.Today.AddDays((double)parameters.CountDays):yyyy-MM-dd}"
                    : "&forecast_days=1")
                ));

        var fs = Parse(weather);

        var message = string.Empty;
        foreach (var f in fs)
        {
            message += $"{StartWithCap(HumanizeDate(f.Date))} {GetWeatherDescriptionRu(f.WeatherCode)}, температура {GetTemperature(f.Temperature)}, ощущается как {GetTemperature(f.TemperatureApparent)}. ";
        }

        dialog.Say(message);
    }

    #region private utils

    private sealed record DailyForecast(
        DateTime Date,
        int WeatherCode,
        int Temperature,
        int TemperatureApparent
    );

    private static DailyForecast[] Parse(JsonDocument weather)
    {
        var daily = weather.RootElement.GetProperty("daily");

        var dates = daily.GetProperty("time").EnumerateArray().ToArray();
        var codes = daily.GetProperty("weather_code").EnumerateArray().ToArray();
        var ts = daily.GetProperty("temperature_2m_mean").EnumerateArray().ToArray();
        var tsApparent = daily.GetProperty("apparent_temperature_mean").EnumerateArray().ToArray();

        var current = weather.RootElement.GetProperty("current");

        var result = new DailyForecast[dates.Length + 1];
        result[0] = new DailyForecast(
                DateTime.Parse(current.GetProperty("time").GetString()!),
                current.GetProperty("weather_code").GetInt32(),
                (int)Math.Round(current.GetProperty("temperature_2m").GetDouble()),
                (int)Math.Round(current.GetProperty("apparent_temperature").GetDouble())
        );

        for (int i = 0; i < dates.Length; i++)
        {
            result[i + 1] = new DailyForecast(
                DateTime.Parse(dates[i].GetString()!),
                codes[i].GetInt32(),
                (int)Math.Round(ts[i].GetDouble()),
                (int)Math.Round(tsApparent[i].GetDouble())
            );
        }
        return result;
    }

    private static string GetWeatherDescriptionRu(int code) =>
        code switch
        {
            0 => "ясно",

            1 or 2 => "преимущественно ясно, переменная облачность",
            3 => "пасмурно",

            45 or 48 => "туман",

            51 => "слабая морось",
            53 => "моросящий дождь",
            55 => "сильная морось",

            56 => "слабая морось с гололёдом",
            57 => "сильная морось с гололёдом",

            61 => "небольшой дождь",
            63 => "дождь",
            65 => "сильный дождь",

            66 => "слабый ледяной дождь",
            67 => "сильный ледяной дождь",

            71 => "небольшой снег",
            73 => "снег",
            75 => "сильный снег",

            77 => "мелкий снег",

            80 => "кратковременные дожди",
            81 => "дожди",
            82 => "сильные ливни",

            85 => "небольшой снегопад",
            86 => "сильный снегопад",

            95 => "гроза",
            96 => "гроза с градом",
            99 => "сильная гроза с градом",

            _ => ""
        };

    private static string HumanizeDate(DateTime date)
    {
        var today = DateTime.Today;
        var target = date.Date;

        if (Math.Abs((target - DateTime.Now).TotalMinutes) <= 1)
            return "cейчас на улице";

        if (target == today)
            return "сегодня в течении дня ожидается";

        if (target == today.AddDays(1))
            return "завтра будет";

        if (target == today.AddDays(2))
            return "послезавтра у нас";

        var culture = new CultureInfo("ru-RU");
        var dayName = culture.DateTimeFormat.GetDayName(target.DayOfWeek);

        var startOfWeek = today.AddDays(-(int)today.DayOfWeek + 1);
        var endOfWeek = startOfWeek.AddDays(6);

        if (target >= startOfWeek && target <= endOfWeek)
            return $"в {dayName} будет";

        return $"в {dayName}, {target:dd.MM}";
    }

    private static string GetTemperature(int t) =>
        $"{(t > 0 ? "плюс " : t < 0 ? "минус " : "") + t}";

    private static string StartWithCap(string s) =>
        string.IsNullOrEmpty(s) ? s : char.ToUpper(s[0]) + s.Substring(1);

    #endregion
}