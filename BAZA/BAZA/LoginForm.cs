using Microsoft.VisualBasic.ApplicationServices;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BAZA
{
    public partial class LoginForm : Form
    {
        public LoginForm()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string login = textBox1.Text;
            string password = textBox2.Text;

            // Проверка учетных данных (замените на свою логику)
            User user = AuthenticateUser(login, password);

            if (user != null)
            {
                // Успешная авторизация
                this.Hide(); // Скрываем форму авторизации

                Form1 mainForm = new Form1();
                mainForm.SetButton1Enabled(user.IsAdmin); // Устанавливаем доступность кнопки на главной форме
                mainForm.SetTextbox1ReadOnly(!user.IsAdmin); // Запрещаем изменение, если не админ
                mainForm.Show();
            }
            else
            {
                MessageBox.Show("Неверный логин или пароль!");
            }
        }
        // Метод для проверки учетных данных
        private User AuthenticateUser(string login, string password)
        {
            // Учетные данные администратора
            string adminLogin = "admin";
            string adminPassword = "admin";

            // Учетные данные пользователя
            string userLogin = "user";
            string userPassword = "user";

            // Проверка учетных данных
            if (login == adminLogin && password == adminPassword)
            {
                return new User { Login = adminLogin, Password = adminPassword, IsAdmin = true };
            }
            else if (login == userLogin && password == userPassword)
            {
                return new User { Login = userLogin, Password = userPassword, IsAdmin = false };
            }
            else
            {
                return null; // Неверный логин или пароль
            }
        }

    }
}
