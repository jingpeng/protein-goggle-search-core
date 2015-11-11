using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProteinGoggle.Model;

namespace ProteinGoggle.BLL
{
    public class Analysis
    {

        private static IDictionary<string, Dictionary<string, int>> aminoAcidTable;
        private IDictionary<string, Dictionary<string, int>> modResAminoAcidTable;
        private IDictionary<string, string> modResSymbolTable;
        private Table table = new Table();

        public Analysis()
        {
            aminoAcidTable = Table.makeAminoAcidsTable();
            modResAminoAcidTable = Table.ModResAminoAcidsTable;
            modResSymbolTable = Table.ModResSymbolTable;
        }

        //分析执行
        public ipc.IPC.Results AnalysisCalc(AnalysisParameter oParameter)
        {
            ipc.IPC isotopePatternCalc = new ipc.IPC();
            ipc.IPC.Options option = new ipc.IPC.Options();

            option.parseChemFormulaAndAdd(oParameter.Formula);
            option.setFastCalc(oParameter.FastCalc);
            option.setPrintOutput(true);
            option.setBinPeaks(ipc.IPC.binningType.FIXED_BINNING);
            option.setCharge(oParameter.ValenceState);

            ipc.IPC.Results result = isotopePatternCalc.execute(option);
            return result;
        }

        //通过序列得到分子式
        public string GetFormulaBySequence(string sSeq)
        {
            StringBuilder sb = new StringBuilder();
            Dictionary<string, int> components = new Dictionary<string, int>();
            Dictionary<string, int>.KeyCollection keys;

            char[] arrSeq = sSeq.ToCharArray();

            foreach (char cSeq in arrSeq)
            {
                keys = aminoAcidTable[cSeq.ToString()].Keys;

                foreach (string key in keys)
                {
                    if (components.Keys.Contains(key) == true)
                        components[key] = (int)components[key] + (int)aminoAcidTable[cSeq.ToString()][key];
                    else
                        components[key] = (int)aminoAcidTable[cSeq.ToString()][key];
                }
            }

            components["H"] = (int)components["H"] - (sSeq.Count() - 1) * 2;
            components["O"] = (int)components["O"] - (sSeq.Count() - 1);
            components["H"] = (int)components["H"] + 1;

            foreach (KeyValuePair<string, int> key in components)
                sb.Append(key.Key + key.Value);

            return sb.ToString();
        }

        //通过序列得到分子式容器
        public Dictionary<string, int> GetFormulaComponentsBySequence(string sSeq)
        {
            Dictionary<string, int> components = new Dictionary<string, int>();
            Dictionary<string, int>.KeyCollection keys;
            char[] arrSeq = sSeq.ToCharArray();

            foreach (char cSeq in arrSeq)
            {
                keys = aminoAcidTable[cSeq.ToString()].Keys;

                foreach (string key in keys)
                {
                    if (components.Keys.Contains(key) == true)
                        components[key] = (int)components[key] + (int)aminoAcidTable[cSeq.ToString()][key];
                    else
                        components[key] = (int)aminoAcidTable[cSeq.ToString()][key];
                }
            }

            components["H"] = (int)components["H"] - (sSeq.Count() - 1) * 2;
            components["O"] = (int)components["O"] - (sSeq.Count() - 1);
            components["H"] = (int)components["H"] + 1;

            return components;
        }

        //通过分子式容器再次生成分子式
        public string GetFormulaByComponents(Dictionary<string, int> components, string[] strModResValue)
        {
            StringBuilder sb = new StringBuilder();
            Dictionary<string, int> dict = new Dictionary<string, int>(components);
            Dictionary<string, int>.KeyCollection keys;

            foreach (string sModResValue in strModResValue)
            {
                keys = modResAminoAcidTable[sModResValue].Keys;

                foreach (string key in keys)
                {
                    if (dict.Keys.Contains(key) == true)
                        dict[key] = (int)dict[key] + (int)modResAminoAcidTable[sModResValue][key];
                    else
                        dict[key] = (int)modResAminoAcidTable[sModResValue][key];
                }
            }

            foreach (KeyValuePair<string, int> key in dict)
                sb.Append(key.Key + key.Value);

            return sb.ToString();
        }

        //通过分子式容器再次生成分子式容器
        public Dictionary<string, int> GetFormulaComponentsByComponents(Dictionary<string, int> components, Dictionary<string, int> dictNew)
        {

            if (dictNew == null)
                return components;

            Dictionary<string, int> dict = new Dictionary<string, int>(components);
            Dictionary<string, int>.KeyCollection keys = dictNew.Keys;

            foreach (string key in keys)
            {
                if (dict.Keys.Contains(key) == true)
                    dict[key] = (int)dict[key] + (int)dictNew[key];
                else
                    dict[key] = (int)dictNew[key];
            }

            return dict;
        }
    }

    public class AnalysisParameter
    {
        //分子式
        public string Formula { get; set; }

        //序列
        public string Sequence { get; set; }

        //价态
        public int ValenceState { get; set; }

        //快速计算
        public long FastCalc { get; set; }
    }
}
