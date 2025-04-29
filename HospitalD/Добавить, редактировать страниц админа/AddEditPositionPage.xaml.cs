using System;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace HospitalD
{
    public partial class AddEditPositionPage : Page
    {
        private Position _currentPosition;
        private Entities1 _db;

        public AddEditPositionPage(Position selectedPosition = null)
        {
            InitializeComponent();

            // Создаем новый контекст для каждой страницы редактирования
            _db = new Entities1();

            if (selectedPosition != null && selectedPosition.ID_Position != 0)
            {
                // Загружаем должность из БД с новым контекстом
                _currentPosition = _db.Positions.Find(selectedPosition.ID_Position);
                TitleTextBlock.Text = "Редактирование должности";
            }
            else
            {
                _currentPosition = new Position();
                TitleTextBlock.Text = "Добавление должности";
            }

            LoadData();
        }

        private void LoadData()
        {
            if (_currentPosition.ID_Position != 0)
            {
                NameTextBox.Text = _currentPosition.Name;
                SalaryTextBox.Text = _currentPosition.Salary.ToString();
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateInput()) return;

            try
            {
                _currentPosition.Name = NameTextBox.Text.Trim();
                _currentPosition.Salary = decimal.Parse(SalaryTextBox.Text);

                if (_currentPosition.ID_Position == 0)
                {
                    _db.Positions.Add(_currentPosition);
                }
                else
                {
                    // Помечаем сущность как измененную
                    _db.Entry(_currentPosition).State = EntityState.Modified;
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
                MessageBox.Show("Введите название должности!", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if (!decimal.TryParse(SalaryTextBox.Text, out decimal salary) || salary <= 0)
            {
                MessageBox.Show("Введите корректную зарплату (число > 0)!", "Ошибка",
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