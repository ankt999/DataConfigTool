using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using Excel;
using System.Data;
using System.Text;
using System.Xml.Serialization;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;

public class Excel2CsBytesTools
{
    static string ExcelPath =  Application.dataPath + "/ExcelTable"; //存放 Excel表
    static string CsPath =  Application.dataPath + "/Scripts/Table"; //存放自动生成的C#文件
    static string BytesPath =  Application.dataPath + "/BytesTable"; //存放转成的Bytes文件
    static string XMLPath = Application.dataPath + "/XMLTable"; //存放C#脚本反射得到的临时XML数据
    
    //首先读取Excel中的 XLSX文件，先转成C#文件，然后使用XML存储C#转成byte的过度数据
    //读Excel：ReadExcelXLSX
    //写Cs：WriteCs
    //Cs转XML：Cs2XML
    //XML转Bytes：XML2Bytes

    private static List<string> xlsxNameLists; //存读取的xlsx文件
    private static Dictionary<string, DataTable> excelDataCache; //存xlsx中的表，key为文件路径，val为 table
    
    [MenuItem("Excel2CsBytesTools/WriteCs")]
    private static void WriteCs()
    {
        xlsxNameLists = ReadExcel(); //获取所有xlsx的完整路径列表
        InitLoadCache(xlsxNameLists); //读取表进内存
        Excel2Cs("weapon");
    }

    private static List<string> ReadExcel()
    {
        if (!Directory.Exists(ExcelPath))
        {
            Debug.Log("加载Excel文件夹所在的目录无效");
            return null;
        }
        //获取对应的文件夹信息
        DirectoryInfo directoryInfo = new DirectoryInfo(ExcelPath);
        FileInfo[] xlsxInfos = directoryInfo.GetFiles("*.xlsx",SearchOption.AllDirectories);
        if (xlsxInfos.Length == 0)
        {
            Debug.Log("该目录下不存在.xlsx文件");
        }
        else
        {
            xlsxNameLists = new List<string>();
            //查找所有的xlsx文件
            foreach (FileInfo xlsxInfo in xlsxInfos)
            {
                //获取完整路径
                string xlsxFullPath = xlsxInfo.FullName;
                //获取文件名（不包含拓展名）
                string xlsxNameWithoutExtension = Path.GetFileNameWithoutExtension(xlsxFullPath);
                Debug.Log($"找到了路径为：{xlsxFullPath}的xlsx文件：{xlsxNameWithoutExtension}");
                xlsxNameLists.Add(xlsxFullPath);
            }
        }
        return xlsxNameLists;
    }

