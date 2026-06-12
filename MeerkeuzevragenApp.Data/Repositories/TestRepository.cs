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
    public class TestRepository : ITestRepository
    {
        private readonly DatabaseConnection _db;

        public TestRepository(DatabaseConnection db)
        {
            _db = db;
        }

        public int MaakTestAan(Test test)
        {
            string sql = @"INSERT INTO Test (Naam, onderwerpID) 
                           VALUES (@Naam, @OnderwerpID);
                           SELECT CAST (SCOPE_IDENTITY() AS INT);";

            using var conn = _db.GetConnection();
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            cmd.Parameters.AddWithValue("@Naam", test.Naam);
            cmd.Parameters.AddWithValue("@OnderwerpID", test.OnderwerpID);

            return (int)cmd.ExecuteScalar();
        }

        public void VoegTestVraagToe(int testID, int vraagID)
        {
            string sql = @"INSERT INTO TestVragen (testID, vraagID) 
                           VALUES (@TestID, @VraagID)";

            using var conn = _db.GetConnection();
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            cmd.Parameters.AddWithValue("@TestID", testID);
            cmd.Parameters.AddWithValue("@VraagID", vraagID);
            cmd.ExecuteNonQuery();
        }

        public List<Test> GetAlleTests()
        {
            var lijst = new List<Test>();
            string sql = "SELECT ID, Naam, onderwerpID FROM Test";

            using var conn = _db.GetConnection();
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                lijst.Add(new Test
                {
                    ID = reader.GetInt32("ID"),
                    Naam = reader.GetString("Naam"),
                    OnderwerpID = reader.GetInt32("onderwerpID")
                });
            }

            return lijst;
        }

        public Test? GetTestMetVragen(int testID)
        {
            Test? test = null;

            // Haal testgegevens op

            string sqlTest = "SELECT ID, Naam, onderwerpID FROM Test WHERE ID = @ID";

            using var conn = _db.GetConnection();
            conn.Open();
            using var cmdTest = conn.CreateCommand();
            cmdTest.CommandText = sqlTest;
            cmdTest.Parameters.AddWithValue("@ID", testID);
            using var readerTest = cmdTest.ExecuteReader();

            if (!readerTest.Read()) return null;

            test = new Test
            {
                ID = readerTest.GetInt32("ID"),
                Naam = readerTest.GetString("Naam"),
                OnderwerpID = readerTest.GetInt32("onderwerpID")
            };
            readerTest.Close();

            // Haal IDs van de vragen op die bij deze test horen

            var vraagIDs = new List<int>();
            string sqlVraagIDs = @"SELECT vraagID FROM TestVragen 
                                   WHERE testID = @TestID";

            using var cmdIDs = conn.CreateCommand();
            cmdIDs.CommandText = sqlVraagIDs;
            cmdIDs.Parameters.AddWithValue("@TestID", testID);
            using var readerIDs = cmdIDs.ExecuteReader();

            while (readerIDs.Read())
                vraagIDs.Add(readerIDs.GetInt32("vraagID"));
            readerIDs.Close();

            // Haal elke vraag + antwoorden op
            foreach (int vraagID in vraagIDs)
            {
                var vraag = GetVraagMetAntwoorden(conn, vraagID);
                if (vraag != null)
                    test.Vragen.Add(vraag);
            }

            return test;
        }

        public int SlaGemaakteTestOp(GemaakteTest gemaakteTest)
        {
            string sql = @"INSERT INTO GemaakteTest (gebruikerID, testID) 
                           VALUES (@GebruikerID, @TestID);
                           SELECT CAST(SCOPE_IDENTITY() AS INT);";

            using var conn = _db.GetConnection();
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            cmd.Parameters.AddWithValue("@GebruikerID", gemaakteTest.GebruikerID);
            cmd.Parameters.AddWithValue("@TestID", gemaakteTest.TestID);

            return (int)cmd.ExecuteScalar();
        }

        public void SlaGemaakteVraagOp(GemaakteVraag gemaakteVraag)
        {
            string sql = @"INSERT INTO GemaakteVraag (gemaakteTestID, vraagID, Tekst) 
                           VALUES (@GemaakteTestID, @VraagID, @Tekst)";

            using var conn = _db.GetConnection();
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            cmd.Parameters.AddWithValue("@GemaakteTestID", gemaakteVraag.GemaakteTestID);
            cmd.Parameters.AddWithValue("@VraagID", gemaakteVraag.VraagID);
            cmd.Parameters.AddWithValue("@Tekst", gemaakteVraag.GekozenAntwoordTekst);
            cmd.ExecuteNonQuery();
        }

        public Vraag? GetVraagMetAntwoorden(SqlConnection conn, int vraagID)
        {
            Vraag? vraag = null;

            string sqlVraag = @"SELECT ID, onderwerpID, Moeilijkheidsgraad,
                                       Tekst, isBeschikbaar
                                FROM Vraag WHERE ID = @ID";

            using var cmdVraag = conn.CreateCommand();
            cmdVraag.CommandText = sqlVraag;
            cmdVraag.Parameters.AddWithValue("@ID", vraagID);
            using var readerVraag = cmdVraag.ExecuteReader();

            if (!readerVraag.Read()) return null;

            vraag = new Vraag
            {
                ID = readerVraag.GetInt32("ID"),
                OnderwerpID = readerVraag.GetInt32("onderwerpID"),
                Moeilijkheidsgraad = readerVraag.GetString("Moeilijkheidsgraad"),
                Tekst = readerVraag.GetString("Tekst"),
                IsBeschikbaar = readerVraag.GetBoolean("isBeschikbaar")
            };
            readerVraag.Close();

            // Antwoorden
            string sqlAntwoorden = @"SELECT vraagID, Tekst, isCorrect, Feedback
                                     FROM Antwoord WHERE vraagID = @VraagID";

            using var cmdAntwoorden = conn.CreateCommand();
            cmdAntwoorden.CommandText = sqlAntwoorden;
            cmdAntwoorden.Parameters.AddWithValue("@VraagID", vraagID);
            using var readerAntwoorden = cmdAntwoorden.ExecuteReader();

            while (readerAntwoorden.Read())
            {
                vraag.Antwoorden.Add(new Antwoord
                {
                    VraagID = readerAntwoorden.GetInt32("vraagID"),
                    Tekst = readerAntwoorden.GetString("Tekst"),
                    IsCorrect = readerAntwoorden.GetBoolean("isCorrect"),
                    Feedback = readerAntwoorden.IsDBNull(
                                    readerAntwoorden.GetOrdinal("Feedback"))
                                ? null
                                : readerAntwoorden.GetString("Feedback")
                });
            }

            return vraag;
        }
    }
}
