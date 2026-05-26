using MeerkeuzevragenApp.DOMEIN;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeerkeuzevragenApp.DATA.Repositories
{
    public interface IVraagRepository
    {
        List<Vraag> GetAlleVragen();
        List<Vraag> GetVragenPerOnderwerp(int onderwerpID);
        List<Vraag> GetBeschikbareVragenPerOnderwerp(int onderwerpID);
        List<Vraag> GetAlleVragenPerOnderwerp(int onderwerpID);
        Vraag GetVraagMetAntwoorden(int vraagID);
        void VoegVraagToe(Vraag vraag);
        void StelNietBeschikbaar(int vraagID);
        List<Onderwerp> GetAlleOnderwerpen();
        int VoegOnderwerpToe(string naam);
    }
}
