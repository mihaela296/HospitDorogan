using System;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace HospitalD
{
    public partial class AddEditDiagnosesPage : Page
    {
        // Добавляем событие для уведомления об успешном сохранении
        public event EventHandler DiagnosisSaved;

        private Diagnosis _currentDiagnosis;
        private Entities1 _db;

        public AddEditDiagnosesPage(Diagnosis selectedDiagnosis = null)
        {
            InitializeComponent();
            _db = new Entities1();

            if (selectedDiagnosis != null)
            {
                _currentDiagnosis = _db.Diagnoses.Find(selectedDiagnosis.ID_Diagnosis);
                TitleTextBlock.Text = "Редактирование диагноза";
            }
            else
            {
                _currentDiagnosis = new Diagnosis();
            }

            LoadData();
        }

        private void LoadData()
        {
            DepartmentComboBox.ItemsSource = _db.Departments.ToList();

            if (_currentDiagnosis.ID_Diagnosis != 0)
            {
                NameTextBox.Text = _currentDiagnosis.Name;
                if (_currentDiagnosis.ID_Department > 0)
                {
                    DepartmentComboBox.SelectedValue = _currentDiagnosis.ID_Department;
                }
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateInput()) return;

            try
            {
                _currentDiagnosis.Name = NameTextBox.Text.Trim();
                _currentDiagnosis.ID_Department = (int)DepartmentComboBox.SelectedValue;

                if (_currentDiagnosis.ID_Diagnosis == 0)
                {
                    _db.Diagnoses.Add(_currentDiagnosis);
                }
                else
                {
                    _db.Entry(_currentDiagnosis).State = EntityState.Modified;
                }

                _db.SaveChanges();

                // Вызываем событие перед возвратом
                DiagnosisSaved?.Invoke(this, EventArgs.Empty);

                MessageBox.Show("Данные сохранены успешно!", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                NavigationService.GoBack();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.InnerException?.Message ?? ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(NameTextBox.Text))
            {
                MessageBox.Show("Введите название диагноза!", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if (DepartmentComboBox.SelectedItem == null)
            {
                MessageBox.Show("Выберите отделение!", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            return true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }
    }
}