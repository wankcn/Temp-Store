using System;
using System.Collections;
using System.Collections.Generic;
using GameNeon.Datas.Event;
using GameNeon.Frameworks;
using GameNeon.Managers;
using GameNeon.Modules.InventoryModule;
using GameNeon.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GameNeon
{
    public class ItemCellView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerUpHandler,
        IPointerDownHandler
    {
        public GameObject container;
        [HideInInspector] public int index = 0;
        [HideInInspector] public int slotType;
        public Image itemIcon;
        public Image outLineImg;
        public Image qualityImg;
        public Button itemBtn;
        public TMP_Text itemNums;

        [HideInInspector] public int count; // 特殊用，用于缓存列表count

        // 标记缩放
        private Vector3 srcScale;
        public float dragRate = 1.05f;
        public float clickRate = 1.1f;
        public ItemDetails itemDetails;
        private bool isOnPointer = false;


        private void Start()
        {
            // *************** 一个奇怪的bug，生成列表后，其中几项的物体被旋转了，很奇怪，这里强制修改 rotation
            // transform.parent.localEulerAngles = new Vector3(0.0f, 0.0f, 0.0f);
            transform.localScale = new Vector3(1, 1, 1);

            GameEventManager.Instance.AddListener<int, int>(EventID.USE_BAG_ITEM, UseItemListener);
            outLineImg.gameObject.SetActive(false);
            srcScale = transform.localScale;
            itemBtn.onClick.AddListener(itemBtnCallBack);
        }

        public void SetData(int dataIndex, ItemDetails data)
        {
            itemDetails = data;
            index = dataIndex;
            count = 27;
            SetItemInfo();
            Log.D($"debug in 【ItemCellView】 {dataIndex} {data.ID}-{data.amount}-{data.name}");
        }


        private void Update()
        {
            // 先用Update 后面改为工程中的事件系统
            // 非选中，取消勾选框，
            if (index != PlayerBagDataManager.Instance.currentSelectIndex)
            {
                outLineImg.gameObject.SetActive(false);
                // 非选择下过渡回原始比例
                if (!isOnPointer)
                {
                    InventoryUtil.DGScaleObject(transform, srcScale);
                }
            }
            // 如果是选中状态，设置为clickRate比例，并且显示勾选框
            else
            {
                transform.localScale = srcScale * clickRate;
                outLineImg.gameObject.SetActive(true);
            }
        }

        #region 点击事件逻辑

        private void UseItemListener(int itemID, int amount)
        {
            if (itemDetails.ID == itemID)
            {
                int curNum = Convert.ToInt32(itemNums.text);
                curNum -= amount;
                if (curNum <= 0)
                {
                    Log.D($"{itemID} 的数量已经为0！刷新列表！");
                    GameEventManager.Instance.BroadCast(EventID.BAG_REMOVE_ITEM, itemID);
                    GameEventManager.Instance.BroadCast(EventID.REFRESH_BAG_TAG, slotType);
                }
                else
                {
                    itemNums.text = curNum.ToString();
                }
            }
        }


        public void SetItemInfo()
        {
            // itemNums.gameObject.SetActive(index < count);
            // itemIcon.gameObject.SetActive(index < count);
            if (index < count)
            {
                itemNums.text = itemDetails.amount.ToString();
                InventoryUtil.LoadIcon(itemIcon, itemDetails.ID);
                InventoryUtil.LoadQualityBox(qualityImg, itemDetails.quality);
            }
            else
            {
                InventoryUtil.LoadQualityBox(qualityImg, 0);
            }
        }


        private void itemBtnCallBack()
        {
            if (index >= count) return;
            Log.D($"当前点击Item {index}:{itemDetails.ID}");
            PlayerBagDataManager.Instance.currentSelectIndex = index;
            PlayerBagDataManager.Instance.currentSelectItemId = itemDetails.ID;
            GameEventManager.Instance.BroadCast(EventID.REFRESH_BAG_UI_INFO, itemDetails);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (index >= count) return;
            isOnPointer = true;
            // 如果非选中 扩大dragRate 旋转下什么也不干
            if (index != PlayerBagDataManager.Instance.currentSelectIndex)
            {
                InventoryUtil.DGScaleObject(transform, dragRate);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (index >= count) return;
            isOnPointer = false;
            // 非选中，缩回比例，选中什么也不干
            if (index != PlayerBagDataManager.Instance.currentSelectIndex)
            {
                InventoryUtil.DGScaleObject(transform, srcScale);
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (index >= count) return;
            InventoryUtil.DGScaleObject(transform, clickRate, srcScale);
            outLineImg.gameObject.SetActive(true);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            transform.localScale = srcScale * clickRate;
        }

        #endregion
    }
}