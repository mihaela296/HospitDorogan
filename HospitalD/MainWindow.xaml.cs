using System.Windows;

namespace HospitalD
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            contentFrame.Navigate(new AuthPage());
        }
    }
}