using MeerkeuzevragenApp.DOMEIN.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeerkeuzevragenApp.DOMEIN.Models
{
    public class Antwoord
    {
        private string _tekst = string.Empty;

        public int VraagID { get; set; }

        public string Tekst
        {
            get => _tekst;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new DomeinException("Antwoordtekst mag niet leeg zijn.");
                _tekst = value.Trim();
            }
        }

        public bool IsCorrect { get; set; }
        public string? Feedback { get; set; }

        public Antwoord() { }

        public Antwoord(int vraagID, string tekst, bool isCorrect, string? feedback = null)
        {
            VraagID = vraagID;
            Tekst = tekst;
            IsCorrect = isCorrect;
            Feedback = feedback;
        }
    }
}
