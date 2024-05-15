using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Channels;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using UpdaterLibrary.Models;
using UpdaterLibrary.Settings;
using UpdaterLibrary.SiteUpdater;
using UpdaterLibrary.Tools;

namespace UpdaterLibrary.TelegramBot;
public class TelegramApi
{
    private readonly TelegramBotClient _telegram = new(SettingsManager.TelegramBotToken);

    private readonly IUpdater _updater;

    private readonly Dictionary<ChannelModel, string> _cacheHtml = new();

    public TelegramApi(IUpdater updater)
    {
        _updater = updater;
    }

    public async Task SendTextMessageAsync(string userName, string text)
    {
        var chatId = new ChatId(userName);
        using (StringReader reader = new StringReader(text))
        {
            StringBuilder message = new();

            while (true)
            {
                var length = message.Length;
                var line = reader.ReadLine();

                var send = line is null || length + line.Length > SettingsManager.MAX_LENGTH_MESSAGE;
                if (send)
                {
                    var partMessage = message.ToString();
                    message.Clear();
                    if (!string.IsNullOrEmpty(partMessage))
                        await InvokeTelegramMethod<Message>(nameof(SendTextMessageAsync), chatId, partMessage);
                }

                if (line is null)
                    break;
                else
                    message.AppendLine(line);
            }
        }

    }


    private void DelayApi()
    {
        Thread.Sleep(SettingsManager.DelayRequestsApi);
    }

    public async Task<int?> GetChatMemberCountAsync(ChannelModel channel)
    {
        return await InvokeTelegramMethod<int>(nameof(GetChatMemberCountAsync), channel);
    }

    public async Task<Chat?> GetChatAsync(ChannelModel channel)
    {
        return await InvokeTelegramMethod<Chat>(nameof(GetChatAsync), channel);
    }

    private async Task<TResult?> InvokeTelegramMethod<TResult>(string methodName, params object[] parametres)
    {
        var chatIdRevers = false;
        var invokeFromHTML = false;
        var attemptsLeft = 1;

        var invokeParametrs = new object[parametres.Length + 1];
        FillParametrsTelegramMethod(parametres, invokeParametrs, chatIdRevers);

        while (attemptsLeft <= SettingsManager.MAX_NUMBER_ATTEMPTS)
        {
            DelayApi();
            try
            {
                if (invokeFromHTML) 
                {
                    return await InvokeTelegramMethodFromURL<TResult>(methodName, parametres);
                } else
                {
                    var task = ReflectionHelper.InvokeMethod(
                        typeof(TelegramBotClientExtensions),
                        methodName, invokeParametrs);
                    if (task == null)
                        break;
                    return await (Task<TResult>)task;
                }
            }
            catch (Exception ex)
            {
                var isErrorNotFound = ex.Message == SettingsManager.ERROR_CHAT_NOT_FOUND;
                var isErrorSupergroup = ex.Message == SettingsManager.ERROR_CHAT_SUPERGROUP;

                //Не понял почему, но в каких-то случаях телеграм принимает username, а в каких-то id
                //А иногда вообще никак не находит, (например для супергруп)
                //  - сначала ищем по стандартному ключу (username для каналов и ID для чатов)
                //  - если не удалось, то ищем наоборот (username для чатов и ID для каналов)
                //  - иначе же просто парсим HTML по ссылке
                if (isErrorNotFound)
                {
                    if (chatIdRevers)
                        invokeFromHTML = true;
                    else
                    {
                        chatIdRevers = true;
                        FillParametrsTelegramMethod(parametres, invokeParametrs, chatIdRevers);
                    }
                } else if (isErrorSupergroup) {
                    invokeFromHTML = true;
                } else {
                    attemptsLeft++;

                    _updater.AddToLog(ex.Message, parametres, LogEventType.Error);
                    handleError(ex);
                }
            }
        }
        return default(TResult);
    }

    private void FillParametrsTelegramMethod(object[] parametrs, object[] invokeParametrs, bool chatIdRevers = false)
    {
        invokeParametrs[0] = _telegram;
        for (int i = 0; i < parametrs.Length; i++)
        {
            var par = parametrs[i];
            if (par is ChannelModel channel)
                par = GetChatId(channel, chatIdRevers);
            invokeParametrs[i + 1] = par;
        }
    }

    private void handleError(Exception ex)
    {
        if (ex is ApiRequestException apiEx)
        {
            var retry = apiEx.Parameters?.RetryAfter ?? 0;
            if (retry > 0)
            {
                Thread.Sleep(retry * 1000);

            }
        }
    }

    public ChatId GetChatId(ChannelModel channel, bool revers = false)
    {
        var returnId = !channel.IsChannel;

        if (revers)
            returnId = !returnId;

        if (returnId)
            return new(channel.ID); 
        else
            return new("@" + channel.UserName);
    }


    private async Task<TResult?> InvokeTelegramMethodFromURL<TResult>(string methodName, params object[] parametres)
    {
        ChannelModel? channel = null;
        foreach (var parametr in parametres)
            if (parametr is ChannelModel ch)
                channel = ch;

        if (channel != null)
        {
            string? html;
            if (_cacheHtml.ContainsKey(channel))
                html = _cacheHtml[channel];
            else
            {
                using var client = new HttpClient();
                html = await client.GetStringAsync(channel.Url);
                _cacheHtml.Add(channel, html);
            }

            if (methodName == nameof(GetChatMemberCountAsync))
                return (TResult)(object)ExtractCountMembersFromURL(html);
            else if (methodName == nameof(GetChatAsync))
            {
                Chat chat = new Chat();
                chat.Title = ExtractNameFromURL(html);
                chat.Description = ExtractDescriptionFromURL(html);
                chat.Type = Telegram.Bot.Types.Enums.ChatType.Supergroup;
                return (TResult)(object)chat;
            }
        }

        return default(TResult);

    }

    private int ExtractCountMembersFromURL(string html)
    {
        var regex = new Regex(@"<div class=""tgme_page_extra"">(\d+)");
        var match = regex.Match(html);

        if (match.Success && int.TryParse(match.Groups[1].Value, out var countMembers))
        {
            return countMembers;
        }

        return 0;
    }

    private string ExtractNameFromURL(string html)
    {
        var regex = new Regex(@"<div class=""tgme_page_title"" dir=""auto"">[\s\S]*?<span dir=""auto"">(.*?)</span>");
        var match = regex.Match(html);

        return match.Success ? match.Groups[1].Value : string.Empty;
    }

    private string ExtractDescriptionFromURL(string html)
    {
        var result = string.Empty;
        var regex = new Regex(@"<div class=""tgme_page_description"" dir=""auto"">(.*?)</div>");
        var match = regex.Match(html);

        if (match.Success)
        {
            result = match.Groups[1].Value.Replace("<br/>", "\n");
            result = RemoveHtmlLinks(result);
        }

        return result;
    }

    private string RemoveHtmlLinks(string input)
    {
        string pattern = "<a href=\"(.*?)\">(.*?)</a>";
        string replacement = "$1";
        string result = Regex.Replace(input, pattern, replacement);
        return result;
    }

}
