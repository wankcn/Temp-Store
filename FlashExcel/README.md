# FlashExcel
FlashExcel（闪电导表工具）是一个界面化的表格导出工具。根据游戏引擎和项目需求，程序可以很方便的编写自定义的导出器，
如果不想编写，那么也可以直接使用工具提供的几种默认的导出器。  

![image](https://images.cnblogs.com/cnblogs_com/TravelingLight/1575583/o_flashExcel3.png)

## 导出器
1. **TXT文本导出器**  
生成一个包含所有表格数据的纯文本，一般多用于C++逻辑服务器。  

2. L**UA脚本导出器**  
生成一个包含所有表格数据的Lua脚本。  

3. **ILR脚本导出器**  
生成一个包含所有表格数据的CS脚本，一般多用于支持ILRuntime的客户端框架。  

4. **BYTE文件导出器**  
生成一个包含所有表格数据的二进制格式的文件，需要CS脚本导出器配合。  

5. **CS脚本导出器**（需要MotionEngine.IO库）  
配合BYTE文件导出器，生成一个支持读取二进制数据的CS脚本。该CS脚本可以实现解析零GC。

## 工程
VS2017 && .net framework 4.6
1. NuGet package : NPOI
2. NuGet package : SharpZipLib

## 特点
表格结构支持定义各种类型：整型，浮点型，布尔，枚举，多语言，字符串，列表，自定义类型。并且单元格支持数值公式和字符串公式。

## 多语言
工具提供了完整的多语言解决方案，自动生成多语言汇总表格。

## 说明
![image](https://images.cnblogs.com/cnblogs_com/TravelingLight/1575583/o_flashExcel1.png)  

![image](https://images.cnblogs.com/cnblogs_com/TravelingLight/1575583/o_flashExcel2.png)
1. 第一行为类型
2. 第二行为名称
3. 第三行为导出标记：客户端(C)，战斗服务器(B)，逻辑服务器(S)。

## 注意事项
Excel内可以设立多个页签，前缀t_的页签会被识别并导出，其它页签可以作为策划的辅助页签或备注页签
