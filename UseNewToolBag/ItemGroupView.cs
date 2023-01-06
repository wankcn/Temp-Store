using System.Collections;
using System.Collections.Generic;
using EnhancedUI;
using EnhancedUI.EnhancedScroller;
using GameNeon.Modules.InventoryModule;
using UnityEngine;

namespace GameNeon
{
    public class ItemGroupView : EnhancedScrollerCellView
    {
        public ItemCellView[] rowCellViews;

        public void SetData(ref List<ItemDetails> data, int startingIndex)
        {
            for (var i = 0; i < rowCellViews.Length; i++)
            {
                var cell = rowCellViews[i];
                var index = startingIndex + i;
                // 记录当前item索引
                cell.index = index;
                cell.count = data.Count;
                // 背包无限或者背包有限制，能赋予数据的部分始终 小于列表信息的数量 因为index从0开始
                if (index < data.Count)
                    cell.itemDetails = data[index];
                cell.SetItemInfo();
            }
        }
    }
}