namespace UpdaterLibrary.Settings;
using Newtonsoft.Json;
using System.Globalization;
using Telegram.Bot.Types.Enums;

public static class SettingsManager
{
    private const string SETTINGS_FILE_NAME = "appsettings.json";
    private const string CHANNELS_FILE_PATH = "data/channels.json";
    public const string TELEGRAM_URL = $"https://t.me/";
    public const string TAG_NAME_BEGIN = "Малыши (<500)";
    public const string TAG_NAME_CLOSE = "Закрыт";
    public const string TAG_NAME_CHANNEL = "Канал";
    public const string TAG_NAME_CHAT = "Чат";
    public const string TAG_NAME_BOT = "Бот";
    public const string TAG_NAME_USER = "Пользователь";
    public const int MAX_LENGTH_MESSAGE = 3000;
    public const int MAX_NUMBER_ATTEMPTS = 3;
    public const string ERROR_CHAT_NOT_FOUND = "Bad Request: chat not found";
    public const string ERROR_CHAT_SUPERGROUP = "Forbidden: bot is not a member of the supergroup chat";

    private static SettingsModel Instance { get; set; }

    public static string PathToFileChannels { get; private set; }
    public static string TelegramBotToken => Instance.TelegramBotToken;
    public static int DelayRequestsApi => Instance.DelayRequestsApi;
    public static List<string> Subscribers => Instance.Subscribers;

    static SettingsManager()
    {
        Instance = ReadSettingsFile();
        PathToFileChannels = FullPathToFile(
            false,
            Instance.PathToSite,
            CHANNELS_FILE_PATH);
    }

    private static SettingsModel ReadSettingsFile()
    {
        var filePath = FullPathToFile(true, SETTINGS_FILE_NAME);

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Файл настроек не найден: {filePath}");
        }

        using (var reader = new StreamReader(filePath))
        {
            var fileContents = reader.ReadToEnd();
            var result = JsonConvert.DeserializeObject<SettingsModel>(fileContents);
            if (result is null)
                throw new FileLoadException($"Файл настроек пустой: {filePath}");
            else
                return result;
        }
    }

    private static string FullPathToFile(bool isRelative, params string[] paths)
    {
        var path = Path.Combine(paths);
        if (isRelative)
        {
            path = Path.Combine(Directory.GetCurrentDirectory(), path);
            path = Path.GetFullPath(path);
        }
        return path;
    }

}