using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebPlc.Scripts
{
    public class CSVUtility
    {
        public static  Dictionary<string, MDataItem> ReadPLCData(string csvPath)
        {
            Dictionary<string, MDataItem> plcDictionary = new Dictionary<string, MDataItem>();

            try
            {
                string[] lines = ReadCSV(csvPath);
                for (int i = 1; i < lines.Length; i++)
                {
                    string line = lines[i];
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    string[] values = line.Split(',');
                    if (values.Length >= 6)
                    {
                        MDataItem dataItem = new MDataItem
                        {
                            Id = values[0],
                            Name = values[1],
                            LogicalAddress = values[3],
                        };

                        switch (values[2])
                        {
                            case "Bit":
                                dataItem.VarType = S7.Net.VarType.Bit;
                                break;
                            case "Byte":
                                dataItem.VarType = S7.Net.VarType.Byte;
                                break;
                            case "Int":
                                dataItem.VarType = S7.Net.VarType.Int;
                                break;
                            case "Real":
                                dataItem.VarType = S7.Net.VarType.Real;
                                break;
                            default:
                                dataItem.VarType = S7.Net.VarType.Bit;
                                break;
                        }
                        switch (values[4])
                        {
                            case "Output":
                                dataItem.DataType = S7.Net.DataType.Output;
                                break;
                            case "Input":
                                dataItem.DataType = S7.Net.DataType.Input;
                                break;
                            default:
                                dataItem.DataType = S7.Net.DataType.Input;
                                break;
                        }
                        string id = values[0] + "_" + values[1];

                        if (!plcDictionary.ContainsKey(id))
                        {
                            plcDictionary.Add(id, dataItem);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading CSV: {ex.Message}");
            }
            Console.WriteLine($"正在读取本地配置文件：{csvPath} 文件数量{plcDictionary.Count}");
            return plcDictionary;
        }


        /// <summary>
        /// 读取 CSV 文件，支持 PC 和 WebGL
        /// </summary>
        private static string[] ReadCSV(string filePath)
        {
            return File.Exists(filePath) ? File.ReadAllLines(filePath) : new string[0];
        }
    }
}
