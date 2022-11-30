// ==============================
// @Author: 文若
// @DateTime: 2022-11-30
// ==============================

using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace GameNeon
{
    public class TooltipSystem : MonoBehaviour
    {
        
        private static TooltipSystem current;


        public Tooltip tooltip;


        private void Awake()
        {
            current = this;
            
        }

        public static void Show()
        {
            current.tooltip.gameObject.SetActive(true);
        }


        public static void SetText(string content, string header = "")
        {
            current.tooltip.SetText(content, header);
        }


        public static void Hide()
        {
            current.tooltip.gameObject.SetActive(false);
        }
    }
}