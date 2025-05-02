using HospitalD.Страницы_админа;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace HospitalD
{
    public partial class AuthPage : Page
    {
        public AuthPage()
        {
            InitializeComponent();
        }
        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(usernameTextBox.Text) ||
                string.IsNullOrEmpty(passwordBox.Password))
            {
                MessageBox.Show("Введите логин и пароль!");
                return;
            }

            string passwordHash = GetHash(passwordBox.Password);

            using (var db = new Entities1())
            {
                var user = db.Users
                    .AsNoTracking()
                    .FirstOrDefault(u => u.Username == usernameTextBox.Text &&
                                       u.Password == passwordHash);

                if (user == null)
                {
                    MessageBox.Show("Пользователь не найден!");
                    return;
                }

                Page newWindow;
                switch (user.ID_Role)
                {
                    case 1: // Администратор
                        newWindow = new AdminPage(user);
                        break;
                    case 2: // Сотрудник
                        newWindow = new EmployeePage(user);
                        break;
                    case 3: // Пациент
                        newWindow = new PatientPage(user);
                        break;
                    default:
                        MessageBox.Show("Неизвестная роль пользователя!");
                        return;
                }

                NavigationService.Navigate(newWindow);
            }
        }

        private void NavigateToReg_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new RegPage());
        }

        public static string GetHash(string password)
        {
            using (var hash = SHA1.Create())
            {
                return string.Concat(hash
                    .ComputeHash(Encoding.UTF8.GetBytes(password))
                    .Select(x => x.ToString("X2")));
            }
        }
    }
}