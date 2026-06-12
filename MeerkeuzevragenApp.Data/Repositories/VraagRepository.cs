using MeerkeuzevragenApp.DOMEIN;
using MeerkeuzevragenApp.DOMEIN.Interfaces;
using MeerkeuzevragenApp.DOMEIN.Models;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeerkeuzevragenApp.DATA.Repositories
{
    public class VraagRepository : IVraagRepository
    {
        private readonly DatabaseConnection _db;

        public VraagRepository(DatabaseConnection db)
        {
            _db = db;
        }

        public List<Onderwerp> GetAlleOnderwerpen()
        {
            var lijst = new List<Onderwerp>();
            string sql = "SELECT ID, Naam FROM Onderwerp";

            using var conn = _db.GetConnection();
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = sql;

            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                lijst.Add(new Onderwerp(
                    reader.GetInt32("ID"),
                    reader.GetString("Naam")));
            }
            return lijst;
        }

        public int VoegOnderwerpToe(string naam)
        {
            string sql = @"INSERT INTO Onderwerp (Naam) VALUES (@Naam);
                           SELECT CAST(SCOPE_IDENTITY() AS INT);";

            using var conn = _db.GetConnection();
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            cmd.Parameters.AddWithValue("@Naam", naam);

            return (int)cmd.ExecuteScalar();
        }

        public List<Vraag> GetAlleVragen()
        {
            var lijst = new List<Vraag>();
            string sql = @"SELECT v.ID as VraagID, 
                          v.onderwerpID, 
                          v.Moeilijkheidsgraad, 
                          v.Tekst as VraagTekst, 
                          v.isBeschikbaar
                   FROM Vraag v";

            using var conn = _db.GetConnection();
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                lijst.Add(new Vraag
                {
                    ID = reader.GetInt32("VraagID"),
                    OnderwerpID = reader.GetInt32("onderwerpID"),
                    Moeilijkheidsgraad = reader.GetString("Moeilijkheidsgraad"),
                    Tekst = reader.GetString("VraagTekst"),
                    IsBeschikbaar = reader.GetBoolean("isBeschikbaar")
                });
            }

            foreach (var vraag in lijst)
                vraag.Antwoorden = GetAntwoordenVoorVraag(vraag.ID);

            return lijst;
        }


        public List<Vraag> GetBeschikbareVragenPerOnderwerp(int onderwerpID)
        {
            var lijst = new List<Vraag>();
            string sql = @"SELECT ID, onderwerpID, Moeilijkheidsgraad, 
                          Tekst, isBeschikbaar
                   FROM Vraag 
                   WHERE onderwerpID = @OnderwerpID 
                   AND isBeschikbaar = 1";

            using var conn = _db.GetConnection();
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            cmd.Parameters.AddWithValue("@OnderwerpID", onderwerpID);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
                lijst.Add(LeesVraag(reader));

            foreach (var vraag in lijst)
                vraag.Antwoorden = GetAntwoordenVoorVraag(vraag.ID);

            return lijst;
        }

        public List<Vraag> GetAlleVragenPerOnderwerp(int onderwerpID)
        {
            var lijst = new List<Vraag>();
            string sql = @"SELECT ID, onderwerpID, Moeilijkheidsgraad,
                          Tekst, isBeschikbaar
                   FROM Vraag 
                   WHERE onderwerpID = @OnderwerpID";

            using var conn = _db.GetConnection();
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            cmd.Parameters.AddWithValue("@OnderwerpID", onderwerpID);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
                lijst.Add(LeesVraag(reader));

            foreach (var vraag in lijst)
                vraag.Antwoorden = GetAntwoordenVoorVraag(vraag.ID);

            return lijst;
        }


        public Vraag GetVraagMetAntwoorden(int vraagID)
        {
            string sql = @"SELECT ID, onderwerpID, Moeilijkheidsgraad,
                                  Tekst, isBeschikbaar
                           FROM Vraag WHERE ID = @ID";

            using var conn = _db.GetConnection();
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            cmd.Parameters.AddWithValue("@ID", vraagID);
            using var reader = cmd.ExecuteReader();

            if (!reader.Read()) return null;

            var vraag = LeesVraag(reader);
            reader.Close();
            vraag.Antwoorden = GetAntwoordenVoorVraag(vraagID);
            return vraag;
        }


        public void VoegVraagToe(Vraag vraag)
        {
            using var conn = _db.GetConnection();
            conn.Open();
            using var transaction = conn.BeginTransaction();

            try
            {
                // Eerst de vraag toevoegen

                string sqlVraag = @"INSERT INTO Vraag 
                            (onderwerpID, Moeilijkheidsgraad, Tekst, isBeschikbaar)
                            VALUES (@OnderwerpID, @Moeilijkheid, @Tekst, @IsBeschikbaar);
                            SELECT CAST(SCOPE_IDENTITY() AS INT);";

                using var cmdVraag = conn.CreateCommand();
                cmdVraag.Transaction = transaction;
                cmdVraag.CommandText = sqlVraag;
                cmdVraag.Parameters.AddWithValue("@OnderwerpID", vraag.OnderwerpID);
                cmdVraag.Parameters.AddWithValue("@Moeilijkheid", vraag.Moeilijkheidsgraad);
                cmdVraag.Parameters.AddWithValue("@Tekst", vraag.Tekst);
                cmdVraag.Parameters.AddWithValue("@IsBeschikbaar", vraag.IsBeschikbaar);

                int vraagID = (int)cmdVraag.ExecuteScalar();


                // Nu de antwoorden toevoegen

                string sqlAntwoord = @"INSERT INTO Antwoord 
                                       (vraagID, Tekst, isCorrect, Feedback)
                                       VALUES (@VraagID, @Tekst, @IsCorrect, @Feedback)";

                foreach (var antwoord in vraag.Antwoorden)
                {
                    using var cmdAntwoord = conn.CreateCommand();
                    cmdAntwoord.Transaction = transaction;
                    cmdAntwoord.CommandText = sqlAntwoord;
                    cmdAntwoord.Parameters.AddWithValue("@VraagID", vraagID);
                    cmdAntwoord.Parameters.AddWithValue("@Tekst", antwoord.Tekst);
                    cmdAntwoord.Parameters.AddWithValue("@IsCorrect", antwoord.IsCorrect);

                    if (antwoord.Feedback == null)
                        cmdAntwoord.Parameters.AddWithValue("@Feedback", DBNull.Value);
                    else
                        cmdAntwoord.Parameters.AddWithValue("@Feedback", antwoord.Feedback);

                    cmdAntwoord.ExecuteNonQuery();
                }

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public void StelNietBeschikbaar(int vraagID)
        {
            string sql = "UPDATE Vraag SET isBeschikbaar = FALSE WHERE ID = @ID";

            using var conn = _db.GetConnection();
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            cmd.Parameters.AddWithValue("@ID", vraagID);
            cmd.ExecuteNonQuery();
        }

        private Vraag LeesVraag(SqlDataReader reader)
        {
            return new Vraag
            {
                ID = reader.GetInt32("ID"),
                OnderwerpID = reader.GetInt32("onderwerpID"),
                Moeilijkheidsgraad = reader.GetString("Moeilijkheidsgraad"),
                Tekst = reader.GetString("Tekst"),
                IsBeschikbaar = reader.GetBoolean("isBeschikbaar")
            };
        }

        private List<Antwoord> GetAntwoordenVoorVraag(int vraagID)
        {
            var lijst = new List<Antwoord>();
            string sql = @"SELECT vraagID, Tekst, isCorrect, Feedback
                           FROM Antwoord WHERE vraagID = @VraagID";

            using var conn = _db.GetConnection();
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            cmd.Parameters.AddWithValue("@VraagID", vraagID);
            using var reader = cmd.ExecuteReader();


            while (reader.Read())
            {
                lijst.Add(new Antwoord
                {
                    VraagID = reader.GetInt32("vraagID"),
                    Tekst = reader.GetString("Tekst"),
                    IsCorrect = reader.GetBoolean("isCorrect"),
                    Feedback = reader.IsDBNull(reader.GetOrdinal("Feedback"))
                                ? null
                                : reader.GetString("Feedback")
                });

            }
            return lijst;
        }
    }
}
