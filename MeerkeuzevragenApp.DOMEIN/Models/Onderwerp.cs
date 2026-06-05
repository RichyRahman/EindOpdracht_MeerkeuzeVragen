using MeerkeuzevragenApp.DOMEIN.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeerkeuzevragenApp.DOMEIN.Models
{
    public class Onderwerp
    {
        private string _naam = string.Empty;

        public int ID { get; set; }

        public string Naam
        {
            get => _naam;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new DomeinException("Onderwerpnaam mag niet leeg zijn.");
                _naam = value.Trim();
            }
        }

        public Onderwerp() { }
        public Onderwerp(string naam) { Naam = naam; }
        public Onderwerp(int id, string naam) { ID = id; Naam = naam; }

        public override string ToString() => Naam;

    }
}
