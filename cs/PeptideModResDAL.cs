using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using ProteinGoggle.Model;
using ProteinGoggle.DBUtility;

namespace ProteinGoggle.DAL
{
    public class PeptideModResDAL
    {
        private readonly string sTableName = "Parent_ions";
        private SQLHelper sqlHelper;

        public PeptideModResDAL(string sDataBase)
        {
            string sServer = System.Configuration.ConfigurationManager.AppSettings["DataBaseServer"].ToString();
            string sUid = System.Configuration.ConfigurationManager.AppSettings["DataBaseUid"].ToString();
            string sPwd = System.Configuration.ConfigurationManager.AppSettings["DataBasePwd"].ToString();
            string sMySqlConnString = string.Format("Server={0};Database={1};Uid={2};Pwd={3};Persist Security Info=True;Pooling=False;", sServer, sDataBase, sUid, sPwd);
            sqlHelper = new SQLHelper(sMySqlConnString);
        }

        public void CreatePeptideModResTable()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat("CREATE TABLE {0} (" , sTableName);
            sb.Append("   ID VARCHAR(50) NOT NULL,");
            sb.Append("   AA_VARIATION VARCHAR(200) NOT NULL,");
            sb.Append("   MOD_RES VARCHAR(300) NOT NULL,");
            sb.Append("   Z INT(10) UNSIGNED NOT NULL,");
            sb.Append("   SEQ TEXT NOT NULL,");
            sb.Append("   MZ DOUBLE NOT NULL,");
            sb.Append("   MZ_ALL VARCHAR(1000) NOT NULL,");
            sb.Append("   Relative_Abundance_ALL VARCHAR(1000) NOT NULL,");
            sb.Append("   FORUMLA VARCHAR(100) NOT NULL,");
            sb.Append(" PRIMARY KEY (ID,AA_VARIATION,MOD_RES,Z)");
            sb.Append(") ENGINE = MYISAM DEFAULT CHARSET=latin1;");
            sqlHelper.ExecuteSql(sb.ToString());

            sb.Clear();
            sb.AppendFormat("ALTER TABLE {0} ADD INDEX MZ_IDX(MZ)",sTableName);
            sqlHelper.ExecuteSql(sb.ToString());

            //sb.Clear();
            //sb.AppendFormat("ALTER TABLE {0} ADD INDEX SEQ_IDX(SEQ)", sTableName);
            //sqlHelper.ExecuteSql(sb.ToString());
        }

        public void Insert_PeptideModRes(List<ModResModel> listModel)
        {
            StringBuilder sbHead = new StringBuilder();
            StringBuilder sb = new StringBuilder();
            int iCnt = 1;

            sbHead.AppendFormat("INSERT INTO {0}", sTableName);
            sbHead.AppendFormat("({0},{1},{2},{3},{4},{5},{6},{7},{8})", "ID", "AA_VARIATION", "MOD_RES", "Z", "SEQ", "MZ", "MZ_ALL", "Relative_Abundance_ALL", "FORUMLA");
            sbHead.Append(" VALUES ");

            foreach (ModResModel model in listModel)
            {

                if (iCnt == 500 || listModel.Count == iCnt)
                {
                    sb.AppendFormat("('{0}',", model.ID);
                    sb.AppendFormat("'{0}',", model.SEQKBN);
                    sb.AppendFormat("'{0}',", model.Mod_Res);
                    sb.AppendFormat("{0},", model.Z);
                    sb.AppendFormat("'{0}',", model.Sequence);
                    sb.AppendFormat("{0},", model.M_Z);
                    sb.AppendFormat("'{0}',", string.Join(",", model.M_Z_ALL));
                    sb.AppendFormat("'{0}',", string.Join(",", model.M_ALL));
                    sb.AppendFormat("'{0}');", model.Forumla);
                    sqlHelper.ExecuteSql(sbHead.ToString() + sb.ToString());
                    sb.Length = 0;
                }
                else
                {
                    sb.AppendFormat("('{0}',", model.ID);
                    sb.AppendFormat("'{0}',", model.SEQKBN);
                    sb.AppendFormat("'{0}',", model.Mod_Res);
                    sb.AppendFormat("{0},", model.Z);
                    sb.AppendFormat("'{0}',", model.Sequence);
                    sb.AppendFormat("{0},", model.M_Z);
                    sb.AppendFormat("'{0}',", string.Join(",", model.M_Z_ALL));
                    sb.AppendFormat("'{0}',", string.Join(",", model.M_ALL));
                    sb.AppendFormat("'{0}'),", model.Forumla);
                }

                iCnt++;
            }
        }

