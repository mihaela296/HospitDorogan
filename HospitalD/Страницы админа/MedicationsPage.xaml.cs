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

        public MedicationsPage()
        {
            InitializeComponent();
            LoadMedications();
            InitializeDosageFilter();
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
            var currentMedications = _db.Medications.AsQueryable();

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
                NavigationService.Navigate(new AddEditMedicationPage(selectedMedication));
            }
            else
            {
                MessageBox.Show("Выберите лекарство для редактирования!",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void ButtonAdd_OnClick(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new AddEditMedicationPage());
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
                if (_db.Entry(selectedMedication).State == EntityState.Detached)
                {
                    _db.Medications.Attach(selectedMedication);
                }

                _db.Medications.Remove(selectedMedication);
                _db.SaveChanges();
                UpdateMedications();
                MessageBox.Show("Лекарство успешно удалено!",
                    "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (System.Data.Entity.Infrastructure.DbUpdateException)
            {
                MessageBox.Show("Невозможно удалить лекарство, так как оно связано с другими записями в базе данных.",
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