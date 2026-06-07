using MeerkeuzevragenApp.DOMEIN;
using MeerkeuzevragenApp.DOMEIN.Models;
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
            var onderwerpen = App.VraagManager.GetAlleOnderwerpen();
            CmbOnderwerp.ItemsSource = onderwerpen;
            CmbOnderwerp.DisplayMemberPath = "Naam";
            CmbOnderwerp.SelectedValuePath = "ID";
            if (onderwerpen.Any()) CmbOnderwerp.SelectedIndex = 0;
        }

        private void LaadBestaandeTesten()
            => DgTesten.ItemsSource = App.TestManager.GetAlleTests();

        private void BtnGenereer_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtTestNaam.Text))
            { ToonFeedback("⚠️ Vul een testnaam in.", false); return; }

            if (CmbOnderwerp.SelectedValue == null)
            { ToonFeedback("⚠️ Kies een onderwerp.", false); return; }

            if (!int.TryParse(TxtAantalVragen.Text, out int aantal) || aantal <= 0)
            { ToonFeedback("⚠️ Vul een geldig aantal in.", false); return; }

            try
            {
                int onderwerpID = (int)CmbOnderwerp.SelectedValue;
                _huidigTest = App.TestManager.GenereerTest(
                    TxtTestNaam.Text.Trim(), onderwerpID, aantal);

                TxtTestInfo.Foreground = new SolidColorBrush(
                    Color.FromRgb(129, 199, 132));
                TxtTestInfo.Text =
                    $"✅ '{_huidigTest.Naam}' — {_huidigTest.Vragen.Count} vragen " +
                    $"(ID: {_huidigTest.ID})";
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
                FileName = $"{_huidigTest.Naam}.txt"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    _huidigTest.ExporteerNaarTxt(dialog.FileName);
                    ToonFeedback($"✅ Geëxporteerd naar {dialog.FileName}", true);
                }
                catch (Exception ex)
                {
                    ToonFeedback($"❌ {ex.Message}", false);
                }
            }
        }

        private void ToonFeedback(string bericht, bool succes)
        {
            TxtFeedback.Foreground = succes
                ? new SolidColorBrush(Color.FromRgb(129, 199, 132))
                : new SolidColorBrush(Color.FromRgb(239, 154, 154));
            TxtFeedback.Text = bericht;
        }

        private void BtnSluiten_Click(object sender, RoutedEventArgs e)
            => Close();
    }
}

