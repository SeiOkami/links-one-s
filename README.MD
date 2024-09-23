[![OpenYellow](https://img.shields.io/endpoint?url=https://openyellow.org/data/badges/4/800923299.json)](https://openyellow.org/grid?data=top&repo=800923299)

# Реестр 1С в Telegram

Репозиторий реестра всех каналов, чатов и т.д. по теме 1С в Telegram

Список ссылок:
- [Опубликованный реестр](https://SeiOkami.github.io/links-one-s/)
- [Предложения по улучшению](https://github.com/SeiOkami/links-one-s/issues)
- [Автор](https://github.com/SeiOkami)
- [Личка в Telegram](https://t.me/SeiOkami)

## Как добавить канал
Для добавления нового канала можно сделать одним из следующих способов:
- Написать мне в личку Telegram
- Зарегистрировать issue на странице предложений
- Самому создать пулл реквест. Для этого необходимо добавить свой канал в конце списка `channels` файла [channels.json](/docs/data/channels.json). Указать нужно URL и ID. Остальная информация будет обработана автоматически

## Структура проекта
Проект содержит:
- [docs](/docs/) - Статический сайт, опубликованный на GitHub Pages
- [channels.json](/docs/data/channels.json) - список всех данных сайта (теги, выводимые колонки, каналы, чаты и т.д.)
- [updater](/tools/updater/) - C# проект для автоматического обновления данных реестра из Telegram
