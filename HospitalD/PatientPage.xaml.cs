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

namespace HospitalD
{
    /// <summary>
    /// Логика взаимодействия для PatientPage.xaml
    /// </summary>
    public partial class PatientPage : Page
    {
        private Patient _patient;
        public PatientPage(User user)
        {
            InitializeComponent();
        }
        private void MedicalRecord_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new PatientMedicalRecordPage(_patient));
        }

        private void Appointment_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new MakeAppointmentPage(_patient));
        }

        private void MyAppointments_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new PatientAppointmentsPage(_patient));
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new AuthPage());
        }
    }
}
