using MeerkeuzevragenApp.DOMEIN.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeerkeuzevragenApp.DOMEIN.Models
{
    public class Vraag
    {
        private string _tekst = string.Empty;
        private string _moeilijkheidsgraad = string.Empty;

        public int ID { get; set; }
        public int OnderwerpID { get; set; }
        public bool IsBeschikbaar { get; set; } = true;
        public Onderwerp? Onderwerp { get; set; }
        public List<Antwoord> Antwoorden { get; set; } = new();

        public string Tekst
        {
            get => _tekst;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new DomeinException("Vraagtekst mag niet leeg zijn.");
                _tekst = value.Trim();
            }
        }

        public string Moeilijkheidsgraad
        {
            get => _moeilijkheidsgraad;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new DomeinException("Moeilijkheidsgraad mag niet leeg zijn.");
                _moeilijkheidsgraad = value.Trim();
            }
        }

        public void Valideer()
        {
            if (string.IsNullOrWhiteSpace(_tekst))
                throw new DomeinException("Vraagtekst mag niet leeg zijn.");
            if (Antwoorden.Count < 2)
                throw new DomeinException("Een vraag moet minstens 2 antwoorden hebben.");
            if (!Antwoorden.Any(a => a.IsCorrect))
                throw new DomeinException("Er moet minstens één correct antwoord zijn.");
        }

        public bool IsCorrectAntwoord(string antwoordTekst)
        {
            return Antwoorden.Any(a => a.IsCorrect && a.Tekst == antwoordTekst);
        }

        public string? GetFeedback(string antwoordTekst)
        {
            var correct = Antwoorden.FirstOrDefault(a => a.IsCorrect);
            if (correct == null) return null;
            return correct.Feedback ?? $"Het correcte antwoord is: {correct.Tekst}";
        }

        public List<Antwoord> GetGeschuddeAntwoorden()
        {
            var random = new Random();
            return Antwoorden.OrderBy(_ => random.Next()).ToList();
        }
    }
}
