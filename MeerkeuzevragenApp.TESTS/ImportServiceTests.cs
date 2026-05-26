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
    public class ImportServiceTests
    {
        private readonly Mock<IVraagRepository> _mockRepo;
        private readonly ImportService _service;

        public ImportServiceTests()
        {
            _mockRepo = new Mock<IVraagRepository>();
            _service = new ImportService(_mockRepo.Object);
        }

        [Fact]
        public void ImporteerBestand_BestaandOnderwerp_VoegtVragenToe()
        {
            // Arrange
            var onderwerpen = new List<Onderwerp>
            {
                new Onderwerp { ID = 1, Naam = "SQL" }
            };
            _mockRepo.Setup(r => r.GetAlleOnderwerpen()).Returns(onderwerpen);

            // Maak tijdelijk testbestand aan
            string inhoud = @"1. Wat is SQL?
A. Structured Query Language
B. Simple Query Language
C. Sequential Query Language
D. System Query Logic

2. Welke opdracht gebruik je om data op te halen?
A. GET
B. SELECT
C. FETCH
D. EXTRACT

Antwoorden
A
B";
            string pad = Path.Combine(Path.GetTempPath(), "test_import.txt");
            File.WriteAllText(pad, inhoud);

            // Act
            _service.ImporteerBestand(pad, "SQL", "Makkelijk");

            // Assert — VoegVraagToe moet 2x aangeroepen zijn
            _mockRepo.Verify(r => r.VoegVraagToe(It.IsAny<Vraag>()), Times.Exactly(2));

            // Cleanup
            File.Delete(pad);
        }

        [Fact]
        public void ImporteerBestand_NieuwOnderwerp_MaaktOnderwerpAan()
        {
            // Arrange — geen onderwerpen in DB
            _mockRepo.Setup(r => r.GetAlleOnderwerpen()).Returns(new List<Onderwerp>());
            _mockRepo.Setup(r => r.VoegOnderwerpToe("Nieuw")).Returns(5);

            string inhoud = @"1. Testvraag?
A. Antwoord A
B. Antwoord B
C. Antwoord C
D. Antwoord D

Antwoorden
A";
            string pad = Path.Combine(Path.GetTempPath(), "test_nieuw.txt");
            File.WriteAllText(pad, inhoud);

            // Act
            _service.ImporteerBestand(pad, "Nieuw", "Gemiddeld");

            // Assert — nieuw onderwerp moet aangemaakt worden
            _mockRepo.Verify(r => r.VoegOnderwerpToe("Nieuw"), Times.Once);

            File.Delete(pad);
        }

        [Fact]
        public void ImporteerBestand_BestandNietGevonden_GooitException()
        {
            _mockRepo.Setup(r => r.GetAlleOnderwerpen()).Returns(new List<Onderwerp>());

            Assert.Throws<FileNotFoundException>(
                () => _service.ImporteerBestand("bestaat_niet.txt", "SQL", "Makkelijk"));
        }
    }
}
