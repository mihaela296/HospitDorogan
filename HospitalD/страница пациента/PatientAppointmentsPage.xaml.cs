using System;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace HospitalD
{
    public partial class PatientAppointmentsPage : Page
    {
        private readonly Entities1 _db = new Entities1();
        private Patient _patient;

        public PatientAppointmentsPage(Patient patient)
        {
            InitializeComponent();
            _patient = patient;
            LoadAppointments();
            StatusFilter.SelectedIndex = 0;
        }

        private void LoadAppointments()
        {
            try
            {
                _db.Configuration.LazyLoadingEnabled = false;

                var appointments = _db.PatientMedicalRecords
                    .Include(a => a.Staff.Department)
                    .Where(a => a.ID_Patient == _patient.ID_Patient)
                    .AsEnumerable() // Переключаемся на LINQ to Objects
                    .Select(a => new
                    {
                        Record = a,
                        Status = a.VisitDate < DateTime.Now ? "Завершено" : "Запланировано"
                    })
                    .OrderByDescending(x => x.Record.VisitDate)
                    .ToList();

                AppointmentsDataGrid.ItemsSource = appointments.Select(x => new
                {
                    x.Record.VisitDate,
                    x.Record.Staff,
                    x.Record.Notes,
                    x.Status,
                    DepartmentName = x.Record.Staff.Department?.Name ?? "Не указано"
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки записей: {ex.Message}");
            }
        }

        private void StatusFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                var appointments = _db.PatientMedicalRecords
                    .Include(a => a.Staff.Department)
                    .Where(a => a.ID_Patient == _patient.ID_Patient)
                    .AsEnumerable()
                    .Select(a => new
                    {
                        Record = a,
                        Status = a.VisitDate < DateTime.Now ? "Завершено" : "Запланировано"
                    });

                switch (StatusFilter.SelectedIndex)
                {
                    case 1: // Предстоящие
                        appointments = appointments.Where(x => x.Record.VisitDate >= DateTime.Now);
                        break;
                    case 2: // Прошедшие
                        appointments = appointments.Where(x => x.Record.VisitDate < DateTime.Now);
                        break;
                }

                AppointmentsDataGrid.ItemsSource = appointments
                    .OrderByDescending(x => x.Record.VisitDate)
                    .Select(x => new
                    {
                        x.Record.VisitDate,
                        x.Record.Staff,
                        x.Record.Notes,
                        x.Status,
                        DepartmentName = x.Record.Staff.Department?.Name ?? "Не указано"
                    })
                    .ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка фильтрации: {ex.Message}");
            }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            LoadAppointments();
        }


        private void ButtonCancel_OnClick(object sender, RoutedEventArgs e)
        {
            var selectedItem = AppointmentsDataGrid.SelectedItem;
            if (selectedItem == null)
            {
                MessageBox.Show("Выберите запись для отмены!");
                return;
            }

            // Get the dynamic object's properties
            dynamic dynamicItem = selectedItem;
            try
            {
                DateTime visitDate = dynamicItem.VisitDate;
                string notes = dynamicItem.Notes;

                // Get the actual record from database using the visit date and notes
                var record = _db.PatientMedicalRecords
                    .FirstOrDefault(a => a.ID_Patient == _patient.ID_Patient
                                     && a.VisitDate == visitDate
                                     && a.Notes == notes);

                if (record == null)
                {
                    MessageBox.Show("Запись не найдена!");
                    return;
                }

                if (record.VisitDate < DateTime.Now)
                {
                    MessageBox.Show("Нельзя отменить прошедшую запись!");
                    return;
                }

                if (MessageBox.Show($"Отменить запись на {record.VisitDate:dd.MM.yyyy HH:mm}?",
                    "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                {
                    return;
                }

                try
                {
                    _db.PatientMedicalRecords.Remove(record);
                    _db.SaveChanges();
                    LoadAppointments();
                    MessageBox.Show("Запись успешно отменена!");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при отмене записи: {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при получении данных записи: {ex.Message}");
            }
        }
    }
}