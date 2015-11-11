using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using ProteinGoggle.Model;

namespace UI
{
    public partial class ProteinGoogleChartPicture : Form
    {
        public string gsPath = string.Empty;
        private Size size = new Size(898, 345);
        private FileProcess fileProcess = new FileProcess();

        public List<MatchedData> MatchDataArray { get; set; }

        public ProteinGoogleChartPicture()
        {
            InitializeComponent();
        }

        private void PictureOutput(bool bkbn)
        {
            MatchedData matchData;
            string sMSPath = string.Empty;
            string sMSFileName = string.Empty;
            string sMS2Path = string.Empty;
            string sMS2FileName = string.Empty;
            string sTitle = string.Empty;

            int iCnt = 0;

            List<string> lstDirectories = new List<string>();

            for (int i = 0; i < MatchDataArray.Count; i++)
            {
                matchData = MatchDataArray[i];

                //MS
                iCnt = 1;
                sMSPath = string.Format(@"{0}\MS-{1}-{2}(Z={3})", gsPath, matchData.Index, matchData.ExpMZ, matchData.Z.ToString());

                if (!lstDirectories.Contains(sMSPath))
                {
                    string sPatten = string.Format(@"*MS-{0}-{1}(Z={2})*", matchData.Index, matchData.ExpMZ, matchData.Z.ToString());
                    string[] sDirectories = System.IO.Directory.GetDirectories(gsPath, sPatten);
                    sDirectories.ToList().ForEach(ms => System.IO.Directory.Delete(ms,true));

                    lstDirectories.Add(sMSPath);
                }

                while (true)
                {
                    if (System.IO.Directory.Exists(sMSPath) == false)
                        break;

                    sMSPath = string.Format(@"{0}\MS-{1}-{2}(Z={3})_{4}", gsPath, matchData.Index, matchData.ExpMZ, matchData.Z.ToString(), iCnt.ToString());
                    iCnt++;
                }

                fileProcess.CreateWithIsExistPath(sMSPath);
                sMSFileName = string.Format(@"{0}\MS-{1}-{2}(Z={3}).jpg",
                    sMSPath, matchData.Index, matchData.ExpMZ, matchData.Z.ToString());

                SetMSChart(matchData, sMSFileName);

                //MS2
                sMS2Path = string.Format(@"{0}\MS2", sMSPath);
                fileProcess.CreateWithIsExistPath(sMS2Path);

                for (int j = 0; j < matchData.MatchedMs2.Count; j++)
                {
                    sTitle = string.Format("{0}   z={1}", matchData.MatchedMs2[j].ID.ToLower(), matchData.MatchedMs2[j].Z.ToString());
                    sMS2FileName = string.Format(@"{0}\MS2-{1}-{2}-({3}+).jpg", sMS2Path, matchData.Index, matchData.MatchedMs2[j].ID, matchData.MatchedMs2[j].Z.ToString());

                    SetMS2Chart((List<MassPoint>)matchData.MatchedMs2[j].Mass_Point,
                                (List<MassPoint>)matchData.MatchedMs2[j].Mass_Point_FromFile,
                                sMS2FileName, sTitle);
                }
            }
        }

        public void PictureOutput()
        {
            MatchedData matchData;
            string sMSPath = string.Empty;
            string sMSFileName = string.Empty;
            string sMS2Path = string.Empty;
            string sMS2FileName = string.Empty;
            string sTitle = string.Empty;

            int iCnt = 0;

            for (int i = 0; i < MatchDataArray.Count; i++)
            {
                matchData = MatchDataArray[i];

                //MS
                iCnt = 1;
                sMSPath = string.Format(@"{0}\MS-{1}-{2}(Z={3})", gsPath, matchData.Index, matchData.ExpMZ, matchData.Z.ToString());

                while (true)
                {
                    if (System.IO.Directory.Exists(sMSPath) == false)
                        break;

                    sMSPath = string.Format(@"{0}\MS-{1}-{2}(Z={3})_{4}", gsPath, matchData.Index, matchData.ExpMZ, matchData.Z.ToString(), iCnt.ToString());
                    iCnt++;
                }

                fileProcess.CreateWithIsExistPath(sMSPath);
                sMSFileName = string.Format(@"{0}\MS-{1}-{2}(Z={3}).jpg",
                    sMSPath, matchData.Index, matchData.ExpMZ, matchData.Z.ToString());

                SetMSChart(matchData, sMSFileName);

                //MS2
                sMS2Path = string.Format(@"{0}\MS2", sMSPath);
                fileProcess.CreateWithIsExistPath(sMS2Path);

                for (int j = 0; j < matchData.MatchedMs2.Count; j++)
                {
                    sTitle = string.Format("{0}   z={1}", matchData.MatchedMs2[j].ID.ToLower(), matchData.MatchedMs2[j].Z.ToString());
                    sMS2FileName = string.Format(@"{0}\MS2-{1}-{2}-({3}+).jpg", sMS2Path, matchData.Index, matchData.MatchedMs2[j].ID, matchData.MatchedMs2[j].Z.ToString());

                    SetMS2Chart((List<MassPoint>)matchData.MatchedMs2[j].Mass_Point,
                                (List<MassPoint>)matchData.MatchedMs2[j].Mass_Point_FromFile,
                                sMS2FileName, sTitle);
                }
            }
        }

