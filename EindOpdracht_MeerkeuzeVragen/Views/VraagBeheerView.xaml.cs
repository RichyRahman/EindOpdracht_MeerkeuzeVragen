using MeerkeuzevragenApp.DOMEIN;
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
    /// Interaction logic for VraagBeheerView.xaml
    /// </summary>
    public partial class VraagBeheerView : Window
    {
        private List<Onderwerp> _onderwerpen;

        public VraagBeheerView()
        {
            InitializeComponent();
            LaadOnderwerpen();
        }
        private void LaadOnderwerpen()
        {
            _onderwerpen = App.VraagService.GetAlleOnderwerpen();
            CmbOnderwerp.ItemsSource = _onderwerpen;
            CmbOnderwerp.DisplayMemberPath = "Naam";
            CmbOnderwerp.SelectedValuePath = "ID";
            if (_onderwerpen.Any())
                CmbOnderwerp.SelectedIndex = 0;
        }

        private void LaadVragen()
        {
            if (CmbOnderwerp.SelectedValue == null) return;

            int onderwerpID = (int)CmbOnderwerp.SelectedValue;
            var vragen = ChkAlleenBeschikbaar.IsChecked == true
                ? App.VraagService.GetBeschikbareVragenPerOnderwerp(onderwerpID)
                : App.VraagService.GetAlleVragenPerOnderwerp(onderwerpID);

            DgVragen.ItemsSource = vragen;
        }

        private void CmbOnderwerp_SelectionChanged(object sender, SelectionChangedEventArgs e)
            => LaadVragen();

        private void Filter_Changed(object sender, RoutedEventArgs e)
            => LaadVragen();

        private void BtnVoegVraagToe_Click(object sender, RoutedEventArgs e)
        {
            if (CmbOnderwerp.SelectedValue == null)
            {
                ToonFeedback("⚠️ Kies een onderwerp.", false);
                return;
            }
            if (string.IsNullOrWhiteSpace(TxtNieuweVraag.Text) ||
                string.IsNullOrWhiteSpace(TxtAntwoordA.Text) ||
                string.IsNullOrWhiteSpace(TxtAntwoordB.Text) ||
                string.IsNullOrWhiteSpace(TxtAntwoordC.Text) ||
                string.IsNullOrWhiteSpace(TxtAntwoordD.Text))
            {
                ToonFeedback("⚠️ Vul de vraagtekst en alle antwoorden in.", false);
                return;
            }

            string correctLabel = (CmbCorrect.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "A";
            string moeilijkheid = (CmbNieuweMoeilijkheid.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Gemiddeld";

            var antwoorden = new List<string> { TxtAntwoordA.Text, TxtAntwoordB.Text, TxtAntwoordC.Text, TxtAntwoordD.Text };
            int correctIndex = correctLabel[0] - 'A';
            string correcteTekst = antwoorden[correctIndex];

            var vraag = new Vraag
            {
                OnderwerpID = (int)CmbOnderwerp.SelectedValue,
                Tekst = TxtNieuweVraag.Text.Trim(),
                Moeilijkheidsgraad = moeilijkheid,
                IsBeschikbaar = true,
                Antwoorden = antwoorden.Select((tekst, i) => new Antwoord
                {
                    Tekst = tekst,
                    IsCorrect = i == correctIndex,
                    Feedback = i != correctIndex ? $"Het correcte antwoord is: {correcteTekst}" : null
                }).ToList()
            };

            try
            {
                App.VraagService.VoegVraagToe(vraag);
                ToonFeedback("✅ Vraag succesvol toegevoegd!", true);
                // Reset velden
                TxtNieuweVraag.Text = "";
                TxtAntwoordA.Text = "";
                TxtAntwoordB.Text = "";
                TxtAntwoordC.Text = "";
                TxtAntwoordD.Text = "";
                LaadVragen();
            }
            catch (Exception ex)
            {
                ToonFeedback($"❌ Fout: {ex.Message}", false);
            }
        }

        private void BtnNietBeschikbaar_Click(object sender, RoutedEventArgs e)
        {
            if (DgVragen.SelectedItem is not Vraag geselecteerd)
            {
                ToonFeedback("⚠️ Selecteer eerst een vraag.", false);
                return;
            }
            if (!geselecteerd.IsBeschikbaar)
            {
                ToonFeedback("⚠️ Deze vraag is al niet beschikbaar.", false);
                return;
            }

            try
            {
                App.VraagService.StelNietBeschikbaar(geselecteerd.ID);
                ToonFeedback("✅ Vraag niet meer beschikbaar gesteld.", true);
                LaadVragen();
            }
            catch (Exception ex)
            {
                ToonFeedback($"❌ Fout: {ex.Message}", false);
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
