using System;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace HospitalD
{
    public partial class MedicationsPage : Page
    {
        private readonly Entities1 _db = new Entities1();
        private bool _shouldRefresh = false;

        public MedicationsPage()
        {
            InitializeComponent();
            LoadMedications();
            InitializeDosageFilter();

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
                UpdateMedications();
                _shouldRefresh = false;
            }
        }

        private void InitializeDosageFilter()
        {
            DosageFilter.Items.Clear();
            DosageFilter.Items.Add(new ComboBoxItem() { Content = "Все" });

            var dosages = _db.Medications
                .Select(m => m.DailyDosage)
                .Distinct()
                .OrderBy(d => d)
                .ToList();

            foreach (var dosage in dosages)
            {
                DosageFilter.Items.Add(new ComboBoxItem() { Content = $"{dosage}" });
            }

            DosageFilter.SelectedIndex = 0;
        }

        private void LoadMedications()
        {
            UpdateMedications();
        }

        private void UpdateMedications()
        {
            try
            {
                var currentMedications = _db.Medications
                    .AsNoTracking()
                    .AsQueryable();

                // Фильтрация по количеству в день
                if (DosageFilter.SelectedIndex > 0 &&
                    DosageFilter.SelectedItem is ComboBoxItem selectedDosageItem)
                {
                    string dosageStr = selectedDosageItem.Content.ToString();
                    if (int.TryParse(dosageStr, out int selectedDosage))
                    {
                        currentMedications = currentMedications.Where(m => m.DailyDosage == selectedDosage);
                    }
                }

                // Фильтрация по названию
                if (!string.IsNullOrWhiteSpace(SearchMedicationName.Text))
                {
                    currentMedications = currentMedications.Where(m =>
                        m.Name.ToLower().Contains(SearchMedicationName.Text.ToLower()));
                }

                // Сортировка
                switch (SortMedicationComboBox.SelectedIndex)
                {
                    case 0: currentMedications = currentMedications.OrderBy(m => m.Name); break;
                    case 1: currentMedications = currentMedications.OrderBy(m => m.Name); break;
                    case 2: currentMedications = currentMedications.OrderByDescending(m => m.Name); break;
                    case 3: currentMedications = currentMedications.OrderBy(m => m.DailyDosage); break;
                    case 4: currentMedications = currentMedications.OrderByDescending(m => m.DailyDosage); break;
                    case 5: currentMedications = currentMedications.OrderBy(m => m.Duration); break;
                    case 6: currentMedications = currentMedications.OrderByDescending(m => m.Duration); break;
                }

                MedicationsDataGrid.ItemsSource = currentMedications.ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при обновлении данных: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DosageFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateMedications();
        }

        private void SearchMedicationName_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateMedications();
        }

        private void SortMedicationComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateMedications();
        }

        private void CleanFilter_OnClick(object sender, RoutedEventArgs e)
        {
            SearchMedicationName.Text = string.Empty;
            SortMedicationComboBox.SelectedIndex = 0;
            DosageFilter.SelectedIndex = 0;
            UpdateMedications();
        }

        private void ButtonEdit_OnClick(object sender, RoutedEventArgs e)
        {
            if (MedicationsDataGrid.SelectedItem is Medication selectedMedication)
            {
                _shouldRefresh = true;
                var editPage = new AddEditMedicationPage(selectedMedication);
                editPage.MedicationSaved += (s, args) => _shouldRefresh = true;
                NavigationService.Navigate(editPage);
            }
            else
            {
                MessageBox.Show("Выберите лекарство для редактирования!",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void ButtonAdd_OnClick(object sender, RoutedEventArgs e)
        {
            _shouldRefresh = true;
            var addPage = new AddEditMedicationPage();
            addPage.MedicationSaved += (s, args) => _shouldRefresh = true;
            NavigationService.Navigate(addPage);
        }

        private void ButtonDel_OnClick(object sender, RoutedEventArgs e)
        {
            if (!(MedicationsDataGrid.SelectedItem is Medication selectedMedication))
            {
                MessageBox.Show("Выберите лекарство для удаления!",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (MessageBox.Show($"Удалить лекарство '{selectedMedication.Name}'?", "Подтверждение",
                    MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
            {
                return;
            }

            try
            {
                _db.Medications.Attach(selectedMedication);
                _db.Medications.Remove(selectedMedication);
                _db.SaveChanges();
                UpdateMedications();
                MessageBox.Show("Лекарство успешно удалено!",
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