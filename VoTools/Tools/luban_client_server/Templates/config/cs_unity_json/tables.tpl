using Bright.Serialization;
using SimpleJSON;
using System.Collections.Generic;

{{
    name = x.name
    namespace = x.namespace
    tables = x.tables
}}

{{cs_start_name_space_grace x.namespace}} 

	/// <summary>
	/// 所有VO对象必须继承该接口
	/// 在数据加载时调用 _LoadData(外部读取的Json_Text)
	/// </summary>
	public interface IVOFun
	{
	    void _LoadData(string json_text);
	}
	   
	public sealed partial class {{name}}
	{
	    public Dictionary<string, object> tables = new Dictionary<string, object>(); 
	    public {{name}}() { }
	    public void TranslateText(){}
	    partial void PostInit();
	    partial void PostResolve();
	}

{{cs_end_name_space_grace x.namespace}}