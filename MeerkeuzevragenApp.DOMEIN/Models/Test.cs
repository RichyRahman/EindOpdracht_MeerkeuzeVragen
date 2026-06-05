using MeerkeuzevragenApp.DOMEIN.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeerkeuzevragenApp.DOMEIN.Models
{
    public class Test
    {
        private string _naam = string.Empty;

        public int ID { get; set; }
        public int OnderwerpID { get; set; }
        public Onderwerp? Onderwerp { get; set; }
        public List<Vraag> Vragen { get; set; } = new();

        public string Naam
        {
            get => _naam;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new DomeinException("Testnaam mag niet leeg zijn.");
                _naam = value.Trim();
            }
        }

        public (int score, int totaal, List<string> feedback) BerekenScore(
            List<string> gegevenAntwoorden)
        {
            if (gegevenAntwoorden.Count != Vragen.Count)
                throw new DomeinException(
                    $"Aantal antwoorden ({gegevenAntwoorden.Count}) " +
                    $"komt niet overeen met aantal vragen ({Vragen.Count}).");

            int score = 0;
            var feedback = new List<string>();

            for (int i = 0; i < Vragen.Count; i++)
            {
                var vraag = Vragen[i];
                var gegeven = gegevenAntwoorden[i];

                if (vraag.IsCorrectAntwoord(gegeven))
                {
                    score++;
                }
                else
                {
                    feedback.Add(
                        $"Vraag {i + 1}: {vraag.Tekst}\n" +
                        $"  Jouw antwoord: {gegeven}\n" +
                        $"  {vraag.GetFeedback(gegeven)}");
                }
            }

            return (score, Vragen.Count, feedback);
        }

        public void ExporteerNaarTxt(string bestandspad)
        {
            var lijnen = new List<string>();
            lijnen.Add($"Test: {Naam}");
            lijnen.Add(new string('-', 40));
            lijnen.Add("");

            for (int i = 0; i < Vragen.Count; i++)
            {
                var vraag = Vragen[i];
                lijnen.Add($"{i + 1}. {vraag.Tekst}");

                var antwoorden = vraag.GetGeschuddeAntwoorden();
                char label = 'A';
                foreach (var antwoord in antwoorden)
                {
                    lijnen.Add($"   {label}. {antwoord.Tekst}");
                    label++;
                }
                lijnen.Add("");
            }

            File.WriteAllLines(bestandspad, lijnen);
        }
    }
}
