using MeerkeuzevragenApp.DATA.Repositories;
using MeerkeuzevragenApp.DOMEIN;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeerkeuzevragenApp.BUSINESS.Services
{
    public class ImportService
    {
        private readonly IVraagRepository _vraagRepo;

        public ImportService(IVraagRepository vraagRepo)
        {
            _vraagRepo = vraagRepo;
        }

        // Deel 2: Importeer een .txt bestand
        public void ImporteerBestand(string bestandspad, string onderwerpNaam, string moeilijkheidsgraad)
        {
            var regels = File.ReadAllLines(bestandspad)
                             .Select(r => r.Trim())
                             .Where(r => !string.IsNullOrWhiteSpace(r))
                             .ToList();

            // Zorg dat onderwerp bestaat
            var onderwerpen = _vraagRepo.GetAlleOnderwerpen();
            var onderwerp = onderwerpen.FirstOrDefault(o =>
                o.Naam.Equals(onderwerpNaam, StringComparison.OrdinalIgnoreCase));
            int onderwerpID = onderwerp?.ID ?? _vraagRepo.VoegOnderwerpToe(onderwerpNaam);

            var vragen = ParseBestand(regels, onderwerpID, moeilijkheidsgraad);

            foreach (var vraag in vragen)
                _vraagRepo.VoegVraagToe(vraag);
        }

        private List<Vraag> ParseBestand(List<string> regels, int onderwerpID, string moeilijkheidsgraad)
        {
            var vragen = new List<Vraag>();
            Vraag huidigeVraag = null;
            var antwoordenTekst = new List<string>();
            bool inAntwoordenBlok = false;

            // Detecteer formaat: "Correct: X" (c_1.txt) of antwoorden onderaan
            bool isCorrectFormaat = regels.Any(r => r.StartsWith("Correct:"));

            if (isCorrectFormaat)
                return ParseCorrectFormaat(regels, onderwerpID, moeilijkheidsgraad);

            // Standaard formaat: antwoorden onderaan na "Antwoorden"
            var vraagRegels = new List<string>();
            var antwoordRegelsOnderaan = new List<string>();

            foreach (var regel in regels)
            {
                if (regel.Equals("Antwoorden", StringComparison.OrdinalIgnoreCase))
                { inAntwoordenBlok = true; continue; }

                if (inAntwoordenBlok)
                    antwoordRegelsOnderaan.Add(regel);
                else
                    vraagRegels.Add(regel);
            }

            // Verwijder titelregel als die aanwezig is (bv. "Aardrijkskunde Quiz (20 vragen)")
            if (vraagRegels.Count > 0 && !System.Text.RegularExpressions.Regex.IsMatch(vraagRegels[0], @"^\d+\."))
                vraagRegels.RemoveAt(0);

            // Antwoorden na "Antwoorden" zijn de correcte, één per lijn
            // Verwijder commentaar zoals "(trickvraag...)"
            var correcteAntwoorden = antwoordRegelsOnderaan
                .Select(r => r.Split(' ')[0].Trim())
                .Where(r => r.Length == 1 && char.IsLetter(r[0]))
                .Select(r => r.ToUpper()[0])
                .ToList();

            // Parse vragen
            int vraagIndex = 0;
            int i = 0;
            while (i < vraagRegels.Count)
            {
                var regel = vraagRegels[i];

                // Vraagregels beginnen met cijfer+punt (bv. "1." of "1.")
                if (System.Text.RegularExpressions.Regex.IsMatch(regel, @"^\d+\."))
                {
                    huidigeVraag = new Vraag
                    {
                        OnderwerpID = onderwerpID,
                        Moeilijkheidsgraad = moeilijkheidsgraad,
                        IsBeschikbaar = true,
                        Antwoorden = new List<Antwoord>()
                    };

                    // Vraagtekst kan over meerdere regels lopen
                    string tekst = System.Text.RegularExpressions.Regex.Replace(regel, @"^\d+\.\s*", "");

                    // Voeg volgende regels toe tot we een antwoordregel zien
                    i++;
                    while (i < vraagRegels.Count &&
                           !System.Text.RegularExpressions.Regex.IsMatch(vraagRegels[i], @"^[A-E]\."))
                    {
                        if (System.Text.RegularExpressions.Regex.IsMatch(vraagRegels[i], @"^\d+\."))
                            break;
                        tekst += " " + vraagRegels[i];
                        i++;
                    }

                    huidigeVraag.Tekst = tekst.Trim();

                    // Lees antwoorden A/B/C/D(/E)
                    while (i < vraagRegels.Count &&
                           System.Text.RegularExpressions.Regex.IsMatch(vraagRegels[i], @"^[A-E]\."))
                    {
                        var antwoordRegel = vraagRegels[i];
                        char label = antwoordRegel[0];
                        string antwoordTekst = antwoordRegel.Substring(2).Trim();

                        bool isCorrect = vraagIndex < correcteAntwoorden.Count &&
                                        correcteAntwoorden[vraagIndex] == label;

                        huidigeVraag.Antwoorden.Add(new Antwoord
                        {
                            Tekst = antwoordTekst,
                            IsCorrect = isCorrect,
                            Feedback = isCorrect ? null : $"Het correct antwoord is: {antwoordTekst}"
                        });
                        i++;
                    }

                    if (huidigeVraag.Antwoorden.Any())
                    {
                        // Zet feedback correct op correcte antwoord
                        var correct = huidigeVraag.Antwoorden.FirstOrDefault(a => a.IsCorrect);
                        if (correct != null)
                            foreach (var ant in huidigeVraag.Antwoorden.Where(a => !a.IsCorrect))
                                ant.Feedback = $"Het correcte antwoord is: {correct.Tekst}";

                        vragen.Add(huidigeVraag);
                        vraagIndex++;
                    }
                }
                else
                {
                    i++;
                }
            }

            return vragen;
        }

        // Parser voor c_1.txt formaat ("Correct: X" per vraag, 5 opties)
        private List<Vraag> ParseCorrectFormaat(List<string> regels, int onderwerpID, string moeilijkheidsgraad)
        {
            var vragen = new List<Vraag>();
            Vraag huidigeVraag = null;
            char? correctLabel = null;

            int i = 0;
            while (i < regels.Count)
            {
                var regel = regels[i];

                if (System.Text.RegularExpressions.Regex.IsMatch(regel, @"^\d+\."))
                {
                    if (huidigeVraag != null && huidigeVraag.Antwoorden.Any())
                        vragen.Add(huidigeVraag);

                    huidigeVraag = new Vraag
                    {
                        OnderwerpID = onderwerpID,
                        Moeilijkheidsgraad = moeilijkheidsgraad,
                        IsBeschikbaar = true,
                        Antwoorden = new List<Antwoord>()
                    };
                    correctLabel = null;

                    string tekst = System.Text.RegularExpressions.Regex.Replace(regel, @"^\d+\.\s*", "").Trim();
                    i++;
                    // Multi-lijn vraagtekst (bv. citaten)
                    while (i < regels.Count &&
                           !System.Text.RegularExpressions.Regex.IsMatch(regels[i], @"^[A-E]\.") &&
                           !regels[i].StartsWith("Correct:"))
                    {
                        tekst += " " + regels[i];
                        i++;
                    }
                    huidigeVraag.Tekst = tekst.Trim();
                }
                else if (System.Text.RegularExpressions.Regex.IsMatch(regel, @"^[A-E]\.") && huidigeVraag != null)
                {
                    huidigeVraag.Antwoorden.Add(new Antwoord
                    {
                        Tekst = regel.Substring(2).Trim(),
                        IsCorrect = false
                    });
                    i++;
                }
                else if (regel.StartsWith("Correct:") && huidigeVraag != null)
                {
                    correctLabel = regel.Replace("Correct:", "").Trim().ToUpper()[0];
                    int labelIndex = correctLabel.Value - 'A';
                    if (labelIndex >= 0 && labelIndex < huidigeVraag.Antwoorden.Count)
                    {
                        huidigeVraag.Antwoorden[labelIndex].IsCorrect = true;
                        var correctTekst = huidigeVraag.Antwoorden[labelIndex].Tekst;
                        foreach (var ant in huidigeVraag.Antwoorden.Where(a => !a.IsCorrect))
                            ant.Feedback = $"Het correcte antwoord is: {correctTekst}";
                    }
                    i++;
                }
                else
                {
                    i++;
                }
            }

            if (huidigeVraag != null && huidigeVraag.Antwoorden.Any())
                vragen.Add(huidigeVraag);

            return vragen;
        }
    }
}
