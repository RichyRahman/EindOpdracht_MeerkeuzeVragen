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
    public class CorrectFormaatParser : ITestParser
    {
        public bool KanVerwerken(string[] regels)
        {
            return regels.Any(r => r.StartsWith("Correct:",
                StringComparison.OrdinalIgnoreCase));
        }

        public List<Vraag> Parse(string[] regels, int onderwerpID, string moeilijkheid)
        {
            var vragen = new List<Vraag>();
            Vraag? huidigeVraag = null;

            int i = 0;
            while (i < regels.Length)
            {
                var regel = regels[i];

                // Nieuwe vraag
                if (Regex.IsMatch(regel, @"^\d+\."))
                {
                    if (huidigeVraag != null && huidigeVraag.Antwoorden.Any())
                        vragen.Add(huidigeVraag);

                    string tekst = Regex.Replace(regel, @"^\d+\.\s*", "").Trim();
                    i++;

                    // Multi-lijn vraagtekst
                    while (i < regels.Length &&
                           !Regex.IsMatch(regels[i], @"^[A-E]\.") &&
                           !regels[i].StartsWith("Correct:",
                               StringComparison.OrdinalIgnoreCase))
                    {
                        tekst += " " + regels[i];
                        i++;
                    }

                    huidigeVraag = new Vraag
                    {
                        OnderwerpID = onderwerpID,
                        Moeilijkheidsgraad = moeilijkheid,
                        IsBeschikbaar = true,
                        Tekst = tekst.Trim()
                    };
                }
                // Antwoordoptie A/B/C/D/E
                else if (Regex.IsMatch(regel, @"^[A-E]\.") && huidigeVraag != null)
                {
                    huidigeVraag.Antwoorden.Add(new Antwoord
                    {
                        Tekst = regel.Substring(2).Trim(),
                        IsCorrect = false
                    });
                    i++;
                }
                // Correct antwoord
                else if (regel.StartsWith("Correct:",
                    StringComparison.OrdinalIgnoreCase) && huidigeVraag != null)
                {
                    string labelStr = regel.Replace("Correct:", "").Trim().ToUpper();
                    if (labelStr.Length > 0)
                    {
                        int labelIndex = labelStr[0] - 'A';
                        if (labelIndex >= 0 &&
                            labelIndex < huidigeVraag.Antwoorden.Count)
                        {
                            huidigeVraag.Antwoorden[labelIndex].IsCorrect = true;
                            string correctTekst =
                                huidigeVraag.Antwoorden[labelIndex].Tekst;

                            foreach (var ant in huidigeVraag.Antwoorden
                                .Where(a => !a.IsCorrect))
                                ant.Feedback =
                                    $"Het correcte antwoord is: {correctTekst}";
                        }
                    }
                    i++;
                }
                else { i++; }
            }

            // Laatste vraag toevoegen
            if (huidigeVraag != null && huidigeVraag.Antwoorden.Any())
                vragen.Add(huidigeVraag);

            return vragen;
        }
    }
}
