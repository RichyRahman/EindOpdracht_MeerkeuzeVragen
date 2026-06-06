using MeerkeuzevragenApp.DOMEIN.Interfaces;
using MeerkeuzevragenApp.DOMEIN.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MeerkeuzevragenApp.DATA.Parsers
{
    public class StandaardFormaatParser : ITestParser
    {
        public bool KanVerwerken(string[] regels)
        {
            return regels.Any(r => r.Equals("Antwoorden",
                StringComparison.OrdinalIgnoreCase));
        }


        public List<Vraag> Parse(string[] regels, int onderwerpID, string moeilijkheid)
        {
            var vragen = new List<Vraag>();
            var vraagRegels = new List<string>();
            var antwoordRegels = new List<string>();
            bool inAntwoordBlok = false;

            foreach (var regel in regels)
            {
                if (regel.Equals("Antwoorden", StringComparison.OrdinalIgnoreCase))
                { inAntwoordBlok = true; continue; }

                if (inAntwoordBlok)
                    antwoordRegels.Add(regel);
                else
                    vraagRegels.Add(regel);
            }

            // Verwijder titelregel als die geen vraagnummer heeft
            if (vraagRegels.Count > 0 &&
                !Regex.IsMatch(vraagRegels[0], @"^\d+\."))
                vraagRegels.RemoveAt(0);

            // Correcte antwoorden — verwijder commentaar bv. "B (trickvraag)"
            var correcteLabels = antwoordRegels
                .Select(r => r.Split(' ')[0].Trim().ToUpper())
                .Where(r => r.Length == 1 && char.IsLetter(r[0]))
                .Select(r => r[0])
                .ToList();

            int vraagIndex = 0;
            int i = 0;

            while (i < vraagRegels.Count)
            {
                var regel = vraagRegels[i];

                if (!Regex.IsMatch(regel, @"^\d+\.")) { i++; continue; }

                // Vraagtekst
                string tekst = Regex.Replace(regel, @"^\d+\.\s*", "");
                i++;

                while (i < vraagRegels.Count &&
                       !Regex.IsMatch(vraagRegels[i], @"^[A-E]\.") &&
                       !Regex.IsMatch(vraagRegels[i], @"^\d+\."))
                {
                    tekst += " " + vraagRegels[i];
                    i++;
                }

                var vraag = new Vraag
                {
                    OnderwerpID = onderwerpID,
                    Moeilijkheidsgraad = moeilijkheid,
                    IsBeschikbaar = true,
                    Tekst = tekst.Trim()
                };

                // Antwoorden A/B/C/D
                char? correctLabel = vraagIndex < correcteLabels.Count
                    ? correcteLabels[vraagIndex] : null;

                while (i < vraagRegels.Count &&
                       Regex.IsMatch(vraagRegels[i], @"^[A-E]\."))
                {
                    char label = vraagRegels[i][0];
                    string antwoordTekst = vraagRegels[i].Substring(2).Trim();
                    bool isCorrect = correctLabel.HasValue && label == correctLabel.Value;

                    vraag.Antwoorden.Add(new Antwoord
                    {
                        Tekst = antwoordTekst,
                        IsCorrect = isCorrect
                    });
                    i++;
                }

                // Stel feedback in
                var correct = vraag.Antwoorden.FirstOrDefault(a => a.IsCorrect);
                if (correct != null)
                    foreach (var ant in vraag.Antwoorden.Where(a => !a.IsCorrect))
                        ant.Feedback = $"Het correcte antwoord is: {correct.Tekst}";

                if (vraag.Antwoorden.Any())
                {
                    vragen.Add(vraag);
                    vraagIndex++;
                }
            }

            return vragen;
        }
    }
}
