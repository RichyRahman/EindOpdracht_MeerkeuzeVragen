using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeerkeuzevragenApp.DOMEIN
{
    public class GemaakteTest
    {
        public int ID { get; set; }
        public int GebruikerID { get; set; }
        public int TestID { get; set; }
        public Gebruiker Gebruiker { get; set; }
        public Test Test { get; set; }
        public List<GemaakteVraag> GemaakteVragen { get; set; } = new();
    }
}
