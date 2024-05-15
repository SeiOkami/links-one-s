using UpdaterLibrary;
using UpdaterLibrary.SiteUpdater;
using UpdaterLibrary.Tools;

namespace ConsoleUpdater;

static class Program
{
    static async Task Main(string[] args)
    {

        string filterChanel = "";

        Console.WriteLine("Обновление сайта с ресурсами по теме 1С".ToUpper());
        Console.WriteLine("---------------------------------------");
        Console.WriteLine("1. Обновить все данные");
        Console.WriteLine("2. Обновить по каналу");
        Console.WriteLine("3. Выйти");

        while (true)
        {
            var key = Console.ReadKey();
            if (key.KeyChar == '1')
            {
                Console.WriteLine("\n Обновление всех данных");
                break;
            } else if (key.KeyChar == '2') {
                Console.Write("\n Имя канала: ");
                filterChanel = Console.ReadLine() ?? "";
                if (!string.IsNullOrEmpty(filterChanel))
                    break;
            }
            else if (key.KeyChar == '3')
            {
                return;
            }
        }

        var updater = new Updater();
        updater.AddLogHandler(AddToLog);
        updater.Parametrs.ChannelUserName = filterChanel;
        await updater.UpdateAsync();

        Console.WriteLine($"Всего каналов: {updater.Data.Channels.Count}");
        Console.WriteLine($"Всего тегов: {updater.Data.Tags.Count}");

        Console.ReadLine();

    }
    public static void AddToLog(string message, object obj, LogEventType type)
    {
        if (type == LogEventType.Error)
            Console.ForegroundColor = ConsoleColor.Red;
        else
            Console.ForegroundColor = ConsoleColor.Green;
        
        Console.WriteLine($"[{DateTime.Now}] {obj.ToStringRecurs()}: {message}");
    }

}
