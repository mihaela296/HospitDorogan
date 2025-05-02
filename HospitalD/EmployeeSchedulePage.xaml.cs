using System;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace HospitalD
{
    public partial class EmployeeSchedulePage : Page
    {
        private readonly Entities1 _db;
        private Staff _currentStaff;

        public EmployeeSchedulePage(Staff staff)
        {
            try
            {
                InitializeComponent();
                _currentStaff = staff ?? throw new ArgumentNullException(nameof(staff));
                _db = Entities1.GetContext();

                if (!_db.Database.Exists())
                {
                    MessageBox.Show("Ошибка подключения к базе данных");
                    return;
                }

                // Загрузка списка врачей для фильтрации
                LoadDoctors();
                LoadSchedule();
                DateFilter.SelectedDate = DateTime.Today;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка инициализации: {ex.Message}");
                NavigationService?.GoBack();
            }
        }

        private void LoadDoctors()
        {
            try
            {
                var doctors = _db.Staffs
                    .Include(s => s.Position)
                    .Where(s => s.Position.Name.ToLower().Contains("врач") ||
                               s.Position.Name.ToLower().Contains("доктор"))
                    .OrderBy(s => s.FullName)
                    .ToList();

                DoctorFilterComboBox.ItemsSource = doctors;
                DoctorFilterComboBox.DisplayMemberPath = "FullName";
                DoctorFilterComboBox.SelectedValuePath = "ID_Staff";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки списка врачей: {ex.Message}");
            }
        }

        private void LoadSchedule()
        {
            try
            {
                _db.Configuration.LazyLoadingEnabled = false;

                var schedule = _db.PatientMedicalRecords
                    .AsNoTracking()
                    .Include(r => r.Patient)
                    .Include(r => r.Staff)
                    .Include(r => r.Department)
                    .Where(r => r.DischargeDate == null &&
                          r.Diagnosis == null &&
                          r.MedicalProcedure == null &&
                          r.Medication == null)
                    .OrderBy(r => r.VisitDate)
                    .ToList();

                ScheduleDataGrid.ItemsSource = schedule;
                ScheduleDataGrid.CanUserAddRows = false; // Отключаем пустую строку
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки расписания: {ex.Message}");
            }
        }

        private void CompleteAppointmentButton_Click(object sender, RoutedEventArgs e)
        {
            if (!(ScheduleDataGrid.SelectedItem is PatientMedicalRecord appointment))
            {
                MessageBox.Show("Выберите прием для завершения");
                return;
            }

            using (var tempContext = new Entities1())
            {
                var freshAppointment = tempContext.PatientMedicalRecords
                    .Include(a => a.Patient)
                    .FirstOrDefault(a => a.ID_Record == appointment.ID_Record);

                if (freshAppointment == null) return;

                // Явный вызов конструктора с PatientMedicalRecord
                NavigationService?.Navigate(new AssignTreatmentPage(_currentStaff, freshAppointment));
            }
        }

        private void ApplyFilter_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var filterDate = DateFilter.SelectedDate;
                var selectedDoctorId = (int?)DoctorFilterComboBox.SelectedValue;

                IQueryable<PatientMedicalRecord> query = _db.PatientMedicalRecords
                    .Include(r => r.Patient)
                    .Include(r => r.Staff) // Добавляем загрузку данных о враче
                    .Include(r => r.Diagnosis)
                    .Include(r => r.MedicalProcedure);

                // Фильтр по дате
                if (filterDate.HasValue)
                {
                    query = query.Where(r => DbFunctions.TruncateTime(r.VisitDate) == filterDate.Value.Date);
                }

                // Фильтр по врачу
                if (selectedDoctorId.HasValue)
                {
                    query = query.Where(r => r.ID_Staff == selectedDoctorId);
                }

                // Фильтр по незавершенным записям
                query = query.Where(r => r.DischargeDate == null || r.DischargeDate > DateTime.Now);

                var filteredSchedule = query
                    .OrderBy(r => r.VisitDate)
                    .ToList();

                ScheduleDataGrid.ItemsSource = filteredSchedule;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка фильтрации: {ex.Message}");
            }
        }

        private void TodayButton_Click(object sender, RoutedEventArgs e) => ApplyFilter_Click(sender, e);
        private void ResetFilter_Click(object sender, RoutedEventArgs e) => LoadSchedule();
    }
}