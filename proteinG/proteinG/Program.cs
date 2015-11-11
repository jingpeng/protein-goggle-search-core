using System;
using System.Configuration;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading;
using System.Xml;
using System.Xml.Linq;
using System.Text.RegularExpressions;

namespace ProteinGoggle.Model
{
    class Program
    {

        string DBName = "p83570_20151109192646";
        string FileName = "Ecoli_20141208_1";
        string FolderName = "C:/Users/Administrator/Desktop/Ecoli_20141208_1";
        string sMZXMLFileName = "C:\\RawTemp\\Ecoli_20141208_1.mzxml";  

        static void Main(string[] args)
        {
            Program p = new Program();
            p.ConvertMatchData();
        
        }

        private void ConvertMatchData()
        {
            //sMZXMLFileName XML 文件

            

            FileProcess filePro = new FileProcess();
            XmlReader xmlReader = XmlReader.Create(sMZXMLFileName);
            PeptideBLL peptideBLL = new PeptideBLL(DBName);
            PeptideModResBLL peModResBLL = new PeptideModResBLL(DBName);

            try
            {

                List<MatchedData> lstMatchedData = this.GetMatchedData(xmlReader);

                if (lstMatchedData == null || lstMatchedData.Count == 0)
                {
                    this.OutputFile(lstMatchedData);
                }

                DataTable dtFlat_Txt = peptideBLL.GetPeptideData();

                for (int idx = 0; idx < dtFlat_Txt.Rows.Count; idx++)
                {
                    DataRow drArr = dtFlat_Txt.Rows[idx];

                    string sAC = drArr["AC"].ToString().Split(';')[0];

                    string sDE = drArr["DE"].ToString();
                    sDE = sDE.Replace("RecName: Full=", "");
                    sDE = sDE.Remove(sDE.Length - 1);

                    string sCC = drArr["CC"].ToString();
                    int iIndex = sCC.IndexOf("-!-", 5);
                    sCC = iIndex == -1 ? sCC.Substring(4) : sCC.Substring(4, iIndex - 4);

                    string sID = drArr["ID"].ToString();
                    string sSQ = drArr["SQ"].ToString();

                    string sModResSingle = drArr["ModResSingle"].ToString();

                    lstMatchedData.Where(ms => ms.ID == sID).ToList().ForEach(ms =>
                    {
                        ms.Database = DBName;
                        ms.DatasetFile = sMZXMLFileName;
                        ms.Repository = FileName;
                        ms.AccessionNumber = sAC;
                        ms.Function = sCC;
                        ms.ID = sID;
                        ms.ProteinName = sDE;
                        ms.Length = ms.Sequence.Length;
                        ms.Actualpmfs = (double.Parse(ms.NMFS) / (2 * (sSQ.Length - 1)) * 100).ToString();
                    });
                }

                lstMatchedData = lstMatchedData.Where(ms => string.IsNullOrEmpty(ms.Actualpmfs) == false
                                                         && decimal.Parse(ms.Actualpmfs) >= 5).ToList();

                for (int idx = 0; idx < lstMatchedData.Count; idx++)
                {
                    string[] modResArray = lstMatchedData[idx].MOD_RES.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                    int iCnt;
                    int iTotalCnt = 0;
                    int iStart;
                    int iEnd;
                    string sKey;

                    DataRow[] drArr = dtFlat_Txt.Select("ID = '" + lstMatchedData[0].ID + "'");
                    string sModResSingle = drArr[0]["ModResSingle"].ToString();

                    if (sModResSingle == null || sModResSingle == string.Empty || sModResSingle.Length == 0)
                        continue;

                    string[] modResDB = sModResSingle.Split('@');

                    for (int iModRes = 0; iModRes < modResArray.Length; iModRes++)
                    {
                        iCnt = 0;

                        List<string> lstStart = (from p in modResDB
                                                 where p.EndsWith(string.Format(",{0};", modResArray[iModRes].Split(',')[1]))
                                                 orderby int.Parse(p.Split(',')[0].Substring(1))
                                                 select p).ToList();

                        lstStart = this.PTM_Score_Filter(lstStart, modResArray[iModRes], "asc");

                        List<string> lstEnd = (from p in modResDB
                                               where p.EndsWith(string.Format(",{0};", modResArray[iModRes].Split(',')[1]))
                                               orderby int.Parse(p.Split(',')[0].Substring(1)) descending
                                               select p).ToList();

                        lstEnd = this.PTM_Score_Filter(lstEnd, modResArray[iModRes], "desc");

                        sKey = modResArray[iModRes].Split(',')[0].Substring(0, 1);

                        if ((lstStart == null || lstStart.Count == 0) && (lstEnd == null || lstEnd.Count == 0))
                        {
                            lstMatchedData[idx].DataBase_PTM_SCORE += string.Format("{0},{1};", sKey, iCnt.ToString());
                            continue;
                        }

                        if (lstStart != null && lstStart.Count != 0)
                        {
                            if (lstStart.Count == 1)
                            {
                                iStart = int.Parse(lstStart[0].Split(',')[0].Substring(1));
                                iEnd = lstMatchedData[idx].Sequence.Length;
                            }
                            else
                            {
                                iStart = int.Parse(lstStart[0].Split(',')[0].Substring(1));
                                iEnd = int.Parse(lstStart[1].Split(',')[0].Substring(1));
                            }

                            for (int i = iStart; i < iEnd; i++)
                            {
                                string strC = string.Format("C{0}", i.ToString());
                                string strB = string.Format("B{0}", i.ToString());
                                if (lstMatchedData[idx].MatchedMs2.Exists(ms2 => ms2.ID == strC || ms2.ID == strB))
                                {
                                    iCnt++;
                                    iTotalCnt++;
                                }
                            }
                        }

                        if (lstEnd != null && lstEnd.Count != 0)
                        {
                            if (lstEnd.Count == 1)
                            {
                                iStart = int.Parse(lstEnd[0].Split(',')[0].Substring(1));
                                iEnd = 0;
                            }
                            else
                            {
                                iStart = int.Parse(lstEnd[0].Split(',')[0].Substring(1));
                                iEnd = int.Parse(lstEnd[1].Split(',')[0].Substring(1));
                            }

                            for (int i = lstMatchedData[idx].Length - iStart + 1; i < lstMatchedData[idx].Length - iEnd + 1; i++)
                            {
                                string strZ = string.Format("Z{0}", i.ToString());
                                string strY = string.Format("Y{0}", i.ToString());
                                if (lstMatchedData[idx].MatchedMs2.Exists(ms2 => ms2.ID == strZ || ms2.ID == strY))
                                {
                                    iCnt++;
                                    iTotalCnt++;
                                }
                            }
                        }

                        sKey = modResArray[iModRes].Split(',')[0].Substring(0, 1);
                        lstMatchedData[idx].DataBase_PTM_SCORE += string.Format("{0},{1};", sKey, iCnt.ToString());
                    }

                    lstMatchedData[idx].Total_PTM_Score = iTotalCnt.ToString();
                }

                lstMatchedData = lstMatchedData.Where(ms =>
                {
                    if (ms.MOD_RES == string.Empty)
                        return true;

                    var temp = ms.DataBase_PTM_SCORE.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

                    for (int i = 0; i < temp.Length; i++)
                    {
                        if (decimal.Parse(temp[i].Split(',')[1]) <1)
                            return false;
                    }

                    return true;
                }).ToList();

                if (lstMatchedData == null || lstMatchedData.Count == 0)
                {
                    this.OutputFile(lstMatchedData);
                    return;
                }

                this.OutputFile(lstMatchedData);

                string sSaveResultPath = "MetaData";

                if (Directory.Exists(sSaveResultPath) == false)
                    System.IO.Directory.CreateDirectory(sSaveResultPath);

                sSaveResultPath = string.Format(@"{0}\{1}.bin", sSaveResultPath, FileName);
                filePro.Serializer<List<MatchedData>>(lstMatchedData, sSaveResultPath);

                //if (sSaveResultPath != string.Empty)
                //{
                //    this.Invoke(new InvokeDialog(OpenView), new object[] { sSaveResultPath, lstMatchedData });
                //}
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                xmlReader.Close();
                xmlReader = null;
            }
        }

