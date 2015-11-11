using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProteinGoggle.Model
{
    public class ProteinSession
    {
        public string ID { get; set; }
        public IList<string> Sequence { get; set; }
        public IList<string> SeqKbn { get; set; }
        public int ValenceState { get; set; }
        public int FastCalc { get; set; }
        public IDictionary<string, IList<string>> Mod_Res { get; set; }
        public int Mod_Res_Count { get; set; }
        public DataBaseParameter DBParameter { get; set; }
        public string DataBase { get; set; }
    }
}
