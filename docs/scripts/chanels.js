{
    const classColumn = "column-"; // Префикс стиля колонки
    const searchInput = document.getElementById('searchInput'); // Поле строки поиска
    const table = document.getElementById('channels'); // Таблица каналов на странице
    let channels = undefined; // Коллекция каналов из файла
    let tags = undefined; // Коллекция тегов из файла
    let metadata = undefined; // Метаданные таблицы каналов из файла
    let filterTags = []; // Теги, выбранные пользователем для просмотра
    let searchText = ""; // Текущий текст в поле поиска

    // Создание таблицы каналов на основе прочитанных данных из файла
    function initialTable() {

        initialTable_columns();
        initialTable_rows();

    }

    // Создание заголовков колонок таблицы каналов
    function initialTable_columns(){
        
        const thead = document.createElement('thead');
        const headerRow = document.createElement('tr');

        let index = 0;
        metadata.Columns.forEach(column => {
            const th = document.createElement('th');
            th.classList.add(classColumn + column.Name);
            const thisIndex = index;
            th.onclick = () => sortTable(thisIndex);
            th.textContent = column.Presentation;
            headerRow.appendChild(th);
            index++;
        });

        thead.appendChild(headerRow);
        table.appendChild(thead);

    }

    // Создание строк таблицы каналов
    function initialTable_rows(){

        const tbody = document.createElement('tbody');

        channels.forEach(item => {
            const row = document.createElement('tr');
            
            metadata.Columns.forEach(column => {
        
                const columnName = column.Name;
                const cell = document.createElement('td');
                cell.classList.add(classColumn + columnName);

                const value = item[columnName];

                if (columnName === 'Tags') {
                    value.forEach(tagName => {
                        addTagElement(tagName, cell, false);
                    });
                } else {
                    cell.innerHTML = makeLinksClickable(value);
                }

                row.appendChild(cell);
            })

            tbody.appendChild(row);

        });

        table.appendChild(tbody);

    }

    // Функция делает ссылки в ячейке кликабельными
    // Метод сделан при помощи claude, можно улучшить
    function makeLinksClickable(text) {
        
        // Разделяем текст на массив строк
        const lines = text.split('\n');

        // Обрабатываем каждую строку
        const formattedLines = lines.map(line => {
            // Регулярное выражение для URL-ссылок
            const urlPattern = /(https?:\/\/[^\s]+)/g;

            // Регулярное выражение для имен пользователей
            const userPattern = /(?<!\/)\B@([\w.]+)/g;

            // Функция для замены URL-ссылок
            const replaceUrl = (match, url) => `<a href="${url}" target="_blank">${url}</a>`;

            // Функция для замены имен пользователей
            const replaceUser = (match, username) => `<a href="https://t.me/${username}" target="_blank">@${username}</a>`;

            // Замена URL-ссылок
            line = line.replace(urlPattern, replaceUrl);

            // Замена имен пользователей
            line = line.replace(userPattern, replaceUser);

            return line;
        });

        // Объединяем строки обратно в один текст
        const formattedText = formattedLines.join('<br>');

        return formattedText;
    }

    // Добавляет элемент тега в указанный объект
    function addTagElement(tagName, parent, deleteExisting){
        const tag = tags.find(el => el.Name === tagName);
        if (tag) {
            const tagElement = document.createElement('span');
            tagElement.classList.add('tag');
            tagElement.textContent = tagName;
            tagElement.style.backgroundColor = `${tag.Color}`;
            tagElement.title = tag?.Description || '';
            tagElement.onclick = () => addFilterTag(tagName, deleteExisting);
            parent.appendChild(tagElement);
            return tagElement;
        } else {
            console.error(`Not found tag: ${tagName}`);
        }
    }

    // Корректирует строку под правила именования классов
    function convertToClassName(str) {
        return str.trim().replace(/\s+/g, '_').toLowerCase();
    }

    // Добавляет указанный тег в фильтры или удаляет из них
    function addFilterTag(tagName, deleteExisting){
        const panel = document.getElementById("filters-panel");
        const id = "tag-filter-" + convertToClassName(tagName);
        let elem = document.getElementById(id);
        if (elem == undefined){
            elem = addTagElement(tagName, panel, true);
            elem.id = id;
            elem.classList.add('tag-filter');
            filterTags.push(tagName);
        } else if (deleteExisting) {
            elem.remove();
            filterTags = filterTags.filter(item => item !== tagName);
        }

        updateFilter();
    }

    // Загрузка данных из файла channels.json
    function loadFile(){
        fetch('data/channels.json')
            .then(response => response.json())
            .then(data => {
                
                channels = data.Channels;
                tags = data.Tags;
                metadata = data.Metadata;

                initialTable();
        })
        .catch(error => console.error('Error file load:', error));
    }

    // Настройка поля ввода поиска по таблице
    function initialSearchInput() {
        searchInput.addEventListener('input', updateFilter);
    }

    // Проверяет соответствие строки условиям отбора
    function rowVisible(row){

        return rowVisible_tag(row) && rowVisible_input(row);

    }

    // Проверяет соответствие строки выбранным тегам
    function rowVisible_tag(row){
        
        let result = true;

        if (filterTags.length) {
            const tagElements = row.querySelectorAll('.column-Tags .tag');
            result = Array.from(filterTags).every(filterTag => 
                Array.from(tagElements).some(element => element.textContent === filterTag));
        }

        return result;

    }

    // Проверяет соответствие строки поиску
    function rowVisible_input(row){

        if (searchText === '') {
            return true;
        } else {
            return Array.from(row.children)
                .map(cell => cell.textContent.toLowerCase())
                .some(data => data.includes(searchText));
        }

    }

    // Обновляет фильтр строк таблицы
    function updateFilter(){
        searchText = searchInput.value.toLowerCase();
        Array.from(table.querySelectorAll('tbody tr')).forEach(row => {
            row.style.display = rowVisible(row) ? '' : 'none';
        });
    }

    // Сортировка по колонке по индексу (при клике на заголовок колонки)
    // Метод сделан при помощи claude, можно улучшить
    function sortTable(columnIndex) {

        const rows = Array.from(table.getElementsByTagName("tr"));

        rows.shift(); // Убираем заголовок таблицы из массива

        // Определяем тип сортировки (по возрастанию или убыванию)
        let sortOrder = 1;
        if (table.rows[0].cells[columnIndex].classList.contains("sorted-asc")) {
            sortOrder = -1;
        }

        // Сортируем строки таблицы
        rows.sort((a, b) => {
            const aValue = a.cells[columnIndex].textContent.trim();
            const bValue = b.cells[columnIndex].textContent.trim();
            if (isNaN(aValue) || isNaN(bValue)) {
                return sortOrder * aValue.localeCompare(bValue);
            } else {
                return sortOrder * (parseInt(aValue) - parseInt(bValue));
            }
        });

        // Удаляем текущие строки из таблицы
        while (table.rows.length > 1) {
            table.deleteRow(1);
        }

        // Вставляем отсортированные строки обратно в таблицу
        const tbody = table.getElementsByTagName("tbody")[0];
        rows.forEach(row => {
            tbody.appendChild(row);
        });

        // Удаляем классы сортировки из всех заголовков столбцов
        Array.from(table.getElementsByTagName("th")).forEach(th => {
            th.classList.remove("sorted-asc");
            th.classList.remove("sorted-desc");
        });

        // Добавляем класс сортировки к выбранному заголовку столбца
        const selectedColumnHeader = table.rows[0].cells[columnIndex];
        if (sortOrder === 1) {
            selectedColumnHeader.classList.add("sorted-asc");
        } else {
            selectedColumnHeader.classList.add("sorted-desc");
        }
    }

    { //Initial

        loadFile();

        initialSearchInput();

    }
}