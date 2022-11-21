using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GameNeon.Frameworks;
using GameNeon.Modules.InventoryModule;
using GameNeon.VOS.Inventory;
using UnityEngine;

namespace GameNeon.Managers
{
    public class InventoryManager : MonoSingleton<InventoryManager>
    {
        /// <summary>
        /// 物品信息列表
        /// 由于item表内存储图片，枚举等都是string，int，这里我考虑对表格信息做一层封装。
        /// 通过物品管理类将这些信息直接显示在背包或商店等Ui上
        /// </summary>
        [Header("物品数据")] public List<ItemDetails> itemDataList = new List<ItemDetails>();

        [Header("背包数据")] public InventoryBagSO playerBag;

        // [Header("背包数据")] List<InventoryItem> playerBag;


        protected override void Init()
        {
            base.Init();
            Log.D("InventoryManager Init");
            InitItemList();
            InitPlayerBag(20);
        }

        private void InitItemList()
        {
            var list = DataManager.Instance.GetVOData<ItemTB>("ItemData").DataList;
            foreach (var t in list) itemDataList.Add(new ItemDetails(t));
        }

        private void InitPlayerBag(int capacity)
        {
            // playerBag = new List<InventoryItem>(capacity);
            // 数据初始化
            for (int i = 0; i < capacity; i++)
            {
            }
        }


        /// <summary>
        /// 通过ID获取物品
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public ItemDetails GetItemDetails(int ID)
        {
            return itemDataList.Find(i => i.itemID == ID);
        }


        /// <summary>
        /// 添加物品
        /// </summary>
        /// <param name="item"></param>
        /// <param name="isDestroy">是否销毁物品</param>
        public void AddItem(InvItem item, bool isDestroy)
        {
            // 是否以包含该物品
            var index = GetItemIndexInBag(item.itemID);
            AddItemAtIndex(item.itemID,index,1);
            
            Log.D(GetItemDetails(item.itemID).itemID + "   " + GetItemDetails(item.itemID).itemName);
            if (isDestroy)
            {
                Destroy(item.gameObject);
            }
        }

        /// <summary>
        /// 检查背包是否有空位
        /// </summary>
        /// <returns></returns>
        private bool CheckBagCapacity()
        {
            for (int i = 0; i < playerBag.itemList.Count; i++)
            {
                if (playerBag.itemList[i].itemID == 0)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// 通过物品ID找到背包已有物品位置
        /// </summary>
        /// <param name="ID">物品ID</param>
        /// <returns>-1则没有这个物品否则返回序号</returns>
        private int GetItemIndexInBag(int ID)
        {
            for (int i = 0; i < playerBag.itemList.Count; i++)
            {
                if (playerBag.itemList[i].itemID == ID)
                    return i;
            }

            return -1;
        }

        /// <summary>
        /// 在指定背包序号位置添加物品
        /// </summary>
        /// <param name="ID">物品ID</param>
        /// <param name="index">序号</param>
        /// <param name="amount">数量</param>
        private void AddItemAtIndex(int ID, int index, int amount)
        {
            if (index == -1 && CheckBagCapacity()) //背包没有这个物品 同时背包有空位
            {
                var item = new InventoryItem { itemID = ID, itemAmount = amount };
                for (int i = 0; i < playerBag.itemList.Count; i++)
                {
                    if (playerBag.itemList[i].itemID == 0)
                    {
                        playerBag.itemList[i] = item;
                        break;
                    }
                }
            }
            else //背包有这个物品
            {
                int curAmount = playerBag.itemList[index].itemAmount + amount;
                var item = new InventoryItem { itemID = ID, itemAmount = curAmount };
                playerBag.itemList[index] = item;
            }
        }

        /// <summary> 
        /// Player背包范围内交换物品
        /// </summary>
        /// <param name="fromIndex">起始序号</param>
        /// <param name="targetIndex">目标数据序号</param>
        public void SwapItem(int fromIndex, int targetIndex)
        {
            InventoryItem currentItem = playerBag.itemList[fromIndex];
            InventoryItem targetItem = playerBag.itemList[targetIndex];

            if (targetItem.itemID != 0)
            {
                playerBag.itemList[fromIndex] = targetItem;
                playerBag.itemList[targetIndex] = currentItem;
            }
            else
            {
                playerBag.itemList[targetIndex] = currentItem;
                playerBag.itemList[fromIndex] = new InventoryItem();
            }
        }


        /// <summary>
        /// 排序
        /// </summary>
        public void SortItemList()
        {
            // playerBag = playerBag.itemList.OrderBy(i => i.itemID).ToList();
        }


        /// <summary>
        /// 移除指定数量的背包物品
        /// </summary>
        /// <param name="ID">物品ID</param>
        /// <param name="removeAmount">数量</param>
        private void RemoveItem(int ID, int removeAmount)
        {
            var index = GetItemIndexInBag(ID);

            if (playerBag.itemList[index].itemAmount > removeAmount)
            {
                var amount = playerBag.itemList[index].itemAmount - removeAmount;
                var item = new InventoryItem { itemID = ID, itemAmount = amount };
                playerBag.itemList[index] = item;
            }
            else if (playerBag.itemList[index].itemAmount == removeAmount)
            {
                var item = new InventoryItem();
                playerBag.itemList[index] = item;
            }
        }

        #region Other

        /// <summary>
        /// Item 能根据图片尺寸修改碰撞体检测范围
        /// </summary>
        /// <param name="sr"></param>
        /// <param name="coll"></param>
        public void MatchBoxSize2D(SpriteRenderer sr, BoxCollider2D coll)
        {
            //修改碰撞体尺寸
            var sprite = sr.sprite;
            Vector2 newSize = new Vector2(sprite.bounds.size.x, sprite.bounds.size.y);
            coll.size = newSize;
            coll.offset = new Vector2(0, sprite.bounds.center.y);
        }

        #endregion
    }
}