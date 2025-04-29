using System;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace HospitalD
{
    public partial class AddEditMedicalProcedurePage : Page
    {
        private MedicalProcedure _currentProcedure;
        private readonly Entities1 _db = new Entities1();

        public AddEditMedicalProcedurePage(MedicalProcedure selectedProcedure = null)
        {
            InitializeComponent();

            // Создаем новый экземпляр контекста для этой страницы
            _db = new Entities1();

            if (selectedProcedure != null)
            {
                // Загружаем сущность заново в новом контексте
                _currentProcedure = _db.MedicalProcedures.Find(selectedProcedure.ID_Procedure);
                TitleTextBlock.Text = "Редактирование процедуры";
            }
            else
            {
                _currentProcedure = new MedicalProcedure();
            }

            LoadData();
        }

        private void LoadData()
        {
            StaffComboBox.ItemsSource = _db.Staffs.ToList();

            if (_currentProcedure.ID_Procedure != 0)
            {
                NameTextBox.Text = _currentProcedure.Name;
                StaffComboBox.SelectedValue = _currentProcedure.ID_Staff;
                DurationTextBox.Text = _currentProcedure.Duration.ToString();
                CostTextBox.Text = _currentProcedure.Cost.ToString();
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateInput()) return;

            try
            {
                _currentProcedure.Name = NameTextBox.Text.Trim();
                _currentProcedure.ID_Staff = (int)StaffComboBox.SelectedValue;
                _currentProcedure.Duration = DurationTextBox.Text; // Сохраняем как строку
                _currentProcedure.Cost = decimal.Parse(CostTextBox.Text);

                if (_currentProcedure.ID_Procedure == 0)
                {
                    _db.MedicalProcedures.Add(_currentProcedure);
                }

                _db.SaveChanges();

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
                MessageBox.Show("Введите название процедуры!", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if (StaffComboBox.SelectedItem == null)
            {
                MessageBox.Show("Выберите ответственного сотрудника!", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if (!int.TryParse(DurationTextBox.Text, out int duration) || duration <= 0)
            {
                MessageBox.Show("Введите корректную продолжительность (целое число > 0)!", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if (!decimal.TryParse(CostTextBox.Text, out decimal cost) || cost <= 0)
            {
                MessageBox.Show("Введите корректную стоимость (число > 0)!", "Ошибка",
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