using System;
using System.Data.Entity.Validation;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace HospitalD
{
    public partial class AddEditDepartmentPage : Page
    {
        private readonly Department _currentDepartment; // Добавлено readonly
        private readonly Entities1 _context = new Entities1();

        public AddEditDepartmentPage(Department selectedDepartment = null)
        {
            InitializeComponent();
            _currentDepartment = selectedDepartment != null
                ? _context.Departments.Find(selectedDepartment.ID_Department)
                : new Department();

            LoadDepartmentData();
        }

        private void LoadDepartmentData()
        {
            if (_currentDepartment == null) return;

            TextBlockTitle.Text = "Редактирование отделения";
            TextBoxName.Text = _currentDepartment.Name;
        }

        private void SaveDepartment_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateInputs()) return;

            try
            {
                _currentDepartment.Name = TextBoxName.Text.Trim();

                if (_currentDepartment.ID_Department == 0)
                {
                    _context.Departments.Add(_currentDepartment);
                }

                _context.SaveChanges();
                MessageBox.Show("Данные отделения успешно сохранены!",
                    "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                NavigationService.GoBack();
            }
            catch (DbEntityValidationException ex)
            {
                var errorMessages = ex.EntityValidationErrors
                    .SelectMany(x => x.ValidationErrors)
                    .Select(x => $"{x.PropertyName}: {x.ErrorMessage}");

                MessageBox.Show("Ошибки валидации:\n" + string.Join("\n", errorMessages),
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении: {ex.InnerException?.Message ?? ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool ValidateInputs()
        {
            if (string.IsNullOrWhiteSpace(TextBoxName.Text))
            {
                MessageBox.Show("Введите название отделения!", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            return true;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            _context.Dispose();
            NavigationService.GoBack();
        }
    }
}