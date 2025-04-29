using System;
using System.Data.Entity.Validation;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Security.Cryptography;
using System.Text;

namespace HospitalD
{
    public partial class AddEditPatientPage : Page
    {
        private Patient _currentPatient;
        private readonly Entities1 _context = new Entities1();

        public AddEditPatientPage(Patient selectedPatient = null)
        {
            InitializeComponent();
            _currentPatient = selectedPatient != null
                ? _context.Patients.Find(selectedPatient.ID_Patient)
                : new Patient();

            LoadPatientData();
        }

        private void LoadPatientData()
        {
            if (_currentPatient == null) return;

            TextBoxFullName.Text = _currentPatient.FullName;
            DatePickerBirthDate.SelectedDate = _currentPatient.BirthDate;
            TextBoxPhone.Text = _currentPatient.Phone;
            TextBoxAddress.Text = _currentPatient.Address;
        }

        private void SavePatient_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateInputs()) return;

            try
            {
                UpdatePatientData();

                if (_currentPatient.ID_Patient == 0)
                {
                    _context.Patients.Add(_currentPatient);
                }

                _context.SaveChanges();
                ShowSuccessMessage();
                NavigationService.GoBack();
            }
            catch (DbEntityValidationException ex)
            {
                ShowValidationErrors(ex);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении: {ex.InnerException?.Message ?? ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool ValidateInputs()
        {
            if (string.IsNullOrWhiteSpace(TextBoxFullName.Text) ||
                DatePickerBirthDate.SelectedDate == null ||
                string.IsNullOrWhiteSpace(TextBoxPhone.Text))
            {
                MessageBox.Show("Заполните все обязательные поля!", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            return true;
        }

        private void UpdatePatientData()
        {
            _currentPatient.FullName = TextBoxFullName.Text.Trim();
            _currentPatient.BirthDate = DatePickerBirthDate.SelectedDate.Value;
            _currentPatient.Phone = TextBoxPhone.Text.Trim();
            _currentPatient.Address = TextBoxAddress.Text?.Trim();
            _currentPatient.ID_Role = 3;
        }

        private void ShowSuccessMessage()
        {
            MessageBox.Show("Данные пациента успешно сохранены!",
                "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ShowValidationErrors(DbEntityValidationException ex)
        {
            var errorMessages = ex.EntityValidationErrors
                .SelectMany(x => x.ValidationErrors)
                .Select(x => $"{x.PropertyName}: {x.ErrorMessage}");

            MessageBox.Show("Ошибки валидации:\n" + string.Join("\n", errorMessages),
                "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private string GenerateTempPassword()
        {
            const string chars = "ABCDEFGHJKLMNOPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, 8)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private string GetHash(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            _context.Dispose();
            NavigationService.GoBack();
        }

    }
}