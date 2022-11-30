// ==============================
// @Author: 文若
// @DateTime: 2022-11-30
// ==============================

using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace GameNeon
{
    public class TooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public string content;
        public string header;
        public float delayTime = 1f;

        public void OnPointerEnter(PointerEventData eventData)
        {
            TooltipSystem.SetText(content, header);
            Invoke(nameof(Show), delayTime);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            TooltipSystem.Hide();
        }

        public void Show()
        {
            TooltipSystem.Show();
        }
    }
}