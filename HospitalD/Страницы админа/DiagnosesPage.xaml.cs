using System;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace HospitalD
{
    public partial class DiagnosesPage : Page
    {
        private readonly Entities1 _db = new Entities1();

        public DiagnosesPage()
        {
            InitializeComponent();
            LoadDiagnoses();
            InitializeFirstLetterFilter();
        }

        private void InitializeFirstLetterFilter()
        {
            // Добавляем буквы А-Я в комбобокс
            FirstLetterFilter.Items.Add(new ComboBoxItem() { Content = "Все" });
            for (char c = 'А'; c <= 'Я'; c++)
            {
                FirstLetterFilter.Items.Add(new ComboBoxItem() { Content = c.ToString() });
            }
            FirstLetterFilter.SelectedIndex = 0;
        }

        private void LoadDiagnoses()
        {
            try
            {
                // Явно загружаем связанные данные отделений
                var diagnoses = _db.Diagnoses
                    .Include(d => d.Department)  // Используем правильное название навигационного свойства
                    .AsNoTracking()
                    .ToList();

                DiagnosesDataGrid.ItemsSource = diagnoses;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateDiagnoses()
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
                NavigationService.Navigate(new AddEditDiagnosesPage(selectedDiagnosis));
            }
            else
            {
                MessageBox.Show("Выберите диагноз для редактирования!",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void ButtonAdd_OnClick(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new AddEditDiagnosesPage());
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
                _db.Diagnoses.Remove(selectedDiagnosis);
                _db.SaveChanges();
                UpdateDiagnoses();
                MessageBox.Show("Диагноз успешно удален!",
                    "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении: {ex.InnerException?.Message ?? ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DiagnosesDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}