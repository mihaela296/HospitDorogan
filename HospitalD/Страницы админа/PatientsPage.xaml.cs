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
        private bool _shouldRefresh = false;

        public PatientsPage()
        {
            InitializeComponent();
            LoadPatients();
            InitializeBirthYearFilter();

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
                UpdatePatients();
                InitializeBirthYearFilter(); // Обновляем фильтр годов
                _shouldRefresh = false;
            }
        }

        private void InitializeBirthYearFilter()
        {
            BirthYearFilter.Items.Clear();
            BirthYearFilter.Items.Add(new ComboBoxItem() { Content = "Все года" });

            var birthYears = _db.Patients
                .Where(p => p.BirthDate.HasValue)
                .Select(p => p.BirthDate.Value.Year)
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
            try
            {
                var currentPatients = _db.Patients.AsNoTracking().AsQueryable();

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
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при обновлении данных: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
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
                _shouldRefresh = true;
                var editPage = new AddEditPatientPage(selectedPatient);
                editPage.PatientSaved += (s, args) => _shouldRefresh = true;
                NavigationService.Navigate(editPage);
            }
            else
            {
                MessageBox.Show("Выберите пациента для редактирования!",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void ButtonAdd_OnClick(object sender, RoutedEventArgs e)
        {
            _shouldRefresh = true;
            var addPage = new AddEditPatientPage();
            addPage.PatientSaved += (s, args) => _shouldRefresh = true;
            NavigationService.Navigate(addPage);
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
                _db.Patients.Attach(selectedPatient);
                _db.Patients.Remove(selectedPatient);
                _db.SaveChanges();
                UpdatePatients();
                InitializeBirthYearFilter(); // Обновляем список годов после удаления
                MessageBox.Show("Пациент успешно удален!",
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