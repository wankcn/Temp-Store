using System.Collections;
using System.Collections.Generic;
using GameNeon.Datas.Event;
using GameNeon.Frameworks;
using GameNeon.Managers;
using GameNeon.Modules.CharacterModule;
using GameNeon.Modules.InventoryModule;
using GameNeon.VOS;
using GameNeon.VOS.Inventory;
using GameNeon.VOS.Inventory.Sample;
using UnityEngine;

namespace Wenruo
{
    public class InventoryManager : MonoSingleton<InventoryManager>
    {
        public string currentCharacterID = "chara_0001";
        public int currentBagID = 990001;
        public ItemContainer currentCharacterData;
        public ItemTB itemVO;

        protected override void Init()
        {
            base.Init();
            itemVO = DataManager.Instance.GetVOData<ItemTB>("ItemData");
            currentCharacterData = new ItemContainer(currentCharacterID);


            Log.D($"---------- Debug In InventoryMgr！----------");
            // 加载本地数据
            GameEventManager.Instance.AddListener<CharacterModel>(EventID.CHANGE_CURRENT_CHARACTER,
                ChangeCharacterListener);
        }

        private void ChangeCharacterListener(CharacterModel model)
        {
            currentCharacterID = model.Id;
            currentBagID = GetCharacterBagID(currentCharacterID);
            // 根据ID加载Item数据  
            currentCharacterData = new ItemContainer(currentCharacterID);
        }


        #region 外部调用接口

        /// 获得某一角色的bagID
        public int GetCharacterBagID(string characterID)
        {
            var characterVO = DataManager.Instance.GetVOData<CharacterConfigTB>("CharacterConfig");
            if (characterVO.HasVO(characterID))
                return characterVO[characterID].BagID;
            return -1;
        }

        /// <summary>
        /// 获得一组物品信息，包括Id和数量
        /// </summary>
        /// <param name="itemID"></param>
        /// <returns></returns>
        public InventoryItem GetItemData(int itemID)
        {
            return currentCharacterData.GetItemData(itemID);
        }

        /// <summary>
        /// 或者物品的数量
        /// </summary>
        /// <param name="itemID"></param>
        /// <returns></returns>
        public int GetItemAmount(int itemID)
        {
            return GetItemData(itemID).itemAmount;
        }

        /// <summary>
        /// 获取物品详情信息
        /// </summary>
        /// <param name="itemID"></param>
        /// <returns></returns>
        public ItemDetails GetItemDetails(int itemID)
        {
            return new ItemDetails(GetItemData(itemID));
        }

        /// <summary>
        /// 获取物品数据结构
        /// </summary>
        /// <param name="itemID"></param>
        /// <returns></returns>
        public Item GetItem(int itemID)
        {
            return itemVO[itemID];
        }

        #endregion


        #region Save / Load / Delete

        public void SaveData()
        {
            Log.D("== Inventory SaveData Successful ==");
            currentCharacterData.SaveData(currentCharacterID);
        }

        public void LoadData()
        {
            currentCharacterData = new ItemContainer(currentCharacterID);
        }

        public void DeleteData()
        {
            LocalSaveManager.DeleteKey(currentCharacterID + "_InventoryData");
        }

        #endregion


        #region ### 角色改动之前的旧数据源 怕以后还有改动，先不删

        // Dic<角色ID，角色物品数据>
        private Dictionary<string, ItemContainer> itemDataMap = new Dictionary<string, ItemContainer>();

        private void LoadInventoryData(string key)
        {
            itemDataMap = (Dictionary<string, ItemContainer>)LocalSaveManager.Load(key);
            if (itemDataMap == null) InitInventoryData();
        }

        public ItemContainer GetInventoryData()
        {
            if (!itemDataMap.ContainsKey(currentCharacterID))
                DataLoader(currentCharacterID, currentBagID);
            return itemDataMap[currentCharacterID];
        }

        public void UpdateInventoryData(string characterID)
        {
            itemDataMap[characterID].SaveData(characterID);
        }

        private void InitInventoryData()
        {
            itemDataMap.Clear();
            var characterVOList = DataManager.Instance.GetVOData<CharacterConfigTB>("CharacterConfig").DataList;
            foreach (var n in characterVOList)
            {
                string characterID = n.Id;
                int characterBagID = n.BagID;
                DataLoader(characterID, characterBagID);
            }
        }

        private void DataLoader(string characterID, int bagID)
        {
            List<InventoryItem> tmpInfo = new List<InventoryItem>();

            // 拿到当前BagID
            var bagVO = DataManager.Instance.GetVOData<BagConfigTB>("BagConfig");
            var bagIDs = bagVO[bagID].ItemInit;
            var bagNums = bagVO[bagID].ItemNumInit;
            if (bagIDs.Count == bagNums.Count)
            {
                for (int i = 0; i < bagIDs.Count; i++)
                    tmpInfo.Add(new InventoryItem(bagIDs[i], bagNums[i]));
            }
            else
            {
                Log.D($"策划表异常！！！请检查初始背包ID和数量列表长度是否相等！");
            }

            var initData = new ItemContainer(tmpInfo);
            itemDataMap.Add(characterID, initData);
        }

        #endregion
    }
}