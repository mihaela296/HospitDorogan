using System;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace HospitalD
{
    public partial class MedicalProceduresPage : Page
    {
        private readonly Entities1 _db = new Entities1();
        private bool _shouldRefresh = false;

        public MedicalProceduresPage()
        {
            InitializeComponent();
            LoadMedicalProcedures();
            InitializeDurationFilter();

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
                UpdateProcedures();
                _shouldRefresh = false;
            }
        }

        private void InitializeDurationFilter()
        {
            DurationFilter.Items.Clear();
            DurationFilter.Items.Add(new ComboBoxItem() { Content = "Все" });

            var durations = _db.MedicalProcedures
                .Select(p => p.Duration)
                .Distinct()
                .OrderBy(d => d)
                .ToList();

            foreach (var duration in durations)
            {
                DurationFilter.Items.Add(new ComboBoxItem() { Content = $"{duration} мин" });
            }

            DurationFilter.SelectedIndex = 0;
        }

        private void LoadMedicalProcedures()
        {
            UpdateProcedures();
        }

        private void UpdateProcedures()
        {
            try
            {
                var currentProcedures = _db.MedicalProcedures
                    .Include(m => m.Staff)
                    .AsNoTracking()
                    .AsQueryable();

                // Фильтрация по продолжительности
                if (DurationFilter.SelectedIndex > 0 &&
                    DurationFilter.SelectedItem is ComboBoxItem selectedDurationItem)
                {
                    string selectedDuration = selectedDurationItem.Content.ToString().Replace(" мин", "");
                    currentProcedures = currentProcedures.Where(p => p.Duration == selectedDuration);
                }

                // Фильтрация по названию
                if (!string.IsNullOrWhiteSpace(SearchProcedureName.Text))
                {
                    currentProcedures = currentProcedures.Where(p =>
                        p.Name.ToLower().Contains(SearchProcedureName.Text.ToLower()));
                }

                // Сортировка
                switch (SortProcedureComboBox.SelectedIndex)
                {
                    case 0: currentProcedures = currentProcedures.OrderBy(p => p.ID_Procedure); break;
                    case 1: currentProcedures = currentProcedures.OrderBy(p => p.Name); break;
                    case 2: currentProcedures = currentProcedures.OrderByDescending(p => p.Name); break;
                    case 3: currentProcedures = currentProcedures.OrderBy(p => p.ID_Procedure); break;
                    case 4: currentProcedures = currentProcedures.OrderByDescending(p => p.ID_Procedure); break;
                    case 5: currentProcedures = currentProcedures.OrderBy(p => p.Duration); break;
                    case 6: currentProcedures = currentProcedures.OrderByDescending(p => p.Duration); break;
                }

                MedicalProceduresDataGrid.ItemsSource = currentProcedures.ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при обновлении данных: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DurationFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateProcedures();
        }

        private void SearchProcedureName_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateProcedures();
        }

        private void SortProcedureComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateProcedures();
        }

        private void CleanFilter_OnClick(object sender, RoutedEventArgs e)
        {
            SearchProcedureName.Text = string.Empty;
            SortProcedureComboBox.SelectedIndex = 0;
            DurationFilter.SelectedIndex = 0;
            UpdateProcedures();
        }

        private void ButtonEdit_OnClick(object sender, RoutedEventArgs e)
        {
            if (MedicalProceduresDataGrid.SelectedItem is MedicalProcedure selectedProcedure)
            {
                _shouldRefresh = true;
                var editPage = new AddEditMedicalProcedurePage(selectedProcedure);
                editPage.ProcedureSaved += (s, args) => _shouldRefresh = true;
                NavigationService.Navigate(editPage);
            }
            else
            {
                MessageBox.Show("Выберите процедуру для редактирования!",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void ButtonAdd_OnClick(object sender, RoutedEventArgs e)
        {
            _shouldRefresh = true;
            var addPage = new AddEditMedicalProcedurePage();
            addPage.ProcedureSaved += (s, args) => _shouldRefresh = true;
            NavigationService.Navigate(addPage);
        }

        private void ButtonDel_OnClick(object sender, RoutedEventArgs e)
        {
            if (!(MedicalProceduresDataGrid.SelectedItem is MedicalProcedure selectedProcedure))
            {
                MessageBox.Show("Выберите процедуру для удаления!",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (MessageBox.Show($"Удалить процедуру '{selectedProcedure.Name}'?", "Подтверждение",
                    MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
            {
                return;
            }

            try
            {
                _db.MedicalProcedures.Attach(selectedProcedure);
                _db.MedicalProcedures.Remove(selectedProcedure);
                _db.SaveChanges();
                UpdateProcedures();
                MessageBox.Show("Процедура успешно удалена!",
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