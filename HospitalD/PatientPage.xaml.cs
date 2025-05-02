using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace HospitalD
{
    public partial class PatientPage : Page
    {
        private User _currentUser;
        private Patient _currentPatient;

        public PatientPage(User user)
        {
            InitializeComponent();
            _currentUser = user;
            LoadPatientData();
        }

        private void LoadPatientData()
        {
            try
            {
                using (var context = new Entities1())
                {
                    // Находим пациента по ID_User
                    _currentPatient = context.Patients
                        .FirstOrDefault(p => p.ID_User == _currentUser.ID_User);

                    if (_currentPatient == null)
                    {
                        MessageBox.Show("Данные пациента не найдены!");
                    }
                    else
                    {
                        // Можно обновить UI с данными пациента
                        // PatientNameTextBlock.Text = _currentPatient.FullName;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных пациента: {ex.Message}");
            }
        }

        private void MedicalRecord_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_currentPatient != null)
                {
                    MainFrame.Navigate(new PatientMedicalRecordPage(_currentPatient));
                }
                else
                {
                    MessageBox.Show("Не удалось загрузить данные пациента");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка открытия медицинской карты: {ex.Message}");
            }
        }

        private void Appointment_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_currentPatient != null)
                {
                    MainFrame.Navigate(new MakeAppointmentPage(_currentPatient));
                }
                else
                {
                    MessageBox.Show("Не удалось загрузить данные пациента");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка открытия формы записи: {ex.Message}");
            }
        }

        private void MyAppointments_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_currentPatient != null)
                {
                    MainFrame.Navigate(new PatientAppointmentsPage(_currentPatient));
                }
                else
                {
                    MessageBox.Show("Не удалось загрузить данные пациента");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка открытия списка записей: {ex.Message}");
            }
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var result = MessageBox.Show("Вы уверены, что хотите выйти?", "Подтверждение выхода",
                    MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    NavigationService?.Navigate(new AuthPage());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при выходе: {ex.Message}");
            }
        }
    }
}