        public List<ModResModel> getPmFromDB(double pmFromFile, double deviation, double ipaco)
        {
            StringBuilder sb = new StringBuilder();

            //sb.AppendFormat("SELECT * FROM {0}", sTableName);
            //sb.AppendFormat(" WHERE ABS(MZ - {0}) < {1}", pmFromFile, deviation);
            //sb.Append(" ORDER BY FORUMLA");

            sb.AppendFormat("SELECT * FROM {0}", sTableName);
            sb.AppendFormat(" WHERE MZ < {0} + {1} AND MZ > {2}", pmFromFile, deviation, pmFromFile);
            sb.AppendFormat("    OR MZ < {0} AND MZ > {1} - {2}", pmFromFile, pmFromFile, deviation);
            sb.Append(" ORDER BY FORUMLA");
            DataSet ds = sqlHelper.Query(sb.ToString());

            List<ModResModel> lst = new List<ModResModel>();

            ModResModel model;
            for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
            {
                model = new ModResModel();
                model.ID = ds.Tables[0].Rows[i]["ID"].ToString();
                model.Mod_Res = ds.Tables[0].Rows[i]["MOD_RES"].ToString();
                model.Z = int.Parse(ds.Tables[0].Rows[i]["Z"].ToString());
                model.Sequence = ds.Tables[0].Rows[i]["SEQ"].ToString();
                model.M_Z = double.Parse(ds.Tables[0].Rows[i]["MZ"].ToString());
                model.Forumla = ds.Tables[0].Rows[i]["FORUMLA"].ToString();
                model.M_Z_ALL = ds.Tables[0].Rows[i]["MZ_ALL"].ToString().Split(',').Select(ms => double.Parse(ms)).ToList();
                model.M_ALL = ds.Tables[0].Rows[i]["Relative_Abundance_ALL"].ToString().Split(',').Select(ms => double.Parse(ms)).ToList();

                List<MassPoint> lstMassPoint = new List<MassPoint>();
                for (int j = 0; j < model.M_Z_ALL.Count; j++)
                {
                    MassPoint mp = new MassPoint();

                    mp.Mass = Convert.ToDouble(model.M_Z_ALL[j]);
                    mp.Intensity = Convert.ToDouble(model.M_ALL[j]);

                    if (Convert.ToDouble(model.M_ALL[j]) * 100 > ipaco)
                    {
                        if (Convert.ToDouble(model.M_ALL[j]) == 1.000)
                        {
                            model.M = lstMassPoint.Count;
                        }
                        lstMassPoint.Add(mp);
                    }
                }

                model.Mass_Point = lstMassPoint;
                lst.Add(model);
            }

            return lst;
        }

        public List<ModResModel> getPmFromDB(double ipaco)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("SELECT max(z) MAXZ,Parent_ions.* FROM Parent_ions group by seq,mod_res");
            //sb.Append(" WHERE ABS(MZ - " + pmFromFile.ToString() + ") < " + deviation + "order by FORUMLA");

            DataSet ds = sqlHelper.Query(sb.ToString());

            int count = ds.Tables[0].Rows.Count;

            List<ModResModel> lst = new List<ModResModel>();

