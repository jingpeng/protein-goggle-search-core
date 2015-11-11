using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Linq;
using System.Text;
using System.Data;

namespace ProteinGoggle.Model
{
    public class PeptideModResBLL
    {
        private ProteinSession session;
        private PeptideModResDAL peptideModResDAL;
        private ProteinParameterRL testParameter = new ProteinParameterRL();
        private List<ModResModel> listModRes = new List<ModResModel>();
        private Analysis analysis = new Analysis();


        public ParameterModel ParameterModel { get; set; }

        public PeptideModResBLL(string sDataBaseName)
        {
            peptideModResDAL = new PeptideModResDAL(sDataBaseName);
        }

        public PeptideModResBLL(ProteinSession pSession)
        {
            session = pSession;
            peptideModResDAL = new PeptideModResDAL(session.DataBase);
        }


        public DataTable Select_GetModResLikeModRes(string seq,string modRes,string order = "asc")
        {
            DataSet ds = peptideModResDAL.Select_GetModResLikeModRes(seq,modRes, order);

            if (ds == null || ds.Tables.Count == 0)
                return null;

            if (ds.Tables[0].Rows.Count == 0)
                return null;

            if (ds.Tables[0].Rows.Count == 1)
                return ds.Tables[0];

            string sKey = modRes.Split(',')[0].Substring(0, 1);
            string sValue = modRes.Split(',')[0].Substring(1);
            string sModRes = modRes.Split(',')[1];

            if (order == "asc")
            {
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    if (int.Parse(ds.Tables[0].Rows[i]["IDX"].ToString()) < int.Parse(sValue))
                        return null;
                }
            }
            else
            {
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    if (int.Parse(ds.Tables[0].Rows[i]["IDX"].ToString()) > int.Parse(sValue))
                        return null;
                }
            }

