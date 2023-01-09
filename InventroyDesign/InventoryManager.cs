using System;
using System.Collections;
using System.Collections.Generic;
using GameNeon.Datas.Event;
using GameNeon.Frameworks;
using GameNeon.Modules.CharacterModule;
using GameNeon.Modules.InventoryModule;
using GameNeon.Modules.TriggerEventModule;
using GameNeon.VOS;
using GameNeon.VOS.Inventory;
using GameNeon.VOS.Character;
using UnityEngine;

namespace GameNeon.Managers
{
    public class InventoryManager : MonoSingleton<InventoryManager>
    {
        public string currentCharacterID = "chara_0001";
        public int currentBagID = 990001;
        private float currentCharacterTimeMoney = 0;
        private ItemContainer currentCharacterData;
        public ItemTB itemVO;

        protected override void Init()
        {
            base.Init();
            itemVO = DataManager.Instance.GetVOData<ItemTB>("ItemData");
            LoadInventoryData();
            LoadCharacterCurrentTimeMoney();

            Log.D($"---------- Debug In InventoryMgr！----------");
            // 加载本地数据
            GameEventManager.Instance.AddListener<CharacterModel>(EventID.CHANGE_CURRENT_CHARACTER,
                SwitchCharacterListener);
            GameEventManager.Instance.AddListener<int, int>(EventID.BUY_ITEM, BuyItemListener);
        }


        #region 事件中心

        private void SwitchCharacterListener(CharacterModel model)
        {
            currentCharacterID = model.Id;
            currentBagID = GetCharacterBagID(currentCharacterID);

            // 更新金钱和物品数据
            LoadInventoryData();
            LoadCharacterCurrentTimeMoney();
        }

        private void BuyItemListener(int itemID, int amount)
        {
            // 添加物品
            AddItemAmount(itemID, amount);

            // 消费金币
            int cost = amount * 15;
            CostTimeMoney(cost);
            ShopDataManager.Instance.currentShopPurchasedData.AddItemAmount(itemID, amount);
        }

        private void SellItemListener()
        {
            // 售出所有物品，这些物品需要从inventory中减去
            var list = ShopDataManager.Instance.GetSellList();
            foreach (var n in list)
            {
                currentCharacterData.RemoveItem(n.itemID, n.itemAmount);
            }
        }

        #endregion


        #region 物品管理外部调用接口

        /// 添加指定数量的物品
        public void AddItemAmount(int ID, int amount)
        {
            currentCharacterData.AddItemAmount(ID, amount);

            EventMessage msg = new EventMessage();
            msg.AddInt("ItemId", ID);
            msg.AddInt("Count", amount);
            TriggerEventManager.Instance.Trigger(TriggerEventType.GET_ITEM, msg);
        }

        /// 移除指定数量的物品
        public int RemoveItem(int ID, int removeAmount)
        {
            int count = currentCharacterData.RemoveItem(ID, removeAmount);

            EventMessage msg = new EventMessage();
            msg.AddInt("ItemId", ID);
            msg.AddInt("Count", count);
            TriggerEventManager.Instance.Trigger(TriggerEventType.COST_ITEM, msg);

            return count;
        }

        /// 彻底删除某物品
        public int DeleteItem(int ID)
        {
            int removeAmount = GetItemData(ID).itemAmount;
            return RemoveItem(ID, removeAmount);
        }

        /// 是否用拥有某物品
        public bool IsHasItem(int itemID)
        {
            return GetItemAmount(itemID) != 0;
        }

        /// 获得物品的数量
        public int GetItemAmount(int itemID)
        {
            return GetItemData(itemID).itemAmount;
        }

        /// 获取已拥有物品详情信息 用于已有物品的UI显示
        public ItemDetails GetItemDetails(int itemID)
        {
            return new ItemDetails(GetItemData(itemID));
        }

        /// 获取物品数据结构信息 不包含该物品数量
        public Item GetItem(int itemID)
        {
            return itemVO[itemID];
        }

        /// 获得物品质量
        public EItemQuality GetItemQuality(int itemID)
        {
            return (EItemQuality)itemVO[itemID].Quality;
        }

        public int GetItemQualityNum(int itemID)
        {
            return itemVO[itemID].Quality;
        }

