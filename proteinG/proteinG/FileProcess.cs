using System;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;

using ProteinGoggle.Model;

namespace ProteinGoggle.Model
{
    public class FileProcess
    {
        [System.Runtime.InteropServices.DllImport("User32.dll", CharSet = System.Runtime.InteropServices.CharSet.Auto)]
        public static extern int GetWindowThreadProcessId(IntPtr hwnd, out int ID);

        public void Serializer<T>(T obj, string sFileName)
        {
            //Path.Combine(SerializerFilePath, fileName)
            using (FileStream stream = new FileStream(sFileName, FileMode.OpenOrCreate, FileAccess.Write))
            {
                BinaryFormatter formater = new BinaryFormatter();
                formater.Serialize(stream, obj);
            }
        }

        public T Deserialize<T>(string sFileName)
        {
            using (FileStream stream = new FileStream(sFileName, FileMode.Open, FileAccess.Read))
            {
                BinaryFormatter formater = new BinaryFormatter();
                T obj = (T)formater.Deserialize(stream);
                return obj;
            }
        }

        public void ExcelOut(List<MatchedData> lst, string sFileName)
        {
            System.Reflection.Missing miss = System.Reflection.Missing.Value;
            MatchedData matchecdData = new MatchedData();

            Excel.Application excelApplication = new Excel.Application();
            Excel.Workbook excelWorkbook = excelApplication.Workbooks.Add();
            excelApplication.UserControl = true;
            excelApplication.Application.DisplayAlerts = false;

            Excel.Worksheet excelWorksheet = (Excel.Worksheet)excelWorkbook.Sheets.Add(miss, miss, miss, miss);
            excelWorksheet.Name = "Protein";

            int iRow = 0;

            try
            {
                for (int i = excelWorkbook.Sheets.Count; i >= 1; i--)
                {
                    Excel.Worksheet excelWorksheetTemp = excelWorkbook.Sheets[i];
                    if (excelWorksheetTemp.Name != "Protein")
                        excelWorksheetTemp.Delete();
                }

                //Header
                iRow = 1;
                excelWorksheet.Cells[iRow, 1] = "Index";
                excelWorksheet.Cells[iRow, 2] = "ID";
                excelWorksheet.Cells[iRow, 3] = "MOD_RES";
                excelWorksheet.Cells[iRow, 4] = "PTM_Score";
                excelWorksheet.Cells[iRow, 5] = "TotalPTM_Score";
                excelWorksheet.Cells[iRow, 6] = "Z";
                excelWorksheet.Cells[iRow, 7] = "DatasetFile";
                excelWorksheet.Cells[iRow, 8] = "Database";
                excelWorksheet.Cells[iRow, 9] = "Repository";
                excelWorksheet.Cells[iRow, 10] = "ScreeningApproach";
                excelWorksheet.Cells[iRow, 11] = "ExpMZ";
                excelWorksheet.Cells[iRow, 12] = "TheoMZ";
                excelWorksheet.Cells[iRow, 13] = "Error_PPM";
                excelWorksheet.Cells[iRow, 14] = "AccessionNumber";
                excelWorksheet.Cells[iRow, 15] = "ProteinName";
                excelWorksheet.Cells[iRow, 16] = "Length";
                excelWorksheet.Cells[iRow, 17] = "Function";
                excelWorksheet.Cells[iRow, 18] = "Sequence";
                excelWorksheet.Cells[iRow, 19] = "NMFs";
                excelWorksheet.Cells[iRow, 20] = "Actual_PMFs";

                //Value
                iRow = 2;
                for (int i = 0; i < lst.Count; i++)
                {
                    matchecdData = lst[i];

                    excelWorksheet.Cells[iRow, 1] = matchecdData.Index;
                    excelWorksheet.Cells[iRow, 2] = matchecdData.ID;
                    excelWorksheet.Cells[iRow, 3] = matchecdData.MOD_RES.Replace(";","").Replace(",","");
                    excelWorksheet.Cells[iRow, 4] = matchecdData.DataBase_PTM_SCORE;
                    excelWorksheet.Cells[iRow, 5] = matchecdData.Total_PTM_Score;
                    excelWorksheet.Cells[iRow, 6] = matchecdData.Z;
                    excelWorksheet.Cells[iRow, 7] = matchecdData.DatasetFile;
                    excelWorksheet.Cells[iRow, 8] = matchecdData.Database;
                    excelWorksheet.Cells[iRow, 9] = matchecdData.Repository;
                    excelWorksheet.Cells[iRow, 10] = matchecdData.ScreeningApproach;
                    excelWorksheet.Cells[iRow, 11] = matchecdData.ExpMZ;
                    excelWorksheet.Cells[iRow, 12] = matchecdData.TheoMZ;
                    excelWorksheet.Cells[iRow, 13] = matchecdData.Error_PPM;
                    excelWorksheet.Cells[iRow, 14] = matchecdData.AccessionNumber;
                    excelWorksheet.Cells[iRow, 15] = matchecdData.ProteinName;
                    excelWorksheet.Cells[iRow, 16] = matchecdData.Length;
                    excelWorksheet.Cells[iRow, 17] = matchecdData.Function;
                    excelWorksheet.Cells[iRow, 18] = matchecdData.Sequence;
                    excelWorksheet.Cells[iRow, 19] = matchecdData.NMFS;
                    excelWorksheet.Cells[iRow, 20] = matchecdData.Actualpmfs;

                    iRow++;
                }

                excelWorkbook.SaveAs(sFileName, miss, miss, miss, miss, miss, Excel.XlSaveAsAccessMode.xlNoChange, miss, miss, miss, miss, miss);
                excelWorkbook.Close(false, miss, miss);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {

                matchecdData = null;

                if (excelApplication != null)
                {
                    excelApplication.Workbooks.Close();
                    excelApplication.Quit();
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(excelApplication.Workbooks);
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(excelApplication);
                    System.GC.Collect();
                }

                if (excelApplication != null)
                {
                    IntPtr t = new IntPtr(excelApplication.Hwnd);
                    int k = 0;
                    GetWindowThreadProcessId(t, out k);
                    System.Diagnostics.Process p = System.Diagnostics.Process.GetProcessById(k);
                    p.Kill();
                }

                excelApplication = null;
                GC.Collect();
            }
        }

        public void CreateWithIsExistPath(string sFilePath)
        {
            if (Directory.Exists(sFilePath) == false)
            {
                Directory.CreateDirectory(sFilePath);
            }
            else
            {
                Directory.Delete(sFilePath, true);
                System.Threading.Thread.Sleep(500);
                Directory.CreateDirectory(sFilePath);
            }

            while (true)
            {
                if (System.IO.Directory.Exists(sFilePath) == true)
                    return;

                System.Threading.Thread.Sleep(1000);
                Directory.CreateDirectory(sFilePath);
            }
        }

    }
}
