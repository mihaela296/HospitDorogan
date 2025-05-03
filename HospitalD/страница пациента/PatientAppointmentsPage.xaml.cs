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

        // In PatientAppointmentsPage.xaml.cs - modify the LoadAppointments method
        private void LoadAppointments()
        {
            try
            {
                _db.Configuration.LazyLoadingEnabled = false;

                var appointments = _db.PatientMedicalRecords
                    .Include(a => a.Staff.Department)
                    .Where(a => a.ID_Patient == _patient.ID_Patient)
                    .AsEnumerable()
                    .Select(a => new
                    {
                        Record = a,
                        Status = a.DischargeDate != null ? "Завершено" : "Запланировано"
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
                        Status = a.DischargeDate != null ? "Завершено" : "Запланировано"
                    });

                switch (StatusFilter.SelectedIndex)
                {
                    case 1: // Предстоящие
                        appointments = appointments.Where(x => x.Record.DischargeDate == null &&
                                                           x.Record.VisitDate >= DateTime.Now);
                        break;
                    case 2: // Прошедшие
                        appointments = appointments.Where(x => x.Record.DischargeDate != null ||
                                                           x.Record.VisitDate < DateTime.Now);
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

            // Получаем свойства динамического объекта
            dynamic dynamicItem = selectedItem;
            try
            {
                DateTime visitDate = dynamicItem.VisitDate;
                string notes = dynamicItem.Notes;

                // Получаем фактическую запись из базы данных по дате визита и заметкам
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

                if (MessageBox.Show($"Отменить запись на {record.VisitDate:dd.MM.yyyy}?",
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
        private void ButtonDelCompleted_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Получаем все завершенные записи для текущего пациента
                var completedAppointments = _db.PatientMedicalRecords
                    .Where(a => a.ID_Patient == _patient.ID_Patient && a.VisitDate < DateTime.Now)
                    .ToList();

                if (completedAppointments.Count == 0)
                {
                    MessageBox.Show("Нет завершенных записей для удаления.");
                    return;
                }

                // Подтверждение удаления
                if (MessageBox.Show($"Удалить {completedAppointments.Count} завершенных записей?", "Подтверждение",
                        MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
                {
                    return;
                }

                // Удаление завершенных записей
                _db.PatientMedicalRecords.RemoveRange(completedAppointments);
                _db.SaveChanges();
                LoadAppointments();
                MessageBox.Show("Завершенные записи успешно удалены!");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении завершенных записей: {ex.Message}");
            }
        }
    }
}
