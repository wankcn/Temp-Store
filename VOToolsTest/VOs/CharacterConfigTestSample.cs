using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CharacterConfigTestSample
{
    public class CharacterConfigSample
    {
        private int id; 
        private float config_id; 
        private string name ; 

        public CharacterConfigSample(int id, float config_id, string name)
        {
            this.id = id;
            this.config_id = config_id;
            this.name = name;
        }


        public int ID => id;
        public float Config_id => config_id;
        public string Name => name;
    }

    private Dictionary<int, CharacterConfigSample> list = new Dictionary<int, CharacterConfigSample>();

    public CharacterConfigTestSample()
    {
        list.Add(1, new CharacterConfigSample(1, 2, "aa"));
        // ...
    }


    public CharacterConfigSample GetVO(int id)
    {
        var tmp = list[1];
        return tmp;
    }

    void test()
    {
        var o = GetVO(1);
        
    }
}