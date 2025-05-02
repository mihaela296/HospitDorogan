using System;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace HospitalD
{
    public partial class AssignTreatmentPage : Page
    {
        private readonly Entities1 _db;
        private Staff _currentStaff;
        private PatientMedicalRecord _existingRecord;
        private Patient _patient;

        // Основной конструктор для работы с существующей записью
        public AssignTreatmentPage(Staff staff, PatientMedicalRecord existingRecord)
        {
            InitializeComponent();
            _db = new Entities1();
            _currentStaff = staff;
            _existingRecord = _db.PatientMedicalRecords
                .Include(r => r.Patient)
                .Include(r => r.Staff)
                .FirstOrDefault(r => r.ID_Record == existingRecord.ID_Record);

            LoadComboBoxData();
            InitializeForm();
        }

        // Альтернативный конструктор для создания новой записи
        public AssignTreatmentPage(Staff staff, Patient patient)
        {
            InitializeComponent();
            _db = new Entities1();
            _currentStaff = staff;
            _patient = patient;

            LoadComboBoxData();
            InitializeNewForm();
        }

        private void LoadComboBoxData()
        {
            // Загрузка данных для ComboBox'ов
            _db.Patients.Load();
            PatientComboBox.ItemsSource = _db.Patients.Local;
            PatientComboBox.DisplayMemberPath = "FullName";
            PatientComboBox.SelectedValuePath = "ID_Patient";

            _db.Diagnoses.Load();
            DiagnosisComboBox.ItemsSource = _db.Diagnoses.Local;
            DiagnosisComboBox.DisplayMemberPath = "Name";
            DiagnosisComboBox.SelectedValuePath = "ID_Diagnosis";

            _db.MedicalProcedures.Load();
            ProcedureComboBox.ItemsSource = _db.MedicalProcedures.Local;
            ProcedureComboBox.DisplayMemberPath = "Name";
            ProcedureComboBox.SelectedValuePath = "ID_Procedure";

            _db.Medications.Load();
            MedicationComboBox.ItemsSource = _db.Medications.Local;
            MedicationComboBox.DisplayMemberPath = "Name";
            MedicationComboBox.SelectedValuePath = "ID_Medication";
        }

        private void InitializeForm()
        {
            if (_existingRecord != null)
            {
                PatientComboBox.SelectedValue = _existingRecord.ID_Patient;
                RecordDatePicker.SelectedDate = _existingRecord.RecordDate;
                NotesTextBox.Text = _existingRecord.Notes;

                if (_existingRecord.ID_Diagnosis.HasValue)
                    DiagnosisComboBox.SelectedValue = _existingRecord.ID_Diagnosis;

                if (_existingRecord.ID_Procedure.HasValue)
                    ProcedureComboBox.SelectedValue = _existingRecord.ID_Procedure;

                if (_existingRecord.ID_Medication.HasValue)
                    MedicationComboBox.SelectedValue = _existingRecord.ID_Medication;
            }
        }

        private void InitializeNewForm()
        {
            if (_patient != null)
            {
                PatientComboBox.SelectedValue = _patient.ID_Patient;
            }
            RecordDatePicker.SelectedDate = DateTime.Now;
        }

        private void SaveTreatment_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (PatientComboBox.SelectedItem == null)
                {
                    MessageBox.Show("Выберите пациента!");
                    return;
                }

                if (_existingRecord != null)
                {
                    // Обновляем существующую запись
                    _existingRecord.ID_Diagnosis = (int?)DiagnosisComboBox.SelectedValue;
                    _existingRecord.ID_Procedure = (int?)ProcedureComboBox.SelectedValue;
                    _existingRecord.ID_Medication = (int?)MedicationComboBox.SelectedValue;
                    _existingRecord.Notes = NotesTextBox.Text;
                    _existingRecord.RecordDate = RecordDatePicker.SelectedDate ?? DateTime.Now;
                    _existingRecord.DischargeDate = DateTime.Now;
                }
                else
                {
                    // Создаем новую запись
                    var treatment = new PatientMedicalRecord
                    {
                        ID_Patient = (int)PatientComboBox.SelectedValue,
                        ID_Staff = _currentStaff.ID_Staff,
                        RecordDate = DateTime.Now,
                        AdmissionDate = DateTime.Now,
                        Notes = NotesTextBox.Text,
                        ID_Diagnosis = (int?)DiagnosisComboBox.SelectedValue,
                        ID_Procedure = (int?)ProcedureComboBox.SelectedValue,
                        ID_Medication = (int?)MedicationComboBox.SelectedValue,
                        ID_Department = _currentStaff.ID_Department
                    };
                    _db.PatientMedicalRecords.Add(treatment);
                }

                _db.SaveChanges();
                MessageBox.Show("Данные успешно сохранены!");
                NavigationService?.GoBack();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}\n{ex.InnerException?.Message}");
            }
        }
    }
}