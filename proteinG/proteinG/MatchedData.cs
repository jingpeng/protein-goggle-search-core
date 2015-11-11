using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace ProteinGoggle.Model
{
    [Serializable]
    public class MatchedData : ICloneable
    {
        public string ID { get; set; }
        public string MOD_RES { get; set; }
        public int Z { get; set; }
        public string DatasetFile { get; set; }
        public string Database { get; set; }
        public string Repository { get; set; }
        public string Index { get; set; }
        public string ScreeningApproach { get; set; }
        public string ExpMZ { get; set; }
        public string TheoMZ { get; set; }
        public string Error_PPM { get; set; }
        public string AccessionNumber { get; set; }
        public string ProteinName { get; set; }
        public int Length { get; set; }
        public string Function { get; set; }
        public string Sequence { get; set; }
        public string ExpMass_DA { get; set; }
        public string NMFS { get; set; }
        public string Actualpmfs { get; set; }
        public string PTM_SCORE { get; set; }
        public string PTM_S { get; set; }
        public List<MassPoint> MatchedMsDataTheo { get; set; }
        public List<MassPoint> MatchedMsDataExp { get; set; }
        public List<ModResModel> MatchedMs2 { get; set; }
        public string DataBase_PTM_SCORE { get; set; }
        public string Global_PTM_SCORE { get; set; }
        public string Total_PTM_Score { get; set; }

        public object Clone()
        {
            MemoryStream ms = new MemoryStream();
            BinaryFormatter bf = new BinaryFormatter();
            bf.Serialize(ms, this);
            ms.Position = 0;
            object obj = bf.Deserialize(ms);
            ms.Close();

            return obj;
        }
    }
}
