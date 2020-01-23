using System;
using OfficeOpenXml;
using System.IO;
using System.Collections.Generic;
using System.Text;

namespace UCCX_API_Service
{
    class Reader
    {
        string filePath { get; set; }
        public Reader(string file)
        {
            filePath = file;
        }
        public List<ExcelSkill> ReadSkillData(string sheetNameInput)
        {
            List<ExcelSkill> skillData = new List<ExcelSkill>();
            FileInfo file = new FileInfo(filePath);
            using (ExcelPackage package = new ExcelPackage(file))
            {
                StringBuilder sb = new StringBuilder();
                ExcelWorksheet worksheet = package.Workbook.Worksheets[sheetNameInput];
                //var totalRows = worksheet.Dimension.Address;
                //ExcelRange data = worksheet.Cells[totalRows];
                //int rowCount = data.Rows;
                //int colCount = data.Columns;

                int rowCount = worksheet.Dimension.End.Row;
                int colCount = worksheet.Dimension.End.Column;
                //Console.WriteLine("ROWS: " + rowCount.ToString() + "\nCOLUMNS: " + colCount.ToString());

                for (int i = 2; i <= rowCount; i++)
                {
                    string name = String.Empty;
                    string add = String.Empty;
                    string remove = String.Empty;
                    for (int j = 1; j <= colCount; j++)
                    {
                        if (j == 1 && worksheet.Cells[i, j].Value != null)
                        {
                            name = worksheet.Cells[i, j].Value.ToString();
                        }
                        else if (j == 2 && worksheet.Cells[i, j].Value != null)
                        {
                            add = worksheet.Cells[i, j].Value.ToString();
                        }
                        else if (j == 3 && worksheet.Cells[i, j].Value != null)
                        {
                            remove = worksheet.Cells[i, j].Value.ToString();
                        }
                        else if (add == String.Empty || add == null)
                        {
                            continue;
                        }
                    }
                    if (add != String.Empty && add != null && add != "" && add.Length > 2)
                    {
                        //Console.WriteLine("Adding " + name);
                        ExcelSkill skill = new ExcelSkill(name, add, remove);
                        skillData.Add(skill);
                    }
                }
            }
            return skillData;
        }
        public List<ExcelAgent> ReadAgentData(string sheetNameInput)
        {

            FileInfo file = new FileInfo(filePath);

            List<ExcelAgent> agentData = new List<ExcelAgent>();
            using (ExcelPackage package = new ExcelPackage(file))
            {
                StringBuilder sb = new StringBuilder();
                ExcelWorksheet worksheet = package.Workbook.Worksheets[sheetNameInput];

                //var totalRows = worksheet.Dimension.Address;
                //ExcelRange data = worksheet.Cells[totalRows];
                //int rowCount = data.Rows;
                //int colCount = data.Columns;

                int rowCount = worksheet.Dimension.End.Row;
                int colCount = worksheet.Dimension.End.Column;

                for (int i = 2; i <= rowCount; i++)
                {
                    string sheetName = String.Empty;
                    string sheetQueue = String.Empty;
                    for (int j = 1; j <= colCount; j++)
                    {
                        if (j == 1 && worksheet.Cells[i, j].Value.ToString() != null)
                        {
                            sheetName = worksheet.Cells[i, j].Value.ToString();
                        }
                        else if (j == 2 & worksheet.Cells[i, j] != null && worksheet.Cells[i, j].Value.ToString() != null)
                        {
                            sheetQueue = worksheet.Cells[i, j].Value.ToString();
                        }
                        else if (sheetQueue == String.Empty || sheetQueue == null)
                        {
                            continue;
                        }
                    }
                    if (sheetQueue != String.Empty && sheetQueue != null)
                    {
                        ExcelAgent agent = new ExcelAgent(sheetName, sheetQueue);
                        agentData.Add(agent);
                    }
                }
            }
            return agentData;
        }
    }
}
