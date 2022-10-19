using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class TestAlgoUtil : MonoBehaviour
{
    private void Start()
    {
        var nums = ArrayGenerator.GenerateRandomArray(10, 100);
        var str = "";
        foreach (var n in nums)
            str += n + " ";
        print($"{str}");
        
        AlgoUtil.QuickSort(nums);
        str = "";
        foreach (var n in nums)
            str += n + " ";
        print($"{str}");
    }
}