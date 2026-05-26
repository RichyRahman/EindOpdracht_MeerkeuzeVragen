using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeerkeuzevragenApp.DOMEIN
{
    public class Antwoord
    {
        public int VraagID { get; set; }
        public string Tekst { get; set; }
        public bool IsCorrect { get; set; }
        public string Feedback { get; set; }
    }
}
