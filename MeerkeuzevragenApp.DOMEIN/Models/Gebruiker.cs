using MeerkeuzevragenApp.DOMEIN.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeerkeuzevragenApp.DOMEIN.Models
{
    public class Gebruiker
    {
        private string _naam = string.Empty;

        public int ID { get; set; }

        public string Naam
        {
            get => _naam;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new DomeinException("Gebruikersnaam mag niet leeg zijn.");
                _naam = value.Trim();
            }
        }

        public Gebruiker() { }
        public Gebruiker(string naam) { Naam = naam; }
        public Gebruiker(int id, string naam) { ID = id; Naam = naam; }
    }
}
