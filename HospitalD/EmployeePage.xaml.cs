using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace HospitalD
{
    public partial class EmployeePage : Page
    {
        private User _currentUser;
        private Staff _currentStaff;

        public EmployeePage(User user)
        {
            InitializeComponent();
            _currentUser = user;
            LoadStaffData();
        }

        private void LoadStaffData()
        {
            using (var context = new Entities1())
            {
                _currentStaff = context.Staffs
                        .FirstOrDefault(p => p.ID_User == _currentUser.ID_User);
            }
        }

        private void MedicalRecords_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new EmployeeMedicalRecordsPage(_currentStaff, MainFrame));
        }

        private void AssignTreatment_Click(object sender, RoutedEventArgs e)
        {
            // Явно указываем какой конструктор использовать
            MainFrame.Navigate(new AssignTreatmentPage(_currentStaff, (Patient)null));
        }

        private void Schedule_Click(object sender, RoutedEventArgs e)
        {
            if (_currentStaff == null)
            {
                MessageBox.Show("Данные сотрудника не загружены");
                return;
            }

            try
            {
                var schedulePage = new EmployeeSchedulePage(_currentStaff);
                MainFrame.Navigate(schedulePage);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка открытия расписания: {ex.Message}");
            }
        }

        private void Patients_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new PatientsListPage(MainFrame));
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Вы уверены, что хотите выйти?",
                "Подтверждение выхода",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                NavigationService?.Navigate(new AuthPage());
            }
        }
    }
}