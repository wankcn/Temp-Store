using System;
using System.Collections;
using System.Collections.Generic;
using GameNeon.Frameworks;
using UnityEngine;
using GameNeon.Managers;

namespace GameNeon.Modules.InventoryModule
{
    public class InvItem : MonoBehaviour
    {
        public int itemID;
        private SpriteRenderer sr;

        private ItemDetails _itemDetails;

        private void Awake()
        {
            sr = GetComponentInChildren<SpriteRenderer>();
        }

        private void Start()
        {
            if (itemID != 0)
            {
                Init(itemID);
            }
        }

        private void Init(int ID)
        {
            itemID = ID;
            _itemDetails = InventoryManager.Instance.GetItemDetails(itemID);
            if (_itemDetails != null)
            {
                sr.sprite = _itemDetails.itemOnWorldSprite != null
                    ? _itemDetails.itemOnWorldSprite
                    : _itemDetails.itemIcon;
            }

            if (_itemDetails != null) Log.D("InvItem Init" + _itemDetails.itemDescription);
        }
    }
}