            ModResModel model = new ModResModel();
            for (int i = 0; i < count; i++)
            {

                //if (model.Forumla == ds.Tables[0].Rows[i]["FORUMLA"].ToString())
                //{
                //continue;
                //}
                model = new ModResModel();
                model.ID = ds.Tables[0].Rows[i]["ID"].ToString();
                model.Mod_Res = ds.Tables[0].Rows[i]["MOD_RES"].ToString();
                model.Z = int.Parse(ds.Tables[0].Rows[i]["MAXZ"].ToString());
                model.Sequence = ds.Tables[0].Rows[i]["SEQ"].ToString();
                model.M_Z = double.Parse(ds.Tables[0].Rows[i]["MZ"].ToString());
                model.Forumla = ds.Tables[0].Rows[i]["FORUMLA"].ToString();
                model.M_Z_ALL = ds.Tables[0].Rows[i]["MZ_ALL"].ToString().Split(',').Select(ms => double.Parse(ms)).ToList();
                model.M_ALL = ds.Tables[0].Rows[i]["Relative_Abundance_ALL"].ToString().Split(',').Select(ms => double.Parse(ms)).ToList();
                List<MassPoint> lstMassPoint = new List<MassPoint>();

                for (int j = 0; j < model.M_Z_ALL.Count; j++)
                {
                    MassPoint mp = new MassPoint();

                    mp.Mass = Convert.ToDouble(model.M_Z_ALL[j]);
                    mp.Intensity = Convert.ToDouble(model.M_ALL[j]);

                    if (Convert.ToDouble(model.M_ALL[j]) * 100 > ipaco)
                    {
                        if (Convert.ToDouble(model.M_ALL[j]) == 1.000)
                        {
                            model.M = lstMassPoint.Count;
                        }
                        lstMassPoint.Add(mp);
                    }


                }

                model.Mass_Point = lstMassPoint;
                lst.Add(model);
            }

