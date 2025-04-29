using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace HospitalD.Страницы_админа
{
    /// <summary>
    /// Логика взаимодействия для AdminPage.xaml
    /// </summary>
    public partial class AdminPage : Page
    {
        public AdminPage(User user)
        {
            InitializeComponent();
        }
        private void Departments_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new DepartmentsPage());
        }

        private void Positions_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new PositionsPage());
        }

        private void Staff_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new StaffPage());
        }

        private void Patients_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new PatientsPage());
        }

        private void Diagnoses_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new DiagnosesPage());
        }

        private void MedicalProcedures_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new MedicalProceduresPage());
        }

        private void Medications_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new MedicationsPage());
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new AuthPage());
        }
    }
}