        private void OutputFile(List<MatchedData> lstMatchedData)
        {
            FileProcess filePro = new FileProcess();

            //Parameter Output
            string sParameterFileName = System.IO.Path.Combine(new string[] { FolderName, "output" +".bin" });
            ParameterModel model = SaveControlToModel();
            filePro.Serializer<ParameterModel>(model, sParameterFileName);

            if (lstMatchedData.Count == 0)
                return;

            //Excel Output
            string sXlsName = System.IO.Path.Combine(new string[] { FolderName, "output" +".xls" });
            filePro.ExcelOut(lstMatchedData, sXlsName);

        }

        private List<string> PTM_Score_Filter(List<string> lst, string modRes, string order)
        {
            if (lst == null || lst.Count == 0)
                return null;

            if (lst.Count == 1)
                return lst;

            string sValue = modRes.Split(',')[0].Substring(1);

            if (order == "asc")
            {
                for (int i = 0; i < lst.Count; i++)
                {
                    if (int.Parse(lst[i].Split(',')[0].Substring(1)) < int.Parse(sValue))
                        return null;
                }
            }
            else
            {
                for (int i = 0; i < lst.Count; i++)
                {
                    if (int.Parse(lst[i].Split(',')[0].Substring(1)) > int.Parse(sValue))
                        return null;
                }
            }

            var table = lst.Take(2);
            return table.ToList();
        }

