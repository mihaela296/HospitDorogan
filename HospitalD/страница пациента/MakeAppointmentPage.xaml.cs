using System;
using System.Data.Entity;
using System.Diagnostics;
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
        // Временный метод для проверки данных
        private void CheckDoctorsData()
        {
            var allStaff = _db.Staffs
                .Include(s => s.Position)
                .Include(s => s.Department)
                .ToList();

            foreach (var staff in allStaff)
            {
                Console.WriteLine($"Сотрудник: {staff.FullName}");
                Console.WriteLine($"Отделение: {staff.Department?.Name ?? "Нет"}");
                Console.WriteLine($"Должность: {staff.Position?.Name ?? "Нет"}");
                Console.WriteLine("----------------------------------");
            }
        }

        private void LoadDepartments()
        {
            try
            {
                var departments = _db.Departments
                    .Include(d => d.Staffs.Select(s => s.Position))  // Загружаем сотрудников с их должностями
                    .OrderBy(d => d.Name)
                    .ToList();

                // Отладочный вывод (разделен на несколько строк для читаемости)
                foreach (var dep in departments)
                {
                    Console.WriteLine("Отделение: " + dep.Name);
                    Console.WriteLine("Всего сотрудников: " + (dep.Staffs?.Count ?? 0));
                    Console.WriteLine("Из них врачей: " +
                        (dep.Staffs?.Count(s =>
                            s.Position != null &&
                            (s.Position.Name.ToLower().Contains("врач") ||
                             s.Position.Name.ToLower().Contains("доктор"))) ?? 0));
                }

                DepartmentComboBox.ItemsSource = departments;
                DepartmentComboBox.DisplayMemberPath = "Name";
                DepartmentComboBox.SelectedValuePath = "ID_Department";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при загрузке отделений: " + ex.Message +
                               "\n" + ex.InnerException?.Message,
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
                    // Получаем всех сотрудников отделения
                    var staffInDepartment = _db.Staffs
                        .Include(s => s.Position)
                        .Include(s => s.Department)
                        .Where(s => s.ID_Department == selectedDepartment.ID_Department)
                        .ToList();

                    // Фильтруем врачей
                    var doctors = staffInDepartment
                        .Where(s => s.Position != null && IsDoctorPosition(s.Position.Name))
                        .OrderBy(s => s.FullName)
                        .ToList();

                    // Если не нашли по строгому фильтру, используем всех сотрудников без сообщения
                    if (doctors.Count == 0)
                    {
                        doctors = staffInDepartment
                            .OrderBy(s => s.FullName)
                            .ToList();
                    }

                    DoctorComboBox.ItemsSource = doctors;
                    DoctorComboBox.DisplayMemberPath = "FullName";
                    DoctorComboBox.SelectedValuePath = "ID_Staff";

                    // Отладочный вывод (можно убрать в релизной версии)
                    Debug.WriteLine($"Врачи в отделении {selectedDepartment.Name}:");
                    foreach (var doctor in doctors)
                    {
                        Debug.WriteLine($"{doctor.FullName} ({doctor.Position?.Name})");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка загрузки врачей: {ex.Message}\n{ex.InnerException?.Message}",
                                  "Ошибка",
                                  MessageBoxButton.OK,
                                  MessageBoxImage.Error);
                }
            }
        }

        // Вспомогательный метод для определения врачебных должностей
        private bool IsDoctorPosition(string positionName)
        {
            if (string.IsNullOrWhiteSpace(positionName))
                return false;

            var lowerPosition = positionName.ToLower();
            return lowerPosition.Contains("врач") ||
                   lowerPosition.Contains("доктор") ||
                   lowerPosition.Contains("терапевт") ||
                   lowerPosition.Contains("хирург") ||
                   lowerPosition.Contains("педиатр") ||
                   lowerPosition.Contains("окулист") ||
                   lowerPosition.Contains("стоматолог");
        }


        private void MakeAppointment_Click(object sender, RoutedEventArgs e)
        {

            // В MakeAppointmentPage в методе MakeAppointment_Click добавьте проверку:
            var selectedDoctor = (Staff)DoctorComboBox.SelectedItem;
            if (selectedDoctor.ID_Department != ((Department)DepartmentComboBox.SelectedItem).ID_Department)
            {
                MessageBox.Show("Внимание! Выбранный врач не работает в указанном отделении. " +
                              "Запись может не отобразиться в расписании.");
            }
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