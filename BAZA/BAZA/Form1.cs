using System.Diagnostics.Metrics;
using System.Diagnostics;
using System.Net;
using System.Configuration;
using System.Collections.Specialized;
using System.Windows.Forms;
using Octokit;
using System.IO;
using System.Threading.Tasks;
using FormsApplication = System.Windows.Forms.Application; // Псевдоним для Application


namespace BAZA
{
    public partial class Form1 : Form
    {

        // Путь к выбранной директории
        private string selectedDirectoryPath = "";

        private const string GitHubUsername = "USERNAME_GITHUB";
        private const string GitHubRepository = "REPOSUTIRY_GITHUB";
        private const string GitHubToken = "TOKEN_GITHUB";

        private static readonly HttpClient _httpClient = new HttpClient();

        public Form1()
        {
            InitializeComponent();

            this.DragEnter += Form1_DragEnter;
            this.DragDrop += Form1_DragDrop;

            //button1.Enabled = false; // Отключаем кнопку по умолчанию
            //button4.Enabled = false; // Отключаем кнопку по умолчанию
            //button5.Enabled = false; // Отключаем кнопку по умолчанию

            // Подключаем обработчик события NodeMouseDoubleClick
            RepoContentsTreeView.NodeMouseDoubleClick += RepoContentsTreeView_NodeMouseDoubleClick;

            _httpClient.DefaultRequestHeaders.Add("Authorization", $"token {GitHubToken}");

        }

        // Метод для установки доступности кнопки button1
        public void SetButton1Enabled(bool enabled)
        {
            button1.Enabled = enabled;
            button4.Enabled = enabled;
            button5.Enabled = enabled;
        }

        public void SetTextbox1ReadOnly(bool readOnly)
        {
            textBox1.ReadOnly = readOnly;
        }

        // Путь к файлу с выбранной директорией (аналогично historyFilePath)
        private string selectedDirectoryFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "BAZA", "selected_directory.txt");