        private void SetMSChart(MatchedData MatchData,string sFileName)
       {
           double[] expX = MatchData.MatchedMsDataExp.Select(md => md.Mass).ToArray<double>();
           double[] expY = MatchData.MatchedMsDataExp.Select(md => md.IntensityPercentage * 100).ToArray<double>();

           double[] theoX = MatchData.MatchedMsDataTheo.Select(md => md.Mass).ToArray<double>();
           double[] theoY = MatchData.MatchedMsDataTheo.Select(md => md.Intensity * 100).ToArray<double>();

           double[] iPAD = MatchData.MatchedMsDataExp.Select(md => md.IPAD_R).ToArray<double>();
           double[] iPMD = MatchData.MatchedMsDataExp.Select(md => md.IPMD_R).ToArray<double>();

           double minexpX = 0d;
           double maxexpX = 0d;
           double mintheoX = 0d;
           double maxtheoX = 0d;

           if (MatchData.MatchedMsDataExp.Count != 0)
           {
               minexpX = MatchData.MatchedMsDataExp.Min(md => SetMinValue(md.Mass));
               maxexpX = MatchData.MatchedMsDataExp.Max(md => SetMaxValue(md.Mass));
           }

           if (MatchData.MatchedMsDataTheo.Count != 0)
           {
               mintheoX = MatchData.MatchedMsDataTheo.Min(md => SetMinValue(md.Mass));
               maxtheoX = MatchData.MatchedMsDataTheo.Max(md => SetMaxValue(md.Mass));
           }

           if (minexpX == 0)
               minexpX = mintheoX;

           ChartCommon chart = new ChartCommon();

           if (iPAD.Length != 0)
           {
               string chartAreasTopName = "chartAreasTop";
               chart.CreateChartAreas(chartAreasTopName);
               chart.SetChartAreasSetting(chartAreasTopName);
               chart.SetChartAreasX(chartAreasTopName, Math.Min(minexpX, mintheoX), Math.Max(maxexpX, maxtheoX));
               chart.SetChartAreasYInterval(chartAreasTopName, 100);
               chart.SetChartAreasXInterval(chartAreasTopName, 0.5);
               chart.ChartE.ChartAreas[chartAreasTopName].Position.Height = 20;
               chart.ChartE.ChartAreas[chartAreasTopName].Position.Width = 100;
               chart.ChartE.ChartAreas[chartAreasTopName].Position.X = 0;
               chart.ChartE.ChartAreas[chartAreasTopName].Position.Y = 0;
               chart.ChartE.ChartAreas[chartAreasTopName].AxisX.MajorGrid.Enabled = true;
               chart.ChartE.ChartAreas[chartAreasTopName].AxisX.MajorGrid.LineColor = Color.Transparent;
               chart.ChartE.ChartAreas[chartAreasTopName].AxisX.MajorTickMark.Enabled = true;
               chart.ChartE.ChartAreas[chartAreasTopName].AxisX.MajorTickMark.LineColor = Color.Transparent;
               chart.ChartE.ChartAreas[chartAreasTopName].AxisX.LabelStyle.Enabled = true;
               chart.ChartE.ChartAreas[chartAreasTopName].AxisX.LineColor = Color.Transparent;
               chart.ChartE.ChartAreas[chartAreasTopName].AxisX.LabelStyle.ForeColor = Color.Transparent;
               chart.ChartE.ChartAreas[chartAreasTopName].AxisY.MajorGrid.Enabled = true;
               chart.ChartE.ChartAreas[chartAreasTopName].AxisY.MajorGrid.LineColor = Color.Transparent;
               chart.ChartE.ChartAreas[chartAreasTopName].AxisY.MajorTickMark.Enabled = true;
               chart.ChartE.ChartAreas[chartAreasTopName].AxisY.MajorTickMark.LineColor = Color.Transparent;
               chart.ChartE.ChartAreas[chartAreasTopName].AxisY.LabelStyle.Enabled = true;
               chart.ChartE.ChartAreas[chartAreasTopName].AxisY.LineColor = Color.Transparent;
               chart.ChartE.ChartAreas[chartAreasTopName].AxisY.LabelStyle.ForeColor = Color.Transparent;
               chart.SetChartAreasYTitle(chartAreasTopName, "Relative Abundance (%)");
               chart.ChartE.ChartAreas[chartAreasTopName].AxisY.TitleForeColor = Color.Transparent;

               string seriesNamePMD_PAD = "iPMD_iPAD";
               chart.CreateSeries(seriesNamePMD_PAD, chartAreasTopName, SeriesChartType.Spline, "", Color.Red);
               chart.ChartE.Series[seriesNamePMD_PAD].IsVisibleInLegend = false;
               chart.ChartE.Series[seriesNamePMD_PAD].Color = Color.Transparent;

               if (theoX.Length > 1)
               {
                   chart.ChartE.Series[seriesNamePMD_PAD].Points.AddXY(theoX[0] - (theoX[1] - theoX[0]), 0);
                   chart.ChartE.Series[seriesNamePMD_PAD].Points[0].Label = "IPAD" + "\n" + "IPMD";
               }
               else
               {
                   chart.ChartE.Series[seriesNamePMD_PAD].Points.AddXY(theoX[0] - 5, 0);
                   chart.ChartE.Series[seriesNamePMD_PAD].Points[0].Label = "IPAD" + "\n" + "IPMD";
               }

               for (int x = 0; x < theoX.Length; x++)
               {
                   chart.ChartE.Series[seriesNamePMD_PAD].Points.AddXY(theoX[x], 0);

                   if (iPMD[x] > -10000)
                       chart.ChartE.Series[seriesNamePMD_PAD].Points[x + 1].Label = ((int)iPAD[x]).ToString() + "\n" + ((int)iPMD[x]).ToString();
                   else
                       chart.ChartE.Series[seriesNamePMD_PAD].Points[x + 1].Label = "".ToString() + "\n" + "".ToString();
               }
           }

           string chartAreasName = "chartAreas";
           chart.CreateChartAreas(chartAreasName);
           chart.SetChartAreasSetting(chartAreasName);

           if (MatchData.MatchedMsDataExp.Count != 0)
               chart.SetChartAreasX(chartAreasName, Math.Min(minexpX, mintheoX), Math.Max(maxexpX, maxtheoX));
           else
               chart.SetChartAreasX(chartAreasName, mintheoX, maxtheoX);

           chart.SetChartAreasY(chartAreasName, 0, 100);
           chart.SetChartAreasSetting(chartAreasName);
           chart.SetChartAreasXInterval(chartAreasName, 0.5);
           chart.SetChartAreasYInterval(chartAreasName, 20);
           chart.SetChartAreasXTitle(chartAreasName, "m/z");
           chart.SetChartAreasYTitle(chartAreasName, "Relative Abundance (%)");
           chart.SetChartAreasXMajorTickMark(chartAreasName, true);
           chart.SetChartAreasYMajorTickMark(chartAreasName, true);

           chart.ChartE.ChartAreas[chartAreasName].Position.Height = 80;
           chart.ChartE.ChartAreas[chartAreasName].Position.Width = 100;
           chart.ChartE.ChartAreas[chartAreasName].Position.Y = 20;

           string legendName = "legend";
           chart.CreateLegend(legendName);

           if (MatchData.MatchedMsDataExp.Count != 0)
           {
               string seriesNameExp = "exp";
               chart.CreateSeries(seriesNameExp, chartAreasName, SeriesChartType.Candlestick, legendName, Color.Black);
               chart.SetSeriesAddPoint(seriesNameExp, expX, expY);
               chart.SetSeriesAddLegendText(seriesNameExp, "Experimental");
           }

           if (MatchData.MatchedMsDataTheo.Count != 0)
           {
               string seriesNameTheo = "theo";
               chart.CreateSeries(seriesNameTheo, chartAreasName, SeriesChartType.Point, legendName, Color.Red);
               chart.SetSeriesAddPoint(seriesNameTheo, theoX, theoY);
               chart.SetSeriesAddLegendText(seriesNameTheo, "Theoretical");
           }

           chart.ChartE.Size = size;
           chart.ChartE.SaveImage(sFileName, ChartImageFormat.Jpeg);
       }

    }
}
