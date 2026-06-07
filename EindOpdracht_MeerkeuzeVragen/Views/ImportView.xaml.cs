using Microsoft.Win32;
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
using System.Windows.Shapes;

namespace MeerkeuzeVragenApp.UI.Views
{
    /// <summary>
    /// Interaction logic for ImportView.xaml
    /// </summary>
    public partial class ImportView : Window
    {
        public ImportView()
        {
            InitializeComponent();
        }

        private void BtnBladeren_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Tekstbestanden (*.txt)|*.txt",
                Title = "Kies een vragenlijst"
            };

            if (dialog.ShowDialog() == true)
                TxtBestandspad.Text = dialog.FileName;
        }

        private void BtnImporteer_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtBestandspad.Text))
            { ToonFeedback("Kies eerst een bestand.", false); return; }

            if (string.IsNullOrWhiteSpace(TxtOnderwerp.Text))
            { ToonFeedback("Vul een onderwerpnaam in.", false); return; }

            var moeilijkheid = (CmbMoeilijkheid.SelectedItem as
                System.Windows.Controls.ComboBoxItem)?.Content?.ToString() ?? "Gemiddeld";

            try
            {
                App.TestManager.ImporteerBestand(
                    TxtBestandspad.Text,
                    TxtOnderwerp.Text.Trim(),
                    moeilijkheid);

                ToonFeedback("✅ Import succesvol afgerond!", true);
                TxtBestandspad.Text = string.Empty;
                TxtOnderwerp.Text = string.Empty;
            }
            catch (Exception ex)
            {
                ToonFeedback($"❌ Fout: {ex.Message}", false);
            }
        }

        private void ToonFeedback(string bericht, bool succes)
        {
            FeedbackBorder.Background = succes
                ? new SolidColorBrush(Color.FromRgb(30, 70, 32))
                : new SolidColorBrush(Color.FromRgb(80, 20, 20));
            FeedbackBorder.BorderBrush = succes
                ? new SolidColorBrush(Color.FromRgb(76, 175, 80))
                : new SolidColorBrush(Color.FromRgb(244, 67, 54));
            FeedbackBorder.BorderThickness = new Thickness(1);
            FeedbackBorder.Visibility = Visibility.Visible;
            TxtFeedback.Text = bericht;
            TxtFeedback.Foreground = succes
                ? new SolidColorBrush(Color.FromRgb(129, 199, 132))
                : new SolidColorBrush(Color.FromRgb(239, 154, 154));
        }

        private void BtnSluiten_Click(object sender, RoutedEventArgs e)
            => Close();
    }
}
