using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ProteinGoggle.Model;

namespace ProteinGoggle.BLL
{
    public class Table
    {
        public static readonly double H = 1.00782d;

        public static List<ProteinModResConfig> ModResConfig = new List<ProteinModResConfig>();
        public static IDictionary<string, string> ModResSymbolTable;
        public static IDictionary<string, Dictionary<string, int>> ModResAminoAcidsTable;

        private static List<string> MolecularElements = new List<string>() { "C", "H", "N", "O", "S", "P" };

        public static Dictionary<string, Dictionary<string, int>> makeAminoAcidsTable()
        {
            Dictionary<string, Dictionary<string, int>> newAminoAcidTable = new Dictionary<string, Dictionary<string, int>>();

            Dictionary<string, int> map = new Dictionary<string, int>();
            newAminoAcidTable.Add("A", map);
            map.Add("C", 3);
            map.Add("H", 7);
            map.Add("N", 1);
            map.Add("O", 2);

            map = new Dictionary<string, int>();
            newAminoAcidTable.Add("R", map);
            map.Add("C", 6);
            map.Add("H", 14);
            map.Add("N", 4);
            map.Add("O", 2);

            map = new Dictionary<string, int>();
            newAminoAcidTable.Add("N", map);
            map.Add("C", 4);
            map.Add("H", 8);
            map.Add("N", 2);
            map.Add("O", 3);

            map = new Dictionary<string, int>();
            newAminoAcidTable.Add("D", map);
            map.Add("C", 4);
            map.Add("H", 7);
            map.Add("N", 1);
            map.Add("O", 4);

            map = new Dictionary<string, int>();
            newAminoAcidTable.Add("c", map);
            map.Add("C", 5);
            map.Add("H", 10);
            map.Add("N", 2);
            map.Add("O", 3);
            map.Add("S", 1);

            map = new Dictionary<string, int>();
            newAminoAcidTable.Add("C", map);
            map.Add("C", 3);
            map.Add("H", 7);
            map.Add("N", 1);
            map.Add("O", 2);
            map.Add("S", 1);

            map = new Dictionary<string, int>();
            newAminoAcidTable.Add("E", map);
            map.Add("C", 5);
            map.Add("H", 9);
            map.Add("N", 1);
            map.Add("O", 4);

            map = new Dictionary<string, int>();
            newAminoAcidTable.Add("Q", map);
            map.Add("C", 5);
            map.Add("H", 10);
            map.Add("N", 2);
            map.Add("O", 3);

            map = new Dictionary<string, int>();
            newAminoAcidTable.Add("G", map);
            map.Add("C", 2);
            map.Add("H", 5);
            map.Add("N", 1);
            map.Add("O", 2);

            map = new Dictionary<string, int>();
            newAminoAcidTable.Add("H", map);
            map.Add("C", 6);
            map.Add("H", 9);
            map.Add("N", 3);
            map.Add("O", 2);

            map = new Dictionary<string, int>();
            newAminoAcidTable.Add("I", map);
            newAminoAcidTable.Add("L", map);
            map.Add("C", 6);
            map.Add("H", 13);
            map.Add("N", 1);
            map.Add("O", 2);

            map = new Dictionary<string, int>();
            newAminoAcidTable.Add("K", map);
            map.Add("C", 6);
            map.Add("H", 14);
            map.Add("N", 2);
            map.Add("O", 2);

            map = new Dictionary<string, int>();
            newAminoAcidTable.Add("M", map);
            map.Add("C", 5);
            map.Add("H", 11);
            map.Add("N", 1);
            map.Add("O", 2);
            map.Add("S", 1);

            map = new Dictionary<string, int>();
            newAminoAcidTable.Add("F", map);
            map.Add("C", 9);
            map.Add("H", 11);
            map.Add("N", 1);
            map.Add("O", 2);

            map = new Dictionary<string, int>();
            newAminoAcidTable.Add("P", map);
            map.Add("C", 5);
            map.Add("H", 9);
            map.Add("N", 1);
            map.Add("O", 2);
            map = new Dictionary<string, int>();
            newAminoAcidTable.Add("S", map);
            map.Add("C", 3);
            map.Add("H", 7);
            map.Add("N", 1);
            map.Add("O", 3);

            map = new Dictionary<string, int>();
            newAminoAcidTable.Add("T", map);
            map.Add("C", 4);
            map.Add("H", 9);
            map.Add("N", 1);
            map.Add("O", 3);

            map = new Dictionary<string, int>();
            newAminoAcidTable.Add("W", map);
            map.Add("C", 11);
            map.Add("H", 12);
            map.Add("N", 2);
            map.Add("O", 2);

            map = new Dictionary<string, int>();
            newAminoAcidTable.Add("Y", map);
            map.Add("C", 9);
            map.Add("H", 11);
            map.Add("N", 1);
            map.Add("O", 3);

            map = new Dictionary<string, int>();
            newAminoAcidTable.Add("V", map);
            map.Add("C", 5);
            map.Add("H", 11);
            map.Add("N", 1);
            map.Add("O", 2);

            //map = new Dictionary<string, int>();
            //newAminoAcidTable.Add("U", map);
            //map.Add("C", 4);
            //map.Add("H", 7);
            //map.Add("N", 1);
            //map.Add("O", 2);

            //map = new Dictionary<string, int>();
            //newAminoAcidTable.Add("O", map);
            //map.Add("C", 5);
            //map.Add("H", 10);
            //map.Add("N", 2);
            //map.Add("O", 1);

            //map = new Dictionary<string, int>();
            //newAminoAcidTable.Add("Z", map);
            //map.Add("C", 5);
            //map.Add("H", 8);
            //map.Add("N", 2);
            //map.Add("O", 2);

            //map = new Dictionary<string, int>();
            //newAminoAcidTable.Add("B", map);
            //map.Add("C", 4);
            //map.Add("H", 6);
            //map.Add("N", 2);
            //map.Add("O", 2);

            //map = new Dictionary<string, int>();
            //newAminoAcidTable.Add("X", map);
            //map.Add("C", 6);
            //map.Add("H", 8);
            //map.Add("N", 2);
            //map.Add("O", 2);

            return newAminoAcidTable;
        }

