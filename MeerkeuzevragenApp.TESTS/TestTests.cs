using MeerkeuzevragenApp.DOMEIN.Exceptions;
using MeerkeuzevragenApp.DOMEIN.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeerkeuzevragenApp.TESTS
{
    public class TestTests
    {
        private Test MaakGeldigeTest()
        {
            return new Test
            {
                ID = 1,
                Naam = "Test SQL 1",
                OnderwerpID = 1,
                Vragen = new List<Vraag>
                {
                    MaakVraagMetAntwoorden(1, "Correct A"),
                    MaakVraagMetAntwoorden(2, "Correct B"),
                    MaakVraagMetAntwoorden(3, "Correct C")
                }
            };
        }

        private Vraag MaakVraagMetAntwoorden(int id, string correctTekst)
        {
            return new Vraag
            {
                ID = id,
                Tekst = $"Vraag {id}",
                Moeilijkheidsgraad = "Makkelijk",
                Antwoorden = new List<Antwoord>
                {
                    new Antwoord { Tekst = correctTekst,   IsCorrect = true  },
                    new Antwoord { Tekst = "Fout antwoord", IsCorrect = false,
                                   Feedback = $"Het correcte antwoord is: {correctTekst}" }
                }
            };
        }

        // Naam property tests
        [Fact]
        public void Naam_LegeNaam_GooitDomeinException()
        {
            Assert.Throws<DomeinException>(() =>
                new Test { Naam = "" });
        }

        [Fact]
        public void Naam_GeldigeNaam_WordtOpgeslagen()
        {
            var test = new Test { Naam = "  Test SQL  " };
            Assert.Equal("Test SQL", test.Naam);
        }

        // BerekenScore tests
        [Fact]
        public void BerekenScore_AllesCorrect_GeeftVolleScore()
        {
            var test = MaakGeldigeTest();
            var antwoorden = new List<string>
            {
                "Correct A", "Correct B", "Correct C"
            };

            var (score, totaal, feedback) = test.BerekenScore(antwoorden);

            Assert.Equal(3, score);
            Assert.Equal(3, totaal);
            Assert.Empty(feedback);
        }

        [Fact]
        public void BerekenScore_AllesFout_GeeftNulScore()
        {
            var test = MaakGeldigeTest();
            var antwoorden = new List<string>
            {
                "Fout antwoord", "Fout antwoord", "Fout antwoord"
            };

            var (score, totaal, feedback) = test.BerekenScore(antwoorden);

            Assert.Equal(0, score);
            Assert.Equal(3, totaal);
            Assert.Equal(3, feedback.Count);
        }

        [Fact]
        public void BerekenScore_DeelsCorrect_GeeftJuisteScore()
        {
            var test = MaakGeldigeTest();
            var antwoorden = new List<string>
            {
                "Correct A",    // ✅
                "Fout antwoord", // ❌
                "Correct C"     // ✅
            };

            var (score, totaal, feedback) = test.BerekenScore(antwoorden);

            Assert.Equal(2, score);
            Assert.Equal(3, totaal);
            Assert.Single(feedback);
        }

        [Fact]
        public void BerekenScore_FoutAantalAntwoorden_GooitDomeinException()
        {
            var test = MaakGeldigeTest();
            var antwoorden = new List<string> { "Correct A" }; // te weinig

            Assert.Throws<DomeinException>(
                () => test.BerekenScore(antwoorden));
        }

        [Fact]
        public void BerekenScore_FeedbackBevatVraagtekst()
        {
            var test = MaakGeldigeTest();
            var antwoorden = new List<string>
            {
                "Fout antwoord", "Fout antwoord", "Fout antwoord"
            };

            var (_, _, feedback) = test.BerekenScore(antwoorden);

            Assert.Contains(feedback, f => f.Contains("Vraag 1"));
        }

        // ExporteerNaarTxt tests
        [Fact]
        public void ExporteerNaarTxt_MaaktBestandAan()
        {
            var test = MaakGeldigeTest();
            string pad = Path.Combine(Path.GetTempPath(), "test_export.txt");

            test.ExporteerNaarTxt(pad);

            Assert.True(File.Exists(pad));
            File.Delete(pad);
        }

        [Fact]
        public void ExporteerNaarTxt_BevatTestNaam()
        {
            var test = MaakGeldigeTest();
            string pad = Path.Combine(Path.GetTempPath(), "test_naam.txt");

            test.ExporteerNaarTxt(pad);
            string inhoud = File.ReadAllText(pad);

            Assert.Contains("Test SQL 1", inhoud);
            File.Delete(pad);
        }

        [Fact]
        public void ExporteerNaarTxt_BevatVraagTekst()
        {
            var test = MaakGeldigeTest();
            string pad = Path.Combine(Path.GetTempPath(), "test_vragen.txt");

            test.ExporteerNaarTxt(pad);
            string inhoud = File.ReadAllText(pad);

            Assert.Contains("Vraag 1", inhoud);
            File.Delete(pad);
        }
    }
}