    private static void InitLoadCache(List<string> xlsxInfoList)
    {
        if (xlsxInfoList.Count==0) return;
        excelDataCache = new Dictionary<string,DataTable>();
        
        //每次执行清空当前的缓存
        excelDataCache.Clear();
        
        try
        {
            foreach (string xlsxName in xlsxNameLists)
            {
                using (FileStream stream = File.Open(xlsxName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    using (IExcelDataReader excelReady = ExcelReaderFactory.CreateOpenXmlReader(stream))
                    {
                        DataSet result = excelReady.AsDataSet();
                        if (result!=null && result.Tables.Count > 0)
                        {
                            DataTable table = result.Tables[0];
                            //剔除完整路径
                            excelDataCache[Path.GetFileNameWithoutExtension(xlsxName)] = table;
                            Debug.Log($"成功使用 Excel.dll 加载表: {xlsxName}, 包含 {table.Rows.Count} 行, {table.Columns.Count} 列");
                        }
                    }
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"加载 Excel 到内存时发生异常: {e.Message}\n请确保引入了正确的 Excel.dll 和 ICSharpCode.SharpZipLib.dll");
        }
    }

    //xlsx生成C#文件
    private static void Excel2Cs(string className)
    {
        if (excelDataCache == null) return;
        if (!Directory.Exists(CsPath))
        {
            Directory.CreateDirectory(CsPath);
        }
        foreach (var kvp in excelDataCache)
        {
            string xlsxName = kvp.Key;
            DataTable table = kvp.Value;
            if (table.Columns.Count > 0)
            {
                //拼接 Cs部分
                try
                {
                    StringBuilder stringBuilder = new StringBuilder();
                    stringBuilder.AppendLine("using System;");
                    stringBuilder.AppendLine("using System.Collections.Generic;");
                    stringBuilder.AppendLine("using System.IO;");
                    stringBuilder.AppendLine("using System.Runtime.Serialization.Formatters.Binary;");
                    stringBuilder.AppendLine("using System.Xml.Serialization;");
            
                    stringBuilder.AppendLine("\n");
            
                    stringBuilder.AppendLine("namespace Table");
                    stringBuilder.AppendLine("{");
                    stringBuilder.AppendLine("    [Serializable]");
                    stringBuilder.AppendLine("    public class " + className);
                    stringBuilder.AppendLine("    {");
                    for (int i = 0; i < table.Columns.Count; i++)
                    {
                        //comment注释
                        string comment = table.Rows[0][i]?.ToString().Trim();
                        string varName = table.Rows[1][i]?.ToString().Trim();
                        string varType = table.Rows[2][i]?.ToString().Trim();

                        stringBuilder.AppendLine("      /// <summary>");
                        stringBuilder.AppendLine("      /// " + comment);
                        stringBuilder.AppendLine("      /// </summary>");
                        stringBuilder.AppendLine("      public " + varType + " " + varName + ";");
                    }
                    stringBuilder.AppendLine("    }");
                    stringBuilder.AppendLine("}");
                    string csPath = Path.Combine(CsPath,className + "." + "cs");
                    //将拼接好的stringBuilder根据csPath写入文件
                    File.WriteAllText(csPath,stringBuilder.ToString(),Encoding.UTF8);
                    Debug.Log("生成weapon文件成功");
                    
                    EditorPrefs.SetBool("Auto_Cs2XML", true);
                    // 刷新 AssetDatabase
                    AssetDatabase.Refresh();
                    Debug.Log("XML文件生成");
                }
                catch (Exception e)
                {
                    Debug.Log($"生成Cs过程有误：{e.Message}");
                }
            }
        }
    }

    private static void Cs2XML()
    {
        if (!Directory.Exists(XMLPath))
        {
            Directory.CreateDirectory(XMLPath);
        }
        
        foreach (var kvp in excelDataCache)
        {
            string xlsxName = kvp.Key;
            DataTable table = kvp.Value;
            
            //使用反射创建动态类
            string className = "Table." + xlsxName;
            Type classType = Assembly.Load("Assembly-CSharp").GetType(className); //className的反射图纸
            if (classType == null)
            {
                Debug.LogError($"Cs2XML反射失败：找不到类 {className}");
                return; 
            }
            
            Type listType = typeof(List<>).MakeGenericType(classType);
            object dataList = Activator.CreateInstance(listType);
            MethodInfo addMethod = listType.GetMethod("Add");
            
            for (int i = 3; i < table.Rows.Count; i++)
            {
                object rowInstance = Activator.CreateInstance(classType);
                
                for (int j = 0; j < table.Columns.Count; j++)
                {
                    string varName = table.Rows[1][j].ToString(); 
                    
                    string cellValue = table.Rows[i][j].ToString();
                    if (String.IsNullOrEmpty(cellValue)) continue; //continue吗
                    
                    FieldInfo fieldInfo = classType.GetField(varName);
                    if (fieldInfo != null && !String.IsNullOrEmpty(cellValue))
                    {
                        try
                        {
                            object realValue = Convert.ChangeType(cellValue, fieldInfo.FieldType);
                            
                            fieldInfo.SetValue(rowInstance, realValue);
                        }
                        catch (Exception e)
                        {
                            Debug.LogError($"数据转换失败！无法把 '{cellValue}' 塞进 {varName} 格子里。");
                        }
                    }
                }
                
                //放入动态 List
                addMethod.Invoke(dataList, new object[] { rowInstance});
            }
            
            string xmlPath = Path.Combine(XMLPath,xlsxName + "." + "xml");
            StreamWriter streamWriter = new StreamWriter(xmlPath);
            XmlSerializer xmlSerializer = new XmlSerializer(listType);
            xmlSerializer.Serialize(streamWriter, dataList);
            streamWriter.Close();
        }
    }

    [MenuItem("Excel2CsBytesTools/XML2Bytes")]
    private static void XML2Bytes()
    {
        if (!Directory.Exists(BytesPath))
        {
            Directory.CreateDirectory(BytesPath);
        }
        
        string[] xmlFilesPath = Directory.GetFiles(XMLPath, "*.xml"); //返回所有xml文件路径
        if (xmlFilesPath.Length <= 0)
        {
            Debug.Log("无XML数据，无法转换为Bytes");
        }

        foreach (var xmlFilePath in xmlFilesPath)
        {
            try
            {
                //剔除完整路径，只保留目标文件名
                string fileName = Path.GetFileNameWithoutExtension(xmlFilePath);
                string className = "Table." + fileName;
                
                Type classType = Assembly.Load("Assembly-CSharp").GetType(className);
                if (classType == null)
                {
                    Debug.LogError($"XML2Bytes反射失败：找不到类 {className}");
                    return; 
                }

                Type listType = typeof(List<>).MakeGenericType(classType);
                object dataList = null;
                using (FileStream xmlStream = new FileStream(xmlFilePath, FileMode.Open, FileAccess.Read))
                {
                    XmlSerializer xmlSerializer = new XmlSerializer(listType);
                    dataList = xmlSerializer.Deserialize(xmlStream);
                }
        
                if (dataList == null)
                {
                    Debug.LogWarning($"XML 文件 {className}.xml 反序列化结果不存在");
                    continue;
                }
                
                //将反序列化后的内存数据(dataList) 转换为 Bytes 二进制文件
                string bytesFilePath = Path.Combine(BytesPath, fileName + ".bytes");
                using (FileStream bytesStream = new FileStream(bytesFilePath, FileMode.Create, FileAccess.Write))
                {
                    // 使用 BinaryFormatter 将对象序列化为二进制流
                    BinaryFormatter binaryFormatter = new BinaryFormatter();
                    binaryFormatter.Serialize(bytesStream, dataList);
                }
                
                Debug.Log($"成功将 {fileName}.xml 转换为 {fileName}.bytes 存入 {BytesPath}");
            }
            catch (Exception e)
            {
                Debug.LogError($"转换 {xmlFilePath} 发生异常: {e.Message}");
            }
        }
        AssetDatabase.Refresh();
    }
    
    // 监听 Unity 脚本编译完成的事件
    [UnityEditor.Callbacks.DidReloadScripts]
    private static void OnScriptsReloaded()
    {
        if (EditorPrefs.GetBool("Auto_Cs2XML", false))
        {
            EditorPrefs.SetBool("Auto_Cs2XML", false); 
            xlsxNameLists = ReadExcel(); 
            InitLoadCache(xlsxNameLists); 

            // 调用生成 XML
            Cs2XML();
            
            bool isCompilingBefore = EditorApplication.isCompiling;
            
            AssetDatabase.Refresh();
            
            if (!isCompilingBefore && !EditorApplication.isCompiling)
            {
                EditorPrefs.SetBool("Auto_Cs2XML", false);
                Cs2XML();
            }
        }
    }
}
