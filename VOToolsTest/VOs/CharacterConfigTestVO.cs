// 该类为自动生成的VO类，根据需求增加变量或方法

using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CharacterConfigTestVO
{
	public class CharacterConfigTest
	{
		private int configId;
		private string name;
		private int configInt;
		private string id;
		private string configStr;

		// 构造时必须赋值
		public CharacterConfigTest(int configId,string name,int configInt,string id,string configStr)
		{
			this.configId = configId;
			this.name = name;
			this.configInt = configInt;
			this.id = id;
			this.configStr = configStr;
		}

		public int ConfigId => configId;
		public string Name => name;
		public int ConfigInt => configInt;
		public string Id => id;
		public string ConfigStr => configStr;
	}

	public Dictionary<string ,CharacterConfigTest> dict = new Dictionary<string ,CharacterConfigTest>();

	public CharacterConfigTestVO()
	{
	}

	public Dictionary<string,CharacterConfigTest> GetVOList()
	{
		return dict;
	}

	public CharacterConfigTest GetVO(string key)
	{
		if (!dict.ContainsKey(key))
			 throw new Exception("CharacterConfigTestVO没有Id为"+ key + "的记录！");
		return dict[key];
	}

	public bool HasVO(string key)
	{
		 return dict.ContainsKey(key) != null;
	}

	// 自定义...
}

