using MeerkeuzevragenApp.DOMEIN.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeerkeuzevragenApp.DOMEIN.Models
{
    public class VraagManager
    {
        private readonly IVraagRepository _repo;

        public VraagManager(IVraagRepository repo)
        {
            _repo = repo;
        }

        public List<Onderwerp> GetAlleOnderwerpen() 
            => _repo.GetAlleOnderwerpen();
        
        public List<Vraag> getBeschikbareVragenPerOnderwerp(int onderwerpID) 
            => _repo.GetBeschikbareVragenPerOnderwerp(onderwerpID);

        public List<Vraag> GetAlleVragenPerOnderwerp(int onderwerpID)
            => _repo.GetAlleVragenPerOnderwerp(onderwerpID);

        public void VoegVraagToe(Vraag vraag)
        {
            vraag.Valideer();
            _repo.VoegVraagToe(vraag);
        }

        public void StelNietBeschikbaar(int vraagID)
            => _repo.StelNietBeschikbaar(vraagID);

        public int VoegOnderwerpToe(string naam)
        {
            var onderwerp = new Onderwerp(naam); 
            return _repo.VoegOnderwerpToe(onderwerp.Naam);
        }
    }
}
