using System.Collections;
using System.Collections.Generic;
using EnhancedUI.EnhancedScroller;
using GameNeon.Datas.Event;
using GameNeon.Frameworks;
using GameNeon.Managers;
using GameNeon.Modules.InventoryModule;
using Unity.VisualScripting.YamlDotNet.Core.Tokens;
using UnityEngine;
using UnityEngine.UI;

namespace GameNeon
{
    public class NewUIBagController : MonoBehaviour, IEnhancedScrollerDelegate
    {
        /// 可变动列表是数据
        private List<ItemDetails> itemList = new List<ItemDetails>();

        public EnhancedScroller scroller;

        public EnhancedScrollerCellView cellViewPrefab;

        public int numberOfCellsPerRow = 3;

        public EBagTagType tagType = EBagTagType.ALL;

        void Start()
        {
            Application.targetFrameRate = 60;
            scroller.Delegate = this;
            GameEventManager.Instance.AddListener<int>(EventID.REFRESH_BAG_TAG, RefreshRVTagList);
            InitConfigData();
        }


        private void InitConfigData()
        {
            itemList.Clear();
            itemList = PlayerBagDataManager.Instance.GetItemDetailsByTagType(tagType);
            // 设置默认第一个数据的ID 每次回到初始索引
            PlayerBagDataManager.Instance.currentSelectIndex = 0;
            PlayerBagDataManager.Instance.currentSelectItemId = itemList[0].ID;
            scroller.ReloadData();
        }

        public int GetNumberOfCells(EnhancedScroller scroller)
        {
            // int tagIndex = (int)tagType;
            // var limit = PlayerBagDataManager.Instance.GetBagLimit(tagIndex);
            // int capacity = limit >= 0 ? limit : itemList.Count;
            // return capacity;
            return itemList.Count;
        }

        public float GetCellViewSize(EnhancedScroller scroller, int dataIndex)
        {
            return 250f;
        }


        public EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
        {
            ItemGroupView groupView = scroller.GetCellView(cellViewPrefab) as ItemGroupView;

            var di = dataIndex * numberOfCellsPerRow;
            groupView.name = "Cell " + (di).ToString() + " to " + ((di) + numberOfCellsPerRow - 1).ToString();
            groupView.SetData(ref itemList, di);
            return groupView;
        }


        public void OnDestroy()
        {
            GameEventManager.Instance.RemoveListener<int>(EventID.REFRESH_BAG_TAG, RefreshRVTagList);
        }


        private void RefreshRVTagList(int tagIndex)
        {
            if (scroller == null) return;

            Log.D($"Bag刷新列表 ---- {tagIndex}");
            tagType = (EBagTagType)tagIndex;
            // 初始化默认信息
            InitConfigData();
        }
    }
}