        private List<MatchedData> GetMatchedData(XmlReader xmlReader)
        {
            List<MassPoint> lstMassPoint = new List<MassPoint>();
            List<MatchedData> lstMatchedData = new List<MatchedData>();
            List<MatchedData> lstTemp = null;
            MassScan scanMS = new MassScan();

            PeptideModResBLL peModResBLL = new PeptideModResBLL(DBName);
            ParameterModel model = this.SaveControlToModel();
            peModResBLL.ParameterModel = model;

            bool bSingle = false;
            int iScanCount = 0;
            int currentCnt = 0;
            int allCnt = (int)99998;
            int avgCnt = 0;
            int surplusCnt = 0;

            if (allCnt >= 90)
            {
                avgCnt = allCnt / 90;
            }
            else
            {
                avgCnt = (int)Math.Round(90.0 / allCnt, MidpointRounding.AwayFromZero);
                surplusCnt = (int)Math.Round((avgCnt - 90.0 / allCnt) * allCnt);
            }

            while (xmlReader.Read())
            {
                if (xmlReader.Name == "sha1")
                    break;

                if (xmlReader.NodeType != XmlNodeType.Element)
                    continue;

                if (xmlReader.Name == "msRun")
                {
                    xmlReader.MoveToAttribute("scanCount");
                    iScanCount = int.Parse(xmlReader.Value);

                    if (allCnt > iScanCount)
                    {
                        allCnt = iScanCount;

                        if (allCnt >= 90)
                            avgCnt = allCnt / 90;
                        else
                        {
                            avgCnt = (int)Math.Round(90.0 / allCnt, MidpointRounding.AwayFromZero);
                            surplusCnt = (int)Math.Round((avgCnt - 90.0 / allCnt) * allCnt);
                        }
                    }

                    if (iScanCount == 1)
                        bSingle = true;
                }

                if (xmlReader.Name == "scan")
                {
                    if (!xmlReader.HasAttributes)
                        continue;

                    //判断 是MS 还是MS2
                    xmlReader.MoveToAttribute("msLevel");

                    if ("1".Equals(xmlReader.Value))
                    {
                        xmlReader.MoveToAttribute("num");
                        scanMS.Index = UInt32.Parse(xmlReader.Value);

                        xmlReader.MoveToAttribute("msLevel");
                        scanMS.MsLevel = UInt32.Parse(xmlReader.Value);

                        xmlReader.MoveToAttribute("peaksCount");
                        scanMS.PeaksCount = UInt32.Parse(xmlReader.Value);

                        xmlReader.MoveToAttribute("polarity");
                        scanMS.Polarity = xmlReader.Value == "+" ? true : false;

                        xmlReader.MoveToAttribute("basePeakIntensity");
                        scanMS.BasePeakIntensity = double.Parse(xmlReader.Value);

                        while (xmlReader.Read())
                        {
                            if (xmlReader.Name == "peaks")
                            {
                                scanMS.IsPeaks = true;
                                scanMS.Peaks = xmlReader.ReadInnerXml();
                                lstMassPoint = Base64Convert.CalcMz(scanMS.Peaks, scanMS.PeaksCount);
                                break;
                            }
                        }
                    }
                    else if ("2".Equals(xmlReader.Value))
                    {
                        //读取属性
                        MassScan scanMS2 = new MassScan();

                        xmlReader.MoveToAttribute("num");
                        scanMS2.Index = UInt32.Parse(xmlReader.Value);

                        xmlReader.MoveToAttribute("msLevel");
                        scanMS2.MsLevel = UInt32.Parse(xmlReader.Value);

                        xmlReader.MoveToAttribute("peaksCount");
                        scanMS2.PeaksCount = UInt32.Parse(xmlReader.Value);

                        xmlReader.MoveToAttribute("polarity");
                        scanMS2.Polarity = xmlReader.Value == "+" ? true : false;

                        xmlReader.MoveToAttribute("basePeakIntensity");
                        scanMS2.BasePeakIntensity = double.Parse(xmlReader.Value);

                        while (xmlReader.Read())
                        {
                            if (xmlReader.NodeType != XmlNodeType.Element)
                                continue;

                            if (xmlReader.Name == "precursorMz")
                            {
                                xmlReader.MoveToAttribute("activationMethod");
                                scanMS2.ScanType = xmlReader.Value;
                                xmlReader.MoveToElement();
                                scanMS2.PrecursorMz = float.Parse(xmlReader.ReadInnerXml());
                            }
                            if (xmlReader.Name == "peaks")
                            {
                                scanMS2.Peaks = xmlReader.ReadInnerXml();
                                break;
                            }
                        }

                        if (bSingle == true)
                        {
                            if (scanMS2 == null)
                                return null;

                            lstTemp = peModResBLL.getMSMatchs(scanMS2);

                            if (lstTemp != null && lstTemp.Count != 0)
                                lstMatchedData.AddRange(lstTemp);


                            return lstMatchedData;
                        }

                        if (scanMS.IsPeaks == true)
                        {
                            if (scanMS2.Index >= 1&& scanMS2.Index <= 99999)
                            {
                                lstTemp = peModResBLL.getMSMatchs(scanMS, scanMS2);

                                if (lstTemp != null && lstTemp.Count != 0)
                                    lstMatchedData.AddRange(lstTemp);

                                if (allCnt <= 0)
                                {
                                }
                                else if (allCnt >= 90)
                                {
                                    currentCnt++;
                                }
                                else
                                {
                                    currentCnt++;
                                   
                                }
                            }
                        }
                    }
                    else
                    {
                        continue;
                    }
                }
            }

            return lstMatchedData;
        }

