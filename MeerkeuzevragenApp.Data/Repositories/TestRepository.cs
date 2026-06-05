using MeerkeuzevragenApp.DOMEIN;
using System;
using System.Collections.Generic;
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
            using var conn = _db.GetConnection();
            conn.Execute(
                "INSERT INTO Test (Naam, onderwerpID) VALUES (@Naam, @OnderwerpID)",
                new { test.Naam, test.OnderwerpID });
            return conn.QuerySingle<int>("SELECT LAST_INSERT_ID();");
        }

        public void VoegTestVraagToe(int testID, int vraagID)
        {
            using var conn = _db.GetConnection();
            conn.Execute(
                "INSERT INTO TestVragen (testID, vraagID) VALUES (@TestID, @VraagID)",
                new { TestID = testID, VraagID = vraagID });
        }

        public List<Test> GetAlleTests()
        {
            using var conn = _db.GetConnection();
            return conn.Query<Test>("SELECT * FROM Test").ToList();
        }

        public Test GetTestMetVragen(int testID)
        {
            using var conn = _db.GetConnection();
            var test = conn.QueryFirstOrDefault<Test>(
                "SELECT * FROM Test WHERE ID = @ID", new { ID = testID });

            if (test == null) return null;

            var vraagIDs = conn.Query<int>(
                "SELECT vraagID FROM TestVragen WHERE testID = @TestID",
                new { TestID = testID }).ToList();

            foreach (var vraagID in vraagIDs)
            {
                var vraag = conn.QueryFirstOrDefault<Vraag>(
                    "SELECT * FROM Vraag WHERE ID = @ID", new { ID = vraagID });
                if (vraag != null)
                {
                    vraag.Antwoorden = conn.Query<Antwoord>(
                        "SELECT * FROM Antwoord WHERE vraagID = @VraagID",
                        new { VraagID = vraagID }).ToList();
                    test.Vragen.Add(vraag);
                }
            }
            return test;
        }

        public int SlaGemaakteTestOp(GemaakteTest gemaakteTest)
        {
            using var conn = _db.GetConnection();
            conn.Execute(
                "INSERT INTO GemaakteTest (gebruikerID, testID) VALUES (@GebruikerID, @TestID)",
                new { gemaakteTest.GebruikerID, gemaakteTest.TestID });
            return conn.QuerySingle<int>("SELECT LAST_INSERT_ID();");
        }

        public void SlaGemaakteVraagOp(GemaakteVraag gemaakteVraag)
        {
            using var conn = _db.GetConnection();
            conn.Execute(
                @"INSERT INTO GemaakteVraag (gemaakteTestID, vraagID, Tekst) 
                  VALUES (@GemaakteTestID, @VraagID, @GekozenAntwoordTekst)",
                gemaakteVraag);
        }

        public GemaakteTest GetGemaakteTestMetDetails(int gemaakteTestID)
        {
            using var conn = _db.GetConnection();
            var gemaakteTest = conn.QueryFirstOrDefault<GemaakteTest>(
                "SELECT * FROM GemaakteTest WHERE ID = @ID", new { ID = gemaakteTestID });

            if (gemaakteTest == null) return null;

            gemaakteTest.GemaakteVragen = conn.Query<GemaakteVraag>(
                "SELECT * FROM GemaakteVraag WHERE gemaakteTestID = @ID",
                new { ID = gemaakteTestID }).ToList();

            foreach (var gv in gemaakteTest.GemaakteVragen)
                gv.Vraag = conn.QueryFirstOrDefault<Vraag>(
                    "SELECT * FROM Vraag WHERE ID = @ID", new { ID = gv.VraagID })!;

            return gemaakteTest;
        }
    }
}
