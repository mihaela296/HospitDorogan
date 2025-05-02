using System;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace HospitalD
{
    public partial class EmployeeMedicalRecordsPage : Page
    {
        private readonly Entities1 _db = new Entities1();
        private Staff _currentStaff;
        private Frame _mainFrame;

        public EmployeeMedicalRecordsPage(Staff staff, Frame mainFrame = null)
        {
            InitializeComponent();
            _currentStaff = staff;
            _mainFrame = mainFrame ?? NavigationService?.Content as Frame;
            LoadPatients();
            LoadMedicalRecords();
        }

        private void LoadPatients()
        {
            var patients = _db.Patients.ToList();
            PatientComboBox.ItemsSource = patients;
        }

        private void LoadMedicalRecords()
        {
            try
            {
                _db.Configuration.LazyLoadingEnabled = false;

                var records = _db.PatientMedicalRecords
                    .Include(r => r.Patient)
                    .Include(r => r.Diagnosis)
                    .Include(r => r.MedicalProcedure)
                    .Include(r => r.Medication)
                    .Include(r => r.Staff)
                    .OrderByDescending(r => r.RecordDate)
                    .ToList();

                MedicalRecordsDataGrid.ItemsSource = records;
                MedicalRecordsDataGrid.CanUserAddRows = false; // Отключаем пустую строку

                if (!records.Any())
                {
                    MessageBox.Show("Записи не найдены");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки записей: {ex.Message}");
            }
        }

        private void ApplyFilter_Click(object sender, RoutedEventArgs e)
        {
            var patientId = (int?)PatientComboBox.SelectedValue;
            var fromDate = DateFromFilter.SelectedDate;
            var toDate = DateToFilter.SelectedDate;

            var records = _db.PatientMedicalRecords
                .Include(r => r.Patient)
                .Include(r => r.Diagnosis)
                .Include(r => r.MedicalProcedure)
                .Include(r => r.Medication)
                .Include(r => r.Staff)
                .AsQueryable();

            if (patientId.HasValue)
                records = records.Where(r => r.ID_Patient == patientId);

            if (fromDate.HasValue)
                records = records.Where(r => r.RecordDate >= fromDate);

            if (toDate.HasValue)
                records = records.Where(r => r.RecordDate <= toDate);

            MedicalRecordsDataGrid.ItemsSource = records
                .OrderByDescending(r => r.RecordDate)
                .ToList();
        }

        private void ResetFilter_Click(object sender, RoutedEventArgs e)
        {
            PatientComboBox.SelectedItem = null;
            DateFromFilter.SelectedDate = null;
            DateToFilter.SelectedDate = null;
            LoadMedicalRecords();
        }

        private void EditRecord_Click(object sender, RoutedEventArgs e)
        {
            if (MedicalRecordsDataGrid.SelectedItem is PatientMedicalRecord record)
            {
                if (_mainFrame != null)
                {
                    _mainFrame.Navigate(new EditMedicalRecordPage(record, _currentStaff));
                }
                else
                {
                    NavigationService?.Navigate(new EditMedicalRecordPage(record, _currentStaff));
                }
            }
        }

        public EmployeeMedicalRecordsPage(Staff staff, Patient patient, Frame mainFrame = null)
        {
            InitializeComponent();
            _currentStaff = staff;
            _mainFrame = mainFrame ?? NavigationService?.Content as Frame;
            LoadPatients();
            LoadMedicalRecords();

            if (patient != null)
            {
                PatientComboBox.SelectedValue = patient.ID_Patient;
                ApplyFilter_Click(null, null);
            }
        }
    }
}