using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProteinGoggle.Model
{
    [Serializable]
    public class ProteinModResConfig
    {
        public string Name { get; set; }
        public string ShortName { get; set; }
        public string Formula { get; set; }
        public string Color { get; set; }
    }
}
