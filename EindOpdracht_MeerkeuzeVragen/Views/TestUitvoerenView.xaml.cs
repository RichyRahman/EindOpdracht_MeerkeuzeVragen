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
            var testen = App.TestManager.GetAlleTests();
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

        // ── INTERACTIEF ────────────────────────────────────────

        private void BtnStartTest_Click(object sender, RoutedEventArgs e)
        {
            if (CmbTest.SelectedValue == null)
            { MessageBox.Show("Kies een test.", "Fout"); return; }
            if (string.IsNullOrWhiteSpace(TxtGebruiker.Text))
            { MessageBox.Show("Vul een naam in.", "Fout"); return; }

            int testID = (int)CmbTest.SelectedValue;
            _huidigTest = App.TestManager.GetTestMetVragen(testID);

            if (_huidigTest == null || !_huidigTest.Vragen.Any())
            { MessageBox.Show("Geen vragen gevonden.", "Fout"); return; }

            _vragen = _huidigTest.Vragen;
            _huidigeVraagIndex = 0;
            _gekozenAntwoorden = new List<string>();

            ScoreBorder.Visibility = Visibility.Collapsed;
            LstFeedback.ItemsSource = null;
            GrpVraag.Visibility = Visibility.Visible;
            BtnVolgend.Visibility = Visibility.Visible;
            BtnIndienen.Visibility = Visibility.Collapsed;

            ToonVraag();
        }

        private void ToonVraag()
        {
            var vraag = _vragen[_huidigeVraagIndex];
            TxtVraagNummer.Text =
                $"VRAAG {_huidigeVraagIndex + 1} VAN {_vragen.Count}";
            TxtVraagTekst.Text = vraag.Tekst;

            var rbs = new[] { RbA, RbB, RbC, RbD, RbE };
            char[] labels = { 'A', 'B', 'C', 'D', 'E' };

            foreach (var rb in rbs)
            {
                rb.IsChecked = false;
                rb.Visibility = Visibility.Collapsed;
            }

            var antwoorden = vraag.GetGeschuddeAntwoorden();
            for (int i = 0; i < antwoorden.Count && i < rbs.Length; i++)
            {
                rbs[i].Content = $"{labels[i]}.  {antwoorden[i].Tekst}";
                rbs[i].Tag = antwoorden[i].Tekst;
                rbs[i].Visibility = Visibility.Visible;
            }

            bool isLaatste = _huidigeVraagIndex == _vragen.Count - 1;
            BtnVolgend.Visibility = isLaatste
                ? Visibility.Collapsed : Visibility.Visible;
            BtnIndienen.Visibility = isLaatste
                ? Visibility.Visible : Visibility.Collapsed;
        }

        private string? GetGeselecteerdAntwoord()
            => new[] { RbA, RbB, RbC, RbD, RbE }
               .FirstOrDefault(rb => rb.IsChecked == true)?.Tag?.ToString();

        private void BtnVolgend_Click(object sender, RoutedEventArgs e)
        {
            var gekozen = GetGeselecteerdAntwoord();
            if (gekozen == null)
            { MessageBox.Show("Kies een antwoord.", "Fout"); return; }

            _gekozenAntwoorden.Add(gekozen);
            _huidigeVraagIndex++;
            ToonVraag();
        }

        private void BtnIndienen_Click(object sender, RoutedEventArgs e)
        {
            var gekozen = GetGeselecteerdAntwoord();
            if (gekozen == null)
            { MessageBox.Show("Kies een antwoord.", "Fout"); return; }

            _gekozenAntwoorden.Add(gekozen);

            var (score, totaal, feedback) =
                _huidigTest!.BerekenScore(_gekozenAntwoorden);

            GrpVraag.Visibility = Visibility.Collapsed;
            BtnVolgend.Visibility = Visibility.Collapsed;
            BtnIndienen.Visibility = Visibility.Collapsed;

            int pct = totaal > 0 ? score * 100 / totaal : 0;
            bool geslaagd = pct >= 50;

            TxtScore.Text = $"🎯  Score: {score}/{totaal}  ({pct}%)  " +
                            (geslaagd ? "✅ Geslaagd" : "❌ Niet geslaagd");
            TxtScore.Foreground = geslaagd
                ? new SolidColorBrush(Color.FromRgb(129, 199, 132))
                : new SolidColorBrush(Color.FromRgb(239, 154, 154));

            ScoreBorder.Visibility = Visibility.Visible;
            LstFeedback.ItemsSource = feedback.Any()
                ? feedback
                : new List<string> { "✅ Alle vragen correct beantwoord!" };
        }

        // ── BULK ───────────────────────────────────────────────

        private void BtnBulkBladeren_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Filter = "CSV bestanden (*.csv)|*.csv|Tekstbestanden (*.txt)|*.txt"
            };
            if (dialog.ShowDialog() == true)
                TxtBulkPad.Text = dialog.FileName;
        }

        private void BtnVerwerkBulk_Click(object sender, RoutedEventArgs e)
        {
            if (CmbBulkTest.SelectedValue == null)
            { ToonBulkFeedback("⚠️ Kies een test.", false); return; }
            if (string.IsNullOrWhiteSpace(TxtBulkPad.Text))
            { ToonBulkFeedback("⚠️ Kies een CSV bestand.", false); return; }

            try
            {
                int testID = (int)CmbBulkTest.SelectedValue;
                var resultaten = App.TestManager.VerwerkBulk(testID, TxtBulkPad.Text);

                DgBulkResultaten.ItemsSource = resultaten.Select(r => new
                {
                    GebruikerID = r.gebruikerID,
                    Score = $"{r.score}/{r.totaal}",
                    Resultaat = r.score * 100 / (r.totaal > 0 ? r.totaal : 1) >= 50
                        ? "✅ Geslaagd" : "❌ Niet geslaagd"
                }).ToList();

                ToonBulkFeedback(
                    $"✅ {resultaten.Count} gebruikers verwerkt.", true);
            }
            catch (Exception ex)
            {
                ToonBulkFeedback($"❌ {ex.Message}", false);
            }
        }

        private void ToonBulkFeedback(string bericht, bool succes)
        {
            TxtBulkFeedback.Foreground = succes
                ? new SolidColorBrush(Color.FromRgb(129, 199, 132))
                : new SolidColorBrush(Color.FromRgb(239, 154, 154));
            TxtBulkFeedback.Text = bericht;
        }

        private void BtnSluiten_Click(object sender, RoutedEventArgs e)
            => Close();
    }
}

