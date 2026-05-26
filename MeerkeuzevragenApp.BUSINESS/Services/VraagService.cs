using MeerkeuzevragenApp.DATA.Repositories;
using MeerkeuzevragenApp.DOMEIN;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeerkeuzevragenApp.BUSINESS.Services
{
    public class VraagService
    {
        private readonly IVraagRepository _vraagRepo;

        public VraagService(IVraagRepository vraagRepo)
        {
            _vraagRepo = vraagRepo;
        }

        public List<Onderwerp> GetAlleOnderwerpen() => _vraagRepo.GetAlleOnderwerpen();

        public List<Vraag> GetBeschikbareVragenPerOnderwerp(int onderwerpID)
            => _vraagRepo.GetBeschikbareVragenPerOnderwerp(onderwerpID);

        public List<Vraag> GetAlleVragenPerOnderwerp(int onderwerpID)
            => _vraagRepo.GetAlleVragenPerOnderwerp(onderwerpID);

        public void VoegVraagToe(Vraag vraag)
        {
            if (string.IsNullOrWhiteSpace(vraag.Tekst))
                throw new ArgumentException("Vraagtekst mag niet leeg zijn.");
            if (vraag.Antwoorden.Count < 2)
                throw new ArgumentException("Een vraag moet minstens 2 antwoorden hebben.");
            if (!vraag.Antwoorden.Any(a => a.IsCorrect))
                throw new ArgumentException("Er moet minstens één correct antwoord zijn.");

            _vraagRepo.VoegVraagToe(vraag);
        }

        public void StelNietBeschikbaar(int vraagID)
            => _vraagRepo.StelNietBeschikbaar(vraagID);

        public int VoegOnderwerpToe(string naam)
        {
            if (string.IsNullOrWhiteSpace(naam))
                throw new ArgumentException("Onderwerpnaam mag niet leeg zijn.");
            return _vraagRepo.VoegOnderwerpToe(naam);
        }
    }
}
