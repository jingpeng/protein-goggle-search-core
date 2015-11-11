using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

using ProteinGoggle.Model;
using ProteinGoggle.DAL;

namespace ProteinGoggle.BLL
{
    public class PeptideBLL
    {
        private ProteinSession session;
        private PeptideDAL peDAL;

        public PeptideBLL(ProteinSession pSession)
        {
            session = pSession;
            peDAL = new PeptideDAL(session.DataBase);
        }

        public PeptideBLL(string sDataBase)
        {
            peDAL = new PeptideDAL(sDataBase);
        }

        public DataTable GetPeptideData()
        {
            DataSet ds = peDAL.Select_Peptide();

            if (ds != null && ds.Tables.Count != 0)
                return ds.Tables[0];

            return null;
        }
    }
}