        private List<string> fileHistory = new List<string>(); // Список для хранения истории
        private string historyFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "BAZA", "history.txt"); // Путь к файлу истории

        private void Form1_Load(object sender, EventArgs e)
        {
            // Загружаем сохраненный путь из настроек
            selectedDirectoryPath = Properties.Settings.Default.SelectedDirectory;
            textBox1.Text = selectedDirectoryPath;

            // Загружаем строку истории из настроек
            string historyString = Properties.Settings.Default.FileHistory;

            // Если история найдена, разбиваем строку на список и добавляем в ListBox
            if (!string.IsNullOrEmpty(historyString))
            {
                fileHistory = new List<string>(historyString.Split('|'));
                UpdateFileHistoryListBox();
            }

            LoadHistoryFromFile(); // Загружаем историю из файла

            // Загружаем сохраненный путь к директории из файла
            selectedDirectoryPath = LoadSelectedDirectoryFromFile();
            textBox1.Text = selectedDirectoryPath;
        }

        // Метод ДИРЕКТОРИЯ ---------------------------------------------------------------------------------------------------------------------------------------------

        private void button1_Click_1(object sender, EventArgs e)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    selectedDirectoryPath = dialog.SelectedPath;

                    // Передаем выбранный путь в метод SaveSelectedDirectory
                    SaveSelectedDirectory(selectedDirectoryPath);

                    // Выводим путь в textBox1
                    textBox1.Text = selectedDirectoryPath;
                }
            }
        }


        // Метод для сохранения выбранной директории в файл
        private void SaveSelectedDirectory(string directoryPath)
        {
            File.WriteAllText(selectedDirectoryFilePath, directoryPath);
        }

        private string LoadSelectedDirectoryFromFile()
        {
            if (File.Exists(selectedDirectoryFilePath))
            {
                return File.ReadAllText(selectedDirectoryFilePath);
            }
            return string.Empty; // Возвращаем пустую строку, если файл не найден
        }


        // Метод ИСТОРИЯ ---------------------------------------------------------------------------------------------------------------------------------------------

        // Метод для добавления записи в историю, обновления ListBox и сохранения в файл
        private void AddFileToHistory(string filePath, string prefix = "Скопирован:")
        {
            // Добавляем префикс, если он передан
            string historyEntry = string.IsNullOrEmpty(prefix) ? filePath : $"{prefix} {filePath}";

            fileHistory.Add(historyEntry);
            UpdateFileHistoryListBox();
            SaveHistoryToFile();
        }


        // Метод для обновления ListBox с историей файлов
        private void UpdateFileHistoryListBox()
        {
            // Очищаем ListBox перед добавлением элементов
            listBox1.Items.Clear();

            // Добавляем все элементы из fileHistory в ListBox
            foreach (string item in fileHistory)
            {
                listBox1.Items.Add(item);
            }
        }


        // Метод для загрузки истории из файла
        private void LoadHistoryFromFile()
        {
            // Создаем папку "BAZA", если она не существует
            string historyFolderPath = Path.GetDirectoryName(historyFilePath);
            if (!Directory.Exists(historyFolderPath))
            {
                Directory.CreateDirectory(historyFolderPath);
            }

            // Проверяем, существует ли файл, и если нет, то создаем его
            if (!File.Exists(historyFilePath))
            {
                File.Create(historyFilePath).Close(); // Создаем файл и сразу закрываем его
            }

            // Теперь файл гарантированно существует, можно читать
            fileHistory = new List<string>(File.ReadAllLines(historyFilePath));
            UpdateFileHistoryListBox();
        }

        // Метод для сохранения истории в файл
        private void SaveHistoryToFile()
        {
            File.WriteAllLines(historyFilePath, fileHistory);
        }

        // Метод КОПИРОВАНИЕ ---------------------------------------------------------------------------------------------------------------------------------------------

        private void button2_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(selectedDirectoryPath))
            {
                MessageBox.Show("Сначала выберите директорию!");
                return;
            }

            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Все файлы (*.*)|*.*";
                openFileDialog.Multiselect = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string message = "";
                    string sourceFilePath = "";
                    int filesCopied = 0; // Добавляем счетчик скопированных файлов

                    foreach (string filePath in openFileDialog.FileNames)
                    {
                        sourceFilePath = filePath;
                        string fileName = Path.GetFileName(sourceFilePath);
                        string extension = Path.GetExtension(sourceFilePath).ToLower().TrimStart('.');

                        string folderPath = Path.Combine(selectedDirectoryPath, extension);
                        Directory.CreateDirectory(folderPath);

                        string destinationFilePath = Path.Combine(folderPath, fileName);

                        int counter = 1;
                        while (File.Exists(destinationFilePath))
                        {
                            string baseFileName = Path.GetFileNameWithoutExtension(fileName);
                            string newFileName = $"{baseFileName}_{counter}{Path.GetExtension(fileName)}";
                            destinationFilePath = Path.Combine(folderPath, newFileName);
                            counter++;
                        }

                        CopyFileWithProgress(sourceFilePath, destinationFilePath, progressBar1);
                        File.Copy(sourceFilePath, destinationFilePath, true);

                        AddFileToHistory(destinationFilePath);

                        if (counter > 1)
                        {
                            message += $"Файл с именем '{fileName}' уже существует.\nФайл переименован в '{Path.GetFileName(destinationFilePath)}' и скопирован в папку '{extension}'.\n\n";
                        }
                        else
                        {
                            message += $"Файл '{Path.GetFileName(destinationFilePath)}' скопирован в папку '{extension}'.\n\n";
                        }
                        filesCopied++; // Увеличиваем счетчик после копирования файла
                    }

                    // Добавляем информацию о количестве файлов в сообщение
                    message += $"\nСкопировано файлов: {filesCopied}";

                    MessageBox.Show(message);
                }
            }
        }



        private void Form1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        private void Form1_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] droppedFiles = (string[])e.Data.GetData(DataFormats.FileDrop);
                string message = "";
                int filesCopied = 0;

                foreach (string droppedFile in droppedFiles)
                {
                    // Вызываем метод для обработки файла 
                    ProcessDroppedFile(droppedFile, ref message, ref filesCopied);
                }

                MessageBox.Show(message + $"\nСкопировано файлов: {filesCopied}");
            }
        }

        private void ProcessDroppedFile(string sourceFilePath, ref string message, ref int filesCopied)
        {
            if (string.IsNullOrEmpty(selectedDirectoryPath))
            {
                MessageBox.Show("Сначала выберите директорию!");
                return;
            }

            string fileName = Path.GetFileName(sourceFilePath);
            string extension = Path.GetExtension(sourceFilePath).ToLower().TrimStart('.');

            string folderPath = Path.Combine(selectedDirectoryPath, extension);
            Directory.CreateDirectory(folderPath);

            string destinationFilePath = Path.Combine(folderPath, fileName);

            int counter = 1;
            while (File.Exists(destinationFilePath))
            {
                string baseFileName = Path.GetFileNameWithoutExtension(fileName);
                string newFileName = $"{baseFileName}_{counter}{Path.GetExtension(fileName)}";
                destinationFilePath = Path.Combine(folderPath, newFileName);
                counter++;
            }

            CopyFileWithProgress(sourceFilePath, destinationFilePath, progressBar1);
            File.Copy(sourceFilePath, destinationFilePath, true);

            AddFileToHistory(destinationFilePath);

            if (counter > 1)
            {
                message += $"Файл с именем '{fileName}' уже существует.\nФайл переименован в '{Path.GetFileName(destinationFilePath)}' и скопирован в папку '{extension}'.\n\n";
            }
            else
            {
                message += $"Файл '{Path.GetFileName(destinationFilePath)}' скопирован в папку '{extension}'.\n\n";
            }

            filesCopied++;
        }



        // Метод для копирования файла с отображением прогресса
        private void CopyFileWithProgress(string sourceFile, string destFile, ProgressBar progressBar1)
        {
            // Настраиваем ProgressBar
            progressBar1.Minimum = 0;
            progressBar1.Maximum = 100;
            progressBar1.Value = 0;

            // Открываем файлы
            using (FileStream sourceStream = new FileStream(sourceFile, System.IO.FileMode.Open, System.IO.FileAccess.Read))
            {
                using (FileStream destStream = new FileStream(destFile, System.IO.FileMode.CreateNew, System.IO.FileAccess.Write))
                {
                    // Размер буфера для копирования (можно менять)
                    byte[] buffer = new byte[4096];
                    int bytesRead;

                    // Копируем файл по блокам
                    while ((bytesRead = sourceStream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        destStream.Write(buffer, 0, bytesRead);

                        // Обновляем ProgressBar
                        int progress = (int)((double)destStream.Position / sourceStream.Length * 100);
                        progressBar1.Value = progress;
                        FormsApplication.DoEvents();  // Обновляем интерфейс
                    }
                }
            }
        }

        // Метод СКАЧИВАНИЕ ---------------------------------------------------------------------------------------------------------------------------------------------

        private string downloadFilePath;
        private string downloadExtension;
        private int downloadCounter;
        private void button3_Click(object sender, EventArgs e)
        {
            string url = textBox2.Text.Trim();

            if (string.IsNullOrEmpty(url))
            {
                MessageBox.Show("Введите URL-адрес файла в TextBox2.");
                return;
            }

            if (string.IsNullOrEmpty(selectedDirectoryPath))
            {
                MessageBox.Show("Сначала выберите директорию!");
                return;
            }

            try
            {
                // Получаем имя файла из URL
                string fileName = GetFileNameFromUrl(url);

                // Создаем папку для скачанного файла (если ее еще нет)
                string extension = Path.GetExtension(fileName).ToLower().TrimStart('.');
                string folderPath = Path.Combine(selectedDirectoryPath, extension);
                Directory.CreateDirectory(folderPath);

                // Формируем путь для сохранения файла
                string filePath = Path.Combine(folderPath, fileName);

                // Проверяем, существует ли уже файл с таким именем
                int counter = 1;
                while (File.Exists(filePath))
                {
                    string baseFileName = Path.GetFileNameWithoutExtension(fileName);
                    string newFileName = $"{baseFileName}_{counter}{Path.GetExtension(fileName)}";
                    filePath = Path.Combine(folderPath, newFileName);
                    counter++;
                }

                downloadFilePath = filePath;
                downloadExtension = extension;
                downloadCounter = counter;

                // Скачиваем файл с помощью WebClient
                using (WebClient client = new WebClient())
                {
                    // Подписываемся на событие окончания загрузки
                    client.DownloadFileCompleted += Client_DownloadFileCompleted;

                    // Подписываемся на событие загрузки прогресса
                    client.DownloadProgressChanged += Client_DownloadProgressChanged;

                    // Скачиваем файл асинхронно, передавая URI в UserState
                    client.DownloadFileAsync(new Uri(url), filePath, new Uri(url));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при скачивании файла: {ex.Message}");
            }
        }

        // Обработчик события окончания загрузки
        private void Client_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                // Обработка ошибки загрузки
                MessageBox.Show($"Ошибка при скачивании файла: {e.Error.Message}");
            }
            else
            {
                // Файл успешно скачан
                string fileName = Path.GetFileName(((Uri)e.UserState).LocalPath); // Получаем имя файла

                // Добавляем файл в историю с подписью "Скачано"
                AddFileToHistory(downloadFilePath, "Скачано:");

                // Сообщение с учетом изменения имени
                string message = downloadCounter > 1
                    ? $"Файл с именем '{fileName}' уже существует.\n" +
                      $"Файл переименован в '{Path.GetFileName(downloadFilePath)}' и сохранен в папку '{downloadExtension}'.\n" +
                      $"Открыть скачанный файл?"
                    : $"Файл '{fileName}' успешно сохранен в папку '{downloadExtension}'.\n" +
                      $"Открыть скачанный файл?";

                // Запоминаем результат вопроса пользователю
                DialogResult result = MessageBox.Show(message, "Загрузка завершена", MessageBoxButtons.YesNo);

                // Открываем файл, если пользователь нажал "Да"
                if (result == DialogResult.Yes)
                {
                    Process.Start(new ProcessStartInfo(downloadFilePath) { UseShellExecute = true });
                }
            }
        }


        // Обработчик события загрузки прогресса
        private void Client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            // Обновляем ProgressBar
            progressBar1.Value = e.ProgressPercentage;
        }

        // Метод для получения имени файла из URL
        private string GetFileNameFromUrl(string url)
        {
            Uri uri = new Uri(url);
            string fileName = Path.GetFileName(uri.LocalPath);
            return fileName;
        }

        // Метод ПОСМОТРЕТЬ ДИРЕКТОРИЮ ---------------------------------------------------------------------------------------------------------------------------------------------

        private void button4_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(selectedDirectoryPath))
            {
                MessageBox.Show("Сначала выберите директорию!");
                return;
            }

            try
            {
                // Открываем выбранную директорию
                Process.Start(new ProcessStartInfo(selectedDirectoryPath) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при открытии директории: {ex.Message}");
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            // Получаем путь к папке "Мои документы"
            string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            // Формируем полный путь к папке "BAZA" в документах
            string bazaFolderPath = Path.Combine(documentsPath, "BAZA");

            // Открываем папку "BAZA" с помощью Проводника Windows
            Process.Start("explorer.exe", bazaFolderPath);
        }


        // Метод ОЧИСТКА И КОПИРОВАНИЕ ИСТОРИИ ---------------------------------------------------------------------------------------------------------------------------------------------

        private void button6_Click(object sender, EventArgs e)
        {
            try
            {
                // Получаем текущую дату в формате "yyyy-MM-dd"
                string currentDate = DateTime.Now.ToString("yyyy-MM-dd");

                // Путь к папке "saved-history"
                string savedHistoryFolderPath = Path.Combine(
                    Path.GetDirectoryName(historyFilePath),
                    "saved-history"
                );

                // Создаем папку, если она не существует
                Directory.CreateDirectory(savedHistoryFolderPath);

                // Формируем базовое имя файла с датой
                string baseFileName = Path.Combine(
                    savedHistoryFolderPath,
                    Path.GetFileNameWithoutExtension(historyFilePath) + "_" + currentDate
                );

                // Проверяем, существует ли уже файл с таким именем
                string newFilePath = baseFileName + Path.GetExtension(historyFilePath);
                int counter = 1;
                while (File.Exists(newFilePath))
                {
                    // Если файл существует, добавляем к имени _1, _2 и т.д.
                    newFilePath = $"{baseFileName}_{counter}{Path.GetExtension(historyFilePath)}";
                    counter++;
                }

                // Копируем файл с перезаписью, если он уже существует
                File.Copy(historyFilePath, newFilePath, true);

                // Формируем сообщение об успешном копировании
                string message = $"Файл истории скопирован в:\n{newFilePath}";

                // Добавляем информацию о переименовании, если файл был переименован
                if (counter > 1)
                {
                    message += $"\n\nФайл был переименован, так как файл с таким именем уже существует.";
                }

                // Очищаем основной файл истории
                File.WriteAllText(historyFilePath, string.Empty);

                // Очищаем список истории и ListBox
                fileHistory.Clear();
                listBox1.Items.Clear();

                MessageBox.Show(message, "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при копировании файла:\n{ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
        private void listBox1_DoubleClick(object sender, EventArgs e)
        {
            if (listBox1.SelectedItem != null)
            {
                string selectedItem = listBox1.SelectedItem.ToString();

                // Извлекаем путь к файлу, игнорируя префикс
                string selectedFilePath = selectedItem.StartsWith("Скачано: ")
                    ? selectedItem.Substring("Скачано: ".Length)
                    : selectedItem;

                // Проверяем, существует ли файл
                if (File.Exists(selectedFilePath))
                {
                    // Открываем файл в приложении по умолчанию
                    Process.Start(new ProcessStartInfo(selectedFilePath) { UseShellExecute = true });
                }
                else
                {
                    MessageBox.Show("Файл не найден.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Подойдет ссылка для скачивания к примеру с GitHub или Giphy.\nВажно, чтобы ссылка в конце имела сам файл с расширением. Пример:\n\nGithub:\nhttps://so1ta.github.io/Images/USP.jpg\n\nСокращена, Яндекс:\nhttps://clck.ru/3AwRYD (Сокращенная ссылка)\n\nGiphy:\nhttps://i.giphy.com/media/v1.Y2lkPTc5MGI3NjExOTJzYTgycXdiODFicjd0NWJwandlNWoxODMzMDF0OWoxcXh0cmhscCZlcD12MV9pbnRlcm5hbF9naWZfYnlfaWQmY3Q9Zw/MDJ9IbxxvDUQM/giphy.gif");
        }

        private void button9_Click(object sender, EventArgs e)
        {
            // Создаем диалоговое окно выбора папки
            using (var folderBrowserDialog = new FolderBrowserDialog())
            {
                // Устанавливаем заголовок окна
                folderBrowserDialog.Description = "Выберите папку:";

                // Отображаем окно и проверяем результат
                DialogResult result = folderBrowserDialog.ShowDialog();

                // Если пользователь выбрал папку и нажал "ОК"
                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(folderBrowserDialog.SelectedPath))
                {
                    // Получаем выбранный путь
                    string selectedPath = folderBrowserDialog.SelectedPath;

                    // Устанавливаем текст textBox3 равным выбранному пути
                    textBox3.Text = selectedPath;
                }
            }
        }

        private void button8_Click(object sender, EventArgs e)
        {
            // Проверяем, выбрана ли директория для сохранения
            if (string.IsNullOrEmpty(selectedDirectoryPath))
            {
                MessageBox.Show("Сначала выберите директорию для сохранения файлов!");
                return;
            }

            // Получаем путь для сканирования из textBox3
            string sourceFolderPath = textBox3.Text;

            // Проверяем, выбран ли путь для сканирования
            if (string.IsNullOrEmpty(sourceFolderPath))
            {
                MessageBox.Show("Сначала выберите папку для сканирования!");
                return;
            }

            // Копируем файлы
            CopyFilesByExtension(sourceFolderPath, selectedDirectoryPath);
        }
        private void CopyFilesByExtension(string sourceFolderPath, string targetFolderPath)
        {
            // Получаем все файлы в исходной папке
            string[] files = Directory.GetFiles(sourceFolderPath, "*.*", SearchOption.AllDirectories);

            // Сбрасываем значение ProgressBar и устанавливаем Maximum
            progressBar1.Value = 0;
            progressBar1.Maximum = files.Length;

            int filesCopied = 0;
            string message = "";

            // Обрабатываем каждый файл
            foreach (string filePath in files)
            {
                string fileName = Path.GetFileName(filePath);
                string extension = Path.GetExtension(filePath).ToLower().TrimStart('.');

                // Создаем папку для файлов с таким расширением, если ее еще нет
                string targetExtensionFolderPath = Path.Combine(targetFolderPath, extension);
                Directory.CreateDirectory(targetExtensionFolderPath);

                // Формируем путь назначения
                string targetFilePath = Path.Combine(targetExtensionFolderPath, fileName);

                // Обработка дубликатов имен файлов
                int counter = 1;
                while (File.Exists(targetFilePath))
                {
                    string baseFileName = Path.GetFileNameWithoutExtension(fileName);
                    string newFileName = $"{baseFileName}_{counter}{Path.GetExtension(fileName)}";
                    targetFilePath = Path.Combine(targetExtensionFolderPath, newFileName);
                    counter++;
                }

                try
                {
                    // Копируем файл
                    File.Copy(filePath, targetFilePath, true);
                    filesCopied++;

                    // Обновляем ProgressBar
                    progressBar1.Value++;

                    // Обновляем текст ProgressBar (опционально)
                    progressBar1.Invalidate();
                    progressBar1.Update();
                    int percent = (int)(((double)progressBar1.Value / progressBar1.Maximum) * 100);
                    progressBar1.CreateGraphics().DrawString(percent.ToString() + "%",
                        new Font("Arial", 8.25f, FontStyle.Regular), Brushes.Black,
                        new PointF(progressBar1.Width / 2 - 10, progressBar1.Height / 2 - 7));

                    // Формируем сообщение
                    if (counter > 1)
                    {
                        message += $"Файл '{fileName}' уже существует.\nФайл переименован в '{Path.GetFileName(targetFilePath)}' и скопирован.\n\n";
                    }
                    else
                    {
                        message += $"Файл '{fileName}' скопирован.\n\n";
                    }
                }
                catch (Exception ex)
                {
                    message += $"Ошибка при копировании файла '{fileName}': {ex.Message}\n\n";
                }
            }

            message += $"\nСкопировано файлов: {filesCopied}";
            MessageBox.Show(message);
        }

        // Метод GitHub ---------------------------------------------------------------------------------------------------------------------------------------------

        private async void LoadRepositoryButton_Click(object sender, EventArgs e)
        {
            try
            {
                // Создание клиента GitHub
                var productHeaderValue = new Octokit.ProductHeaderValue("WinBotSearch");
                var client = new GitHubClient(productHeaderValue)
                {
                    Credentials = new Credentials(GitHubToken)
                };

                // Получение содержимого репозитория
                var contents = await client.Repository.Content.GetAllContents(GitHubUsername, GitHubRepository);

                // Очистка предыдущих данных
                RepoContentsTreeView.Nodes.Clear();

                // Отображение содержимого в TreeView
                foreach (var content in contents)
                {
                    var node = new TreeNode(content.Name);
                    node.Tag = content;
                    RepoContentsTreeView.Nodes.Add(node);

                    // Рекурсивно загружаем папки
                    if (content.Type == ContentType.Dir)
                    {
                        await LoadDirectoryContents(node, content.Path);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }


        // Рекурсивная загрузка содержимого папок
        private async Task LoadDirectoryContents(TreeNode parentNode, string path)
        {
            // Используем Octokit.ProductHeaderValue
            var productHeaderValue = new Octokit.ProductHeaderValue("WinBotSearch");
            var client = new GitHubClient(productHeaderValue);
            client.Credentials = new Credentials(GitHubToken);

            var contents = await client.Repository.Content.GetAllContentsByRef(GitHubUsername, GitHubRepository, path, "main");

            foreach (var content in contents)
            {
                var node = new TreeNode(content.Name);

                // Проверяем, что это файл
                if (content.Type == ContentType.File)
                {
                    // ИСПРАВЛЕНИЕ: Используем content.HtmlUrl вместо content.DownloadUrl
                    string url = content.HtmlUrl;

                    // Устанавливаем URL-адрес в Tag узла
                    node.Tag = content.DownloadUrl;
                }

                parentNode.Nodes.Add(node);

                if (content.Type == ContentType.Dir)
                {
                    await LoadDirectoryContents(node, content.Path);
                }
            }
        }

        private async void RepoContentsTreeView_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Node.Tag != null && e.Node.Tag is string downloadUrl)
            {
                try
                {
                    // 1. Извлекаем имя файла без параметров запроса
                    string fileName = Path.GetFileNameWithoutExtension(e.Node.Text); // Используем e.Node.Text для имени

                    // 2. Получаем расширение файла
                    string fileExtension = Path.GetExtension(e.Node.Text); // Используем e.Node.Text для расширения

                    // 3. Формируем корректное имя файла
                    fileName = $"{fileName}{fileExtension}";

                    // 4. Формируем путь к папке "GitHub" в вашей директории
                    string gitHubFolderPath = Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                        "BAZA",
                        "GitHub"
                    );

                    // 5. Создаем папку "GitHub", если ее нет
                    Directory.CreateDirectory(gitHubFolderPath);

                    // 6. Формируем полный путь к файлу для сохранения
                    string filePath = Path.Combine(gitHubFolderPath, fileName);

                    // 6.1 Проверяем, существует ли уже файл с таким именем
                    int counter = 1;
                    while (File.Exists(filePath))
                    {
                        // Если файл существует, добавляем _1 (или _2, _3 и т.д.) к имени
                        string newFileName = Path.GetFileNameWithoutExtension(fileName) + $"_{counter++}" + Path.GetExtension(fileName);
                        filePath = Path.Combine(gitHubFolderPath, newFileName);
                    }

                    // 7. Скачиваем файл с помощью HttpClient
                    using (var response = await _httpClient.GetAsync(new Uri(downloadUrl))) // Используем downloadUrl
                    {
                        response.EnsureSuccessStatusCode(); // Проверяем на успешный код ответа (2xx)

                        // Сохраняем содержимое ответа в файл
                        using (var fileStream = new FileStream(filePath, System.IO.FileMode.CreateNew))
                        {
                            await response.Content.CopyToAsync(fileStream);
                        }
                    }

                    // 8. Предлагаем открыть файл
                    DialogResult result = MessageBox.Show($"Файл успешно сохранен в:\n{filePath}\n\nОткрыть файл?", "Успех", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                    if (result == DialogResult.Yes)
                    {
                        Process.Start(new ProcessStartInfo(filePath) { UseShellExecute = true }); // Открываем файл
                    }
                }
                catch (Exception ex) // Ловим общее исключение для любых ошибок
                {
                    MessageBox.Show($"Ошибка при скачивании файла: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}
