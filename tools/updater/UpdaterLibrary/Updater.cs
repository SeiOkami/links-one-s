using UpdaterLibrary.Models;
using UpdaterLibrary.Settings;
using System.Text.Json;
using UpdaterLibrary.UpdateHandlers;
using System.Text;
using UpdaterLibrary.SiteUpdater;
using Telegram.Bot.Types;
using System.Text.Encodings.Web;
using UpdaterLibrary.TelegramBot;
using UpdaterLibrary.Tools;

namespace UpdaterLibrary;

public class Updater : IUpdater
{
    public UpdateDataModel Data { get; set; } = null!;
    public UpdateParametrs Parametrs { get; set; } = new();
    public Dictionary<string, LogEventModel> Log { get; set; } = new();

    private DateTime _dateStartUpdate;

    private readonly TelegramApi _telegram;

    private readonly List<Action<string, object, LogEventType>> logHandlers = new();

    private readonly List<IUpdateHandler> _handlers = [
        new IrrelevantTagsUpdateHandler(),
        new ChannelInfoUpdateHandler(),
        new MemberCountUpdateHandler(),
        //new TempUpdateHandler(),
    ];

    public Updater()
    {
        _telegram = new(this);
    }

    public async Task UpdateAsync()
    {
        try
        {
            _dateStartUpdate = DateTime.Now;

            ReadSiteData();

            foreach (var handler in _handlers)
            {
                await handler.UpdateAsync(this);
            }

            await SaveChangesAsync();

            await NotifySubscribers();
        }
        catch (Exception ex)
        {
            await NotifyExceptionSubscribers(ex);
            throw;
        }

    }

    public void ReadSiteData()
    {
        var filePath = SettingsManager.PathToFileChannels;

        if (!System.IO.File.Exists(filePath))
        {
            throw new FileNotFoundException($"Не найден файл каналов: {filePath}");
        }

        using (var reader = new StreamReader(filePath))
        {
            var fileContents = reader.ReadToEnd();

            var options = new JsonSerializerOptions { 
                PropertyNameCaseInsensitive = true,
            };
            var result = JsonSerializer.Deserialize<UpdateDataModel>(fileContents, options);

            if (result is null)
                throw new FileLoadException($"Файл каналов пустой: {filePath}");
            else if (result.Channels is null)
                throw new FileLoadException($"Файл не содержит каналы: {filePath}");
            else if (result.Tags is null)
                throw new FileLoadException($"Файл не содержит теги: {filePath}");
            else
                Data = result;
        }
    }

    public void UpdateChannelTag(ChannelModel channel, string TagName, bool Add)
    {
        var contains = channel.Tags.Contains(TagName);
        if (Add && !contains)
        {
            channel.Tags.Add(TagName);
            AddToLog($"Добавлен тег '{TagName}'", channel);
        }
        else if (!Add && contains)
        {
            channel.Tags.Remove(TagName);
            AddToLog($"Удален тег '{TagName}'", channel);
        }
    }

    public void AddLogHandler(Action<string, object, LogEventType> handler)
    {
        logHandlers.Add(handler);
    }

    public void AddToLog(string message, object obj, LogEventType type = LogEventType.Change)
    {
        LogEventModel logEvent;

        if (Log.ContainsKey(message))
        {
            logEvent = Log[message];
        }
        else
        {
            logEvent = new();
            logEvent.Text = message;
            logEvent.Type = type;
            Log.Add(message, logEvent);
        }

        if (!logEvent.Objects.Contains(obj))
        {
            logEvent.Objects.Add(obj);
        }

        if (type == LogEventType.Change && obj is ChannelModel)
        {
            ((ChannelModel)obj).Actual = DateTime.Now;
        }

        foreach (var logHandler in logHandlers)
        {
            logHandler.Invoke(message, obj, type);
        }
        
    }

    public string LogText(LogEventType? filterType = null)
    {
        var builder = new StringBuilder();
        
        foreach (var item in Log)
        {
            var logEvent = item.Value;

            if (filterType != null && logEvent.Type != filterType)
                continue;

            List<object> listStr = new();

            builder.Append($"{item.Key}: ");

            foreach (var itemObj in logEvent.Objects)
            {
                var thisStr = itemObj.ToStringRecurs();
                if (!listStr.Contains(thisStr))
                   listStr.Add(thisStr);
            }

            builder.Append(listStr.ToStringRecurs());
            builder.AppendLine();
        }
        
        return builder.ToString();
    }

    public bool ChannelUpdateRequired(ChannelModel channel)
    {
        if (Parametrs.ChannelID != 0)
            return channel.ID == Parametrs.ChannelID;
        else if (!string.IsNullOrEmpty(Parametrs.ChannelUserName))
            return string.Equals(channel.UserName, Parametrs.ChannelUserName, StringComparison.OrdinalIgnoreCase);
        else
            return !channel.Tags.Contains(SettingsManager.TAG_NAME_CLOSE);
    }

    private async Task NotifySubscribers()
    {
        var dateFinishUpdate = DateTime.Now;

        var errors = LogText(LogEventType.Error);
        var changes = LogText(LogEventType.Change);

        foreach (var subscriber in SettingsManager.Subscribers)
        {
            await _telegram.SendTextMessageAsync(subscriber, $"Начало: {_dateStartUpdate}, Конец: {dateFinishUpdate}");

            if (!string.IsNullOrEmpty(errors))
            {
                errors = $"ОШИБКИ!!! \n{errors}";
                await _telegram.SendTextMessageAsync(subscriber, errors);
            }

            if (!string.IsNullOrEmpty(changes))
            {
                changes = $"Изменения: \n{changes}";
                await _telegram.SendTextMessageAsync(subscriber, changes);
            }
        }
    }

    private async Task NotifyExceptionSubscribers(Exception ex)
    {
        foreach (var subscriber in SettingsManager.Subscribers)
        {
            await _telegram.SendTextMessageAsync(subscriber, ex.Message);
        }
    }
    private async Task SaveChangesAsync()
    {
        if (!Parametrs.SaveChanges)
            return;

        var options = new JsonSerializerOptions { 
            WriteIndented = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        string jsonString = JsonSerializer.Serialize(Data, options);

        await System.IO.File.WriteAllTextAsync(SettingsManager.PathToFileChannels, jsonString);
        
    }

    public Task<int?> GetMemberCountAsync(ChannelModel channel)
    {
        return _telegram.GetChatMemberCountAsync(channel);
    }

    public Task<Chat?> GetChatAsync(ChannelModel channel)
    {
        return _telegram.GetChatAsync(channel);
    }

}
