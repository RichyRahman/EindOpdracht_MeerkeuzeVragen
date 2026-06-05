using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeerkeuzevragenApp.DOMEIN.Models
{
    public class GemaakteVraag
    {
        public int GemaakteTestID { get; set; }
        public int VraagID { get; set; }
        public string GekozenAntwoordTekst { get; set; } = string.Empty;
    }
}
