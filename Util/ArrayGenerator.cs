//
// @Description: 测试用例生成器
// @Author: 文若
// @CreateDate: 2022-10-19
// 

using System;


public class ArrayGenerator
{
    private ArrayGenerator()
    {
    }

    /// <summary>
    /// 生成一个顺序数组
    /// </summary>
    /// <param name="n">指定数组大小</param>
    /// <returns></returns>
    public static Int32[] GenerateOrderedArray(int n)
    {
        Int32[] arr = new Int32[n];
        for (int i = 0; i < n; i++)
            arr[i] = i;
        return arr;
    }

    /// <summary>
    /// 生成长度为n,每个数组范围[0,bound)
    /// </summary>
    /// <param name="n">指定数组大小</param>
    /// <param name="bound">生成数的最大边界</param>
    /// <returns></returns>
    public static Int32[] GenerateRandomArray(int n, int bound)
    {
        Int32[] arr = new Int32[n];
        Random rnd = new Random();
        for (int i = 0; i < n; i++)
            arr[i] = rnd.Next(bound);
        return arr;
    }
}