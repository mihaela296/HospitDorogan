using System;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace HospitalD
{
    public partial class MakeAppointmentPage : Page
    {
        private readonly Entities1 _db = new Entities1();
        private Patient _patient;

        public MakeAppointmentPage(Patient patient)
        {
            InitializeComponent();
            _patient = patient;
            LoadDepartments();

            // Выбираем первое отделение по умолчанию
            if (DepartmentComboBox.Items.Count > 0)
            {
                DepartmentComboBox.SelectedIndex = 0;
            }

            AppointmentDatePicker.SelectedDate = DateTime.Today;
            TimeComboBox.SelectedIndex = 0;
        }

        private void LoadDepartments()
        {
            try
            {
                var departments = _db.Departments
                    .Include(d => d.Staffs)  // Явно загружаем сотрудников
                    .OrderBy(d => d.Name)
                    .ToList();

                // Для отладки - проверим сотрудников в каждом отделении
                foreach (var dep in departments)
                {
                    Console.WriteLine($"Отделение: {dep.Name}");
                    Console.WriteLine($"Количество сотрудников: {dep.Staffs?.Count ?? 0}");
                }

                DepartmentComboBox.ItemsSource = departments;
                DepartmentComboBox.DisplayMemberPath = "Name";
                DepartmentComboBox.SelectedValuePath = "ID_Department";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке отделений: {ex.Message}",
                              "Ошибка",
                              MessageBoxButton.OK,
                              MessageBoxImage.Error);
            }
        }

        private void DepartmentComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DepartmentComboBox.SelectedItem is Department selectedDepartment)
            {
                try
                {
                    // Получаем всех сотрудников выбранного отделения
                    var staffInDepartment = _db.Staffs
                        .Include(s => s.Position)
                        .Include(s => s.Department)
                        .Where(s => s.ID_Department == selectedDepartment.ID_Department)
                        .ToList();

                    // Фильтруем только врачей (можно добавить другие критерии)
                    var doctors = staffInDepartment
                        .Where(s => s.Position != null &&
                                  (s.Position.Name.Contains("Врач") ||
                                   s.Position.Name.Contains("Доктор") ||
                                   s.Position.Name.Contains("Терапевт") ||
                                   s.Position.Name.Contains("Педиатр") ||
                                   s.Position.Name.Contains("Хирург")))
                        .OrderBy(s => s.FullName)
                        .ToList();

                    DoctorComboBox.ItemsSource = doctors;
                    DoctorComboBox.DisplayMemberPath = "FullName";
                    DoctorComboBox.SelectedValuePath = "ID_Staff";

                    // Для отладки - выводим в консоль список врачей
                    Console.WriteLine($"Врачи в отделении {selectedDepartment.Name}:");
                    foreach (var doctor in doctors)
                    {
                        Console.WriteLine($"{doctor.FullName} ({doctor.Position?.Name})");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при загрузке врачей: {ex.Message}",
                                  "Ошибка",
                                  MessageBoxButton.OK,
                                  MessageBoxImage.Error);
                }
            }
        }

        private void MakeAppointment_Click(object sender, RoutedEventArgs e)
        {
            // 1. Проверка выбора врача
            if (DoctorComboBox.SelectedItem == null)
            {
                MessageBox.Show("Пожалуйста, выберите врача из списка.",
                               "Не выбран врач",
                               MessageBoxButton.OK,
                               MessageBoxImage.Warning);
                return;
            }

            // 2. Проверка наличия врачей в отделении
            if (DoctorComboBox.Items.Count == 0)
            {
                MessageBox.Show("В выбранном отделении нет доступных врачей.",
                               "Нет доступных врачей",
                               MessageBoxButton.OK,
                               MessageBoxImage.Warning);
                return;
            }

            // 3. Проверка даты приема
            if (!AppointmentDatePicker.SelectedDate.HasValue)
            {
                MessageBox.Show("Пожалуйста, выберите дату приема.",
                               "Не выбрана дата",
                               MessageBoxButton.OK,
                               MessageBoxImage.Warning);
                return;
            }

            var selectedDate = AppointmentDatePicker.SelectedDate.Value;
            if (selectedDate < DateTime.Today)
            {
                MessageBox.Show("Нельзя записаться на прошедшую дату.",
                               "Некорректная дата",
                               MessageBoxButton.OK,
                               MessageBoxImage.Warning);
                return;
            }

            // 4. Проверка описания жалоб
            if (string.IsNullOrWhiteSpace(ComplaintsTextBox.Text))
            {
                MessageBox.Show("Пожалуйста, опишите ваши жалобы.",
                               "Нет описания жалоб",
                               MessageBoxButton.OK,
                               MessageBoxImage.Warning);
                return;
            }

            try
            {
                // Получаем выбранное время (только для отображения в сообщении)
                var selectedTime = TimeComboBox.SelectedItem != null
                    ? ((ComboBoxItem)TimeComboBox.SelectedItem).Content.ToString()
                    : "время не указано";

                // Создаем запись без времени
                var appointment = new PatientMedicalRecord
                {
                    ID_Patient = _patient.ID_Patient,
                    ID_Staff = (int)DoctorComboBox.SelectedValue,
                    ID_Department = (int)DepartmentComboBox.SelectedValue,
                    VisitDate = selectedDate,
                    RecordDate = DateTime.Now,
                    AdmissionDate = selectedDate,
                    Notes = $"Запланировано на: {selectedTime}\nЖалобы: {ComplaintsTextBox.Text}"
                };

                // Проверка на существующую запись (только по дате и врачу)
                var existingAppointment = _db.PatientMedicalRecords
                    .FirstOrDefault(a => a.ID_Staff == appointment.ID_Staff &&
                                       a.VisitDate == appointment.VisitDate);

                if (existingAppointment != null)
                {
                    MessageBox.Show("У вас уже есть запись к этому врачу на выбранную дату.",
                                  "Запись существует",
                                  MessageBoxButton.OK,
                                  MessageBoxImage.Warning);
                    return;
                }

                // Сохраняем запись
                _db.PatientMedicalRecords.Add(appointment);
                _db.SaveChanges();

                // Формируем информацию о записи
                var doctorInfo = ((Staff)DoctorComboBox.SelectedItem).FullName;
                var departmentInfo = ((Department)DepartmentComboBox.SelectedItem).Name;

                MessageBox.Show(
                    $"Запись успешно создана!\n\n" +
                    $"Врач: {doctorInfo}\n" +
                    $"Отделение: {departmentInfo}\n" +
                    $"Дата: {selectedDate.ToShortDateString()}\n" +
                    $"Время: {selectedTime}",
                    "Успешная запись",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка при создании записи:\n{ex.Message}",
                              "Ошибка",
                              MessageBoxButton.OK,
                              MessageBoxImage.Error);
            }
        }
    }
}