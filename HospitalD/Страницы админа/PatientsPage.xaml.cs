using System;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace HospitalD
{
    public partial class PatientsPage : Page
    {
        private readonly Entities1 _db = new Entities1();

        public PatientsPage()
        {
            InitializeComponent();
            LoadPatients();
            InitializeBirthYearFilter();
        }

        private void InitializeBirthYearFilter()
        {
            BirthYearFilter.Items.Clear();
            BirthYearFilter.Items.Add(new ComboBoxItem() { Content = "Все года" });

            var birthYears = _db.Patients
                .Where(p => p.BirthDate.HasValue) // Фильтруем только тех, у кого есть дата рождения
                .Select(p => p.BirthDate.Value.Year) // Берем Year только после проверки на null
                .Distinct()
                .OrderByDescending(y => y)
                .ToList();

            foreach (var year in birthYears)
            {
                BirthYearFilter.Items.Add(new ComboBoxItem() { Content = year.ToString() });
            }

            BirthYearFilter.SelectedIndex = 0;
        }

        private void LoadPatients()
        {
            UpdatePatients();
        }

        private void UpdatePatients()
        {
            var currentPatients = _db.Patients.AsQueryable();

            // Фильтрация по году рождения
            if (BirthYearFilter.SelectedIndex > 0 &&
                BirthYearFilter.SelectedItem is ComboBoxItem selectedYearItem)
            {
                if (int.TryParse(selectedYearItem.Content.ToString(), out int selectedYear))
                {
                    currentPatients = currentPatients
                        .Where(p => p.BirthDate.HasValue && p.BirthDate.Value.Year == selectedYear);
                }
            }

            // Фильтрация по ФИО
            if (!string.IsNullOrWhiteSpace(SearchPatientName.Text))
            {
                currentPatients = currentPatients.Where(p =>
                    p.FullName.ToLower().Contains(SearchPatientName.Text.ToLower()));
            }

            // Сортировка
            switch (SortPatientComboBox.SelectedIndex)
            {
                case 0: currentPatients = currentPatients.OrderBy(p => p.FullName); break;
                case 1: currentPatients = currentPatients.OrderBy(p => p.FullName); break;
                case 2: currentPatients = currentPatients.OrderByDescending(p => p.FullName); break;
                case 3: currentPatients = currentPatients.OrderBy(p => p.BirthDate); break;
                case 4: currentPatients = currentPatients.OrderByDescending(p => p.BirthDate); break;
            }

            PatientsDataGrid.ItemsSource = currentPatients.ToList();
        }

        private void BirthYearFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdatePatients();
        }

        private void SearchPatientName_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdatePatients();
        }

        private void SortPatientComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdatePatients();
        }

        private void CleanFilter_OnClick(object sender, RoutedEventArgs e)
        {
            SearchPatientName.Text = string.Empty;
            SortPatientComboBox.SelectedIndex = 0;
            BirthYearFilter.SelectedIndex = 0;
            UpdatePatients();
        }

        private void ButtonEdit_OnClick(object sender, RoutedEventArgs e)
        {
            if (PatientsDataGrid.SelectedItem is Patient selectedPatient)
            {
                NavigationService.Navigate(new AddEditPatientPage(selectedPatient));
            }
            else
            {
                MessageBox.Show("Выберите пациента для редактирования!",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void ButtonAdd_OnClick(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new AddEditPatientPage(null));
        }

        private void ButtonDel_OnClick(object sender, RoutedEventArgs e)
        {
            if (!(PatientsDataGrid.SelectedItem is Patient selectedPatient))
            {
                MessageBox.Show("Выберите пациента для удаления!",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (MessageBox.Show($"Удалить пациента '{selectedPatient.FullName}'?", "Подтверждение",
                    MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
            {
                return;
            }

            try
            {
                if (_db.Entry(selectedPatient).State == EntityState.Detached)
                {
                    _db.Patients.Attach(selectedPatient);
                }

                _db.Patients.Remove(selectedPatient);
                _db.SaveChanges();
                UpdatePatients();
                InitializeBirthYearFilter(); // Обновляем список годов после удаления
                MessageBox.Show("Пациент успешно удален!",
                    "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (System.Data.Entity.Infrastructure.DbUpdateException)
            {
                MessageBox.Show("Невозможно удалить пациента, так как он связан с другими записями в базе данных.",
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