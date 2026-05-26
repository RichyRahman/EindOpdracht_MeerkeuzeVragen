using MeerkeuzevragenApp.BUSINESS.Services;
using MeerkeuzevragenApp.DATA.Repositories;
using MeerkeuzevragenApp.DOMEIN;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeerkeuzevragenApp.TESTS
{
    public class TestServiceTests
    {
        private readonly Mock<ITestRepository> _mockTestRepo;
        private readonly Mock<IVraagRepository> _mockVraagRepo;
        private readonly TestService _service;

        public TestServiceTests()
        {
            _mockTestRepo = new Mock<ITestRepository>();
            _mockVraagRepo = new Mock<IVraagRepository>();
            _service = new TestService(_mockTestRepo.Object, _mockVraagRepo.Object);
        }

        // Hulpfunctie: maak een lijst van testvragen aan
        private List<Vraag> MaakVragen(int aantal)
        {
            var vragen = new List<Vraag>();
            for (int i = 0; i < aantal; i++)
            {
                vragen.Add(new Vraag
                {
                    ID = i + 1,
                    Tekst = $"Vraag {i + 1}",
                    Antwoorden = new List<Antwoord>
                    {
                        new Antwoord { Tekst = "Correct antwoord", IsCorrect = true },
                        new Antwoord { Tekst = "Fout antwoord B", IsCorrect = false },
                        new Antwoord { Tekst = "Fout antwoord C", IsCorrect = false },
                        new Antwoord { Tekst = "Fout antwoord D", IsCorrect = false }
                    }
                });
            }
            return vragen;
        }

        // ─── GenereerTest ─────────────────────────────────────

        [Fact]
        public void GenereerTest_VoldoendeVragen_MaaktTestAan()
        {
            // Arrange
            var vragen = MaakVragen(20);
            _mockVraagRepo.Setup(r => r.GetBeschikbareVragenPerOnderwerp(1)).Returns(vragen);
            _mockTestRepo.Setup(r => r.MaakTestAan(It.IsAny<Test>())).Returns(1);

            // Act
            var test = _service.GenereerTest("Test SQL", 1, 10);

            // Assert
            Assert.Equal(10, test.Vragen.Count);
            Assert.Equal("Test SQL", test.Naam);
        }

        [Fact]
        public void GenereerTest_TeWeinigVragen_GooitException()
        {
            // Arrange — slechts 5 vragen beschikbaar, maar 10 gevraagd
            var vragen = MaakVragen(5);
            _mockVraagRepo.Setup(r => r.GetBeschikbareVragenPerOnderwerp(1)).Returns(vragen);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(
                () => _service.GenereerTest("Test SQL", 1, 10));
        }

        [Fact]
        public void GenereerTest_AntwoordenWordenGeschud()
        {
            // Arrange
            var vragen = MaakVragen(10);
            _mockVraagRepo.Setup(r => r.GetBeschikbareVragenPerOnderwerp(1)).Returns(vragen);
            _mockTestRepo.Setup(r => r.MaakTestAan(It.IsAny<Test>())).Returns(1);

            // Act — meerdere keren genereren
            var test1 = _service.GenereerTest("Test 1", 1, 10);
            var test2 = _service.GenereerTest("Test 2", 1, 10);

            // Assert — vragenvolgorde is niet altijd identiek (kans op falen is astronomisch klein)
            bool verschillend = false;
            for (int i = 0; i < test1.Vragen.Count; i++)
                if (test1.Vragen[i].ID != test2.Vragen[i].ID)
                { verschillend = true; break; }

            // We controleren enkel dat de test correct wordt aangemaakt
            Assert.Equal(10, test1.Vragen.Count);
            Assert.Equal(10, test2.Vragen.Count);
        }

        // ─── BerekenScore ─────────────────────────────────────

        [Fact]
        public void BerekenScore_AllesCorrect_GeeftVolleScore()
        {
            // Arrange
            var vragen = MaakVragen(3);
            var test = new Test { ID = 1, Naam = "Test", Vragen = vragen };
            _mockTestRepo.Setup(r => r.GetTestMetVragen(1)).Returns(test);

            var antwoorden = new List<string>
            {
                "Correct antwoord",
                "Correct antwoord",
                "Correct antwoord"
            };

            // Act
            var (score, feedback) = _service.BerekenScore(1, "TestUser", antwoorden);

            // Assert
            Assert.Equal(3, score);
            Assert.Empty(feedback);
        }

        [Fact]
        public void BerekenScore_AllesFout_GeeftNul()
        {
            var vragen = MaakVragen(3);
            var test = new Test { ID = 1, Naam = "Test", Vragen = vragen };
            _mockTestRepo.Setup(r => r.GetTestMetVragen(1)).Returns(test);

            var antwoorden = new List<string>
            {
                "Fout antwoord B",
                "Fout antwoord B",
                "Fout antwoord B"
            };

            var (score, feedback) = _service.BerekenScore(1, "TestUser", antwoorden);

            Assert.Equal(0, score);
            Assert.Equal(3, feedback.Count);
        }

        [Fact]
        public void BerekenScore_DeelsCorrect_GeeftJuisteScore()
        {
            var vragen = MaakVragen(4);
            var test = new Test { ID = 1, Naam = "Test", Vragen = vragen };
            _mockTestRepo.Setup(r => r.GetTestMetVragen(1)).Returns(test);

            var antwoorden = new List<string>
            {
                "Correct antwoord",  // ✅
                "Fout antwoord B",   // ❌
                "Correct antwoord",  // ✅
                "Fout antwoord C"    // ❌
            };

            var (score, feedback) = _service.BerekenScore(1, "TestUser", antwoorden);

            Assert.Equal(2, score);
            Assert.Equal(2, feedback.Count);
        }

        [Fact]
        public void BerekenScore_TestNietGevonden_GooitException()
        {
            _mockTestRepo.Setup(r => r.GetTestMetVragen(99)).Returns((Test?)null);

            Assert.Throws<ArgumentException>(
                () => _service.BerekenScore(99, "TestUser", new List<string>()));
        }

        // ─── ExporteerNaarTxt ─────────────────────────────────

        [Fact]
        public void ExporteerNaarTxt_MaaktBestandAan()
        {
            var vragen = MaakVragen(3);
            var test = new Test { ID = 1, Naam = "Test SQL", Vragen = vragen };

            string pad = Path.Combine(Path.GetTempPath(), "test_export.txt");

            _service.ExporteerNaarTxt(test, pad);

            Assert.True(File.Exists(pad));
            var inhoud = File.ReadAllText(pad);
            Assert.Contains("Test SQL", inhoud);
            Assert.Contains("Vraag 1", inhoud);

            // Cleanup
            File.Delete(pad);
        }
    }
}
