using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProteinGoggle.Model
{
    [Serializable]
    public class ParameterModel
    {
        public double MS2_FAT { get; set; }
        public double MS2_IPMD { get; set; }
        public double MS2_IPACO { get; set; }
        public double MS2_IPMDO { get; set; }
        public double MS2_IPMDOM { get; set; }
        public double MS2_IPAD { get; set; }
        public double MS2_IPADO { get; set; }
        public double MS2_IPADOM { get; set; }
        public double MS2_PMFS { get; set; }
        public double MS2_PTMS { get; set; }
        public int MS2_SA { get; set; }

        public double MS_PAT { get; set; }
        public double MS_IPMD { get; set; }
        public double MS_IPACO { get; set; }
        public double MS_IPMDO { get; set; }
        public double MS_IPMDOM { get; set; }
        public double MS_IPAD { get; set; }
        public double MS_IPADO { get; set; }
        public double MS_IPADOM { get; set; }
        public double MS_WINDOW { get; set; }
    }
}