        public static IDictionary<string, Dictionary<string, int>> makeModResAminoAcidsTable()
        {
            IDictionary<string, Dictionary<string, int>> newAminoAcidTable = new Dictionary<string, Dictionary<string, int>>();
            Dictionary<string, string> map = null;
            string sFormula = string.Empty;

            try
            {
                foreach (var ProteinModResConfig in ModResConfig)
                {
                    map = new Dictionary<string, string>();

                    sFormula = ProteinModResConfig.Formula;
                    Console.WriteLine(ProteinModResConfig.ShortName);
                    string sContent = string.Empty;
                    string key = string.Empty;
                    bool bFirst = false;
                    for (int i = 0; i < sFormula.Length; i++)
                    {
                        sContent = string.Empty;

                        if (MolecularElements.Contains(sFormula.Substring(i, 1)))
                        {
                            key = sFormula.Substring(i, 1);
                            map.Add(key, "1");
                            bFirst = true;
                        }
                        else
                        {
                            if (bFirst == true)
                            {
                                map[key] = string.Empty;
                                bFirst = false;
                            }

                            map[key] += sFormula.Substring(i, 1);
                        }
                    }

                    newAminoAcidTable.Add(ProteinModResConfig.ShortName, map.ToDictionary(ms => ms.Key, ms => int.Parse(ms.Value)));
                }
            }
            catch (Exception)
            {

                throw;
            }



            return newAminoAcidTable;
        }

        public static IDictionary<string, string> makeModResSymbolTable()
        {
            IDictionary<string, string> newAminoAcidTable = new Dictionary<string, string>();

            foreach (var ProteinModResConfig in ModResConfig)
            {
                newAminoAcidTable.Add(ProteinModResConfig.Name, ProteinModResConfig.ShortName);
            }

            return newAminoAcidTable;
        }

        public static Dictionary<string, int> makeETDCXTable()
        {
            Dictionary<string, int> newETDTable = new Dictionary<string, int>();
            newETDTable.Add("N", 1);
            newETDTable.Add("H", 1);
            newETDTable.Add("O", -1);
            return newETDTable;
        }

        public static Dictionary<string, int> makeETDYZTable()
        {
            Dictionary<string, int> newETDTable = new Dictionary<string, int>();
            newETDTable.Add("N", -1);
            newETDTable.Add("H", -2);
            return newETDTable;
        }

        public static Dictionary<string, int> makeHCDBXTable()
        {
            Dictionary<string, int> newHCDTable = new Dictionary<string, int>();
            newHCDTable.Add("O", -1);
            newHCDTable.Add("H", -2);
            return newHCDTable;
        }
    }
}
