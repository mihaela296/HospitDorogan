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

        // In EmployeeSchedulePage.xaml.cs - modify the CompleteAppointmentButton_Click method
        private void CompleteAppointmentButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!(ScheduleDataGrid.SelectedItem is PatientMedicalRecord selectedAppointment))
                {
                    MessageBox.Show("Выберите запись для завершения");
                    return;
                }

                // Обновляем запись в базе данных
                using (var context = new Entities1())
                {
                    var appointment = context.PatientMedicalRecords
                        .FirstOrDefault(a => a.ID_Record == selectedAppointment.ID_Record);

                    if (appointment != null)
                    {
                        // Устанавливаем признак завершения
                        appointment.DischargeDate = DateTime.Now;
                        context.SaveChanges();
                    }
                }

                // Открываем страницу назначения лечения
                NavigationService?.Navigate(new AssignTreatmentPage(_currentStaff, selectedAppointment));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при завершении приема: {ex.Message}\n\n{ex.InnerException?.Message}");
            }
        }

        private void ApplyFilter_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DateTime? filterDate = DateFilter.SelectedDate;
                int? doctorId = (int?)DoctorFilterComboBox.SelectedValue;

                // Получаем базовый запрос
                var query = _db.PatientMedicalRecords
                    .Include(r => r.Patient)
                    .Include(r => r.Staff)
                    .Include(r => r.Diagnosis)
                    .Include(r => r.MedicalProcedure)
                    .AsQueryable();

                // Фильтр по дате (исправленная версия)
                if (filterDate.HasValue)
                {
                    DateTime startDate = filterDate.Value.Date;
                    DateTime endDate = startDate.AddDays(1);
                    query = query.Where(r => r.VisitDate >= startDate && r.VisitDate < endDate);
                }

                // Фильтр по врачу
                if (doctorId.HasValue)
                {
                    query = query.Where(r => r.ID_Staff == doctorId.Value);
                }

                // Применяем сортировку и загрузку
                ScheduleDataGrid.ItemsSource = query
                    .OrderBy(r => r.VisitDate)
                    .ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка фильтрации: {ex.Message}\n\nПопробуйте выбрать другую дату или врача.");
            }
        }

        private void TodayButton_Click(object sender, RoutedEventArgs e)
        {
            DateFilter.SelectedDate = DateTime.Today;
            ApplyFilter_Click(sender, e);
        }

        private void ResetFilter_Click(object sender, RoutedEventArgs e)
        {
            DateFilter.SelectedDate = null;
            DoctorFilterComboBox.SelectedItem = null;
            LoadSchedule();
        }
    }
}