        private ParameterModel SaveControlToModel()
        {
            ParameterModel model = new ParameterModel();

            model.GetType().GetProperty("MS_PAT").SetValue(model, 0.5, null);
            model.GetType().GetProperty("MS_IPMD").SetValue(model, 10.0, null);
            model.GetType().GetProperty("MS_IPACO").SetValue(model, 5, null);
            model.GetType().GetProperty("MS_IPMDO").SetValue(model, 20, null);
            model.GetType().GetProperty("MS_IPMDOM").SetValue(model, 30.0, null);
            model.GetType().GetProperty("MS_IPAD").SetValue(model, 50, null);
            model.GetType().GetProperty("MS_IPADO").SetValue(model, 20, null);
            model.GetType().GetProperty("MS_IPADOM").SetValue(model, 100, null);
            model.GetType().GetProperty("MS_WINDOW").SetValue(model, 10, null);

            model.GetType().GetProperty("MS2_FAT").SetValue(model, 0.5, null);
            model.GetType().GetProperty("MS2_IPMD").SetValue(model, 10.0, null);
            model.GetType().GetProperty("MS2_IPACO").SetValue(model, 5, null);
            model.GetType().GetProperty("MS2_IPMDO").SetValue(model, 20, null);
            model.GetType().GetProperty("MS2_IPMDOM").SetValue(model, 30.0, null);
            model.GetType().GetProperty("MS2_IPAD").SetValue(model, 50, null);
            model.GetType().GetProperty("MS2_IPADO").SetValue(model, 20, null);
            model.GetType().GetProperty("MS2_IPADOM").SetValue(model, 100, null);
            model.GetType().GetProperty("MS2_PMFS").SetValue(model, 5.0, null);
            model.GetType().GetProperty("MS2_PTMS").SetValue(model, 1, null);
            

            model.MS2_SA = 1;

            return model;
        }

    }
}
