using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeerkeuzevragenApp.DOMEIN
{
    public class Test
    {
        public int ID { get; set; }
        public string Naam { get; set; }
        public int OnderwerpID { get; set; }
        public Onderwerp Onderwerp { get; set; }
        public List<Vraag> Vragen { get; set; } = new();
    }
}
