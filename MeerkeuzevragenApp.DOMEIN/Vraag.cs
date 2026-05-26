using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeerkeuzevragenApp.DOMEIN
{
    public class Vraag
    {
        public int ID { get; set; }
        public int OnderwerpID { get; set; }
        public string Moeilijkheidsgraad { get; set; }
        public string Tekst { get; set; }
        public bool IsBeschikbaar { get; set; } = true;
        public List<Antwoord> Antwoorden { get; set; } = new();
        public Onderwerp Onderwerp { get; set; }
    }
}
