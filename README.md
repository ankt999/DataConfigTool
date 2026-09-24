前往Releases下载unitypackage
导入包后，在Editor文件夹中查看相关文件

#  Unity 导表工具

## 

轻量级、自动化的 Unity 导表工具。主要作用是将策划配置的 Excel (`.xlsx`) 表格数据，一键转换为 Unity 游戏中可直接读取的 **C# 实体类** 和 **二进制（.bytes）数据文件**。

---

## 目录结构

```text
Assets/
├── ExcelTable/                # 配置的 Excel (.xlsx) 表格
│   └── weapon.xlsx 
│
├── Editor/                  
│   └── Excel2CsBytesTools.cs     # 导表脚本
│
├──  Scripts/                   
│   └──  Table/                 
│       └── weapon.cs             # 存放自动生成的cs文件
│
├──  XMLTable/                  # 
│   └── weapon.xml                # 序列化过渡数据
│
└──  BytesTable/                
    └── weapon.bytes              # 转换的二进制数据
```

---

步骤：
1. **读取 Excel (`ReadExcel`)**：利用 `Excel.dll` 解析 `ExcelTable/` 目录下的 `.xlsx` 文件，并将数据缓存到内存（`DataTable`）中。
2. **生成 C# 类 (`Excel2Cs`)**：根据表格的前三行（注释、变量名、类型），动态拼接并生成对应的 C# 数据结构代码，存入 `Scripts/Table/` 目录。
3. **转换为 XML (`Cs2XML`)**：利用 C# 的反射机制，将内存中的表格数据实例化为刚生成的 C# 对象，并序列化保存为 `XMLTable/` 目录下的 `.xml` 临时文件。
4. **转换为 Bytes (`XML2Bytes`)**：读取上一步的 XML 文件，通过 `BinaryFormatter` 反序列化后，再以二进制流的形式写入 `BytesTable/` 目录，供游戏运行时高效读取。

---

##  使用方法

### 1. 准备表格
在 `Assets/ExcelTable/` 目录下放置你的 `.xlsx` 表格文件。
表格的前三行必须严格按照以下格式配置：
- **第一行**：字段注释（如：`唯一ID`, `武器名称`）
- **第二行**：字段变量名（如：`id`, `name`）
- **第三行**：字段数据类型（如：`int`, `string`, `float`, `bool`）
- **第四行及以后**：实际的数据内容。

### 2. 导出 C# 脚本与 XML 数据
在 Unity 顶部菜单栏中，点击：
 `Excel2CsBytesTools` -> `WriteCs`
- 控制台会打印生成日志。
- `Assets/Scripts/Table/` 下会自动生成对应的 `.cs` 文件。
- `Assets/XMLTable/` 下会自动生成对应的临时 `.xml` 文件。

### 3. 导出二进制 Bytes 数据
在 Unity 顶部菜单栏中，点击：
 `Excel2CsBytesTools` -> `XML2Bytes`
- 控制台会打印转换成功的日志。
- `Assets/BytesTable/` 下会自动生成最终的 `.bytes` 二进制文件。

---
