using System;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace HospitalD
{
    public partial class PatientMedicalRecordPage : Page
    {
        private readonly Entities1 _db = new Entities1();
        private Patient _patient;

        public PatientMedicalRecordPage(Patient patient)
        {
            InitializeComponent();
            _patient = patient;
            LoadMedicalRecords();
        }

        private void LoadMedicalRecords()
        {
            var records = _db.PatientMedicalRecords
                .Include(r => r.Diagnosis)
                .Include(r => r.MedicalProcedure)
                .Include(r => r.Medication)
                .Include(r => r.Staff)
                .Include(r => r.Department)
                .Where(r => r.ID_Patient == _patient.ID_Patient)
                .OrderByDescending(r => r.RecordDate)
                .ToList();

            MedicalRecordsDataGrid.ItemsSource = records;
        }

        private void ApplyFilter_Click(object sender, RoutedEventArgs e)
        {
            var fromDate = DateFromFilter.SelectedDate;
            var toDate = DateToFilter.SelectedDate;

            var records = _db.PatientMedicalRecords
                .Include(r => r.Diagnosis)
                .Include(r => r.MedicalProcedure)
                .Include(r => r.Medication)
                .Include(r => r.Staff)
                .Include(r => r.Department)
                .Where(r => r.ID_Patient == _patient.ID_Patient)
                .AsQueryable();

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
            DateFromFilter.SelectedDate = null;
            DateToFilter.SelectedDate = null;
            LoadMedicalRecords();
        }
    }
}