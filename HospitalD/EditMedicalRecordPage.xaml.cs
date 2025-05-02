using System;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace HospitalD
{
    public partial class EditMedicalRecordPage : Page
    {
        private readonly Entities1 _db;
        private PatientMedicalRecord _record;
        private Staff _currentStaff;

        public EditMedicalRecordPage(PatientMedicalRecord record, Staff staff)
        {
            InitializeComponent();

            // Инициализация контекста новой базы данных
            _db = new Entities1();
            _currentStaff = staff;

            try
            {
                // Загружаем запись из новой базы данных
                _record = _db.PatientMedicalRecords
                    .Include(r => r.Patient)
                    .Include(r => r.Staff)
                    .FirstOrDefault(r => r.ID_Record == record.ID_Record);

                if (_record == null)
                {
                    MessageBox.Show("Запись не найдена в базе данных");
                    NavigationService?.GoBack();
                    return;
                }

                LoadData();
                DisplayRecordInfo();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}");
                NavigationService?.GoBack();
            }
        }

        private void LoadData()
        {
            try
            {
                // Загружаем данные для ComboBox
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

                _db.Departments.Load();
                DepartmentComboBox.ItemsSource = _db.Departments.Local;
                DepartmentComboBox.DisplayMemberPath = "Name";
                DepartmentComboBox.SelectedValuePath = "ID_Department";

                // Устанавливаем текущие значения
                DiagnosisComboBox.SelectedValue = _record.ID_Diagnosis;
                ProcedureComboBox.SelectedValue = _record.ID_Procedure;
                MedicationComboBox.SelectedValue = _record.ID_Medication;
                DepartmentComboBox.SelectedValue = _record.ID_Department;
                DischargeDatePicker.SelectedDate = _record.DischargeDate;
                NotesTextBox.Text = _record.Notes;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}");
            }
        }

        private void DisplayRecordInfo()
        {
            try
            {
                string patientName = _record.Patient?.FullName ?? "Не указан";
                string visitDate = _record.VisitDate.ToString("dd.MM.yyyy");
                string staffName = _record.Staff?.FullName ?? "Не указан";

                RecordInfoText.Text = $"Пациент: {patientName}\n" +
                                    $"Дата приема: {visitDate}\n" +
                                    $"Врач: {staffName}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка отображения информации: {ex.Message}");
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (_record == null)
            {
                MessageBox.Show("Ошибка: запись не загружена.");
                return;
            }

            // Проверка обязательных полей
            if (DepartmentComboBox.SelectedValue == null)
            {
                MessageBox.Show("Выберите отделение!");
                return;
            }

            try
            {
                // Обновляем данные записи
                _record.ID_Diagnosis = (int?)DiagnosisComboBox.SelectedValue;
                _record.ID_Procedure = (int?)ProcedureComboBox.SelectedValue;
                _record.ID_Medication = (int?)MedicationComboBox.SelectedValue;
                _record.ID_Department = (int)DepartmentComboBox.SelectedValue;
                _record.DischargeDate = DischargeDatePicker.SelectedDate;
                _record.Notes = NotesTextBox.Text;
                _record.ID_Staff = _currentStaff.ID_Staff;

                _db.SaveChanges();
                MessageBox.Show("Изменения сохранены успешно!");
                NavigationService?.GoBack();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.InnerException?.Message ?? ex.Message}");
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.GoBack();
        }
    }
}