using System.Collections;
using System.Collections.Generic;
using EnhancedUI.EnhancedScroller;
using GameNeon.Datas.Event;
using GameNeon.Managers;
using GameNeon.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GameNeon
{
    public class ItemTagView : EnhancedScrollerCellView
    {
        // index也是标签组
        [HideInInspector] public int index;
        public static int lastClickIndex = 0;
        public Image tagImage;
        public Button tagBtn;

        #region 滑动条UI

        public bool isNotShowSlider = true;
        public GameObject onOff;
        public Image sliderImg;

        public TMP_Text totalNumsText;
        public TMP_Text nowNumsText;
        [HideInInspector] public int capacity;
        [HideInInspector] public int nowNums;

        #endregion

        private void Awake()
        {
            tagBtn.onClick.AddListener(ClickTagBtn);
            GameEventManager.Instance.AddListener<int>(EventID.REFRESH_BAG_TAG, TagISOnOffCallBack);
        }


        /// <summary>
        /// 用于刷新显示
        /// </summary>
        public void Refresh()
        {
            onOff.SetActive(isNotShowSlider); // 控制是否高亮
            // 是否加载高亮
            bool isOn = index == 0;
            InventoryUtil.LoadTagSprite(tagImage, index, isOn);

            // 滑动条部分
            totalNumsText.text = capacity.ToString();
            nowNumsText.text = nowNums.ToString();
            var rate = Mathf.Round((nowNums * 100) / capacity) / 100;
            InventoryUtil.LoadSlider(sliderImg, rate);
            sliderImg.fillAmount = rate;
        }


        private void ClickTagBtn()
        {
            // 如果点击的还是当前列表，则不刷新
            if (lastClickIndex == index) return;
            lastClickIndex = index;
            // 点击切换按钮时，需要通知列表刷新以及itemInfo刷新
            GameEventManager.Instance.BroadCast(EventID.REFRESH_BAG_TAG, index);
        }

        /// <summary>
        /// 只用来控制图片加载和现实，不关联刷新列表 只控制加载图
        /// </summary>
        /// <param name="clickIndex"></param>
        private void TagISOnOffCallBack(int clickIndex)
        {
            if (tagImage == null) return;

            bool isOn = index == clickIndex;
            InventoryUtil.LoadTagSprite(tagImage, index, isOn);
        }

        private void OnDestroy()
        {
            GameEventManager.Instance.RemoveListener<int>(EventID.REFRESH_BAG_TAG, TagISOnOffCallBack);
        }
    }
}