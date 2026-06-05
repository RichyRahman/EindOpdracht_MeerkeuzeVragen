using MeerkeuzevragenApp.DOMEIN.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeerkeuzevragenApp.DOMEIN.Interfaces
{
    public interface ITestParser
    {
        bool KanVerwerken(string[] regels);
        List<Vraag> Parse(string[] regels, int onderwerpID, string moeilijkheid);
    }
}
