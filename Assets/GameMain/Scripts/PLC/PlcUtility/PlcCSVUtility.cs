using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace ATF
{
    public static class PlcCSVUtility
    {
        public static async Task<Dictionary<string, MDataItem>> ReadPLCData(string csvPath)
        {
            Dictionary<string, MDataItem> plcDictionary = new Dictionary<string, MDataItem>();

            try
            {
                string[] lines = await ReadCSV(csvPath);
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
                Debug.LogError($"Error reading CSV: {ex.Message}");
            }

            return plcDictionary;
        }

        public static async Task<Dictionary<string, string>> ReadJieXianData(string csvPath)
        {
            Dictionary<string, string> jiexianDictionary = new Dictionary<string, string>();

            try
            {
                string[] lines = await ReadCSV(csvPath);
                for (int i = 1; i < lines.Length; i++)
                {
                    string line = lines[i];
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    string[] values = line.Split(',');
                    if (values.Length >= 2)
                    {
                        if (!jiexianDictionary.ContainsKey(values[0]))
                        {
                            jiexianDictionary.Add(values[0], values[1]);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error reading CSV: {ex.Message}");
            }

            return jiexianDictionary;
        }

        /// <summary>
        /// 读取 CSV 文件，支持 PC 和 WebGL
        /// </summary>
        private static async Task<string[]> ReadCSV(string filePath)
        {
#if UNITY_WEBGL
        using (UnityWebRequest request = UnityWebRequest.Get(filePath))
        {
            var operation = request.SendWebRequest();
            while (!operation.isDone)
            {
                await Task.Yield();
            }

            if (!request.isHttpError && !request.isNetworkError)
            {
                return request.downloadHandler.text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
            }
            else
            {
                Debug.LogError($"Failed to load CSV from {filePath}: {request.error}");
                return new string[0];
            }
        }
#else
            // PC 直接读取本地文件
            return File.Exists(filePath) ? File.ReadAllLines(filePath) : new string[0];
#endif
        }
    }
}