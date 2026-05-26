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
    /// Interaction logic for TestUitvoerenView.xaml
    /// </summary>
    public partial class TestUitvoerenView : Window
    {
        private Test? _huidigTest;
        private List<Vraag> _vragen = new();
        private int _huidigeVraagIndex = 0;
        private List<string> _gekozenAntwoorden = new();
        public TestUitvoerenView()
        {
            InitializeComponent();
            LaadTesten();
        }

        private void LaadTesten()
        {
            var testen = App.TestService.GetAlleTests();
            CmbTest.ItemsSource = testen;
            CmbTest.DisplayMemberPath = "Naam";
            CmbTest.SelectedValuePath = "ID";

            CmbBulkTest.ItemsSource = testen;
            CmbBulkTest.DisplayMemberPath = "Naam";
            CmbBulkTest.SelectedValuePath = "ID";

            if (testen.Any())
            {
                CmbTest.SelectedIndex = 0;
                CmbBulkTest.SelectedIndex = 0;
            }
        }

        // ─── INTERACTIEF ────────────────────────────────────────

        private void BtnStartTest_Click(object sender, RoutedEventArgs e)
        {
            if (CmbTest.SelectedValue == null)
            {
                MessageBox.Show("Kies een test.", "Fout");
                return;
            }
            if (string.IsNullOrWhiteSpace(TxtGebruiker.Text))
            {
                MessageBox.Show("Vul een gebruikersnaam in.", "Fout");
                return;
            }

            int testID = (int)CmbTest.SelectedValue;
            _huidigTest = App.TestService.GetTestMetVragen(testID);

            if (_huidigTest == null || !_huidigTest.Vragen.Any())
            {
                MessageBox.Show("Geen vragen gevonden voor deze test.", "Fout");
                return;
            }

            _vragen = _huidigTest.Vragen;
            _huidigeVraagIndex = 0;
            _gekozenAntwoorden = new List<string>();

            TxtScore.Visibility = Visibility.Collapsed;
            LstFeedback.ItemsSource = null;
            GrpVraag.Visibility = Visibility.Visible;
            BtnVolgend.Visibility = Visibility.Visible;
            BtnIndienen.Visibility = Visibility.Collapsed;

            ToonVraag();
        }

        private void ToonVraag()
        {
            var vraag = _vragen[_huidigeVraagIndex];
            TxtVraagNummer.Text = $"Vraag {_huidigeVraagIndex + 1} van {_vragen.Count}";
            TxtVraagTekst.Text = vraag.Tekst;

            var radioButtons = new[] { RbA, RbB, RbC, RbD, RbE };

            // Reset alle radio buttons
            foreach (var rb in radioButtons)
            {
                rb.IsChecked = false;
                rb.Visibility = Visibility.Collapsed;
            }

            // Vul antwoorden in
            var antwoorden = vraag.Antwoorden;
            char[] labels = { 'A', 'B', 'C', 'D', 'E' };

            for (int i = 0; i < antwoorden.Count && i < radioButtons.Length; i++)
            {
                radioButtons[i].Content = $"{labels[i]}. {antwoorden[i].Tekst}";
                radioButtons[i].Visibility = Visibility.Visible;
                radioButtons[i].Tag = antwoorden[i].Tekst;
            }

            // Laatste vraag → toon Indienen ipv Volgende
            if (_huidigeVraagIndex == _vragen.Count - 1)
            {
                BtnVolgend.Visibility = Visibility.Collapsed;
                BtnIndienen.Visibility = Visibility.Visible;
            }
            else
            {
                BtnVolgend.Visibility = Visibility.Visible;
                BtnIndienen.Visibility = Visibility.Collapsed;
            }
        }

        private string? GetGeselecteerdAntwoord()
        {
            var radioButtons = new[] { RbA, RbB, RbC, RbD, RbE };
            return radioButtons.FirstOrDefault(rb => rb.IsChecked == true)?.Tag?.ToString();
        }

        private void BtnVolgend_Click(object sender, RoutedEventArgs e)
        {
            var gekozen = GetGeselecteerdAntwoord();
            if (gekozen == null)
            {
                MessageBox.Show("Kies een antwoord.", "Fout");
                return;
            }

            _gekozenAntwoorden.Add(gekozen);
            _huidigeVraagIndex++;
            ToonVraag();
        }

        private void BtnIndienen_Click(object sender, RoutedEventArgs e)
        {
            var gekozen = GetGeselecteerdAntwoord();
            if (gekozen == null)
            {
                MessageBox.Show("Kies een antwoord.", "Fout");
                return;
            }

            _gekozenAntwoorden.Add(gekozen);

            try
            {
                var (score, feedback) = App.TestService.BerekenScore(
                    _huidigTest!.ID,
                    TxtGebruiker.Text.Trim(),
                    _gekozenAntwoorden);

                GrpVraag.Visibility = Visibility.Collapsed;
                BtnVolgend.Visibility = Visibility.Collapsed;
                BtnIndienen.Visibility = Visibility.Collapsed;

                TxtScore.Text = $"🎯 Score: {score}/{_vragen.Count} ({score * 100 / _vragen.Count}%)";
                TxtScore.Foreground = score >= _vragen.Count / 2
                    ? System.Windows.Media.Brushes.Green
                    : System.Windows.Media.Brushes.Red;
                TxtScore.Visibility = Visibility.Visible;

                LstFeedback.ItemsSource = feedback.Any()
                    ? feedback
                    : new List<string> { "✅ Alle vragen correct beantwoord!" };
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fout: {ex.Message}", "Fout");
            }
        }

        // ─── BULK ────────────────────────────────────────────────

        private void BtnBulkBladeren_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Filter = "CSV bestanden (*.csv)|*.csv|Tekstbestanden (*.txt)|*.txt",
                Title = "Kies een bulk antwoordbestand"
            };
            if (dialog.ShowDialog() == true)
                TxtBulkPad.Text = dialog.FileName;
        }

        private void BtnVerwerkBulk_Click(object sender, RoutedEventArgs e)
        {
            if (CmbBulkTest.SelectedValue == null)
            {
                TxtBulkFeedback.Text = "⚠️ Kies een test.";
                return;
            }
            if (string.IsNullOrWhiteSpace(TxtBulkPad.Text))
            {
                TxtBulkFeedback.Text = "⚠️ Kies een CSV bestand.";
                return;
            }

            try
            {
                int testID = (int)CmbBulkTest.SelectedValue;
                var resultaten = App.TestService.VerwerkBulk(testID, TxtBulkPad.Text);

                var test = App.TestService.GetTestMetVragen(testID);
                int aantalVragen = test?.Vragen.Count ?? 1;

                var weergave = resultaten.Select(r => new
                {
                    GebruikerID = r.gebruikerID,
                    Score = $"{r.score}/{aantalVragen}",
                    Resultaat = r.score >= aantalVragen / 2 ? "✅ Geslaagd" : "❌ Niet geslaagd"
                }).ToList();

                DgBulkResultaten.ItemsSource = weergave;
                TxtBulkFeedback.Foreground = System.Windows.Media.Brushes.Green;
                TxtBulkFeedback.Text = $"✅ {resultaten.Count} gebruikers verwerkt.";
            }
            catch (Exception ex)
            {
                TxtBulkFeedback.Foreground = System.Windows.Media.Brushes.Red;
                TxtBulkFeedback.Text = $"❌ Fout: {ex.Message}";
            }
        }

        private void BtnSluiten_Click(object sender, RoutedEventArgs e)
            => this.Close();
    }
}

