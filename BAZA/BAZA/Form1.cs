using System.Diagnostics.Metrics;
using System.Diagnostics;
using System.Net;
using System.Configuration;
using System.Collections.Specialized;
using System.Windows.Forms;
using Octokit;
using System.IO;
using System.Threading.Tasks;
using FormsApplication = System.Windows.Forms.Application; // ��������� ��� Application


namespace BAZA
{
    public partial class Form1 : Form
    {

        // ���� � ��������� ����������
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

            //button1.Enabled = false; // ��������� ������ �� ���������
            //button4.Enabled = false; // ��������� ������ �� ���������
            //button5.Enabled = false; // ��������� ������ �� ���������

            // ���������� ���������� ������� NodeMouseDoubleClick
            RepoContentsTreeView.NodeMouseDoubleClick += RepoContentsTreeView_NodeMouseDoubleClick;

            _httpClient.DefaultRequestHeaders.Add("Authorization", $"token {GitHubToken}");

        }

        // ����� ��� ��������� ����������� ������ button1
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

        // ���� � ����� � ��������� ����������� (���������� historyFilePath)
        private string selectedDirectoryFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "BAZA", "selected_directory.txt");

        private List<string> fileHistory = new List<string>(); // ������ ��� �������� �������
        private string historyFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "BAZA", "history.txt"); // ���� � ����� �������

        private void Form1_Load(object sender, EventArgs e)
        {
            // ��������� ����������� ���� �� ��������
            selectedDirectoryPath = Properties.Settings.Default.SelectedDirectory;
            textBox1.Text = selectedDirectoryPath;

            // ��������� ������ ������� �� ��������
            string historyString = Properties.Settings.Default.FileHistory;

            // ���� ������� �������, ��������� ������ �� ������ � ��������� � ListBox
            if (!string.IsNullOrEmpty(historyString))
            {
                fileHistory = new List<string>(historyString.Split('|'));
                UpdateFileHistoryListBox();
            }

            LoadHistoryFromFile(); // ��������� ������� �� �����

            // ��������� ����������� ���� � ���������� �� �����
            selectedDirectoryPath = LoadSelectedDirectoryFromFile();
            textBox1.Text = selectedDirectoryPath;
        }

        // ����� ���������� ---------------------------------------------------------------------------------------------------------------------------------------------

        private void button1_Click_1(object sender, EventArgs e)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    selectedDirectoryPath = dialog.SelectedPath;

                    // �������� ��������� ���� � ����� SaveSelectedDirectory
                    SaveSelectedDirectory(selectedDirectoryPath);

                    // ������� ���� � textBox1
                    textBox1.Text = selectedDirectoryPath;
                }
            }
        }


        // ����� ��� ���������� ��������� ���������� � ����
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
            return string.Empty; // ���������� ������ ������, ���� ���� �� ������
        }


        // ����� ������� ---------------------------------------------------------------------------------------------------------------------------------------------

        // ����� ��� ���������� ������ � �������, ���������� ListBox � ���������� � ����
        private void AddFileToHistory(string filePath, string prefix = "����������:")
        {
            // ��������� �������, ���� �� �������
            string historyEntry = string.IsNullOrEmpty(prefix) ? filePath : $"{prefix} {filePath}";

            fileHistory.Add(historyEntry);
            UpdateFileHistoryListBox();
            SaveHistoryToFile();
        }


        // ����� ��� ���������� ListBox � �������� ������
        private void UpdateFileHistoryListBox()
        {
            // ������� ListBox ����� ����������� ���������
            listBox1.Items.Clear();

            // ��������� ��� �������� �� fileHistory � ListBox
            foreach (string item in fileHistory)
            {
                listBox1.Items.Add(item);
            }
        }


        // ����� ��� �������� ������� �� �����
        private void LoadHistoryFromFile()
        {
            // ������� ����� "BAZA", ���� ��� �� ����������
            string historyFolderPath = Path.GetDirectoryName(historyFilePath);
            if (!Directory.Exists(historyFolderPath))
            {
                Directory.CreateDirectory(historyFolderPath);
            }

            // ���������, ���������� �� ����, � ���� ���, �� ������� ���
            if (!File.Exists(historyFilePath))
            {
                File.Create(historyFilePath).Close(); // ������� ���� � ����� ��������� ���
            }

            // ������ ���� �������������� ����������, ����� ������
            fileHistory = new List<string>(File.ReadAllLines(historyFilePath));
            UpdateFileHistoryListBox();
        }

        // ����� ��� ���������� ������� � ����
        private void SaveHistoryToFile()
        {
            File.WriteAllLines(historyFilePath, fileHistory);
        }

        // ����� ����������� ---------------------------------------------------------------------------------------------------------------------------------------------

        private void button2_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(selectedDirectoryPath))
            {
                MessageBox.Show("������� �������� ����������!");
                return;
            }

            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "��� ����� (*.*)|*.*";
                openFileDialog.Multiselect = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string message = "";
                    string sourceFilePath = "";
                    int filesCopied = 0; // ��������� ������� ������������� ������

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
                            message += $"���� � ������ '{fileName}' ��� ����������.\n���� ������������ � '{Path.GetFileName(destinationFilePath)}' � ���������� � ����� '{extension}'.\n\n";
                        }
                        else
                        {
                            message += $"���� '{Path.GetFileName(destinationFilePath)}' ���������� � ����� '{extension}'.\n\n";
                        }
                        filesCopied++; // ����������� ������� ����� ����������� �����
                    }

                    // ��������� ���������� � ���������� ������ � ���������
                    message += $"\n����������� ������: {filesCopied}";

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
                    // �������� ����� ��� ��������� ����� 
                    ProcessDroppedFile(droppedFile, ref message, ref filesCopied);
                }

                MessageBox.Show(message + $"\n����������� ������: {filesCopied}");
            }
        }

        private void ProcessDroppedFile(string sourceFilePath, ref string message, ref int filesCopied)
        {
            if (string.IsNullOrEmpty(selectedDirectoryPath))
            {
                MessageBox.Show("������� �������� ����������!");
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
                message += $"���� � ������ '{fileName}' ��� ����������.\n���� ������������ � '{Path.GetFileName(destinationFilePath)}' � ���������� � ����� '{extension}'.\n\n";
            }
            else
            {
                message += $"���� '{Path.GetFileName(destinationFilePath)}' ���������� � ����� '{extension}'.\n\n";
            }

            filesCopied++;
        }



        // ����� ��� ����������� ����� � ������������ ���������
        private void CopyFileWithProgress(string sourceFile, string destFile, ProgressBar progressBar1)
        {
            // ����������� ProgressBar
            progressBar1.Minimum = 0;
            progressBar1.Maximum = 100;
            progressBar1.Value = 0;

            // ��������� �����
            using (FileStream sourceStream = new FileStream(sourceFile, System.IO.FileMode.Open, System.IO.FileAccess.Read))
            {
                using (FileStream destStream = new FileStream(destFile, System.IO.FileMode.CreateNew, System.IO.FileAccess.Write))
                {
                    // ������ ������ ��� ����������� (����� ������)
                    byte[] buffer = new byte[4096];
                    int bytesRead;

                    // �������� ���� �� ������
                    while ((bytesRead = sourceStream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        destStream.Write(buffer, 0, bytesRead);

                        // ��������� ProgressBar
                        int progress = (int)((double)destStream.Position / sourceStream.Length * 100);
                        progressBar1.Value = progress;
                        FormsApplication.DoEvents();  // ��������� ���������
                    }
                }
            }
        }

        // ����� ���������� ---------------------------------------------------------------------------------------------------------------------------------------------

        private string downloadFilePath;
        private string downloadExtension;
        private int downloadCounter;
        private void button3_Click(object sender, EventArgs e)
        {
            string url = textBox2.Text.Trim();

            if (string.IsNullOrEmpty(url))
            {
                MessageBox.Show("������� URL-����� ����� � TextBox2.");
                return;
            }

            if (string.IsNullOrEmpty(selectedDirectoryPath))
            {
                MessageBox.Show("������� �������� ����������!");
                return;
            }

            try
            {
                // �������� ��� ����� �� URL
                string fileName = GetFileNameFromUrl(url);

                // ������� ����� ��� ���������� ����� (���� �� ��� ���)
                string extension = Path.GetExtension(fileName).ToLower().TrimStart('.');
                string folderPath = Path.Combine(selectedDirectoryPath, extension);
                Directory.CreateDirectory(folderPath);

                // ��������� ���� ��� ���������� �����
                string filePath = Path.Combine(folderPath, fileName);

                // ���������, ���������� �� ��� ���� � ����� ������
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

                // ��������� ���� � ������� WebClient
                using (WebClient client = new WebClient())
                {
                    // ������������� �� ������� ��������� ��������
                    client.DownloadFileCompleted += Client_DownloadFileCompleted;

                    // ������������� �� ������� �������� ���������
                    client.DownloadProgressChanged += Client_DownloadProgressChanged;

                    // ��������� ���� ����������, ��������� URI � UserState
                    client.DownloadFileAsync(new Uri(url), filePath, new Uri(url));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"������ ��� ���������� �����: {ex.Message}");
            }
        }

        // ���������� ������� ��������� ��������
        private void Client_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                // ��������� ������ ��������
                MessageBox.Show($"������ ��� ���������� �����: {e.Error.Message}");
            }
            else
            {
                // ���� ������� ������
                string fileName = Path.GetFileName(((Uri)e.UserState).LocalPath); // �������� ��� �����

                // ��������� ���� � ������� � �������� "�������"
                AddFileToHistory(downloadFilePath, "�������:");

                // ��������� � ������ ��������� �����
                string message = downloadCounter > 1
                    ? $"���� � ������ '{fileName}' ��� ����������.\n" +
                      $"���� ������������ � '{Path.GetFileName(downloadFilePath)}' � �������� � ����� '{downloadExtension}'.\n" +
                      $"������� ��������� ����?"
                    : $"���� '{fileName}' ������� �������� � ����� '{downloadExtension}'.\n" +
                      $"������� ��������� ����?";

                // ���������� ��������� ������� ������������
                DialogResult result = MessageBox.Show(message, "�������� ���������", MessageBoxButtons.YesNo);

                // ��������� ����, ���� ������������ ����� "��"
                if (result == DialogResult.Yes)
                {
                    Process.Start(new ProcessStartInfo(downloadFilePath) { UseShellExecute = true });
                }
            }
        }


        // ���������� ������� �������� ���������
        private void Client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            // ��������� ProgressBar
            progressBar1.Value = e.ProgressPercentage;
        }

        // ����� ��� ��������� ����� ����� �� URL
        private string GetFileNameFromUrl(string url)
        {
            Uri uri = new Uri(url);
            string fileName = Path.GetFileName(uri.LocalPath);
            return fileName;
        }

        // ����� ���������� ���������� ---------------------------------------------------------------------------------------------------------------------------------------------

        private void button4_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(selectedDirectoryPath))
            {
                MessageBox.Show("������� �������� ����������!");
                return;
            }

            try
            {
                // ��������� ��������� ����������
                Process.Start(new ProcessStartInfo(selectedDirectoryPath) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"������ ��� �������� ����������: {ex.Message}");
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            // �������� ���� � ����� "��� ���������"
            string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            // ��������� ������ ���� � ����� "BAZA" � ����������
            string bazaFolderPath = Path.Combine(documentsPath, "BAZA");

            // ��������� ����� "BAZA" � ������� ���������� Windows
            Process.Start("explorer.exe", bazaFolderPath);
        }


        // ����� ������� � ����������� ������� ---------------------------------------------------------------------------------------------------------------------------------------------

        private void button6_Click(object sender, EventArgs e)
        {
            try
            {
                // �������� ������� ���� � ������� "yyyy-MM-dd"
                string currentDate = DateTime.Now.ToString("yyyy-MM-dd");

                // ���� � ����� "saved-history"
                string savedHistoryFolderPath = Path.Combine(
                    Path.GetDirectoryName(historyFilePath),
                    "saved-history"
                );

                // ������� �����, ���� ��� �� ����������
                Directory.CreateDirectory(savedHistoryFolderPath);

                // ��������� ������� ��� ����� � �����
                string baseFileName = Path.Combine(
                    savedHistoryFolderPath,
                    Path.GetFileNameWithoutExtension(historyFilePath) + "_" + currentDate
                );

                // ���������, ���������� �� ��� ���� � ����� ������
                string newFilePath = baseFileName + Path.GetExtension(historyFilePath);
                int counter = 1;
                while (File.Exists(newFilePath))
                {
                    // ���� ���� ����������, ��������� � ����� _1, _2 � �.�.
                    newFilePath = $"{baseFileName}_{counter}{Path.GetExtension(historyFilePath)}";
                    counter++;
                }

                // �������� ���� � �����������, ���� �� ��� ����������
                File.Copy(historyFilePath, newFilePath, true);

                // ��������� ��������� �� �������� �����������
                string message = $"���� ������� ���������� �:\n{newFilePath}";

                // ��������� ���������� � ��������������, ���� ���� ��� ������������
                if (counter > 1)
                {
                    message += $"\n\n���� ��� ������������, ��� ��� ���� � ����� ������ ��� ����������.";
                }

                // ������� �������� ���� �������
                File.WriteAllText(historyFilePath, string.Empty);

                // ������� ������ ������� � ListBox
                fileHistory.Clear();
                listBox1.Items.Clear();

                MessageBox.Show(message, "�����", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"������ ��� ����������� �����:\n{ex.Message}", "������", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

                // ��������� ���� � �����, ��������� �������
                string selectedFilePath = selectedItem.StartsWith("�������: ")
                    ? selectedItem.Substring("�������: ".Length)
                    : selectedItem;

                // ���������, ���������� �� ����
                if (File.Exists(selectedFilePath))
                {
                    // ��������� ���� � ���������� �� ���������
                    Process.Start(new ProcessStartInfo(selectedFilePath) { UseShellExecute = true });
                }
                else
                {
                    MessageBox.Show("���� �� ������.", "������", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            MessageBox.Show("�������� ������ ��� ���������� � ������� � GitHub ��� Giphy.\n�����, ����� ������ � ����� ����� ��� ���� � �����������. ������:\n\nGithub:\nhttps://so1ta.github.io/Images/USP.jpg\n\n���������, ������:\nhttps://clck.ru/3AwRYD (����������� ������)\n\nGiphy:\nhttps://i.giphy.com/media/v1.Y2lkPTc5MGI3NjExOTJzYTgycXdiODFicjd0NWJwandlNWoxODMzMDF0OWoxcXh0cmhscCZlcD12MV9pbnRlcm5hbF9naWZfYnlfaWQmY3Q9Zw/MDJ9IbxxvDUQM/giphy.gif");
        }

        private void button9_Click(object sender, EventArgs e)
        {
            // ������� ���������� ���� ������ �����
            using (var folderBrowserDialog = new FolderBrowserDialog())
            {
                // ������������� ��������� ����
                folderBrowserDialog.Description = "�������� �����:";

                // ���������� ���� � ��������� ���������
                DialogResult result = folderBrowserDialog.ShowDialog();

                // ���� ������������ ������ ����� � ����� "��"
                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(folderBrowserDialog.SelectedPath))
                {
                    // �������� ��������� ����
                    string selectedPath = folderBrowserDialog.SelectedPath;

                    // ������������� ����� textBox3 ������ ���������� ����
                    textBox3.Text = selectedPath;
                }
            }
        }

        private void button8_Click(object sender, EventArgs e)
        {
            // ���������, ������� �� ���������� ��� ����������
            if (string.IsNullOrEmpty(selectedDirectoryPath))
            {
                MessageBox.Show("������� �������� ���������� ��� ���������� ������!");
                return;
            }

            // �������� ���� ��� ������������ �� textBox3
            string sourceFolderPath = textBox3.Text;

            // ���������, ������ �� ���� ��� ������������
            if (string.IsNullOrEmpty(sourceFolderPath))
            {
                MessageBox.Show("������� �������� ����� ��� ������������!");
                return;
            }

            // �������� �����
            CopyFilesByExtension(sourceFolderPath, selectedDirectoryPath);
        }
        private void CopyFilesByExtension(string sourceFolderPath, string targetFolderPath)
        {
            // �������� ��� ����� � �������� �����
            string[] files = Directory.GetFiles(sourceFolderPath, "*.*", SearchOption.AllDirectories);

            // ���������� �������� ProgressBar � ������������� Maximum
            progressBar1.Value = 0;
            progressBar1.Maximum = files.Length;

            int filesCopied = 0;
            string message = "";

            // ������������ ������ ����
            foreach (string filePath in files)
            {
                string fileName = Path.GetFileName(filePath);
                string extension = Path.GetExtension(filePath).ToLower().TrimStart('.');

                // ������� ����� ��� ������ � ����� �����������, ���� �� ��� ���
                string targetExtensionFolderPath = Path.Combine(targetFolderPath, extension);
                Directory.CreateDirectory(targetExtensionFolderPath);

                // ��������� ���� ����������
                string targetFilePath = Path.Combine(targetExtensionFolderPath, fileName);

                // ��������� ���������� ���� ������
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
                    // �������� ����
                    File.Copy(filePath, targetFilePath, true);
                    filesCopied++;

                    // ��������� ProgressBar
                    progressBar1.Value++;

                    // ��������� ����� ProgressBar (�����������)
                    progressBar1.Invalidate();
                    progressBar1.Update();
                    int percent = (int)(((double)progressBar1.Value / progressBar1.Maximum) * 100);
                    progressBar1.CreateGraphics().DrawString(percent.ToString() + "%",
                        new Font("Arial", 8.25f, FontStyle.Regular), Brushes.Black,
                        new PointF(progressBar1.Width / 2 - 10, progressBar1.Height / 2 - 7));

                    // ��������� ���������
                    if (counter > 1)
                    {
                        message += $"���� '{fileName}' ��� ����������.\n���� ������������ � '{Path.GetFileName(targetFilePath)}' � ����������.\n\n";
                    }
                    else
                    {
                        message += $"���� '{fileName}' ����������.\n\n";
                    }
                }
                catch (Exception ex)
                {
                    message += $"������ ��� ����������� ����� '{fileName}': {ex.Message}\n\n";
                }
            }

            message += $"\n����������� ������: {filesCopied}";
            MessageBox.Show(message);
        }

        // ����� GitHub ---------------------------------------------------------------------------------------------------------------------------------------------

        private async void LoadRepositoryButton_Click(object sender, EventArgs e)
        {
            try
            {
                // �������� ������� GitHub
                var productHeaderValue = new Octokit.ProductHeaderValue("WinBotSearch");
                var client = new GitHubClient(productHeaderValue)
                {
                    Credentials = new Credentials(GitHubToken)
                };

                // ��������� ����������� �����������
                var contents = await client.Repository.Content.GetAllContents(GitHubUsername, GitHubRepository);

                // ������� ���������� ������
                RepoContentsTreeView.Nodes.Clear();

                // ����������� ����������� � TreeView
                foreach (var content in contents)
                {
                    var node = new TreeNode(content.Name);
                    node.Tag = content;
                    RepoContentsTreeView.Nodes.Add(node);

                    // ���������� ��������� �����
                    if (content.Type == ContentType.Dir)
                    {
                        await LoadDirectoryContents(node, content.Path);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"������: {ex.Message}");
            }
        }


        // ����������� �������� ����������� �����
        private async Task LoadDirectoryContents(TreeNode parentNode, string path)
        {
            // ���������� Octokit.ProductHeaderValue
            var productHeaderValue = new Octokit.ProductHeaderValue("WinBotSearch");
            var client = new GitHubClient(productHeaderValue);
            client.Credentials = new Credentials(GitHubToken);

            var contents = await client.Repository.Content.GetAllContentsByRef(GitHubUsername, GitHubRepository, path, "main");

            foreach (var content in contents)
            {
                var node = new TreeNode(content.Name);

                // ���������, ��� ��� ����
                if (content.Type == ContentType.File)
                {
                    // �����������: ���������� content.HtmlUrl ������ content.DownloadUrl
                    string url = content.HtmlUrl;

                    // ������������� URL-����� � Tag ����
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
                    // 1. ��������� ��� ����� ��� ���������� �������
                    string fileName = Path.GetFileNameWithoutExtension(e.Node.Text); // ���������� e.Node.Text ��� �����

                    // 2. �������� ���������� �����
                    string fileExtension = Path.GetExtension(e.Node.Text); // ���������� e.Node.Text ��� ����������

                    // 3. ��������� ���������� ��� �����
                    fileName = $"{fileName}{fileExtension}";

                    // 4. ��������� ���� � ����� "GitHub" � ����� ����������
                    string gitHubFolderPath = Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                        "BAZA",
                        "GitHub"
                    );

                    // 5. ������� ����� "GitHub", ���� �� ���
                    Directory.CreateDirectory(gitHubFolderPath);

                    // 6. ��������� ������ ���� � ����� ��� ����������
                    string filePath = Path.Combine(gitHubFolderPath, fileName);

                    // 6.1 ���������, ���������� �� ��� ���� � ����� ������
                    int counter = 1;
                    while (File.Exists(filePath))
                    {
                        // ���� ���� ����������, ��������� _1 (��� _2, _3 � �.�.) � �����
                        string newFileName = Path.GetFileNameWithoutExtension(fileName) + $"_{counter++}" + Path.GetExtension(fileName);
                        filePath = Path.Combine(gitHubFolderPath, newFileName);
                    }

                    // 7. ��������� ���� � ������� HttpClient
                    using (var response = await _httpClient.GetAsync(new Uri(downloadUrl))) // ���������� downloadUrl
                    {
                        response.EnsureSuccessStatusCode(); // ��������� �� �������� ��� ������ (2xx)

                        // ��������� ���������� ������ � ����
                        using (var fileStream = new FileStream(filePath, System.IO.FileMode.CreateNew))
                        {
                            await response.Content.CopyToAsync(fileStream);
                        }
                    }

                    // 8. ���������� ������� ����
                    DialogResult result = MessageBox.Show($"���� ������� �������� �:\n{filePath}\n\n������� ����?", "�����", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                    if (result == DialogResult.Yes)
                    {
                        Process.Start(new ProcessStartInfo(filePath) { UseShellExecute = true }); // ��������� ����
                    }
                }
                catch (Exception ex) // ����� ����� ���������� ��� ����� ������
                {
                    MessageBox.Show($"������ ��� ���������� �����: {ex.Message}", "������", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}
