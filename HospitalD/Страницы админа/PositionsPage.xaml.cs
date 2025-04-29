using System;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace HospitalD
{
    public partial class PositionsPage : Page
    {
        private readonly Entities1 _db = new Entities1();

        public PositionsPage()
        {
            InitializeComponent();
            LoadPositions();
        }

        private void LoadPositions()
        {
            UpdatePositions();
        }

        private void UpdatePositions()
        {
            var currentPositions = _db.Positions.AsNoTracking().AsQueryable();

            // Фильтрация по зарплате
            if (SalaryFilter.SelectedIndex > 0)
            {
                switch (SalaryFilter.SelectedIndex)
                {
                    case 1: currentPositions = currentPositions.Where(p => p.Salary < 50000); break;
                    case 2: currentPositions = currentPositions.Where(p => p.Salary >= 50000 && p.Salary < 100000); break;
                    case 3: currentPositions = currentPositions.Where(p => p.Salary >= 100000 && p.Salary < 150000); break;
                    case 4: currentPositions = currentPositions.Where(p => p.Salary >= 150000); break;
                }
            }

            // Фильтрация по названию
            if (!string.IsNullOrWhiteSpace(SearchPositionName.Text))
            {
                currentPositions = currentPositions.Where(p =>
                    p.Name.ToLower().Contains(SearchPositionName.Text.ToLower()));
            }

            // Сортировка
            switch (SortPositionComboBox.SelectedIndex)
            {
                case 0: currentPositions = currentPositions.OrderBy(p => p.Name); break;
                case 1: currentPositions = currentPositions.OrderBy(p => p.Name); break;
                case 2: currentPositions = currentPositions.OrderByDescending(p => p.Name); break;
                case 3: currentPositions = currentPositions.OrderBy(p => p.Salary); break;
                case 4: currentPositions = currentPositions.OrderByDescending(p => p.Salary); break;
            }

            PositionsDataGrid.ItemsSource = currentPositions.ToList();
        }

        private void SalaryFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdatePositions();
        }

        private void SearchPositionName_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdatePositions();
        }

        private void SortPositionComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdatePositions();
        }

        private void CleanFilter_OnClick(object sender, RoutedEventArgs e)
        {
            SearchPositionName.Text = string.Empty;
            SortPositionComboBox.SelectedIndex = 0;
            SalaryFilter.SelectedIndex = 0;
            UpdatePositions();
        }

        private void ButtonEdit_OnClick(object sender, RoutedEventArgs e)
        {
            if (PositionsDataGrid.SelectedItem is Position selectedPosition)
            {
                NavigationService.Navigate(new AddEditPositionPage(
                    new Position { ID_Position = selectedPosition.ID_Position }));
            }
            else
            {
                MessageBox.Show("Выберите должность для редактирования!",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void ButtonAdd_OnClick(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new AddEditPositionPage());
        }

        private void ButtonDel_OnClick(object sender, RoutedEventArgs e)
        {
            if (!(PositionsDataGrid.SelectedItem is Position selectedPosition))
            {
                MessageBox.Show("Выберите должность для удаления!",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (MessageBox.Show($"Удалить должность '{selectedPosition.Name}'?", "Подтверждение",
                    MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
            {
                return;
            }

            try
            {
                if (_db.Entry(selectedPosition).State == EntityState.Detached)
                {
                    _db.Positions.Attach(selectedPosition);
                }

                _db.Positions.Remove(selectedPosition);
                _db.SaveChanges();
                UpdatePositions();
                MessageBox.Show("Должность успешно удалена!",
                    "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (System.Data.Entity.Infrastructure.DbUpdateException)
            {
                MessageBox.Show("Невозможно удалить должность, так как она связана с другими записями в базе данных.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении: {ex.InnerException?.Message ?? ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}