using MeerkeuzevragenApp.DOMEIN.Exceptions;
using MeerkeuzevragenApp.DOMEIN.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeerkeuzevragenApp.TESTS
{
    public class AntwoordTests
    {
        // Tekst property tests
        [Fact]
        public void Tekst_LegeTekst_GooitDomeinException()
        {
            Assert.Throws<DomeinException>(() =>
                new Antwoord { Tekst = "" });
        }

        [Fact]
        public void Tekst_WhitespaceTekst_GooitDomeinException()
        {
            Assert.Throws<DomeinException>(() =>
                new Antwoord { Tekst = "   " });
        }

        [Fact]
        public void Tekst_GeldigeTekst_WordtGetrimd()
        {
            var antwoord = new Antwoord { Tekst = "  Antwoord A  " };
            Assert.Equal("Antwoord A", antwoord.Tekst);
        }

        [Fact]
        public void Constructor_GeldigeWaarden_MaaktAntwoordAan()
        {
            var antwoord = new Antwoord(1, "Antwoord A", true, "Feedback");
            Assert.Equal(1, antwoord.VraagID);
            Assert.Equal("Antwoord A", antwoord.Tekst);
            Assert.True(antwoord.IsCorrect);
            Assert.Equal("Feedback", antwoord.Feedback);
        }
    }
}
