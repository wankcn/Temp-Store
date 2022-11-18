using GameNeon.Utils;
using GameNeon.VOS.Inventory;
using UnityEngine;

namespace GameNeon.Modules.InventoryModule
{
    [System.Serializable]
    public class ItemDetails
    {
        public int itemID;
        public ItemType itemType;
        public bool showPoint;
        public string itemName;
        public Sprite itemIcon;
        public Sprite itemOnWorldSprite;
        public string itemDescription;
        public int itemUseRadius;
        public bool canPickUp;
        public bool canDrop;
        public bool canCarry;
        public int itemPrice;
        [Range(0, 1)] public float sellPercentage;

        /// <summary>
        /// 物品信息描述
        /// 当前该数据来源于客户端Excel表，后续服务器的信息描述在这个类统一描述
        /// </summary>
        /// <param name="data"></param>
        public ItemDetails(Item data)
        {
            // 先临时加三个数据进行测试
            itemID = data.ItemID;
            itemName = data.ItemName;
            itemDescription = data.DetailDesc;
            
        }
    }
}