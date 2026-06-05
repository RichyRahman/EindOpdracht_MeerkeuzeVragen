using MeerkeuzevragenApp.DOMEIN.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeerkeuzevragenApp.DOMEIN.Interfaces
{
    public interface IVraagRepository
    {
        List<Onderwerp> GetAlleOnderwerpen();
        int VoegOnderwerpToe(string naam);
        List<Vraag> GetAlleVragen();
        List<Vraag> GetBeschikbareVragenPerOnderwerp(int onderwerpID);
        List<Vraag> GetAlleVragenPerOnderwerp(int onderwerpID);
        Vraag GetVraagMetAntwoorden(int vraagID);
        void VoegVraagToe(Vraag vraag);
        void StelNietBeschikbaar(int vraagID);
    }
}
