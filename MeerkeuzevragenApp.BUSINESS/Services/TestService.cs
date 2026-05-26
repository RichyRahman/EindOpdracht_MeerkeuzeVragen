using MeerkeuzevragenApp.DATA.Repositories;
using MeerkeuzevragenApp.DOMEIN;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeerkeuzevragenApp.BUSINESS.Services
{
    public class TestService
    {
        private readonly ITestRepository _testRepo;
        private readonly IVraagRepository _vraagRepo;
        private readonly Random _random = new();

        public TestService(ITestRepository testRepo, IVraagRepository vraagRepo)
        {
            _testRepo = testRepo;
            _vraagRepo = vraagRepo;
        }

        // Deel 1: Genereer een test
        public Test GenereerTest(string naam, int onderwerpID, int aantalVragen)
        {
            var beschikbaar = _vraagRepo.GetBeschikbareVragenPerOnderwerp(onderwerpID);

            if (beschikbaar.Count < aantalVragen)
                throw new InvalidOperationException(
                    $"Niet genoeg vragen beschikbaar. Gevraagd: {aantalVragen}, beschikbaar: {beschikbaar.Count}");

            // Kies willekeurige vragen
            var gekozen = beschikbaar.OrderBy(_ => _random.Next()).Take(aantalVragen).ToList();

            // Schud antwoorden per vraag
            foreach (var vraag in gekozen)
                vraag.Antwoorden = vraag.Antwoorden.OrderBy(_ => _random.Next()).ToList();

            // Sla test op in DB
            var test = new Test { Naam = naam, OnderwerpID = onderwerpID };
            int testID = _testRepo.MaakTestAan(test);
            test.ID = testID;

            foreach (var vraag in gekozen)
            {
                _testRepo.VoegTestVraagToe(testID, vraag.ID);
                test.Vragen.Add(vraag);
            }

            return test;
        }

        // Deel 1: Exporteer test naar .txt
        public void ExporteerNaarTxt(Test test, string bestandspad)
        {
            var lijnen = new List<string>();
            lijnen.Add($"Test: {test.Naam}");
            lijnen.Add(new string('-', 40));

            for (int i = 0; i < test.Vragen.Count; i++)
            {
                var vraag = test.Vragen[i];
                lijnen.Add($"{i + 1}. {vraag.Tekst}");

                char label = 'A';
                foreach (var antwoord in vraag.Antwoorden)
                {
                    lijnen.Add($"   {label}. {antwoord.Tekst}");
                    label++;
                }
                lijnen.Add("");
            }

            File.WriteAllLines(bestandspad, lijnen);
        }

        public List<Test> GetAlleTests() => _testRepo.GetAlleTests();

        public Test GetTestMetVragen(int testID) => _testRepo.GetTestMetVragen(testID);

        // Deel 4: Bereken score
        public (int score, List<string> feedback) BerekenScore(
            int testID, string gebruikerNaam, List<string> gekozenAntwoorden)
        {
            var test = _testRepo.GetTestMetVragen(testID);
            if (test == null) throw new ArgumentException("Test niet gevonden.");

            // Zorg dat gebruiker bestaat
            int score = 0;
            var feedback = new List<string>();

            for (int i = 0; i < test.Vragen.Count && i < gekozenAntwoorden.Count; i++)
            {
                var vraag = test.Vragen[i];
                var gekozen = gekozenAntwoorden[i];
                var correctAntwoord = vraag.Antwoorden.FirstOrDefault(a => a.IsCorrect);

                if (correctAntwoord != null && gekozen == correctAntwoord.Tekst)
                {
                    score++;
                }
                else
                {
                    var feedbackTekst = correctAntwoord?.Feedback ?? $"Correct: {correctAntwoord?.Tekst}";
                    feedback.Add($"Vraag {i + 1}: {vraag.Tekst}\n  Jouw antwoord: {gekozen}\n  {feedbackTekst}");
                }
            }

            return (score, feedback);
        }

        // Deel 4: Bulk verwerking
        public List<(string gebruikerID, int score)> VerwerkBulk(int testID, string csvPad)
        {
            var resultaten = new List<(string, int)>();
            var lijnen = File.ReadAllLines(csvPad);

            foreach (var lijn in lijnen.Skip(1)) // header overslaan
            {
                if (string.IsNullOrWhiteSpace(lijn)) continue;
                var delen = lijn.Split(',');
                if (delen.Length < 2) continue;

                string gebruikerID = delen[0].Trim();
                string antwoordstring = delen[1].Trim();

                var test = _testRepo.GetTestMetVragen(testID);
                int score = 0;

                for (int i = 0; i < test.Vragen.Count && i < antwoordstring.Length; i++)
                {
                    // antwoordstring bevat letters A/B/C/D
                    // we mappen die naar de index in de geschudde antwoordenlijst
                    int index = antwoordstring[i] - 'A';
                    if (index >= 0 && index < test.Vragen[i].Antwoorden.Count)
                    {
                        var gekozen = test.Vragen[i].Antwoorden[index];
                        if (gekozen.IsCorrect) score++;
                    }
                }

                resultaten.Add((gebruikerID, score));
            }

            return resultaten;
        }
    }
}
