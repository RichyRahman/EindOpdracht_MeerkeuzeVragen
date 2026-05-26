using MeerkeuzevragenApp.DOMEIN;
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
    /// Interaction logic for TestBeheerView.xaml
    /// </summary>
    public partial class TestBeheerView : Window
    {
        private Test? _huidigTest;
        public TestBeheerView()
        {
            InitializeComponent();
            LaadOnderwerpen();
            LaadBestaandeTesten();
        }

        private void LaadOnderwerpen()
        {
            var onderwerpen = App.VraagService.GetAlleOnderwerpen();
            CmbOnderwerp.ItemsSource = onderwerpen;
            CmbOnderwerp.DisplayMemberPath = "Naam";
            CmbOnderwerp.SelectedValuePath = "ID";
            if (onderwerpen.Any())
                CmbOnderwerp.SelectedIndex = 0;
        }

        private void LaadBestaandeTesten()
        {
            DgTesten.ItemsSource = App.TestService.GetAlleTests();
        }

        private void BtnGenereer_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtTestNaam.Text))
            {
                ToonFeedback("⚠️ Vul een testnaam in.", false);
                return;
            }
            if (CmbOnderwerp.SelectedValue == null)
            {
                ToonFeedback("⚠️ Kies een onderwerp.", false);
                return;
            }
            if (!int.TryParse(TxtAantalVragen.Text, out int aantal) || aantal <= 0)
            {
                ToonFeedback("⚠️ Vul een geldig aantal vragen in.", false);
                return;
            }

            try
            {
                int onderwerpID = (int)CmbOnderwerp.SelectedValue;
                _huidigTest = App.TestService.GenereerTest(
                    TxtTestNaam.Text.Trim(), onderwerpID, aantal);

                TxtTestInfo.Foreground = System.Windows.Media.Brushes.Black;
                TxtTestInfo.Text = $"✅ Test '{_huidigTest.Naam}' gegenereerd met {_huidigTest.Vragen.Count} vragen. (ID: {_huidigTest.ID})";
                BtnExporteer.IsEnabled = true;
                LaadBestaandeTesten();
                ToonFeedback("", true);
            }
            catch (Exception ex)
            {
                ToonFeedback($"❌ {ex.Message}", false);
            }
        }

        private void BtnExporteer_Click(object sender, RoutedEventArgs e)
        {
            if (_huidigTest == null) return;

            var dialog = new SaveFileDialog
            {
                Filter = "Tekstbestanden (*.txt)|*.txt",
                FileName = $"{_huidigTest.Naam}.txt",
                Title = "Sla test op als..."
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    App.TestService.ExporteerNaarTxt(_huidigTest, dialog.FileName);
                    ToonFeedback($"✅ Test geëxporteerd naar {dialog.FileName}", true);
                }
                catch (Exception ex)
                {
                    ToonFeedback($"❌ Fout bij exporteren: {ex.Message}", false);
                }
            }
        }

        private void ToonFeedback(string bericht, bool succes)
        {
            TxtFeedback.Foreground = succes
                ? System.Windows.Media.Brushes.Green
                : System.Windows.Media.Brushes.Red;
            TxtFeedback.Text = bericht;
        }

        private void BtnSluiten_Click(object sender, RoutedEventArgs e)
            => this.Close();
    }
}

