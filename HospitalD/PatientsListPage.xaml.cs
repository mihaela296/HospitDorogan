using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace HospitalD
{
    public partial class PatientsListPage : Page
    {
        private readonly Entities1 _db = new Entities1();
        private Frame _mainFrame;

        public PatientsListPage(Frame mainFrame = null)
        {
            InitializeComponent();
            _mainFrame = mainFrame ?? NavigationService?.Content as Frame;
            LoadPatients();
        }

        private void LoadPatients()
        {
            _db.Patients.Load();
            PatientsDataGrid.ItemsSource = _db.Patients.Local;

            // Убираем возможность добавления новых строк
            PatientsDataGrid.CanUserAddRows = false;
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            var searchText = SearchTextBox.Text.ToLower();

            var patients = _db.Patients
                .Where(p => p.FullName.ToLower().Contains(searchText) ||
                           p.Phone.Contains(searchText))
                .ToList();

            PatientsDataGrid.ItemsSource = patients;
        }

        private void ResetSearch_Click(object sender, RoutedEventArgs e)
        {
            SearchTextBox.Text = string.Empty;
            LoadPatients();
        }

        private void ViewMedicalRecord_Click(object sender, RoutedEventArgs e)
        {
            if (PatientsDataGrid.SelectedItem is Patient patient)
            {
                if (_mainFrame != null)
                {
                    _mainFrame.Navigate(new EmployeeMedicalRecordsPage(null, patient, _mainFrame));
                }
                else
                {
                    NavigationService?.Navigate(new EmployeeMedicalRecordsPage(null, patient));
                }
            }
        }
    }
}