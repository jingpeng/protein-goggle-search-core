using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace ProteinGoggle.Model
{
    [Serializable]
    public class ModResModel : ICloneable
    {
        public string ID { get; set; }
        public string Mod_Res { get; set; }
        public string SEQKBN { get; set; }
        public string Sequence { get; set; }
        public int Z { get; set; }
        public double M_Z { get; set; }
        public double M { get; set; }
        public List<double> M_Z_ALL { get; set; }
        public List<double> M_ALL { get; set; }
        public string Forumla { get; set; }
        public bool IsGet { get; set; }
        public List<MassPoint> Mass_Point { get; set; }
        public List<MassPoint> Mass_Point_FromFile { get; set; }

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
