namespace BAZA
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            button1 = new Button();
            textBox1 = new TextBox();
            button2 = new Button();
            progressBar1 = new ProgressBar();
            button3 = new Button();
            textBox2 = new TextBox();
            button4 = new Button();
            listBox1 = new ListBox();
            button5 = new Button();
            button6 = new Button();
            label1 = new Label();
            button7 = new Button();
            button8 = new Button();
            button9 = new Button();
            textBox3 = new TextBox();
            LoadRepositoryButton = new Button();
            RepoContentsTreeView = new TreeView();
            SuspendLayout();
            // 
            // button1
            // 
            button1.Location = new Point(12, 12);
            button1.Name = "button1";
            button1.Size = new Size(75, 23);
            button1.TabIndex = 0;
            button1.Text = "Путь";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click_1;
            // 
            // textBox1
            // 
            textBox1.Location = new Point(93, 12);
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(228, 23);
            textBox1.TabIndex = 1;
            // 
            // button2
            // 
            button2.Location = new Point(12, 41);
            button2.Name = "button2";
            button2.Size = new Size(75, 23);
            button2.TabIndex = 2;
            button2.Text = "Файл";
            button2.UseVisualStyleBackColor = true;
            button2.Click += button2_Click;
            // 
            // progressBar1
            // 
            progressBar1.Location = new Point(93, 41);
            progressBar1.Name = "progressBar1";
            progressBar1.Size = new Size(228, 23);
            progressBar1.TabIndex = 3;
            // 
            // button3
            // 
            button3.Location = new Point(12, 113);
            button3.Name = "button3";
            button3.Size = new Size(75, 23);
            button3.TabIndex = 4;
            button3.Text = "Скачать";
            button3.UseVisualStyleBackColor = true;
            button3.Click += button3_Click;
            // 
            // textBox2
            // 
            textBox2.Location = new Point(93, 113);
            textBox2.Name = "textBox2";
            textBox2.Size = new Size(228, 23);
            textBox2.TabIndex = 5;
            // 
            // button4
            // 
            button4.AllowDrop = true;
            button4.Location = new Point(12, 415);
            button4.Name = "button4";
            button4.Size = new Size(150, 23);
            button4.TabIndex = 6;
            button4.Text = "Выбранная директория";
            button4.UseVisualStyleBackColor = true;
            button4.Click += button4_Click;
            // 
            // listBox1
            // 
            listBox1.FormattingEnabled = true;
            listBox1.ItemHeight = 15;
            listBox1.Location = new Point(354, 12);
            listBox1.Name = "listBox1";
            listBox1.Size = new Size(434, 124);
            listBox1.TabIndex = 7;
            listBox1.SelectedIndexChanged += listBox1_SelectedIndexChanged;
            listBox1.DoubleClick += listBox1_DoubleClick;
            // 
            // button5
            // 
            button5.Location = new Point(168, 415);
            button5.Name = "button5";
            button5.Size = new Size(153, 23);
            button5.TabIndex = 8;
            button5.Text = "Директория программы";
            button5.UseVisualStyleBackColor = true;
            button5.Click += button5_Click;
            // 
            // button6
            // 
            button6.Location = new Point(667, 142);
            button6.Name = "button6";
            button6.Size = new Size(121, 23);
            button6.TabIndex = 9;
            button6.Text = "Очистить историю";
            button6.UseVisualStyleBackColor = true;
            button6.Click += button6_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(12, 73);
            label1.Name = "label1";
            label1.Size = new Size(209, 30);
            label1.TabIndex = 10;
            label1.Text = "Можно выбрать несколько файлов, \r\nтакже можно перетащить файлы\r\n";
            // 
            // button7
            // 
            button7.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 204);
            button7.Location = new Point(325, 113);
            button7.Name = "button7";
            button7.Size = new Size(23, 23);
            button7.TabIndex = 11;
            button7.Text = "?";
            button7.UseVisualStyleBackColor = true;
            button7.Click += button7_Click;
            // 
            // button8
            // 
            button8.Location = new Point(12, 194);
            button8.Name = "button8";
            button8.Size = new Size(86, 23);
            button8.TabIndex = 12;
            button8.Text = "Сканировать";
            button8.UseVisualStyleBackColor = true;
            button8.Click += button8_Click;
            // 
            // button9
            // 
            button9.Location = new Point(12, 165);
            button9.Name = "button9";
            button9.Size = new Size(86, 23);
            button9.TabIndex = 13;
            button9.Text = "Путь";
            button9.UseVisualStyleBackColor = true;
            button9.Click += button9_Click;
            // 
            // textBox3
            // 
            textBox3.Location = new Point(104, 165);
            textBox3.Name = "textBox3";
            textBox3.Size = new Size(217, 23);
            textBox3.TabIndex = 14;
            // 
            // LoadRepositoryButton
            // 
            LoadRepositoryButton.Location = new Point(667, 318);
            LoadRepositoryButton.Name = "LoadRepositoryButton";
            LoadRepositoryButton.Size = new Size(121, 23);
            LoadRepositoryButton.TabIndex = 15;
            LoadRepositoryButton.Text = "GitHub";
            LoadRepositoryButton.UseVisualStyleBackColor = true;
            LoadRepositoryButton.Click += LoadRepositoryButton_Click;
            // 
            // RepoContentsTreeView
            // 
            RepoContentsTreeView.Location = new Point(354, 171);
            RepoContentsTreeView.Name = "RepoContentsTreeView";
            RepoContentsTreeView.Size = new Size(434, 141);
            RepoContentsTreeView.TabIndex = 16;
            // 
            // Form1
            // 
            AllowDrop = true;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(RepoContentsTreeView);
            Controls.Add(LoadRepositoryButton);
            Controls.Add(textBox3);
            Controls.Add(button9);
            Controls.Add(button8);
            Controls.Add(button7);
            Controls.Add(label1);
            Controls.Add(button6);
            Controls.Add(button5);
            Controls.Add(listBox1);
            Controls.Add(button4);
            Controls.Add(textBox2);
            Controls.Add(button3);
            Controls.Add(progressBar1);
            Controls.Add(button2);
            Controls.Add(textBox1);
            Controls.Add(button1);
            Name = "Form1";
            Text = "Form1";
            Load += Form1_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button button1;
        private TextBox textBox1;
        private Button button2;
        private ProgressBar progressBar1;
        private Button button3;
        private TextBox textBox2;
        private Button button4;
        private ListBox listBox1;
        private Button button5;
        private Button button6;
        private Label label1;
        private Button button7;
        private Button button8;
        private Button button9;
        private TextBox textBox3;
        private Button LoadRepositoryButton;
        private TreeView RepoContentsTreeView;
    }
}
