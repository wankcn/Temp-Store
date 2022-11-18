using System.Collections;
using System.Collections.Generic;
using GameNeon.Managers;
using UnityEngine;

namespace GameNeon
{
    public class TmpCharacter : MonoBehaviour
    {
        
        void Awake()
        {
            ControlledCharacterManager.Instance.SpawnCurrentCharacter(currentCharacterGO=> { });
        }

       
    }
}
