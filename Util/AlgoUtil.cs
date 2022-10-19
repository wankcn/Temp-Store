//
// @Description: 与计算有关的算法工具类
// @Author: 文若
// @CreateDate: 2022-10-19
// 

using System;
using System.Collections.Generic;


public class AlgoUtil
{
    private AlgoUtil(){}


    /// <summary>
    /// KnuthShuffle 真随机
    /// </summary>
    /// <param name="arr"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T[] KnuthShuffle<T>(T[] arr)
    {
        Random random = new Random();
        for (int i = 1; i < arr.Length; i++)
            Swap(arr, i, random.Next(0, i));
        return arr;
    }

    /// <summary>
    /// 通过洗牌算法获得一个真随机样本
    /// </summary>
    /// <param name="arr">当前所需打乱数组</param>
    /// <returns>该值随机出现在数组任意位置的概率相同</returns>
    public static T GetShuffleSample<T>(T[] arr)
    {
        return KnuthShuffle(arr)[0];
    }

    private static void Swap<T>(T[] arr, int x, int y)
    {
        (arr[x], arr[y]) = (arr[y], arr[x]);
    }

    /// <summary>
    /// 原地移动算法将lst1的子集lst2全部移动到lst1的末尾
    /// 应用于分段排序场景，如一组商品列表分为已购买。可购买，无法购买
    /// </summary>
    /// <param name="lst1">列表1</param>
    /// <param name="lst2">列表2（长度一定小于列表1）</param>
    public static void FlowData<T>(List<T> lst1, List<T> lst2) where T : struct
    {
        int tag = 0;
        for (int i = 0; i < lst1.Count; i++)
            if (!lst2.Contains(lst1[i]))
                lst1[tag++] = lst1[i];

        for (int i = tag; i < lst1.Count; i++)
            lst1[i] = lst2[i - tag];
    }

    /// <summary>
    /// 三路快排，一般应用于排行榜等场景的活动排序
    /// </summary>
    /// <param name="arr"></param>
    /// <typeparam name="T"></typeparam>
    public static void QuickSort<T>(T[] arr) where T : IComparable
    {
        Random rnd = new Random();
        QuickSort(arr, 0, arr.Length - 1, rnd);
    }

    private static void QuickSort<T>(T[] arr, int l, int r, Random rnd) where T : IComparable
    {
        if (l >= r) return;
        int p = l + rnd.Next(r - l + 1);
        Swap(arr, l, p);
        int lt = l, i = l + 1, gt = r + 1;
        while (i < gt)
        {
            if (arr[i].CompareTo(arr[l]) < 0)
            {
                lt++;
                Swap(arr, lt, i);
                i++;
            }
            else if (arr[i].CompareTo(arr[l]) > 0)
            {
                gt--;
                Swap(arr, i, gt);
            }
            else
            {
                i++;
            }
        }

        Swap(arr, l, lt);
        QuickSort(arr, l, lt - 1, rnd);
        QuickSort(arr, gt, r, rnd);
    }
}