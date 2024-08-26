# Программа для работы с малой базой данных
Суть программы - загружать,сортировать и выгружать файлы в базе данных. Базой данных выступает либо компьютер админа, либо приватный репозиторий GitHub, либо и то и другое.

# Вид программы

![Image alt](https://So1ta.github.io/image.png)

# Как начать работать с запущенной программой

1. Админ указывает директорию базы данных (кнопка **Путь** сверху)
2. Пользователь загружает файл\ы в базу (кнопка **Файл**, под кнопкой **Путь**)

# Дополнительные функции

* Файлы можно загружать в базу просто перетаскивая их в в окно программы.
* Можно скачивать файлы по кнопке **Скачать**, но перед этим нужно указать ссылку для скачивания в текстовом поле справа от кнопки. Работают только прямые ссылки.
* Можно комплексно копировать файлы в базу по кнопке **Сканировать**. Нужно указать путь сканирования по второй кнопке **Путь**, после нажать кнопку Сканировать. Все обнаруженные файлы будут перенесены в базу.
* Верхний блок отображает историю всех скопированных\удаленных\скачанных файлов.
* Нижний блок отображает базу данных из репозитория GitHub, база обновляется по кнопке **GitHub**.
* Кнопка **Выбранная директория** отображает действущий путь на компьютере где находится база данных.
* Кнопка **Директория программы** отображает действующий путь где находятся рабочие файлы программы.

# Настройка для корректного запуска программы

Папка **Gotovo** содержит пример запущенной программы, которая может работать только с локальным компьютером, без GitHub.

Папка BAZA содержит проект с программой для Visual Studio.
> [!IMPORTANT]
> Язык программы - C#
> Пакет, который нужно скачать для работы программы(**указана версия на которой создавалась программа**):
> * Octokit 11.0.1
>
> Важно указать свой Username, Repository, Token GitHub в данных строчках вначале кода:
> ```
> private const string GitHubUsername = "USERNAME_GITHUB";
> private const string GitHubRepository = "REPOSUTIRY_GITHUB";
> private const string GitHubToken = "TOKEN_GITHUB";
> ```
> Дальше скомпилируйте программу и готово!

# Работа программы в связке с GitHub
В нижнем блоке отображаются файлы, хранящиейся в вашем приватном(либо публичном) репозитории GitHub, который выступает базой данных. Отображаются не только файлы, но и папки, которые можно открыть списком.

Двойным щелчком по файлу вы можете скачать файл, который сохранится в локальную директорию по пути Документы\BAZA\GitHub (Программа сама создает папку GitHub).

### Загрузка файлов на репозиторий GitHub
На данном этапе разработки программы существует лишь один способ загрузить файлы на репозиторий GitHub - через телеграмм бота (исходные файлы бота прикреплены).

В телеграмм боте BotReader(исходное название) существуют 2 кнопки:
* Отправить сообщение на GitHub
* Очистить репозиторий GitHub
По кнопке **Отправить сообщение на GitHub** вы можете отправить любой файл сообщением, в том числе обычный текст.

По кнопке **Очистить репозиторий GitHub** вы можете полностью очистить репозиторий.

Дальше в программе нажмите кнопку GitHub, нижний блок обновится и отобразит вновь загруженные файлы.

# Сортировка файлов
> [!WARNING]
> Сортировка файлов настроена по их расширению(.exe, .xml, .txt и т.д.).
> Расширение может быть любым. В зависимости от расширения файла, программа сама создаст папку в директории с названием расширения при копировании файла в базу и скопирует его в эту папку. (повторные файлы также обрабатываются и помечаются, смотри описание исключений ниже)

# Рабочие файлы программы
> [!WARNING]
> При первом запуске программа автоматически создает в документах папку BAZA где хранится история и выбранная директория в текстовом файле.
> Логирование. Когда пользователь очищает историю через кнопку, программа автоматически создает папку **saved-history** где сохраняется файл с историей, где ему присваевается дата удаления в название, а прошлый файл с историей очищается.

# Исключения

> [!NOTE]
> В программе проработы большинство исключений по типу "Укажите директорию", если она не указана, "Вставьте ссылку", если она не вставлена. Исключения с одинаковыми названием или расширением файлов также проработаны. В случае, если будет скопирован файл, которые уже есть в конце названия будет присваиваться тег _1, _2, _3 и т.д.
> Также в программе отключены исключения, для отключения некоторых кнопок для типа "Пользователь", которые должны быть только у Администратора. Вы их можете настроить сами, а также нужно добавить свою форму входа.
> Вы можете переработать программу под SQL, либо Access, тут уж на ваш выбор.
> Код подписан комментариями.

> [!TIP]
> В будущем будут следующие изменения:
> * Добавлю форму входа, со всем вытекающим (хранения паролей, логинов, распределение ролей).
> * Переработаю дизайн программы
> * Почищу код от мусора (сейчас там его много).
> * Добавлю загрузку файлов на GitHub через программу.
> * Добавлю возможно загружать одновременно по несколько файлов через бота.
> * Добавлю роли в телеграмм боте.
