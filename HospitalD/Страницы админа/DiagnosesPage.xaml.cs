using System;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace HospitalD
{
    public partial class DiagnosesPage : Page
    {
        private readonly Entities1 _db = new Entities1();
        private bool _shouldRefresh = false;

        public DiagnosesPage()
        {
            InitializeComponent();
            LoadDiagnoses();
            InitializeFirstLetterFilter();

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
            if (e.Content == this && _shouldRefresh)
            {
                UpdateDiagnoses();
                _shouldRefresh = false;
            }
        }

        private void InitializeFirstLetterFilter()
        {
            FirstLetterFilter.Items.Add(new ComboBoxItem() { Content = "Все" });
            for (char c = 'А'; c <= 'Я'; c++)
            {
                FirstLetterFilter.Items.Add(new ComboBoxItem() { Content = c.ToString() });
            }
            FirstLetterFilter.SelectedIndex = 0;
        }

        private void LoadDiagnoses()
        {
            UpdateDiagnoses();
        }

        private void UpdateDiagnoses()
        {
            try
            {
                var currentDiagnoses = _db.Diagnoses
                    .Include(d => d.Department)
                    .AsNoTracking()
                    .ToList();

                // Фильтрация по первой букве
                if (FirstLetterFilter.SelectedIndex > 0 &&
                    FirstLetterFilter.SelectedItem is ComboBoxItem selectedLetterItem)
                {
                    string letter = selectedLetterItem.Content.ToString();
                    currentDiagnoses = currentDiagnoses.Where(d =>
                        d.Name.StartsWith(letter, StringComparison.OrdinalIgnoreCase)).ToList();
                }

                // Фильтрация по названию
                if (!string.IsNullOrWhiteSpace(SearchDiagnosisName.Text))
                {
                    currentDiagnoses = currentDiagnoses.Where(x =>
                        x.Name.ToLower().Contains(SearchDiagnosisName.Text.ToLower())).ToList();
                }

                // Сортировка
                switch (SortDiagnosisComboBox.SelectedIndex)
                {
                    case 0: currentDiagnoses = currentDiagnoses.OrderBy(d => d.ID_Diagnosis).ToList(); break;
                    case 1: currentDiagnoses = currentDiagnoses.OrderBy(d => d.Name).ToList(); break;
                    case 2: currentDiagnoses = currentDiagnoses.OrderByDescending(d => d.Name).ToList(); break;
                    case 3: currentDiagnoses = currentDiagnoses.OrderBy(d => d.Department.Name).ToList(); break;
                    case 4: currentDiagnoses = currentDiagnoses.OrderByDescending(d => d.Department.Name).ToList(); break;
                }

                DiagnosesDataGrid.ItemsSource = currentDiagnoses;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при обновлении данных: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void FirstLetterFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateDiagnoses();
        }

        private void SearchDiagnosisName_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateDiagnoses();
        }

        private void SortDiagnosisComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateDiagnoses();
        }

        private void CleanFilter_OnClick(object sender, RoutedEventArgs e)
        {
            SearchDiagnosisName.Text = string.Empty;
            SortDiagnosisComboBox.SelectedIndex = 0;
            FirstLetterFilter.SelectedIndex = 0;
            UpdateDiagnoses();
        }

        private void ButtonEdit_OnClick(object sender, RoutedEventArgs e)
        {
            if (DiagnosesDataGrid.SelectedItem is Diagnosis selectedDiagnosis)
            {
                _shouldRefresh = true;
                var editPage = new AddEditDiagnosesPage(selectedDiagnosis);
                editPage.DiagnosisSaved += (s, args) => _shouldRefresh = true;
                NavigationService.Navigate(editPage);
            }
            else
            {
                MessageBox.Show("Выберите диагноз для редактирования!",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void ButtonAdd_OnClick(object sender, RoutedEventArgs e)
        {
            _shouldRefresh = true;
            var addPage = new AddEditDiagnosesPage();
            addPage.DiagnosisSaved += (s, args) => _shouldRefresh = true;
            NavigationService.Navigate(addPage);
        }

        private void ButtonDel_OnClick(object sender, RoutedEventArgs e)
        {
            if (!(DiagnosesDataGrid.SelectedItem is Diagnosis selectedDiagnosis))
            {
                MessageBox.Show("Выберите диагноз для удаления!",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (MessageBox.Show($"Удалить диагноз '{selectedDiagnosis.Name}'?", "Подтверждение",
                    MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
            {
                return;
            }

            try
            {
                _db.Diagnoses.Attach(selectedDiagnosis);
                _db.Diagnoses.Remove(selectedDiagnosis);
                _db.SaveChanges();
                UpdateDiagnoses();
                MessageBox.Show("Диагноз успешно удален!", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении: {ex.InnerException?.Message ?? ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DiagnosesDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Дополнительная логика при изменении выбора, если необходимо
        }
    }
}