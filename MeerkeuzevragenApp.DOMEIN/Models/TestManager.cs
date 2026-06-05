using MeerkeuzevragenApp.DOMEIN.Exceptions;
using MeerkeuzevragenApp.DOMEIN.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeerkeuzevragenApp.DOMEIN.Models
{
    public class TestManager
    {
        private readonly ITestRepository _testRepo;
        private readonly IVraagRepository _vraagRepo;
        private readonly List<ITestParser> _parsers;
        private readonly Random _random = new();

        public TestManager(
            ITestRepository testRepo,
            IVraagRepository vraagRepo,
            List<ITestParser> parsers)
        {
            _testRepo = testRepo;
            _vraagRepo = vraagRepo;
            _parsers = parsers;
        }

        public Test GenereerTest(string naam, int onderwerpID, int aantalVragen)
        {
            var beschikbaar = _vraagRepo.GetBeschikbareVragenPerOnderwerp(onderwerpID);

            if (beschikbaar.Count < aantalVragen)
                throw new DomeinException(
                    $"Niet genoeg vragen beschikbaar. " +
                    $"Gevraagd: {aantalVragen}, beschikbaar: {beschikbaar.Count}.");

            var gekozen = beschikbaar
                .OrderBy(_ => _random.Next())
                .Take(aantalVragen)
                .ToList();

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

        public Test? GetTestMetVragen(int testID)
            => _testRepo.GetTestMetVragen(testID);

        public List<Test> GetAlleTests()
            => _testRepo.GetAlleTests();

        public void ImporteerBestand(string pad, string onderwerpNaam, string moeilijkheid)
        {
            var regels = File.ReadAllLines(pad)
                             .Select(r => r.Trim())
                             .Where(r => !string.IsNullOrWhiteSpace(r))
                             .ToArray();

            var onderwerpen = _vraagRepo.GetAlleOnderwerpen();
            var onderwerp = onderwerpen.FirstOrDefault(
                o => o.Naam.Equals(onderwerpNaam, StringComparison.OrdinalIgnoreCase));
            int onderwerpID = onderwerp?.ID ?? _vraagRepo.VoegOnderwerpToe(onderwerpNaam);

            var parser = _parsers.FirstOrDefault(p => p.KanVerwerken(regels))
                ?? throw new DomeinException(
                    "Geen geschikte parser gevonden voor dit bestandsformaat.");

            var vragen = parser.Parse(regels, onderwerpID, moeilijkheid);

            if (!vragen.Any())
                throw new DomeinException("Geen vragen gevonden in het bestand.");

            foreach (var vraag in vragen)
                _vraagRepo.VoegVraagToe(vraag);
        }

        public List<(string gebruikerID, int score, int totaal)> VerwerkBulk(
            int testID, string csvPad)
        {
            var test = _testRepo.GetTestMetVragen(testID)
                ?? throw new DomeinException("Test niet gevonden.");

            var resultaten = new List<(string, int, int)>();
            var lijnen = File.ReadAllLines(csvPad);

            foreach (var lijn in lijnen.Skip(1))
            {
                if (string.IsNullOrWhiteSpace(lijn)) continue;
                var delen = lijn.Split(',');
                if (delen.Length < 2) continue;

                string gebruikerID = delen[0].Trim();
                string antwoordString = delen[1].Trim();

                var antwoorden = new List<string>();
                for (int i = 0; i < test.Vragen.Count && i < antwoordString.Length; i++)
                {
                    int index = antwoordString[i] - 'A';
                    var vraag = test.Vragen[i];
                    if (index >= 0 && index < vraag.Antwoorden.Count)
                        antwoorden.Add(vraag.Antwoorden[index].Tekst);
                    else
                        antwoorden.Add(string.Empty);
                }

                var (score, totaal, _) = test.BerekenScore(antwoorden);
                resultaten.Add((gebruikerID, score, totaal));
            }

            return resultaten;
        }
    }
}