            var table = ds.Tables[0].AsEnumerable().Take(2);
            return table.CopyToDataTable();
        }

        public DataTable Select_GetModResLikeModRes(string id,string seq)
        {
            DataSet ds = peptideModResDAL.Select_GetModResLikeModRes(id, seq);

            if (ds == null || ds.Tables.Count == 0)
                return null;

            if (ds.Tables[0].Rows.Count == 0)
                return null;

            return ds.Tables[0];
        }

        public List<MatchedData> getMSMatchs(MassScan msScan, MassScan msScan2)
        {
            List<MatchedData> matchedDataList = new List<MatchedData>();

            testParameter.IPMD = ParameterModel.MS_IPMD;
            testParameter.IPACO = ParameterModel.MS_IPACO;
            testParameter.IPAD = ParameterModel.MS_IPAD;
            testParameter.IPADOM = ParameterModel.MS_IPADOM;
            testParameter.IPMDOM = ParameterModel.MS_IPMDOM;
            testParameter.IPADO = ParameterModel.MS_IPADO;
            testParameter.IPMDO = ParameterModel.MS_IPMDO;

            if (testParameter.IPMD > testParameter.IPMDOM)
                testParameter.IPMDOM = testParameter.IPMD;

            if (testParameter.IPAD > testParameter.IPADOM)
                testParameter.IPADOM = testParameter.IPAD;

            testParameter.Window = Convert.ToInt16(ParameterModel.MS_WINDOW / 2);
            testParameter.PAT = ParameterModel.MS_PAT;


            double ms2Point = msScan2.PrecursorMz;
            double maxInten = msScan.BasePeakIntensity;
            MatchedData matchedData = new MatchedData();
            //Console.WriteLine("scanMS.Index Begin2          " + DateTime.Now);

            //Console.WriteLine("scanMS.Index Begin3          " + DateTime.Now);
            List<MassPoint> msFromFile;
            List<MassPoint> msFromDB = null;
            //从文件取得MS谱图
            msFromFile = Base64Convert.CalcMz(msScan.Peaks, msScan.PeaksCount);
            Console.WriteLine("scanMS.Index Begin4          " + DateTime.Now);
            //根据MS2开窗
            List<MassPoint> msFromFileOpenWindow = getOpenWindowFromFile(msFromFile, testParameter.Window, ms2Point);
            //List<MassPoint>  msFromFileOpenWindow = msFromFile;
            Console.WriteLine("scanMS.Index Begin5          " + DateTime.Now);
            //过滤掉5%以下的峰
            List<MassPoint> msFromFileReal = getRealMS2FromFile(msFromFileOpenWindow, testParameter.PAT, maxInten);
            // Console.WriteLine("scanMS.Index Begin6          " + DateTime.Now);
            //List<MassPoint> msFromFileReal = msFromFileOpenWindow;
            int timesFlag = 0;
            int maxMZIndex = 0;
            while (timesFlag < 1000)
            {
                //取得最高点1
                maxMZIndex = getMaxMZ(msFromFileReal);
                //Console.WriteLine("maxMZIndex.ToString()maxMZIndex.ToString()maxMZIndex.ToString()maxMZIndex.ToString()                     " + maxMZIndex.ToString());
                if (maxMZIndex == -1)
                {
                    break;
                }
                else
                {
                    timesFlag = timesFlag + 1;
                }

                //计算偏差
                double deviationInten = msFromFileReal[maxMZIndex].Mass - msFromFileReal[maxMZIndex].Mass / (testParameter.IPMD / 1000000.0 + 1.0);

                //MS最高点与DB匹配 包括三点处理
                //Console.WriteLine("scanMS.Index Begin7          " + DateTime.Now);
                List<ModResModel> modResFromDB = peptideModResDAL.getPmFromDB(msFromFileReal[maxMZIndex].Mass, deviationInten, testParameter.IPACO);
                //Console.WriteLine("scanMS.Index Begin8          " + DateTime.Now);
                msFromFileReal[maxMZIndex].IsSearch = false;

                if (modResFromDB.Count == 0)
                {
                    //没有匹配上
                    continue;

                }
                else
                {


                    //返回匹配结果（最高峰横坐标匹配上的列表）
                    bool compareFlag = false;
                    string Forumla = "";
                    List<MassPoint> msReturn = new List<MassPoint>(); ;
                    for (int k = 0; k < modResFromDB.Count; k++)
                    {


                        msFromDB = modResFromDB[k].Mass_Point;

                        if (Forumla == modResFromDB[k].Forumla && compareFlag == true)
                        {
                            matchedData = new MatchedData();
                            //matchedData.MatchedMsDataExp = msReturn.ToList();
                            matchedData.MatchedMsDataExp = new List<MassPoint>();
                            foreach (MassPoint item in msReturn)
                            {
                                MassPoint cloneMasspoint = new MassPoint();
                                cloneMasspoint.Index = item.Index;
                                cloneMasspoint.Intensity = item.Intensity;
                                cloneMasspoint.IntensityPercentage = item.IntensityPercentage;
                                cloneMasspoint.IPAD_R = item.IPAD_R;
                                cloneMasspoint.IPADOM_R = item.IPADOM_R;
                                cloneMasspoint.IPMD_R = item.IPMD_R;
                                cloneMasspoint.IPMDOM_R = item.IPMDOM_R;
                                cloneMasspoint.Mass = item.Mass;
                                cloneMasspoint.MatchIndex = item.MatchIndex;

                                matchedData.MatchedMsDataExp.Add(cloneMasspoint);
                            }
                            //matchedData.MatchedMsDataExp = msReturn.ToList();
                            matchedData.MatchedMsDataTheo = msFromDB;
                            matchedData.ID = modResFromDB[k].ID;
                            matchedData.MOD_RES = modResFromDB[k].Mod_Res;
                            matchedData.Z = modResFromDB[k].Z;
                            matchedData.Repository = "";
                            matchedData.ExpMZ = (msFromFileReal[maxMZIndex].Mass).ToString();
                            matchedData.TheoMZ = modResFromDB[k].M_Z.ToString();
                            matchedData.Error_PPM = Convert.ToString(((msFromFileReal[maxMZIndex].Mass - modResFromDB[k].M_Z) / modResFromDB[k].M_Z) * 1000000);
                            matchedData.Index = msScan2.Index.ToString();
                            matchedData.ExpMass_DA = Convert.ToString(((msFromFileReal[maxMZIndex].Mass - modResFromDB[k].M_Z) / modResFromDB[k].M_Z) * 1000000);
                            matchedData.NMFS = "";
                            matchedData.Actualpmfs = "";
                            matchedData.Sequence = modResFromDB[k].Sequence;
                            matchedDataList.Add(matchedData);
                            continue;
                        }
                        Forumla = modResFromDB[k].Forumla;
                        // MZ_FromDB = modResFromDB[0].M;
                        compareFlag = false;
                        int maxMZindex_DB = Convert.ToInt16(modResFromDB[k].M);
                        //逐一匹配谱图

                        msReturn = comparePMAll(msFromFileReal, maxMZIndex, maxMZindex_DB, msFromDB);                        //验证匹配率若匹配
                        if (getComparePercentage(msReturn, maxMZindex_DB, msFromDB))
                        {
                            compareFlag = true;
                            matchedData = new MatchedData();
                            //matchedData.MatchedMsDataExp = new List<MassPoint>();
                            matchedData.MatchedMsDataExp = new List<MassPoint>();
                            foreach (MassPoint item in msReturn)
                            {
                                MassPoint cloneMasspoint = new MassPoint();
                                cloneMasspoint.Index = item.Index;
                                cloneMasspoint.Intensity = item.Intensity;
                                cloneMasspoint.IntensityPercentage = item.IntensityPercentage;
                                cloneMasspoint.IPAD_R = item.IPAD_R;
                                cloneMasspoint.IPADOM_R = item.IPADOM_R;
                                cloneMasspoint.IPMD_R = item.IPMD_R;
                                cloneMasspoint.IPMDOM_R = item.IPMDOM_R;
                                cloneMasspoint.Mass = item.Mass;
                                cloneMasspoint.MatchIndex = item.MatchIndex;
                                matchedData.MatchedMsDataExp.Add(cloneMasspoint);
                            }
                            matchedData.MatchedMsDataTheo = msFromDB;
                            matchedData.ID = modResFromDB[k].ID;
                            matchedData.MOD_RES = modResFromDB[k].Mod_Res;
                            matchedData.Z = modResFromDB[k].Z;
                            matchedData.Index = msScan2.Index.ToString();
                            matchedData.Repository = "";
                            matchedData.ExpMZ = (msFromFileReal[maxMZIndex].Mass).ToString();
                            matchedData.TheoMZ = modResFromDB[k].M_Z.ToString();
                            matchedData.Error_PPM = Convert.ToString(((msFromFileReal[maxMZIndex].Mass - modResFromDB[k].M_Z) / modResFromDB[k].M_Z) * 1000000);

                            matchedData.ExpMass_DA = Convert.ToString(((msFromFileReal[maxMZIndex].Mass - modResFromDB[k].M_Z) / modResFromDB[k].M_Z) * 1000000);
                            matchedData.NMFS = "";
                            matchedData.Actualpmfs = "";
                            matchedData.Sequence = modResFromDB[k].Sequence;
                            matchedDataList.Add(matchedData);
                            //break;
                            for (int i = 0; i < msFromFileReal.Count; i++)
                            {

                                double baseIntens = 0.0;
                                for (int j = 0; j < msReturn.Count; j++)
                                {
                                    baseIntens = msFromFileReal[j].Intensity / msReturn[j].IntensityPercentage;
                                    msFromFileReal[msReturn[j].MatchIndex].Intensity =
                                            msFromFileReal[msReturn[j].MatchIndex].Intensity - (baseIntens * msFromDB[j].Intensity);
                                }
                            }

                        }
                    }
                    if (matchedData != null)
                        break;
                }
            }

            if (matchedDataList.Count > 0)
            {
                for (int j = 0; j < matchedDataList.Count; j++)
                {
                    matchedData = matchedDataList[j];
                

                    if (ParameterModel.MS2_SA == 1)
                    {
                        getMS2MatchsTopDown(msScan2, matchedData);
                        matchedDataList[j].ScreeningApproach = "TopDown";
                    }
                    else
                    {
                        getMS2MatchsTargeted(msScan2, matchedData);
                        matchedDataList[j].ScreeningApproach = "Targeted";
                    }

                    if (matchedDataList[j].MatchedMs2 == null)
                        matchedDataList[j].NMFS = "0";
                    else
                        matchedDataList[j].NMFS = matchedDataList[j].MatchedMs2.Count().ToString();


                    //Console.WriteLine("matched MS2:\t" + matchedData.MatchedMs2.Count.ToString());
                }
            }

            return matchedDataList;
        }

        public List<MatchedData> getMSMatchs(MassScan msScan2)
        {
            testParameter.IPMD = ParameterModel.MS_IPMD;
            testParameter.IPACO = ParameterModel.MS_IPACO;
            testParameter.IPAD = ParameterModel.MS_IPAD;
            testParameter.IPADOM = ParameterModel.MS_IPADOM;
            testParameter.IPMDOM = ParameterModel.MS_IPMDOM;
            testParameter.IPADO = ParameterModel.MS_IPADO;
            testParameter.IPMDO = ParameterModel.MS_IPMDO;

            if (testParameter.IPMD > testParameter.IPMDOM)
                testParameter.IPMDOM = testParameter.IPMD;

            if (testParameter.IPAD > testParameter.IPADOM)
                testParameter.IPADOM = testParameter.IPAD;

            //testParameter.Window = 5;
            testParameter.PAT = ParameterModel.MS_PAT;
            List<MatchedData> matchedDataList = new List<MatchedData>();
            List<ModResModel> modResFromDB = peptideModResDAL.getPmFromDB(testParameter.IPACO);
            MatchedData matchedData = null;
            List<MassPoint> msFromDB = null;
            for (int k = 0; k < modResFromDB.Count; k++)
            {
                msFromDB = modResFromDB[k].Mass_Point;
                matchedData = new MatchedData();
                //matchedData.MatchedMsDataExp = msReturn.ToList();
                matchedData.MatchedMsDataExp = new List<MassPoint>();
                //matchedData.MatchedMsDataExp = msReturn.ToList();
                matchedData.MatchedMsDataTheo = msFromDB;
                matchedData.ID = modResFromDB[k].ID;
                matchedData.MOD_RES = modResFromDB[k].Mod_Res;
                matchedData.Z = modResFromDB[k].Z;
                matchedData.Z = 15;
                matchedData.Repository = "";
                matchedData.TheoMZ = modResFromDB[k].M_Z.ToString(); matchedData.NMFS = "";
                matchedData.Sequence = modResFromDB[k].Sequence;
                matchedData.Actualpmfs = "";
                //matchedData.Index = 1;
                matchedDataList.Add(matchedData);
            }


            if (matchedDataList.Count > 0)
            {
                for (int j = 0; j < matchedDataList.Count; j++)
                {
                    matchedData = matchedDataList[j];

                    if (ParameterModel.MS2_SA == 1)
                    {
                        getMS2MatchsTopDown(msScan2, matchedData);
                        matchedDataList[j].ScreeningApproach = "TopDown";
                    }
                    else
                    {
                        getMS2MatchsTargeted(msScan2, matchedData);
                        matchedDataList[j].ScreeningApproach = "Targeted";
                    }
                    try
                    {
                        matchedDataList[j].NMFS = matchedDataList[j].MatchedMs2.Count().ToString();
                    }
                    catch
                    {
                        matchedDataList[j].NMFS = "0";
                    }
                    matchedData.Index = "1";
                    //Console.WriteLine("matched MS2:\t" + matchedData.MatchedMs2.Count.ToString());
                }
            }

            return matchedDataList;
        }

        public List<MassPoint> getOpenWindowFromFile(List<MassPoint> msFromFile, int openWindow, double ms2Point)
        {
            int n = 4;

            double windowLeft = ms2Point - openWindow;
            double windowRight = ms2Point + openWindow;
            List<MassPoint> msFromFileOpenWindow = new List<MassPoint>();
            int i = 0;
            for (i = msFromFile.Count - 1; i >= 0; i = i - 50)
            {
                if (msFromFile[i].Mass < windowLeft)
                {
                    for (int j = i; j < msFromFile.Count; j++)
                    {
                        if (msFromFile[j].Mass > windowRight)
                        {
                            break;
                        }
                        if (msFromFile[j].Mass > windowLeft)
                        {
                            msFromFileOpenWindow.Add(msFromFile[j]);
                        }

                    }
                    break;
                }
            }
            if (i < 0)
            {
                for (int j = 0; j < msFromFile.Count; j++)
                {
                    if (msFromFile[j].Mass > windowRight)
                    {
                        break;
                    }
                    if (msFromFile[j].Mass > windowLeft)
                    {
                        msFromFileOpenWindow.Add(msFromFile[j]);
                    }

                }
            }
            return msFromFileOpenWindow;
        }

        //过滤掉PERFILEUSE%以下的试验数据
        public List<MassPoint> getRealMS2FromFile(List<MassPoint> ms2FromFile, double PERFILEUSE, double max)
        {
            List<MassPoint> ms2FromFileReal = new List<MassPoint>();
            //max = 0.0;
            //for (int j = 0; j < ms2FromFile.Count; j++)
            //{
            //    if (max < ms2FromFile[j].Intensity)
            //    {
            //        max = ms2FromFile[j].Intensity;
            //    }

            //}

            for (int j = 0; j < ms2FromFile.Count; j++)
            {
                if (j == 2825)
                {
                    j = 2825;
                }
                if (ms2FromFile[j].Intensity < max * PERFILEUSE / 100)
                {
                    continue;
                }
                ms2FromFileReal.Add(ms2FromFile[j]);
            }
            return ms2FromFileReal;
        }

        public int getMaxMZ(List<MassPoint> msFromFileReal)
        {
            int maxMZIndex = -1;
            double maxMZ = 0;

            for (int i = 0; i < msFromFileReal.Count; i++)
            {
                if (msFromFileReal[i].Intensity > maxMZ && msFromFileReal[i].IsSearch)
                {
                    maxMZ = msFromFileReal[i].Intensity;
                    maxMZIndex = i;
                }
            }
            return maxMZIndex;
        }

        public List<MassPoint> comparePMAll(List<MassPoint> msFromFileReal, int maxMZIndex_File, int maxMZIndex_DB, List<MassPoint> msFromDB)
        {
            //int IPMD = 10;
            List<MassPoint> msReturn = new List<MassPoint>();
            MassPoint loopUsePoint = new MassPoint();
            //int k = 0;
            int maxIntenIndex = 0;
            int loopIndex = 0;
            //double realDeviation = 100.0;

            List<MassPoint> msFromFileRealCopy = new List<MassPoint>(msFromFileReal.ToArray());

            for (int i = 0; i < msFromDB.Count; i++)
            {
                loopUsePoint = new MassPoint();
                int comparedFlag = 0;
                //realDeviation = 100.00;
                for (int j = loopIndex; j < msFromFileRealCopy.Count; j++)
                {
                    if (msFromFileRealCopy[j].ComPared == 1)
                    {
                        continue;
                    }
                    double deviation = System.Math.Abs(msFromFileRealCopy[j].Mass - msFromFileRealCopy[j].Mass * (testParameter.IPMDOM / 1000000.0 + 1.0));

                    //if (System.Math.Abs(msFromFileRealCopy[j].Mass - msFromDB[i].Mass) < deviation && System.Math.Abs(msFromFileRealCopy[j].Mass - msFromDB[i].Mass) < realDeviation)
                    if (msFromFileRealCopy[j].Mass - msFromDB[i].Mass > deviation)
                    {
                        break;
                    }
                    if (System.Math.Abs(msFromFileRealCopy[j].Mass - msFromDB[i].Mass) < deviation)
                    {
                        if (comparedFlag == 0)
                        {
                            comparedFlag = 1;
                            //realDeviation = System.Math.Abs(msFromFileRealCopy[j].Mass - msFromDB[i].Mass);
                            //msReturn.Add(msFromFileReal[j]);
                            //loopUsePoint = msFromFileRealCopy[j];

                            loopUsePoint.Index = msFromFileRealCopy[j].Index;
                            loopUsePoint.Intensity = msFromFileRealCopy[j].Intensity;
                            loopUsePoint.IsSearch = true;
                            loopUsePoint.Mass = msFromFileRealCopy[j].Mass;

                            loopUsePoint.IPMD_R = (msFromFileRealCopy[j].Mass - msFromDB[i].Mass) / msFromDB[i].Mass * 1000000;
                            loopUsePoint.MatchIndex = j;
                            loopIndex = j;
                            //k = k + 1;
                        }
                        else
                        {
                            if (System.Math.Abs(msFromFileRealCopy[j].Mass - msFromDB[i].Mass) < System.Math.Abs(msFromFileRealCopy[j - 1].Mass - msFromDB[i].Mass))
                            {
                                if (msFromFileRealCopy[j].Mass > 903 && msFromFileRealCopy[j].Mass < 904)
                                {
                                    msFromFileRealCopy[j].Mass = msFromFileRealCopy[j].Mass;
                                }
                                //loopUsePoint = msFromFileRealCopy[j];
                                loopUsePoint.Index = msFromFileRealCopy[j].Index;
                                loopUsePoint.Intensity = msFromFileRealCopy[j].Intensity;
                                loopUsePoint.IsSearch = true;
                                loopUsePoint.Mass = msFromFileRealCopy[j].Mass;

                                loopUsePoint.IPMD_R = (msFromFileRealCopy[j].Mass - msFromDB[i].Mass) / msFromDB[i].Mass * 1000000;
                                loopUsePoint.MatchIndex = j;
                                //loopIndex = j;
                            }

                        }
                    }
                }

                if (msFromDB[i].Intensity == 1.000)
                {
                    maxIntenIndex = i;
                }
                //loopUsePoint.ComPared = 1;
                loopUsePoint.IPAD_R = loopUsePoint.Intensity / msFromFileRealCopy[maxMZIndex_File].Intensity;

                msReturn.Add(loopUsePoint);
            }


            return msReturn;
        }

        public void getMS2MatchsTopDown(MassScan msScan2, MatchedData matchedData)
        {

            testParameter.IPMD = ParameterModel.MS2_IPMD;
            testParameter.IPACO = ParameterModel.MS2_IPACO;
            testParameter.IPAD = ParameterModel.MS2_IPAD;
            testParameter.IPADOM = ParameterModel.MS2_IPADOM;
            testParameter.IPMDOM = ParameterModel.MS2_IPMDOM;
            testParameter.IPADO = ParameterModel.MS2_IPADO;
            testParameter.IPMDO = ParameterModel.MS2_IPMDO;
            if (testParameter.IPMD > testParameter.IPMDOM)
                testParameter.IPMDOM = testParameter.IPMD;

            if (testParameter.IPAD > testParameter.IPADOM)
                testParameter.IPADOM = testParameter.IPAD;
            //testParameter.Window = 5;
            testParameter.PAT = ParameterModel.MS2_FAT;
            List<MassPoint> ms2FromFile = null;
            List<MassPoint> msFromDB = null;
            //ModResModel ms2Matched = new ModResModel();
            List<ModResModel> MatchedMs2 = new List<ModResModel>();
            double maxInten = msScan2.BasePeakIntensity;
            //过滤掉5%以下的峰
            ms2FromFile = Base64Convert.CalcMz(msScan2.Peaks, msScan2.PeaksCount);
            List<MassPoint> msFromFileReal = getRealMS2FromFile(ms2FromFile, testParameter.PAT, maxInten);

            List<string[]> fmValue = peptideModResDAL.getFmFromDB(matchedData.ID, matchedData.MOD_RES, msScan2.ScanType, matchedData.Sequence);
            if (fmValue.Count == 0)
            {
                return;
            }
            while (true)
            {
                //取得最高点1
                int maxMZIndex = getMaxMZ(msFromFileReal);
                if (maxMZIndex == -1)
                {
                    break;
                }
                if (maxMZIndex == 157)
                {
                    maxMZIndex = 157;
                }
                //计算偏差
                double deviationInten = msFromFileReal[maxMZIndex].Mass - msFromFileReal[maxMZIndex].Mass / (testParameter.IPMD / 1000000.0 + 1.0);

                //MS最高点与DB匹配 包括三点处理
                List<ModResModel> modResFromDB = getFMPointInfo(msFromFileReal[maxMZIndex].Mass, deviationInten, fmValue, matchedData.Z, testParameter.IPACO);
                msFromFileReal[maxMZIndex].IsSearch = false;
                if (modResFromDB == null)
                {
                    //没有匹配上
                    continue;

                }
                else
                {
                    for (int k = 0; k < modResFromDB.Count; k++)
                    {
                        msFromDB = modResFromDB[k].Mass_Point;
                        int maxMZindex_DB = Convert.ToInt16(modResFromDB[k].M_Z);
                        //逐一匹配谱图
                        List<MassPoint> msReturn = comparePMAll(msFromFileReal, maxMZIndex, maxMZindex_DB, msFromDB);
                        if (modResFromDB[k].ID == "B5")
                        {
                            modResFromDB[k].ID = "B5";
                        }
                        //验证匹配率若匹配
                        if (getComparePercentage(msReturn, maxMZindex_DB, msFromDB))
                        {
                            ModResModel ms2Matched = new ModResModel();
                            double baseIntens = msFromFileReal[maxMZIndex].Intensity;
                            ms2Matched.Mass_Point = new List<MassPoint>(msFromDB.ToArray());
                            ms2Matched.Mass_Point_FromFile = new List<MassPoint>();
                            for (int j = 0; j < msReturn.Count; j++)
                            {
                                MassPoint massPoint = new MassPoint();
                                massPoint.Index = msReturn[j].Index;
                                //massPoint.Intensity = new Double();
                                massPoint.Intensity = msReturn[j].Intensity;
                                massPoint.IPAD_R = msReturn[j].IPAD_R;
                                massPoint.IPADOM_R = msReturn[j].IPADOM_R;
                                massPoint.IPMD_R = msReturn[j].IPMD_R;
                                massPoint.IPMDOM_R = msReturn[j].IPMDOM_R;
                                massPoint.Mass = msReturn[j].Mass;
                                massPoint.Index = msReturn[j].Index;
                                massPoint.IntensityPercentage = msReturn[j].IntensityPercentage;
                                ms2Matched.Mass_Point_FromFile.Add(massPoint);
                            }
                            ms2Matched.Z = modResFromDB[k].Z;
                            ms2Matched.ID = modResFromDB[k].ID;

                            MatchedMs2.Add(ms2Matched);
                            //matchedData.MatchedMs2DataExp = ms2Matched;

                            for (int j = 0; j < msReturn.Count; j++)
                            {
                                baseIntens = msFromFileReal[j].Intensity / msReturn[j].IntensityPercentage;
                                msFromFileReal[msReturn[j].MatchIndex].Intensity =
                                        msFromFileReal[msReturn[j].MatchIndex].Intensity - (baseIntens * msFromDB[j].Intensity);
                            }
                        }
                    }
                }
            }
            matchedData.MatchedMs2 = MatchedMs2;
        }

        public void getMS2MatchsTargeted(MassScan msScan2, MatchedData matchedData)
        {
            //testParameter.IPMD = 10;
            //testParameter.IPACO = 5;
            //testParameter.IPAD = 50;
            //testParameter.IPADOM = 100;
            //testParameter.IPMDOM = 30;
            //testParameter.IPADO = 20;
            //testParameter.IPMDO = 20;
            //testParameter.Window = 5;
            //testParameter.PAT = 0.2;

            testParameter.IPMD = ParameterModel.MS2_IPMD;
            testParameter.IPACO = ParameterModel.MS2_IPACO;
            testParameter.IPAD = ParameterModel.MS2_IPAD;
            testParameter.IPADOM = ParameterModel.MS2_IPADOM;
            testParameter.IPMDOM = ParameterModel.MS2_IPMDOM;
            testParameter.IPADO = ParameterModel.MS2_IPADO;
            testParameter.IPMDO = ParameterModel.MS2_IPMDO;
            if (testParameter.IPMD > testParameter.IPMDOM)
                testParameter.IPMDOM = testParameter.IPMD;

            if (testParameter.IPAD > testParameter.IPADOM)
                testParameter.IPADOM = testParameter.IPAD;

            //testParameter.Window = 5;
            testParameter.PAT = ParameterModel.MS2_FAT;

            //if (msScan2.ScanType == "CID")
            //{
            //    return;
            //}
            List<MassPoint> ms2FromFile = null;
            List<MassPoint> msFromDB = null;
            //ModResModel ms2Matched = new ModResModel();
            List<ModResModel> MatchedMs2 = new List<ModResModel>();
            double maxInten = msScan2.BasePeakIntensity;
            //过滤掉5%以下的峰
            ms2FromFile = Base64Convert.CalcMz(msScan2.Peaks, msScan2.PeaksCount);
            List<MassPoint> msFromFileReal = getRealMS2FromFile(ms2FromFile, testParameter.PAT, maxInten);

            List<string[]> fmValue = peptideModResDAL.getFmFromDB(matchedData.ID, matchedData.MOD_RES, msScan2.ScanType, matchedData.Sequence);
            if (fmValue.Count == 0)
            {
                return;
            }
            //msFromDB = getFMDataPoints(fmValue);
            for (int i = 0; i < fmValue[0].Count(); i++)
            {
                //逐一匹配谱图
                List<ModResModel> modResFromDB = comparePMFromDB(fmValue[0][i], fmValue[1][i], fmValue[2][i], msFromFileReal, matchedData.Z, testParameter.IPACO);
                if (modResFromDB.Count > 1)
                {
                    //modResFromDB = null;
                }
                for (int m = 0; m < modResFromDB.Count; m++)
                {
                    if (modResFromDB[m].ID == "Y40")
                    {
                        modResFromDB[m].ID = "Y40";
                    }
                    msFromDB = modResFromDB[m].Mass_Point;
                    int maxMZIndex = Convert.ToInt16(modResFromDB[m].M);//临时使用
                    int maxMZindex_DB = Convert.ToInt16(modResFromDB[m].M_Z);
                    //逐一匹配谱图
                    List<MassPoint> msReturn = comparePMAll(msFromFileReal, maxMZIndex, maxMZindex_DB, msFromDB);
                    //验证匹配率若匹配
                    if (getComparePercentage(msReturn, maxMZindex_DB, msFromDB))
                    {
                        ModResModel ms2Matched = new ModResModel();
                        double baseIntens = msFromFileReal[maxMZIndex].Intensity;
                        ms2Matched.Mass_Point = new List<MassPoint>(msFromDB.ToArray());
                        ms2Matched.Mass_Point_FromFile = new List<MassPoint>();
                        for (int j = 0; j < msReturn.Count; j++)
                        {
                            MassPoint massPoint = new MassPoint();
                            massPoint.Index = msReturn[j].Index;
                            //massPoint.Intensity = new Double();
                            massPoint.Intensity = msReturn[j].Intensity;
                            massPoint.IPAD_R = msReturn[j].IPAD_R;
                            massPoint.IPADOM_R = msReturn[j].IPADOM_R;
                            massPoint.IPMD_R = msReturn[j].IPMD_R;
                            massPoint.IPMDOM_R = msReturn[j].IPMDOM_R;
                            massPoint.Mass = msReturn[j].Mass;
                            massPoint.Index = msReturn[j].Index;
                            massPoint.IntensityPercentage = msReturn[j].IntensityPercentage;
                            ms2Matched.Mass_Point_FromFile.Add(massPoint);
                        }
                        //matchedData.ID = modResFromDB[0].ID;
                        //matchedData.MOD_RES = modResFromDB[0].Mod_Res;
                        //matchedData.Z = modResFromDB[0].Z;
                        ms2Matched.Z = modResFromDB[m].Z;
                        ms2Matched.ID = modResFromDB[m].ID;
                        MatchedMs2.Add(ms2Matched);
                        //matchedData.MatchedMs2DataExp = ms2Matched;

                        for (int j = 0; j < msReturn.Count; j++)
                        {
                            //ms2Matched.Mass_Point_FromFile[j].Intensity = new Double();
                            //ms2Matched.Mass_Point_FromFile[j].Intensity = msFromFileReal[msReturn[j].MatchIndex].Intensity;
                            baseIntens = msFromFileReal[j].Intensity / msReturn[j].IntensityPercentage;
                            msFromFileReal[msReturn[j].MatchIndex].Intensity =
                                    msFromFileReal[msReturn[j].MatchIndex].Intensity - (baseIntens * msFromDB[j].Intensity);
                            //if (maxInten * testParameter.IPACO / 100 > msFromFileReal[msReturn[j].MatchIndex].Intensity)
                            //{
                            //    msFromFileReal[msReturn[j].MatchIndex].Intensity = 0;
                            //}
                        }

                    }
                }
            }

            matchedData.MatchedMs2 = MatchedMs2;
            //for (int i = 0; i < MatchedMs2.Count; i++)
            //{
            //    Console.WriteLine("Ion:  " + MatchedMs2[i].ID + "\t" + "Z:  " + MatchedMs2[i].Z);
            //    for (int j = 0; j < MatchedMs2[i].Mass_Point.Count; j++)
            //    {
            //        Console.WriteLine("Mass: \t " + MatchedMs2[i].Mass_Point[j].Mass + "\t" + MatchedMs2[i].Mass_Point_FromFile[j].Mass + "\tIntensity: \t" + MatchedMs2[i].Mass_Point[j].Intensity + "\t" + MatchedMs2[i].Mass_Point_FromFile[j].IntensityPercentage + "\t" + MatchedMs2[i].Mass_Point_FromFile[j].IPAD_R);
            //    }

            //}

        }

        private List<ModResModel> getFMPointInfo(double mzs, double deviationInten, List<string[]> fmValue, int Z, double fat)
        {
            string[] useStr;
            string[] maxMZ = fmValue[2];
            string[] ETD_MZ = fmValue[0];//.Split(new char[] { ';', '=', ',' });
            string[] ETD_M = fmValue[1];//.Split(new char[] { ';', '=', ',' });
            string[] ETD_FM_MZ = null;
            string[] ETD_FM_M = null;
            double MZ_N = 0.0;
            double deviationIntenNow = deviationInten;
            List<ModResModel> returnValue = new List<ModResModel>();
            ModResModel modResModel = new ModResModel();
            List<MassPoint> Mass_PointList = new List<MassPoint>();
            int flag = -1;
            List<int> matchedList = new List<int>();
            for (int i = 0; i < maxMZ.Count(); i++)
            {
                //MZ_N = mzs * j - (j - 1) * 1.00782;
                useStr = maxMZ[i].Split('=');

                for (int j = 1; j < Z; j++)
                {
                    MZ_N = (Convert.ToDouble(useStr[1]) + (j - 1) * 1.00782) / j;
                    if (System.Math.Abs(MZ_N - mzs) < deviationIntenNow)
                    {
                        deviationIntenNow = System.Math.Abs(MZ_N - mzs);
                        modResModel = new ModResModel();
                        modResModel.Z = j;
                        flag = i;
                        modResModel.M_Z = MZ_N;
                        matchedList.Add(i);
                        returnValue.Add(modResModel);
                        //i = maxMZ.Count();
                        // break;
                    }
                }

            }

            //ETD_FM_MZ = ETD_MZ[i].Split(new char[] { ';', '=', ',' });
            //ETD_FM_M = ETD_M[i].Split(new char[]{';','=',','});
            if (flag < 0)
            {
                return null;
            }
            for (int k = 0; k < matchedList.Count(); k++)
            {
                ETD_FM_MZ = ETD_MZ[matchedList[k]].Split(new char[] { ';', '=', ',' });
                ETD_FM_M = ETD_M[matchedList[k]].Split(new char[] { ';', '=', ',' });
                returnValue[k].ID = ETD_FM_MZ[0];
                modResModel.ID = ETD_FM_MZ[0];
                Mass_PointList = new List<MassPoint>();
                for (int i = 2; i < ETD_FM_MZ.Count(); i++)
                {
                    if (Convert.ToDouble(ETD_FM_M[i]) * 100 > fat)
                    {
                        MassPoint Mass_Point = new MassPoint();
                        MZ_N = (Convert.ToDouble(Convert.ToDouble(ETD_FM_MZ[i])) + (returnValue[k].Z - 1) * 1.00782) / returnValue[k].Z;
                        Mass_Point.Mass = MZ_N;
                        Mass_Point.Intensity = Convert.ToDouble(ETD_FM_M[i]);

                        if (Mass_Point.Intensity == 1.00)
                        {
                            returnValue[k].M_Z = Mass_PointList.Count;
                        }
                        Mass_PointList.Add(Mass_Point);


                    }
                }
                returnValue[k].Mass_Point = Mass_PointList;
            }
            /*
                if (flag >= 0)
                {
                    ETD_FM_MZ = ETD_MZ[flag].Split(new char[] { ';', '=', ',' });
                    ETD_FM_M = ETD_M[flag].Split(new char[] { ';', '=', ',' });
                    modResModel.ID = ETD_FM_MZ[0];

                    for (int i = 2; i < ETD_FM_MZ.Count(); i++)
                    {
                        if (Convert.ToDouble(ETD_FM_M[i]) * 100 > fat)
                        {
                            MassPoint Mass_Point = new MassPoint();
                            MZ_N = (Convert.ToDouble(Convert.ToDouble(ETD_FM_MZ[i])) + (modResModel.Z - 1) * 1.00782) / modResModel.Z;
                            Mass_Point.Mass = MZ_N;
                            Mass_Point.Intensity = Convert.ToDouble(ETD_FM_M[i]);

                            if (Mass_Point.Intensity == 1.00)
                            {
                                modResModel.M_Z = Mass_PointList.Count;
                            }
                            Mass_PointList.Add(Mass_Point);

                        }
                    }
                }
            modResModel.Mass_Point = Mass_PointList;
             */
            //returnValue.Add(modResModel);
            return returnValue;
        }

        private List<ModResModel> comparePMFromDB(string pmMZ, string pmM, string pmMAX, List<MassPoint> msFromFile, int Z, double fat)
        {
            string[] pmMS = pmM.Split(new char[] { ';', '=', ',' });
            string[] pmMZS = pmMZ.Split(new char[] { ';', '=', ',' });
            string[] pmMAXS = pmMAX.Split('=');
            int matchedMZ = 0;
            //int matchedIndex = 0;
            double mz_N = 0.0;
            MassPoint massPoint = null;
            List<ModResModel> returnValue = new List<ModResModel>();
            List<MassPoint> returnMassPointList = null;
            for (int i = 1; i < Z; i++)
            {
                mz_N = (Convert.ToDouble(Convert.ToDouble(pmMAXS[1])) + (i - 1) * 1.00782) / i;
                int j = 0;
                if (mz_N < msFromFile[0].Mass || mz_N > msFromFile[msFromFile.Count - 1].Mass)
                {
                    continue;
                }
                int cutFlag = 0;
                for (j = 100; j < msFromFile.Count; j = j + 100)
                {
                    //double deviationInten = msFromFile[j].Mass - msFromFile[j].Mass / (mz_N / 1000000.0 + 1.0);

                    if (mz_N <= msFromFile[j].Mass)
                    {
                        cutFlag = 1;
                        break;

                    }
                }
                if (j <= 100)
                    j = 100;
                if (cutFlag == 0)
                    j = msFromFile.Count;

                int k = 0;

                for (k = j - 100; k < j; k++)
                {
                    double deviationInten = msFromFile[k].Mass - msFromFile[k].Mass / (mz_N / 1000000.0 + 1.0);

                    if ((System.Math.Abs(msFromFile[k].Mass - mz_N)) < deviationInten)
                    {
                        matchedMZ = i;
                        ModResModel modResModel = new ModResModel();
                        returnMassPointList = new List<MassPoint>();
                        //for (int l = 2; l < pmMS.Count(); l++)
                        //{
                        //    massPoint = new MassPoint();
                        //    massPoint.Mass = Convert.ToDouble(pmMZS[l]);
                        //    massPoint.Intensity = Convert.ToDouble(pmMZS[l]);
                        //    returnMassPointList.Add(massPoint);
                        //}

                        modResModel.ID = pmMAXS[0];
                        double MZ_N = 0.0;
                        modResModel.Z = i;
                        for (int m = 2; m < pmMS.Count(); m++)
                        {
                            if (Convert.ToDouble(pmMS[m]) * 100 > fat)
                            {

                                MassPoint Mass_Point = new MassPoint();
                                MZ_N = (Convert.ToDouble(Convert.ToDouble(pmMZS[m])) + (modResModel.Z - 1) * 1.00782) / modResModel.Z;
                                Mass_Point.Mass = MZ_N;
                                Mass_Point.Intensity = Convert.ToDouble(pmMS[m]);

                                if (Mass_Point.Intensity == 1.00)
                                {
                                    modResModel.M_Z = returnMassPointList.Count;
                                }

                                returnMassPointList.Add(Mass_Point);

                            }
                        }
                        modResModel.Mass_Point = returnMassPointList;

                        //modResModel.M_Z = k;
                        modResModel.ID = pmMS[0];
                        modResModel.M = k;
                        returnValue.Add(modResModel);
                        break;
                    }
                }
            }


            return returnValue;
        }

        public bool getComparePercentage(List<MassPoint> msReturn, int pmIndex, List<MassPoint> msFromDB)
        {
            // 1 匹配
            // 2 不匹配
            // 3 最高点峰值不匹配 重新计算比率
            double maxInten = msReturn[pmIndex].Intensity;
            double nowInten = 0;
            int IPMDOCount = 0;
            int IPADOCount = 0;
            //int IPMDOMCount = 0;
            int IPADOMCount = 0;
            for (int i = 0; i < msReturn.Count; i++)
            {
                nowInten = msReturn[i].Intensity / maxInten;
                if (double.IsNaN(msReturn[i].Mass) || msReturn[i].Mass < 0.000001)
                {
                    return false;
                }
                if (double.IsNaN(nowInten))
                {
                    nowInten = -100;
                }
                double deviationInten = (nowInten - msFromDB[i].Intensity) / msFromDB[i].Intensity * 100;
                double deviationMZ = (msReturn[i].Mass - msFromDB[i].Mass) / msFromDB[i].Mass * 1000000.0;

                msReturn[i].IPMD_R = deviationMZ;
                msReturn[i].IPAD_R = deviationInten;
                msReturn[i].IntensityPercentage = nowInten;
                if (System.Math.Abs(deviationInten) == 1000000.0)
                {
                    continue;
                }

                if (System.Math.Abs(deviationMZ) == 1000000.0)
                {
                    IPADOCount = IPADOCount + 1;
                    IPMDOCount = IPMDOCount + 1;
                    continue;
                }

                //横坐标超过最大偏差
                if (System.Math.Abs(deviationMZ) > testParameter.IPMDOM)
                {
                    return false;
                }


                //纵坐标超过最大偏差

                if (System.Math.Abs(deviationInten) > testParameter.IPADOM)
                {
                    //return false;
                    IPADOMCount = IPADOMCount + 1;
                }

                //横坐标超过正常偏差
                if (System.Math.Abs(deviationMZ) > testParameter.IPMD)
                {
                    IPMDOCount = IPMDOCount + 1;
                }
                //纵坐标超过正常偏差
                if (System.Math.Abs(deviationInten) > testParameter.IPAD)
                {
                    IPADOCount = IPADOCount + 1;
                }

            }

            //横坐标超过正常偏差个数超过比例
            //Convert.ToDouble
            if (IPMDOCount == 0)
            {
                IPMDOCount = 0;
            }
            if (IPMDOCount / Convert.ToDouble(msReturn.Count) * 100 > testParameter.IPMDO)
            {
                return false;
            }

            //纵坐标超过正常偏差个数超过比例
            if ((IPADOCount / Convert.ToDouble(msReturn.Count) * 100 > testParameter.IPADO) || IPADOMCount > 0)
            {
                if (msReturn.Count == 1)
                    return false;

                //取得相邻峰为基准重新计算。

                //if (pmIndex != 0)
                //{
                //    maxInten = msReturn[pmIndex - 1].Intensity;
                //}
                //if (msReturn.Count > pmIndex + 1)
                //{
                //    if (msReturn[pmIndex - 1].Intensity < msReturn[pmIndex + 1].Intensity)
                //    {
                //        maxInten = msReturn[pmIndex + 1].Intensity;
                //    }
                //}

                if (pmIndex != 0)
                {
                    maxInten = msReturn[pmIndex - 1].Intensity / msFromDB[pmIndex - 1].Intensity;
                    if (msReturn.Count > pmIndex + 1)
                    {
                        if (msReturn[pmIndex - 1].Intensity < msReturn[pmIndex + 1].Intensity)
                        {
                            maxInten = msReturn[pmIndex + 1].Intensity / msFromDB[pmIndex + 1].Intensity;
                        }
                    }
                }
                else
                {
                    maxInten = msReturn[pmIndex + 1].Intensity / msFromDB[pmIndex + 1].Intensity;
                }

                IPADOCount = 0;
                //IPMDOMCount = 0;
                for (int i = 0; i < msReturn.Count; i++)
                {
                    nowInten = msReturn[i].Intensity / maxInten;
                    if (double.IsNaN(nowInten))
                    {
                        nowInten = -100;
                    }
                    msReturn[i].IntensityPercentage = nowInten;
                    double deviationInten = (nowInten - msFromDB[i].Intensity) / msFromDB[i].Intensity * 100;
                    msReturn[i].IPAD_R = deviationInten;
                    //校正后纵坐标超过最大偏差

                    if (System.Math.Abs(deviationInten) > testParameter.IPADOM)
                    {
                        return false;
                        //IPMDOMCount = IPMDOMCount + 1;
                    }


                    //校正后纵坐标超过正常偏差
                    if (System.Math.Abs(deviationInten) > testParameter.IPAD)
                    {

                        IPADOCount = IPADOCount + 1;

                    }
                }

            }
            //校正后纵坐标超过正常范围个数仍大于阀值
            if (IPADOCount / Convert.ToDouble(msReturn.Count) * 100 > testParameter.IPADO)
            {
                return false;
            }

            return true;


        }

    }
}
