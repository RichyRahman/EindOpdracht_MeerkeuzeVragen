using MeerkeuzevragenApp.DOMEIN;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeerkeuzevragenApp.DATA.Repositories
{
    public interface ITestRepository
    {
        int MaakTestAan(Test test);
        void VoegTestVraagToe(int testID, int vraagID);
        Test GetTestMetVragen(int testID);
        List<Test> GetAlleTests();
        int SlaGemaakteTestOp(GemaakteTest gemaakteTest);
        void SlaGemaakteVraagOp(GemaakteVraag gemaakteVraag);
        GemaakteTest GetGemaakteTestMetDetails(int gemaakteTestID);
    }
}
