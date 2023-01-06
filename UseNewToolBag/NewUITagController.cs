using System.Collections;
using System.Collections.Generic;
using EnhancedUI.EnhancedScroller;
using GameNeon.Datas.Event;
using GameNeon.Frameworks;
using GameNeon.Managers;
using GameNeon.Modules.InventoryModule;
using UnityEngine;
using UnityEngine.UI;

namespace GameNeon
{
    public class NewUITagController : MonoBehaviour, IEnhancedScrollerDelegate
    {
        public EnhancedScroller scroller;

        public ItemTagView tagViewPrefab;

        private List<int> tagList = new List<int>() { 0, 1, 2, 3, 4 };

        void Start()
        {
            Application.targetFrameRate = 60;
            scroller.Delegate = this;
            scroller.gameObject.GetComponent<ScrollRect>().enabled = false;
        }


        public int GetNumberOfCells(EnhancedScroller scroller)
        {
            return tagList.Count;
        }

        public float GetCellViewSize(EnhancedScroller scroller, int dataIndex)
        {
            return 215f;
        }

        public EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int index, int cellIndex)
        {
            ItemTagView tag = scroller.GetCellView(tagViewPrefab) as ItemTagView;
            tag.index = index;
            var limit = PlayerBagDataManager.Instance.GetBagLimit(index);
            tag.capacity = limit;
            tag.isNotShowSlider = limit != -1; // 0,4不显示滑动条
            // 记录滑动条相关
            // 如果这里为-1 可以不处理
            tag.nowNums = PlayerBagDataManager.Instance.GetBagLimit(index);
            tag.Refresh(); // 初始化调一次 不是All全部加载灰
            return tag;
        }
    }
}