        /// 获得原始数据容器，一般用于debug或编辑器
        public ItemContainer GetInventoryData()
        {
            return currentCharacterData;
        }

        /// 获得物品ID列表  
        public Dictionary<int, int>.KeyCollection GetItemIDList()
        {
            return currentCharacterData.GetCheckListKeys();
        }

        /// 获得一组物品信息，包括Id和数量
        private InventoryItem GetItemData(int itemID)
        {
            return currentCharacterData.GetItemData(itemID);
        }

        #endregion


        #region TimeMoney 管理

        public float GetTimeMoney()
        {
            return currentCharacterTimeMoney;
        }

        public void AddTimeMoney(float money)
        {
            currentCharacterTimeMoney += money;
            GameEventManager.Instance.BroadCast(EventID.REFRESH_TIME_MONEY, currentCharacterTimeMoney);
        }

        public void CostTimeMoney(float money)
        {
            currentCharacterTimeMoney -= money;
            // TODO 金钱<0 游戏失败
            GameEventManager.Instance.BroadCast(EventID.REFRESH_TIME_MONEY, currentCharacterTimeMoney);
        }

        /// 当前金钱是为为负债
        public bool isBeInDebt()
        {
            return currentCharacterTimeMoney < 0;
        }

        public void SaveTimeMoney()
        {
            LocalSaveManager.Save($"{currentCharacterID}_TimeMoney", currentCharacterTimeMoney);
        }

        private void LoadCharacterCurrentTimeMoney()
        {
            string loadMoneyKey = $"{currentCharacterID}_TimeMoney";
            if (LocalSaveManager.KeyExists(loadMoneyKey))
                currentCharacterTimeMoney = (float)LocalSaveManager.Load(loadMoneyKey);
            else
                currentCharacterTimeMoney = GetInitTimeMoney(currentBagID);
        }

        #endregion


        #region InventoryItem管理 Save / Load / Delete

        public void SaveData()
        {
            SaveTimeMoney();
            currentCharacterData.SaveData(currentCharacterID);
            Log.D("== Inventory SaveData Successful ==");
        }

        private void LoadInventoryData()
        {
            currentCharacterData = new ItemContainer(currentCharacterID);
            if (currentCharacterData.IsNoneData())
            {
                // 根据角色iD拿当前角色的背包ID
                var bagId = GetCharacterBagID(currentCharacterID);
                var data = DataLoader(bagId);
                currentCharacterData = new ItemContainer(data);
            }
        }

        private List<InventoryItem> DataLoader(int bagID)
        {
            List<InventoryItem> list = new List<InventoryItem>();
            var bagVO = DataManager.Instance.GetVOData<BagConfigTB>();
            if (!bagVO.HasVO(bagID))
            {
                throw new Exception($"BagConfigTB 表不存在ID为 {bagID} 的数据！");
            }

            // 存在BagID 进行数据初始化
            var bagIDs = bagVO[bagID].ItemInit;
            var bagNums = bagVO[bagID].ItemNumInit;

            if (bagIDs.Count == bagNums.Count)
            {
                for (int i = 0; i < bagIDs.Count; i++)
                    list.Add(new InventoryItem(bagIDs[i], bagNums[i]));
            }
            else
            {
                throw new Exception($"BagConfigTB表{bagID}配置IDList与NumList长度不相等！");
            }

            return list;
        }

        public void DeleteData()
        {
            LocalSaveManager.DeleteKey(currentCharacterID + "_InventoryData");
        }

        #endregion


        #region Other Tool

        /// 通过BagId设置默认时间货币
        private int GetInitTimeMoney(int bagID)
        {
            var bagVO = DataManager.Instance.GetVOData<BagConfigTB>();
            if (bagVO.HasVO(bagID))
                return bagVO[bagID].MoneyInit;
            return 0;
        }


        /// 获得某一角色的bagID
        private int GetCharacterBagID(string characterID)
        {
            var characterVO = DataManager.Instance.GetVOData<CharacterConfigTB>("CharacterConfig");
            if (characterVO.HasVO(characterID))
                return characterVO[characterID].BagID;
            return -1;
        }

        #endregion
    }
}