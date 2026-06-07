using MeerkeuzevragenApp.DOMEIN.Exceptions;
using MeerkeuzevragenApp.DOMEIN.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeerkeuzevragenApp.TESTS
{
    public class OnderwerpTests
    {
        [Fact]
        public void Naam_LegeNaam_GooitDomeinException()
        {
            Assert.Throws<DomeinException>(() =>
                new Onderwerp { Naam = "" });
        }

        [Fact]
        public void Naam_WhitespaceNaam_GooitDomeinException()
        {
            Assert.Throws<DomeinException>(() =>
                new Onderwerp { Naam = "   " });
        }

        [Fact]
        public void Naam_GeldigeNaam_WordtGetrimd()
        {
            var onderwerp = new Onderwerp { Naam = "  SQL  " };
            Assert.Equal("SQL", onderwerp.Naam);
        }

        [Fact]
        public void Constructor_MetNaam_MaaktOnderwerpAan()
        {
            var onderwerp = new Onderwerp("SQL");
            Assert.Equal("SQL", onderwerp.Naam);
        }

        [Fact]
        public void Constructor_MetIdEnNaam_MaaktOnderwerpAan()
        {
            var onderwerp = new Onderwerp(1, "SQL");
            Assert.Equal(1, onderwerp.ID);
            Assert.Equal("SQL", onderwerp.Naam);
        }

        [Fact]
        public void ToString_GeeftNaamTerug()
        {
            var onderwerp = new Onderwerp(1, "SQL");
            Assert.Equal("SQL", onderwerp.ToString());
        }
    }
}
