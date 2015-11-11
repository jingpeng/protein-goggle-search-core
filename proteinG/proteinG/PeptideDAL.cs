using System;
using System.Configuration;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;


namespace ProteinGoggle.Model
{
    public class PeptideDAL
    {
        private readonly string sTableName = "Flat_txt";
        private SQLHelper sqlHelper;

        private List<string> lstColName = new List<string>();

        public PeptideDAL(string sDataBase)
        {
            string sServer = "10.60.43.39";
            string sUid = "root";
            string sPwd = "root";
            string sMySqlConnString = string.Format("Server={0};Database={1};Uid={2};Pwd={3};Persist Security Info=True;Pooling=False;", sServer, sDataBase, sUid, sPwd);
            sqlHelper = new SQLHelper(sMySqlConnString);

            lstColName.Add("ID");
            lstColName.Add("AC");
            lstColName.Add("DT");
            lstColName.Add("DE");
            lstColName.Add("GN");
            lstColName.Add("OS");
            lstColName.Add("OC");
            lstColName.Add("OX");
            lstColName.Add("RN");
            lstColName.Add("RP");
            lstColName.Add("RX");
            lstColName.Add("RA");
            lstColName.Add("RT");
            lstColName.Add("RL");
            lstColName.Add("RC");
            lstColName.Add("RG");
            lstColName.Add("CC");
            lstColName.Add("DR");
            lstColName.Add("PE");
            lstColName.Add("KW");
            lstColName.Add("FT");
            lstColName.Add("SQ");
            lstColName.Add("ModResSingle");
        }

        public DataSet Select_PeptideWithID(string ID)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("SELECT * FROM " + sTableName);
            sb.AppendFormat(" WHERE ID = '{0}'", ID);
            return sqlHelper.Query(sb.ToString());
        }

        public DataSet Select_Peptide()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("SELECT * FROM " + sTableName);
            return sqlHelper.Query(sb.ToString());
        }

        public DataSet Select_PeptideCreateTime(string sDataBaseName)
        {
            StringBuilder sb = new StringBuilder();

            try
            {
                sb.AppendFormat("SELECT DISTINCT StartTime,EndTime FROM {0}.{1}", sDataBaseName, sTableName);
                return sqlHelper.Query(sb.ToString());
            }
            catch (Exception)
            {
                return null;
            }
        }

        public void UpdateCreateTime(string startTime,string endTime)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("UPDATE {0} ", sTableName);
            sb.AppendFormat("SET StartTime = '{0}',", startTime);
            sb.AppendFormat("    EndTime = '{0}'", endTime);
            sqlHelper.ExecuteSql(sb.ToString());
        }

        public void InsertPeptide(IDictionary<string, string> dict)
        {
            StringBuilder sb = new StringBuilder();
            StringBuilder sbHead = new StringBuilder();
            StringBuilder sbValue = new StringBuilder();

            IList<string> listKey = dict.Keys.ToList<string>();
            IList<string> listValue = dict.Values.ToList<string>();

            for (int i = 0; i < listKey.Count; i++)
            {
                if (this.lstColName.Contains(listKey[i]))
                {
                    sbHead.AppendFormat(i == 0 ? "{0}" : ",{0}", listKey[i]);
                    sbValue.AppendFormat(i == 0 ? "'{0}'" : ",'{0}'", listValue[i].Replace("'", "''"));
                }
            }

            sb.AppendFormat("INSERT INTO {0} (", sTableName);
            sb.AppendFormat("{0}",sbHead.ToString());
            sb.Append(") VALUES (");
            sb.AppendFormat("{0}",sbValue.ToString());
            sb.Append(");");
            sqlHelper.ExecuteSql(sb.ToString());
        }

        public void CreatePeptideTable()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat("CREATE TABLE {0} (", sTableName);
            sb.Append("   ID VARCHAR(500) NOT NULL,");
            sb.Append("   AC TEXT,");
            sb.Append("   DT TEXT,");
            sb.Append("   DE TEXT,");
            sb.Append("   GN TEXT,");
            sb.Append("   OS TEXT,");
            sb.Append("   OC TEXT,");
            sb.Append("   OX TEXT,");
            sb.Append("   RN TEXT,");
            sb.Append("   RP TEXT,");
            sb.Append("   RX TEXT,");
            sb.Append("   RA TEXT,");
            sb.Append("   RT TEXT,");
            sb.Append("   RL TEXT,");
            sb.Append("   RC TEXT,");
            sb.Append("   RG TEXT,");
            sb.Append("   CC TEXT,");
            sb.Append("   DR TEXT,");
            sb.Append("   PE TEXT,");
            sb.Append("   KW TEXT,");
            sb.Append("   FT TEXT,");
            sb.Append("   SQ TEXT,");
            sb.Append("   ModResSingle TEXT,");
            sb.Append("   StartTime VARCHAR(20),");
            sb.Append("   EndTime VARCHAR(20),");
            sb.Append(" PRIMARY KEY (ID)");
            sb.Append(") ENGINE = MYISAM DEFAULT CHARSET=latin1;");
            sqlHelper.ExecuteSql(sb.ToString());
        }
    }
}
