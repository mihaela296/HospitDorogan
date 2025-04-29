using System;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace HospitalD
{
    public partial class AddEditMedicationPage : Page
    {
        private Entities1 _db = new Entities1();
        private Medication _currentMedication;

        public AddEditMedicationPage(Medication selectedMedication = null)
        {
            InitializeComponent();

            if (selectedMedication != null)
            {
                _currentMedication = _db.Medications.Find(selectedMedication.ID_Medication);
                TitleTextBlock.Text = "Редактирование лекарства";
                NameTextBox.Text = _currentMedication.Name;
                DosageTextBox.Text = _currentMedication.DailyDosage.ToString();
                DurationTextBox.Text = _currentMedication.Duration.ToString();
            }
            else
            {
                _currentMedication = new Medication();
                TitleTextBlock.Text = "Добавление лекарства";
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NameTextBox.Text))
            {
                MessageBox.Show("Введите название лекарства!", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!decimal.TryParse(DosageTextBox.Text, out decimal dosage) || dosage <= 0)
            {
                MessageBox.Show("Введите корректную дневную дозу (число > 0)!", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!int.TryParse(DurationTextBox.Text, out int duration) || duration <= 0)
            {
                MessageBox.Show("Введите корректную продолжительность (целое число > 0)!", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                _currentMedication.Name = NameTextBox.Text;
                _currentMedication.DailyDosage = (int)dosage;
                _currentMedication.Duration = duration;

                if (_currentMedication.ID_Medication == 0)
                {
                    _db.Medications.Add(_currentMedication);
                }

                _db.SaveChanges();
                MessageBox.Show("Данные сохранены успешно!", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                NavigationService.GoBack();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.InnerException?.Message ?? ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }
    }
}