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
        private bool _shouldRefresh = false;

        public PositionsPage()
        {
            InitializeComponent();
            LoadPositions();

            // Подписываемся на событие NavigationService
            this.Loaded += (s, e) =>
            {
                if (NavigationService != null)
                {
                    NavigationService.Navigated += NavigationService_Navigated;
                }
            };
        }

        private void NavigationService_Navigated(object sender, NavigationEventArgs e)
        {
            // Обновляем данные только если вернулись на эту страницу
            if (e.Content == this && _shouldRefresh)
            {
                UpdatePositions();
                _shouldRefresh = false;
            }
        }

        private void LoadPositions()
        {
            UpdatePositions();
        }

        private void UpdatePositions()
        {
            try
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
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при обновлении данных: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
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
                _shouldRefresh = true;
                var editPage = new AddEditPositionPage(selectedPosition);
                editPage.PositionSaved += (s, args) => _shouldRefresh = true;
                NavigationService.Navigate(editPage);
            }
            else
            {
                MessageBox.Show("Выберите должность для редактирования!",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void ButtonAdd_OnClick(object sender, RoutedEventArgs e)
        {
            _shouldRefresh = true;
            var addPage = new AddEditPositionPage();
            addPage.PositionSaved += (s, args) => _shouldRefresh = true;
            NavigationService.Navigate(addPage);
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
                _db.Positions.Attach(selectedPosition);
                _db.Positions.Remove(selectedPosition);
                _db.SaveChanges();
                UpdatePositions();
                MessageBox.Show("Должность успешно удалена!",
                    "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении: {ex.InnerException?.Message ?? ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}