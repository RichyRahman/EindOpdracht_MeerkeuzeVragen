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
            // Validatie
            if (string.IsNullOrWhiteSpace(TxtBestandspad.Text))
            {
                TxtFeedback.Foreground = System.Windows.Media.Brushes.Red;
                TxtFeedback.Text = "⚠️ Kies eerst een bestand.";
                return;
            }

            if (string.IsNullOrWhiteSpace(TxtOnderwerp.Text))
            {
                TxtFeedback.Foreground = System.Windows.Media.Brushes.Red;
                TxtFeedback.Text = "⚠️ Vul een onderwerpnaam in.";
                return;
            }

            var moeilijkheid = (CmbMoeilijkheid.SelectedItem as System.Windows.Controls.ComboBoxItem)?.Content?.ToString() ?? "Gemiddeld";

            try
            {
                App.ImportService.ImporteerBestand(
                    TxtBestandspad.Text,
                    TxtOnderwerp.Text.Trim(),
                    moeilijkheid);

                TxtFeedback.Foreground = System.Windows.Media.Brushes.Green;
                TxtFeedback.Text = "✅ Import succesvol afgerond!";
            }
            catch (Exception ex)
            {
                TxtFeedback.Foreground = System.Windows.Media.Brushes.Red;
                TxtFeedback.Text = $"❌ Fout bij importeren: {ex.Message}";
            }

        }
        private void BtnSluiten_Click(object sender, RoutedEventArgs e)
            => this.Close();
    }
}
