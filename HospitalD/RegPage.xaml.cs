using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Data.Entity.Validation; // Обязательно добавьте это пространство имен!

namespace HospitalD
{
    public partial class RegPage : Page
    {
        public RegPage()
        {
            InitializeComponent();
        }
        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            if (usernameRegTextBox.Text.Length == 0)
            {
                MessageBox.Show("Укажите логин!");
                return;
            }

            using (var db = new Entities1())
            {
                var existingUser = db.Users
                    .AsNoTracking()
                    .FirstOrDefault(u => u.Username == usernameRegTextBox.Text);

                if (existingUser != null)
                {
                    MessageBox.Show("Пользователь уже существует!");
                    return;
                }

                if (passwordRegBox.Password != confirmPasswordRegBox.Password)
                {
                    MessageBox.Show("Пароли не совпадают!");
                    return;
                }

                bool hasLetter = false;
                bool hasNumber = false;
                foreach (char c in passwordRegBox.Password)
                {
                    if (char.IsLetter(c)) hasLetter = true;
                    if (char.IsDigit(c)) hasNumber = true;
                }

                var phoneRegex = new Regex(@"^\+7\d{10}$");

                StringBuilder errors = new StringBuilder();

                if (passwordRegBox.Password.Length < 6)
                    errors.AppendLine("Пароль должен быть длиннее 6 символов");
                if (!phoneRegex.IsMatch(phoneNumberTextBox.Text))
                    errors.AppendLine("Номер телефона должен быть в формате +7XXXXXXXXXX");
                if (!hasLetter)
                    errors.AppendLine("Пароль должен содержать буквы");
                if (!hasNumber)
                    errors.AppendLine("Пароль должен содержать цифры");

                if (errors.Length > 0)
                {
                    MessageBox.Show(errors.ToString());
                    return;
                }

                var newPatient = new Patient
                {
                    FullName = fullNameTextBox.Text,
                    BirthDate = birthDatePicker.DisplayDate,
                    Phone = phoneNumberTextBox.Text,
                    Address = addressTextBox.Text,
                    ID_Role = 3
                };

                db.Patients.Add(newPatient);
                db.SaveChanges();

                var newUser = new User
                {
                    Username = usernameRegTextBox.Text,
                    Password = AuthPage.GetHash(passwordRegBox.Password),
                    ID_Role = 3,
                    ID_User = newPatient.ID_Patient
                };

                db.Users.Add(newUser);
                db.SaveChanges();

                MessageBox.Show("Регистрация успешна!");
                NavigationService.Navigate(new AuthPage());
            }
        }
        private void NavigateToAuth_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }    
    }
 }
