using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GameNeon.Frameworks;
using GameNeon.Modules.InventoryModule;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace GameNeon.Managers
{
    public class InventoryManager : MonoSingleton<InventoryManager>
    {
        private readonly string m_key = "ItemDataList";
        [Header("本地存储数据")] public DataContainer<InventoryItem> m_items;
        [Header("本地存储数据")] public List<InventoryItem> itemDataList;


        protected override void Init()
        {
            base.Init();
            Log.D("---------- InventoryManager Init ----------");
            m_items = new DataContainer<InventoryItem>();
            m_items.LoadInventoryData(m_key);

            // itemDataList = LocalSaveManager.KeyExists(m_key) ? LoadInventoryData() : new List<InventoryItem>();
        }

        /// <summary>
        /// 通过ID获取物品信息
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public ItemDetails? GetItemDetails(int ID)
        {
            return null;
        }

        /// <summary>
        /// 通过ID获取物品数据
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public InventoryItem? GetItemData(int ID)
        {
            if (GetItemIndex(ID) != -1)
            {
                return itemDataList[GetItemIndex(ID)];
            }

            return null;
        }

        /// <summary>
        /// 通过ID获取物品数量
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public int GetItemAmount(int ID)
        {
            return itemDataList.Find(i => i.itemID == ID).itemAmount;
        }

        /// <summary>
        /// 通过ID添加物品
        /// </summary>
        /// <param name="id"></param>
        /// <param name="isDestroy">是否销毁物品</param>
        public void AddItem(int id, bool isDestroy)
        {
            // 拿到该物品列表中的索引位置 不在则返回-1
            var index = GetItemIndex(id);
            AddItemAtIndex(id, index, 1);
        }

        /// <summary>
        /// 通过物品ID找到在当前列表中物品位置
        /// </summary>
        /// <param name="ID">物品ID</param>
        /// <returns>-1则没有这个物品否则返回序号</returns>
        private int GetItemIndex(int ID)
        {
            for (int i = 0; i < itemDataList.Count; i++)
            {
                if (itemDataList[i].itemID == ID)
                    return i;
            }

            return -1;
        }

        /// <summary>
        /// 检查item数据是否已经存在
        /// </summary>
        /// <param name="itemID"></param>
        /// <returns></returns>
        public bool IsHaveItem(int itemID)
        {
            for (int i = 0; i < itemDataList.Count; i++)
            {
                if (itemDataList[i].itemID == itemID)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// 以索引的方式添加指定物品
        /// </summary>
        /// <param name="ID">物品ID</param>
        /// <param name="index">序号</param>
        /// <param name="amount">数量</param>
        private void AddItemAtIndex(int ID, int index, int amount)
        {
            // -1 表示列表中没有该物品直接添加
            if (index == -1)
            {
                InventoryItem item = new InventoryItem { itemID = ID, itemAmount = amount };
                itemDataList.Add(item);
            }
            else
            {
                int curAmount = itemDataList[index].itemAmount + amount;
                InventoryItem item = new InventoryItem { itemID = ID, itemAmount = curAmount };
                itemDataList[index] = item;
            }
        }

        /// <summary>
        /// 通过id对物品排序
        /// </summary>
        public void SortItemList()
        {
            itemDataList = itemDataList.OrderBy(i => i.itemID).ToList();
        }

        /// <summary>
        /// 移除指定数量的物品
        /// </summary>
        /// <param name="ID">物品ID</param>
        /// <param name="removeAmount">数量</param>
        public void RemoveItemAmount(int ID, int removeAmount)
        {
            var index = GetItemIndex(ID);

            if (itemDataList[index].itemAmount > removeAmount)
            {
                var amount = itemDataList[index].itemAmount - removeAmount;
                var item = new InventoryItem { itemID = ID, itemAmount = amount };
                itemDataList[index] = item;
            }
            else if (itemDataList[index].itemAmount == removeAmount)
            {
                itemDataList.RemoveAt(index);
            }
        }

        /// <summary>
        /// 通过ID删除物品
        /// </summary>
        /// <param name="ID"></param>
        public void RemoveItem(int ID)
        {
            var index = GetItemIndex(ID);
            itemDataList.RemoveAt(index);
        }


        #region Save/Load

        public void SaveInventoryData()
        {
            LocalSaveManager.Save(m_key, itemDataList);
        }

        public List<InventoryItem> LoadInventoryData()
        {
            return m_items.LoadInventoryData(m_key);
        }

        public void ClearInventoryData()
        {
            itemDataList.Clear();
            LocalSaveManager.DeleteKey(m_key);
        }

        #endregion

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

        /// <summary>
        /// 方便测试，临时随机存放一组数据
        /// </summary>
        /// <param name="capacity"></param>
        private void RandomSaveBagTestData(int capacity)
        {
            List<InventoryItem> data = new List<InventoryItem>(capacity);
            // 数据初始化
            for (int i = 0; i < capacity; i++)
            {
                var tmp = new InventoryItem { itemID = 1000 + i, itemAmount = Random.Range(0, 15) };
                data.Add(tmp);
            }

            // 存放一组测试数据
            LocalSaveManager.Save(m_key, data);
        }

        #endregion
    }
}