            return lst;
        }

        public List<string[]> getFmFromDB(string ID, string mod_res,string scanTpye,string seq)
        {
            StringBuilder sb = new StringBuilder();
            List<string[]> returnValue = new List<string[]>();
            sb.Append("SELECT * FROM " + "Fragment_ions");
            sb.Append(" WHERE MOD_RES='" + mod_res + "'");
            //sb.Append(" and SEQ='" + seq + "'");
            //sb.Append(" WHERE ID ='" + ID+"' and MOD_RES='"+mod_res+"'");
            //sb.Append(" WHERE ID ='" + ID + "' and MOD_RES='S1,ac;K20,me2;'");
            //sb.Append(" WHERE ID ='" + ID + "' and MOD_RES=''");
            DataSet ds = sqlHelper.Query(sb.ToString());

            //int count = ds.Tables[0].Rows.Count;
            var drrArr = ds.Tables[0].AsEnumerable().Where(query => query["SEQ"].Equals(seq));
            DataTable dt = drrArr.CopyToDataTable();
            int count = dt.Rows.Count;

            List<ModResModel> lst = new List<ModResModel>();

            ModResModel model = new ModResModel();

            string[] etd_mz = null;
            string[] etd_m = null;
            string[] etd_max = null;
            string useStr = "";
            for (int i = 0; i < count; i++)
            {
                if (scanTpye == "ETD")
                {
                    dt.Rows[i]["ETD_MZ"].ToString();
                    dt.Rows[i]["ETD_Relative_Abundance"].ToString();
                    dt.Rows[i]["ETD_MZ_MAX"].ToString();
                    useStr = dt.Rows[i]["ETD_MZ"].ToString();
                    useStr = System.Text.RegularExpressions.Regex.Replace(useStr, @"([\r\n])[\s]+", "", RegexOptions.IgnoreCase);
                    etd_mz = useStr.Split(new string[] { "&" }, StringSplitOptions.RemoveEmptyEntries);
                    useStr = dt.Rows[i]["ETD_Relative_Abundance"].ToString();
                    useStr = System.Text.RegularExpressions.Regex.Replace(useStr, @"([\r\n])[\s]+", "", RegexOptions.IgnoreCase);
                    etd_m = useStr.Split(new string[] { "&" }, StringSplitOptions.RemoveEmptyEntries);
                    useStr = dt.Rows[i]["ETD_MZ_MAX"].ToString();
                    useStr = System.Text.RegularExpressions.Regex.Replace(useStr, @"([\r\n])[\s]+", "", RegexOptions.IgnoreCase);
                    etd_max = useStr.Split(new string[] { "&" }, StringSplitOptions.RemoveEmptyEntries);
                }
                else
                {
                    dt.Rows[i]["CID_MZ"].ToString();
                    dt.Rows[i]["CID_Relative_Abundance"].ToString();
                    dt.Rows[i]["CID_MZ_MAX"].ToString();
                    useStr = dt.Rows[i]["CID_MZ"].ToString();
                    useStr = System.Text.RegularExpressions.Regex.Replace(useStr, @"([\r\n])[\s]+", "", RegexOptions.IgnoreCase);
                    etd_mz = useStr.Split(new string[] { "&" }, StringSplitOptions.RemoveEmptyEntries);
                    useStr = dt.Rows[i]["CID_Relative_Abundance"].ToString();
                    useStr = System.Text.RegularExpressions.Regex.Replace(useStr, @"([\r\n])[\s]+", "", RegexOptions.IgnoreCase);
                    etd_m = useStr.Split(new string[] { "&" }, StringSplitOptions.RemoveEmptyEntries);
                    useStr = dt.Rows[i]["CID_MZ_MAX"].ToString();
                    useStr = System.Text.RegularExpressions.Regex.Replace(useStr, @"([\r\n])[\s]+", "", RegexOptions.IgnoreCase);
                    etd_max = useStr.Split(new string[] { "&" }, StringSplitOptions.RemoveEmptyEntries);

                }
                returnValue.Add(etd_mz);
                returnValue.Add(etd_m);
                returnValue.Add(etd_max);
            }

            return returnValue;
        }

        public DataSet Select_GetModResLikeModRes(string seq,string modRes, string order)
        {
            StringBuilder sb = new StringBuilder();

            string sKey = modRes.Split(',')[0].Substring(0, 1);
            string sValue = modRes.Split(',')[0].Substring(1);
            string sModRes = modRes.Split(',')[1];

            string sFormat1 = string.Format("__,{1};", sKey, sModRes);
            string sFormat2 = string.Format("___,{1};", sKey, sModRes);
            string sFormat3 = string.Format("____,{1};", sKey, sModRes);
            string sFormat4 = string.Format("_____,{1};", sKey, sModRes);

            sb.Append(" SELECT TAB.* FROM ");
            sb.Append(" ( ");
            sb.Append("     SELECT SUBSTRING(SUBSTRING_INDEX(MOD_RES,',',1),2) IDX,MOD_RES");
            sb.AppendFormat(" FROM {0} ",sTableName);
            sb.Append("      WHERE Z = 1");
            sb.AppendFormat("  AND SEQ = '{0}'" ,seq);
            sb.AppendFormat("  AND (MOD_RES LIKE '{0}%' AND char_length(MOD_RES) = {1}", sFormat1, sFormat1.Length);
            sb.AppendFormat("    OR MOD_RES LIKE '{0}%' AND char_length(MOD_RES) = {1}", sFormat2, sFormat2.Length);
            sb.AppendFormat("    OR MOD_RES LIKE '{0}%' AND char_length(MOD_RES) = {1}", sFormat3, sFormat3.Length);
            sb.AppendFormat("    OR MOD_RES LIKE '{0}%' AND char_length(MOD_RES) = {1})", sFormat4, sFormat4.Length);
            sb.Append(" ) TAB ");
            //sb.AppendFormat(" WHERE CONVERT(TAB.IDX,SIGNED) > {0}", sValue);
            sb.Append(" WHERE 1 = 1 ");
            sb.AppendFormat(" ORDER BY CONVERT(TAB.IDX,SIGNED) {0};", order);
            return sqlHelper.Query(sb.ToString());
        }

        public DataSet Select_GetModResLikeModRes(string id, string seq)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(" SELECT DISTINCT SUBSTRING(SUBSTRING_INDEX(MOD_RES,',',1),2) IDX,");
            sb.AppendFormat("           SUBSTRING_INDEX(MOD_RES,';',1) ModRes FROM {0} ", sTableName);
            sb.Append("        WHERE Z = 1 AND MOD_RES != '' ");
            sb.AppendFormat("    AND ID = '{0}'", id);
            sb.AppendFormat("    AND SEQ = '{0}'", seq);
            return sqlHelper.Query(sb.ToString());
        }

        public DataSet Select_DatabaseInfo(string sDataBaseName)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("SELECT EntriesTbl.Entries AS Entries,");
            sb.Append("       IsoformTbl.Isoforms AS Isoforms");
            sb.Append("  FROM ");
            sb.AppendFormat("(SELECT COUNT(*) AS Isoforms FROM {0}.{1} WHERE Z = 1) IsoformTbl,", sDataBaseName, sTableName);
            sb.AppendFormat("(SELECT COUNT(*) AS Entries FROM {0}.{1}) EntriesTbl", sDataBaseName, sTableName);

            return sqlHelper.Query(sb.ToString());
        }
    }
}
