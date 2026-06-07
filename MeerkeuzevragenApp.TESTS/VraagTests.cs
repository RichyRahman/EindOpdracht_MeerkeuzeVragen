using MeerkeuzevragenApp.DOMEIN.Exceptions;
using MeerkeuzevragenApp.DOMEIN.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeerkeuzevragenApp.TESTS
{
    public class VraagTests
    {
        private Vraag MaakGeldigeVraag()
        {
            return new Vraag
            {
                ID = 1,
                OnderwerpID = 1,
                Tekst = "Wat is SQL?",
                Moeilijkheidsgraad = "Makkelijk",
                IsBeschikbaar = true,
                Antwoorden = new List<Antwoord>
                {
                    new Antwoord { Tekst = "Structured Query Language",
                                   IsCorrect = true,
                                   Feedback = null },
                    new Antwoord { Tekst = "Simple Query Language",
                                   IsCorrect = false,
                                   Feedback = "Het correcte antwoord is: Structured Query Language" },
                    new Antwoord { Tekst = "Sequential Query Language",
                                   IsCorrect = false,
                                   Feedback = "Het correcte antwoord is: Structured Query Language" },
                    new Antwoord { Tekst = "System Query Logic",
                                   IsCorrect = false,
                                   Feedback = "Het correcte antwoord is: Structured Query Language" }
                }
            };
        }

        [Fact]
        public void Tekst_LegeTekst_GooitDomeinException()
        {
            Assert.Throws<DomeinException>(() =>
                new Vraag { Tekst = "" });
        }

        [Fact]
        public void Tekst_WhitespaceTekst_GooitDomeinException()
        {
            Assert.Throws<DomeinException>(() =>
                new Vraag { Tekst = "   " });
        }

        [Fact]
        public void Tekst_GeldigeTekst_WordtOpgeslagen()
        {
            var vraag = new Vraag { Tekst = "  Wat is SQL?  " };
            Assert.Equal("Wat is SQL?", vraag.Tekst);
        }


        // Moeilijkheidsgraad validatie

        [Fact]
        public void Moeilijkheidsgraad_LegeWaarde_GooitDomeinException()
        {
            Assert.Throws<DomeinException>(() =>
                new Vraag { Moeilijkheidsgraad = "" });
        }


        // Valideer methoden

        [Fact]
        public void Valideer_GeldigeVraag_GooitGeenException()
        {
            var vraag = MaakGeldigeVraag();
            var ex = Record.Exception(() => vraag.Valideer());
            Assert.Null(ex);
        }

        [Fact]
        public void Valideer_MinderDanTweeAntwoorden_GooitDomeinException()
        {
            var vraag = MaakGeldigeVraag();
            vraag.Antwoorden = new List<Antwoord>
            {
                new Antwoord { Tekst = "A", IsCorrect = true }
            };
            Assert.Throws<DomeinException>(() => vraag.Valideer());
        }

        [Fact]
        public void Valideer_GeenCorrectAntwoord_GooitDomeinException()
        {
            var vraag = MaakGeldigeVraag();
            vraag.Antwoorden.ForEach(a => a.IsCorrect = false);
            Assert.Throws<DomeinException>(() => vraag.Valideer());
        }


        // IsCorrectAntwoord methoden

        [Fact]
        public void IsCorrectAntwoord_CorrectAntwoord_GeeftTrue()
        {
            var vraag = MaakGeldigeVraag();
            Assert.True(vraag.IsCorrectAntwoord("Structured Query Language"));
        }

        [Fact]
        public void IsCorrectAntwoord_FoutAntwoord_GeeftFalse()
        {
            var vraag = MaakGeldigeVraag();
            Assert.False(vraag.IsCorrectAntwoord("Simple Query Language"));
        }

        [Fact]
        public void IsCorrectAntwoord_OnbestaandAntwoord_GeeftFalse()
        {
            var vraag = MaakGeldigeVraag();
            Assert.False(vraag.IsCorrectAntwoord("Dit bestaat niet"));
        }


        // GetFeedback methoden

        [Fact]
        public void GetFeedback_FoutAntwoord_GeeftFeedbackTerug()
        {
            var vraag = MaakGeldigeVraag();
            var feedback = vraag.GetFeedback("Simple Query Language");
            Assert.NotNull(feedback);
            Assert.Contains("Structured Query Language", feedback);
        }


        // GetGeschuddeAntwoorden methoden

        [Fact]
        public void GetGeschuddeAntwoorden_GeeftAlleAntwoordenTerug()
        {
            var vraag = MaakGeldigeVraag();
            var geschud = vraag.GetGeschuddeAntwoorden();
            Assert.Equal(vraag.Antwoorden.Count, geschud.Count);
        }

        [Fact]
        public void GetGeschuddeAntwoorden_BevatCorrectAntwoord()
        {
            var vraag = MaakGeldigeVraag();
            var geschud = vraag.GetGeschuddeAntwoorden();
            Assert.Contains(geschud, a => a.IsCorrect);
        }
    }
}
