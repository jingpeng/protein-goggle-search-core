private void ConvertMatchData()
       {
           //sMZXMLFileName XML 文件

           FileProcess filePro = new FileProcess();
           XmlReader xmlReader = XmlReader.Create(sMZXMLFileName);
           PeptideBLL peptideBLL = new PeptideBLL(this.cmbDataBase.Text);
           PeptideModResBLL peModResBLL = new PeptideModResBLL(this.cmbDataBase.Text);

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
                       ms.Database = this.cmbDataBase.Text;
                       ms.DatasetFile = this.txtDataFile.Text;
                       ms.Repository = msMetaName;
                       ms.AccessionNumber = sAC;
                       ms.Function = sCC;
                       ms.ID = sID;
                       ms.ProteinName = sDE;
                       ms.Length = ms.Sequence.Length;
                       ms.Actualpmfs = (double.Parse(ms.NMFS) / (2 * (sSQ.Length - 1)) * 100).ToString();
                   });
               }

               lstMatchedData = lstMatchedData.Where(ms => string.IsNullOrEmpty(ms.Actualpmfs) == false
                                                        && decimal.Parse(ms.Actualpmfs) >= this.num_MS2_PMFS.Value).ToList();

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
                       if (decimal.Parse(temp[i].Split(',')[1]) < this.num_MS2_PTMS.Value)
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

               string sSaveResultPath = ConfigurationManager.AppSettings["SaveResultPath"];

               if (Directory.Exists(sSaveResultPath) == false)
                   System.IO.Directory.CreateDirectory(sSaveResultPath);

               sSaveResultPath = string.Format(@"{0}\{1}.bin", sSaveResultPath, msMetaName);
               filePro.Serializer<List<MatchedData>>(lstMatchedData, sSaveResultPath);

               if (sSaveResultPath != string.Empty)
               {
                   this.Invoke(new InvokeDialog(OpenView), new object[] { sSaveResultPath, lstMatchedData });
               }
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
            string sParameterFileName = System.IO.Path.Combine(new string[] { msOutputFilePath, msRawFileName + ".bin" });
            ParameterModel model = SaveControlToModel();
            filePro.Serializer<ParameterModel>(model, sParameterFileName);

            if (lstMatchedData.Count == 0)
                return;

            //Excel Output
            string sXlsName = System.IO.Path.Combine(new string[] { msOutputFilePath, msRawFileName + ".xls" });
            filePro.ExcelOut(lstMatchedData, sXlsName);

            //Picture Output
            ProteinGoogleChartPicture chartPicture = new ProteinGoogleChartPicture();
            chartPicture.MatchDataArray = lstMatchedData;
            chartPicture.gsPath = msOutputFilePath;
            chartPicture.PictureOutput();
        }

        private List<string> PTM_Score_Filter(List<string> lst,string modRes,string order)
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
