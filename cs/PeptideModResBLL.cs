using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Linq;
using System.Text;
using System.Data;

using ProteinGoggle.DAL;
using ProteinGoggle.Model;
using ProteinGoggle.BLL.Base64Csharp;

namespace ProteinGoggle.BLL
{
    public class PeptideModResBLL
    {
        private ProteinSession session;
        private PeptideModResDAL peptideModResDAL;
        private ProteinParameterRL testParameter = new ProteinParameterRL();
        private List<ModResModel> listModRes = new List<ModResModel>();
        private Analysis analysis = new Analysis();

        public System.Windows.Forms.ProgressBar ProgressBra { get; set; }

        public ParameterModel ParameterModel { get; set; }

        public PeptideModResBLL(string sDataBaseName)
        {
            peptideModResDAL = new PeptideModResDAL(sDataBaseName);
        }

        public PeptideModResBLL(ProteinSession pSession)
        {
            session = pSession;
            peptideModResDAL = new PeptideModResDAL(session.DataBase);
        }


        public DataTable Select_GetModResLikeModRes(string seq,string modRes,string order = "asc")
        {
            DataSet ds = peptideModResDAL.Select_GetModResLikeModRes(seq,modRes, order);

            if (ds == null || ds.Tables.Count == 0)
                return null;

            if (ds.Tables[0].Rows.Count == 0)
                return null;

            if (ds.Tables[0].Rows.Count == 1)
                return ds.Tables[0];

            string sKey = modRes.Split(',')[0].Substring(0, 1);
            string sValue = modRes.Split(',')[0].Substring(1);
            string sModRes = modRes.Split(',')[1];

            if (order == "asc")
            {
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    if (int.Parse(ds.Tables[0].Rows[i]["IDX"].ToString()) < int.Parse(sValue))
                        return null;
                }
            }
            else
            {
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    if (int.Parse(ds.Tables[0].Rows[i]["IDX"].ToString()) > int.Parse(sValue))
                        return null;
                }
            }

            var table = ds.Tables[0].AsEnumerable().Take(2);
            return table.CopyToDataTable();
        }

        public DataTable Select_GetModResLikeModRes(string id,string seq)
        {
            DataSet ds = peptideModResDAL.Select_GetModResLikeModRes(id, seq);

            if (ds == null || ds.Tables.Count == 0)
                return null;

            if (ds.Tables[0].Rows.Count == 0)
                return null;

            return ds.Tables[0];
        }
    }
}
