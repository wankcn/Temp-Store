// ==============================
// @Author: 文若
// @DateTime: 2022-11-30
// ==============================

using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GameNeon
{
    public class Tooltip : MonoBehaviour
    {
        public TextMeshProUGUI headerField;
        public TextMeshProUGUI contentField;
        public LayoutElement layoutElement;
        public int characterWrapLimit;
        private RectTransform rectTransform;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
        }


        private void OnEnable()
        {
            SetRectPivot();
        }

        public void SetText(string content, string header = "")
        {
            if (string.IsNullOrEmpty(header))
            {
                headerField.gameObject.SetActive(false);
            }
            else
            {
                headerField.gameObject.SetActive(true);
                headerField.text = header;
            }

            contentField.text = content;
        }

        private void SetRectPivot()
        {
            if (Application.isEditor)
            {
                int headerLength = headerField.text.Length;
                int contentLength = contentField.text.Length;
                layoutElement.enabled = headerLength > characterWrapLimit || contentLength > characterWrapLimit;
            }

            var position = Input.mousePosition;
            float pivotX = position.x / Screen.width;
            float pivotY = position.y / Screen.height;

            rectTransform.pivot = new Vector2(pivotX, pivotY);
            transform.position = position;
        }
    }
}