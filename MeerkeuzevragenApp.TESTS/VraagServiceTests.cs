using MeerkeuzevragenApp.BUSINESS.Services;
using MeerkeuzevragenApp.DATA.Repositories;
using MeerkeuzevragenApp.DOMEIN;
using Moq;

namespace MeerkeuzevragenApp.TESTS
{
    public class VraagServiceTests
    {
        private readonly Mock<IVraagRepository> _mockRepo;
        private readonly VraagService _service;

        public VraagServiceTests()
        {
            _mockRepo = new Mock<IVraagRepository>();
            _service = new VraagService(_mockRepo.Object);
        }

        // ─── GetAlleOnderwerpen ───────────────────────────────

        [Fact]
        public void GetAlleOnderwerpen_GeeftLijstTerug()
        {
            // Arrange
            var onderwerpen = new List<Onderwerp>
            {
                new Onderwerp { ID = 1, Naam = "SQL" },
                new Onderwerp { ID = 2, Naam = "Muziek" }
            };
            _mockRepo.Setup(r => r.GetAlleOnderwerpen()).Returns(onderwerpen);

            // Act
            var resultaat = _service.GetAlleOnderwerpen();

            // Assert
            Assert.Equal(2, resultaat.Count);
            Assert.Equal("SQL", resultaat[0].Naam);
        }

        [Fact]
        public void GetAlleOnderwerpen_LeegeLijst_GeeftLeegeLijstTerug()
        {
            _mockRepo.Setup(r => r.GetAlleOnderwerpen()).Returns(new List<Onderwerp>());
            var resultaat = _service.GetAlleOnderwerpen();
            Assert.Empty(resultaat);
        }

        // ─── VoegVraagToe ─────────────────────────────────────

        [Fact]
        public void VoegVraagToe_GeldigeVraag_RoeptRepositoryAan()
        {
            // Arrange
            var vraag = new Vraag
            {
                Tekst = "Wat is SQL?",
                OnderwerpID = 1,
                Moeilijkheidsgraad = "Makkelijk",
                Antwoorden = new List<Antwoord>
                {
                    new Antwoord { Tekst = "Structured Query Language", IsCorrect = true },
                    new Antwoord { Tekst = "Simple Query Language", IsCorrect = false }
                }
            };

            // Act
            _service.VoegVraagToe(vraag);

            // Assert
            _mockRepo.Verify(r => r.VoegVraagToe(vraag), Times.Once);
        }

        [Fact]
        public void VoegVraagToe_LegeTekst_GooitException()
        {
            var vraag = new Vraag
            {
                Tekst = "",
                Antwoorden = new List<Antwoord>
                {
                    new Antwoord { Tekst = "A", IsCorrect = true },
                    new Antwoord { Tekst = "B", IsCorrect = false }
                }
            };

            Assert.Throws<ArgumentException>(() => _service.VoegVraagToe(vraag));
        }

        [Fact]
        public void VoegVraagToe_MinderDanTweeAntwoorden_GooitException()
        {
            var vraag = new Vraag
            {
                Tekst = "Wat is SQL?",
                Antwoorden = new List<Antwoord>
                {
                    new Antwoord { Tekst = "Structured Query Language", IsCorrect = true }
                }
            };

            Assert.Throws<ArgumentException>(() => _service.VoegVraagToe(vraag));
        }

        [Fact]
        public void VoegVraagToe_GeenCorrectAntwoord_GooitException()
        {
            var vraag = new Vraag
            {
                Tekst = "Wat is SQL?",
                Antwoorden = new List<Antwoord>
                {
                    new Antwoord { Tekst = "Antwoord A", IsCorrect = false },
                    new Antwoord { Tekst = "Antwoord B", IsCorrect = false }
                }
            };

            Assert.Throws<ArgumentException>(() => _service.VoegVraagToe(vraag));
        }

        // ─── StelNietBeschikbaar ──────────────────────────────

        [Fact]
        public void StelNietBeschikbaar_RoeptRepositoryAan()
        {
            _service.StelNietBeschikbaar(1);
            _mockRepo.Verify(r => r.StelNietBeschikbaar(1), Times.Once);
        }

        // ─── VoegOnderwerpToe ─────────────────────────────────

        [Fact]
        public void VoegOnderwerpToe_GeldigeNaam_RoeptRepositoryAan()
        {
            _mockRepo.Setup(r => r.VoegOnderwerpToe("SQL")).Returns(1);
            var id = _service.VoegOnderwerpToe("SQL");
            Assert.Equal(1, id);
            _mockRepo.Verify(r => r.VoegOnderwerpToe("SQL"), Times.Once);
        }

        [Fact]
        public void VoegOnderwerpToe_LegeNaam_GooitException()
        {
            Assert.Throws<ArgumentException>(() => _service.VoegOnderwerpToe(""));
        }

        [Fact]
        public void VoegOnderwerpToe_WhitespaceNaam_GooitException()
        {
            Assert.Throws<ArgumentException>(() => _service.VoegOnderwerpToe("   "));
        }
    }
}
