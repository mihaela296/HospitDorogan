using System;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace HospitalD
{
    public partial class StaffPage : Page
    {
        private readonly Entities1 _db = new Entities1();

        public StaffPage()
        {
            InitializeComponent();
            LoadStaff();
            InitializeDepartmentFilter();
        }

        private void InitializeDepartmentFilter()
        {
            DepartmentFilter.Items.Clear();
            DepartmentFilter.Items.Add(new ComboBoxItem() { Content = "Все отделения" });

            var departments = _db.Departments.ToList();
            foreach (var department in departments)
            {
                DepartmentFilter.Items.Add(new ComboBoxItem()
                {
                    Content = department.Name,
                    Tag = department.ID_Department
                });
            }
            DepartmentFilter.SelectedIndex = 0;
        }

        private void LoadStaff()
        {
            // Получаем данные с явной загрузкой связанных сущностей
            var staffList = _db.Staffs
                .Include(s => s.Department)
                .Include(s => s.Position)
                .AsNoTracking()
                .ToList();

            // Применяем фильтрацию
            if (DepartmentFilter.SelectedIndex > 0 &&
                DepartmentFilter.SelectedItem is ComboBoxItem selectedItem &&
                selectedItem.Tag is int departmentId)
            {
                staffList = staffList.Where(s => s.ID_Department == departmentId).ToList();
            }

            // Применяем поиск
            if (!string.IsNullOrWhiteSpace(SearchLastName.Text))
            {
                staffList = staffList.Where(s => s.FullName.ToLower().Contains(SearchLastName.Text.ToLower())).ToList();
            }

            // Применяем сортировку
            switch (SortStaffComboBox.SelectedIndex)
            {
                case 1: staffList = staffList.OrderBy(s => s.FullName).ToList(); break;
                case 2: staffList = staffList.OrderByDescending(s => s.FullName).ToList(); break;
                case 3: staffList = staffList.OrderBy(s => s.Position?.Name).ToList(); break;
                case 4: staffList = staffList.OrderByDescending(s => s.Position?.Name).ToList(); break;
            }

            StaffDataGrid.ItemsSource = staffList;
        }



        private void DepartmentFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadStaff();
        }

        private void SearchLastName_TextChanged(object sender, TextChangedEventArgs e)
        {
            LoadStaff();
        }

        private void SortStaffComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadStaff();
        }

        private void CleanFilter_OnClick(object sender, RoutedEventArgs e)
        {
            SearchLastName.Text = string.Empty;
            SortStaffComboBox.SelectedIndex = 0;
            DepartmentFilter.SelectedIndex = 0;
            LoadStaff();
        }

        private void ButtonEdit_OnClick(object sender, RoutedEventArgs e)
        {
            if (StaffDataGrid.SelectedItem is Staff selectedStaff)
            {
                NavigationService.Navigate(new AddEditStaffPage(selectedStaff));
            }
            else
            {
                MessageBox.Show("Выберите сотрудника для редактирования!",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void ButtonAdd_OnClick(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new AddEditStaffPage());
        }

        private void ButtonDel_OnClick(object sender, RoutedEventArgs e)
        {
            if (!(StaffDataGrid.SelectedItem is Staff selectedStaff))
            {
                MessageBox.Show("Выберите сотрудника для удаления!",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (MessageBox.Show($"Удалить сотрудника '{selectedStaff.FullName}'?", "Подтверждение",
                    MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
            {
                return;
            }

            try
            {
                var staffToDelete = _db.Staffs.Find(selectedStaff.ID_Staff);
                if (staffToDelete != null)
                {
                    _db.Staffs.Remove(staffToDelete);
                    _db.SaveChanges();
                    LoadStaff();
                    MessageBox.Show("Сотрудник успешно удален!",
                        "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (System.Data.Entity.Infrastructure.DbUpdateException)
            {
                MessageBox.Show("Невозможно удалить сотрудника, так как он связан с другими записями в базе данных.",
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