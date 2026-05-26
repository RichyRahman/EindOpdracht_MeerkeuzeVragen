using Dapper;
using MeerkeuzevragenApp.DOMEIN;
using System;
using System.Collections.Generic;
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
            using var conn = _db.GetConnection();
            return conn.Query<Onderwerp>("SELECT * FROM Onderwerp").ToList();
        }

        public int VoegOnderwerpToe(string naam)
        {
            using var conn = _db.GetConnection();
            // Use QuerySingle to get the last inserted ID
            return conn.QuerySingle<int>(
                "INSERT INTO Onderwerp (Naam) VALUES (@Naam); SELECT LAST_INSERT_ID();",
                new { Naam = naam });
        }

        public List<Vraag> GetAlleVragen()
        {
            using var conn = _db.GetConnection();
            var sql = @"SELECT v.*, o.ID, o.Naam 
                        FROM Vraag v 
                        JOIN Onderwerp o ON v.onderwerpID = o.ID";

            return conn.Query<Vraag, Onderwerp, Vraag>(sql,
                (vraag, onderwerp) => { vraag.Onderwerp = onderwerp; return vraag; },
                splitOn: "ID").ToList();
        }

        public List<Vraag> GetVragenPerOnderwerp(int onderwerpID)
        {
            using var conn = _db.GetConnection();
            var sql = "SELECT * FROM Vraag WHERE onderwerpID = @OnderwerpID";
            var vragen = conn.Query<Vraag>(sql, new { OnderwerpID = onderwerpID }).ToList();

            foreach (var vraag in vragen)
                vraag.Antwoorden = GetAntwoordenVoorVraag(vraag.ID);

            return vragen;
        }

        public List<Vraag> GetBeschikbareVragenPerOnderwerp(int onderwerpID)
        {
            using var conn = _db.GetConnection();
            var sql = "SELECT * FROM Vraag WHERE onderwerpID = @OnderwerpID AND isBeschikbaar = TRUE";
            var vragen = conn.Query<Vraag>(sql, new { OnderwerpID = onderwerpID }).ToList();

            foreach (var vraag in vragen)
                vraag.Antwoorden = GetAntwoordenVoorVraag(vraag.ID);

            return vragen;
        }

        public List<Vraag> GetAlleVragenPerOnderwerp(int onderwerpID)
        {
            using var conn = _db.GetConnection();
            var vragen = conn.Query<Vraag>(
                "SELECT * FROM Vraag WHERE onderwerpID = @OnderwerpID",
                new { OnderwerpID = onderwerpID }).ToList();

            foreach (var vraag in vragen)
                vraag.Antwoorden = GetAntwoordenVoorVraag(vraag.ID);

            return vragen;
        }

        public Vraag GetVraagMetAntwoorden(int vraagID)
        {
            using var conn = _db.GetConnection();
            var vraag = conn.QueryFirstOrDefault<Vraag>(
                "SELECT * FROM Vraag WHERE ID = @ID", new { ID = vraagID });

            if (vraag != null)
                vraag.Antwoorden = GetAntwoordenVoorVraag(vraagID);

            return vraag;
        }

        private List<Antwoord> GetAntwoordenVoorVraag(int vraagID)
        {
            using var conn = _db.GetConnection();
            return conn.Query<Antwoord>(
                "SELECT * FROM Antwoord WHERE vraagID = @VraagID",
                new { VraagID = vraagID }).ToList();
        }

        public void VoegVraagToe(Vraag vraag)
        {
            using var conn = _db.GetConnection();
            conn.Open();
            using var transaction = conn.BeginTransaction();
            try
            {
                var sqlVraag = @"INSERT INTO Vraag (onderwerpID, Moeilijkheidsgraad, Tekst, isBeschikbaar) 
                                 VALUES (@OnderwerpID, @Moeilijkheidsgraad, @Tekst, @IsBeschikbaar);
                                 SELECT LAST_INSERT_ID();";
                int vraagID = conn.QuerySingle<int>(sqlVraag, vraag, transaction);

                foreach (var antwoord in vraag.Antwoorden)
                {
                    conn.Execute(
                        "INSERT INTO Antwoord (vraagID, Tekst, isCorrect, Feedback) VALUES (@VraagID, @Tekst, @IsCorrect, @Feedback)",
                        new { VraagID = vraagID, antwoord.Tekst, antwoord.IsCorrect, antwoord.Feedback },
                        transaction);
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
            using var conn = _db.GetConnection();
            conn.Execute("UPDATE Vraag SET isBeschikbaar = FALSE WHERE ID = @ID", new { ID = vraagID });
        }
    }
}
