using System.Collections.Generic;
using GameNeon.Frameworks;
using GameNeon.Managers;
using GameNeon.VOS.Inventory;


namespace GameNeon.Modules.InventoryModule
{
    /// <summary>
    /// 重新定义物品数据结构，item容器为id和amount的格式
    /// </summary>
    public class ItemContainer
    {
        private const string m_key = "_InventoryData";
        private List<InventoryItem> m_List;
        private Dictionary<int, int> checkList;

        // 记录空数据的index
        private List<int> isNullIndexList;

        public ItemContainer()
        {
            // 分配空间并加载数据
            AllocateSpace();
        }

        public ItemContainer(string loadID)
        {
            // 分配空间并加载数据
            AllocateSpace();
            m_List = LoadData(loadID);
            if (m_List != null)
            {
                // 构建检索表
                int index = 0;
                foreach (var item in m_List) checkList.Add(item.itemID, index++);
            }
            else
            {
                m_List = new List<InventoryItem>();
            }
        }

        // 一个用于测试的构造函数
        public ItemContainer(List<InventoryItem> dataList)
        {
            AllocateSpace();
            if (dataList.Count != 0)
            {
                m_List = dataList;
                // 构建搜索表
                int index = 0;
                foreach (var item in m_List) checkList.Add(item.itemID, index++);
            }
        }

        #region 将数据以角色为单位进行存储 Save / Load

        private List<InventoryItem> LoadData(string id)
        {
            // 对这里进行修改！！
            // 加载数据改为加载存档，如果没有返回空，外部自己加载预设，不在这个类做处理
            if (!LocalSaveManager.KeyExists(id + m_key)) return null;
            return (List<InventoryItem>)LocalSaveManager.Load(id + m_key);
        }


        public bool IsNoneData()
        {
            return m_List == null || m_List.Count == 0;
        }


        public void DeleteData(string id)
        {
            LocalSaveManager.DeleteKey(id + m_key);
        }

        public void SaveData(string id)
        {
            // 先缓存份数据，然后清空数据结构
            var resList = GetItemList();
            LocalSaveManager.Save(id + m_key, resList);
            // ClearData(); // 不需要清空避免手动保持后重新加载数据，数据在每次初始化会清空
        }

        #endregion


        /// <summary>
        /// 增加指定数量的物品
        /// </summary>
        /// <param name="ID">物品ID</param>
        /// <param name="amount">数量</param>
        public void AddItemAmount(int ID, int amount)
        {
            var index = GetIndexInListByID(ID);
            // 如果列表中没这个物品，列表中增加一个，并且增加一个检索表
            if (index == -1)
            {
                var item = new InventoryItem { itemID = ID, itemAmount = amount };

                // 返回空数据的首个index
                var isNoneIndex = GetListHasNullIndex();
                int addIndex = isNoneIndex != -1 ? isNoneIndex : m_List.Count;

                // 先处理索引，后处理list
                checkList.Add(ID, addIndex);
                // 当为-1时，说明不需要从index表中删除索引，或者addIndex==m_List.count时
                if (isNoneIndex != -1) isNullIndexList.Remove(isNoneIndex);
                // 如果有空的位置替换，没有追加
                if (addIndex < m_List.Count) m_List[isNoneIndex] = item;
                else m_List.Add(item);
            }
            else // 列表中有该物品
            {
                int currentAmount = m_List[index].itemAmount + amount;
                var item = new InventoryItem { itemID = ID, itemAmount = currentAmount };
                m_List[index] = item;
            }
        }

        /// <summary>
        /// 移除指定数量的物品
        /// </summary>
        /// <param name="ID">物品ID</param>
        /// <param name="removeAmount">数量</param>
        public int RemoveItem(int ID, int removeAmount)
        {
            // 需要删除的物品一定存在index 不能存在返回
            int index = GetIndexInListByID(ID);
            if (index == -1)
            {
                Log.D($"物品{ID}并不存在，执行失败");
                return -1;
            }

            // 获得实际删除的数值
            int real = m_List[index].itemAmount >= removeAmount ? removeAmount : m_List[index].itemAmount;
            // 如果物品还有剩余，修改list内容
            if (m_List[index].itemAmount > removeAmount)
            {
                int currentAmount = m_List[index].itemAmount - removeAmount;
                var item = new InventoryItem { itemID = ID, itemAmount = currentAmount };
                m_List[index] = item;
            }
            else if (m_List[index].itemAmount == removeAmount)
            {
                // 此时物品数量为0，将数据置空，也可以不做修改。
                m_List[index] = new InventoryItem { itemID = 0, itemAmount = 0 };
                // 将物品ID从checkList中删除
                checkList.Remove(ID);
                // 记录删除位置的index，用于下次新增覆盖
                isNullIndexList.Add(index);
            }
            else
            {
                var residue = removeAmount - m_List[index].itemAmount;
                m_List[index] = new InventoryItem { itemID = 0, itemAmount = 0 };
                // 将物品ID从checkList中删除
                checkList.Remove(ID);
                // 记录删除位置的index，用于下次新增覆盖
                isNullIndexList.Add(index);

                Log.D($"减去超载{residue}" !);
            }

            return real;
        }

        /// <summary>
        /// 通过ID直接删除某一物品
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public int RemoveItemByID(int ID)
        {
            int index = GetIndexInListByID(ID);
            if (index == -1)
            {
                Log.D($"物品{ID}并不存在，执行失败");
                return -1;
            }

            int real = m_List[index].itemAmount;
            m_List[index] = new InventoryItem { itemID = 0, itemAmount = 0 };
            // 将物品ID从checkList中删除
            checkList.Remove(ID);
            // 记录删除位置的index，用于下次新增覆盖
            isNullIndexList.Add(index);
            return real;
        }

        public void RemoveAllItem()
        {
            ClearData();
        }


        /// <summary>
        /// 通过物品ID找到物品在list中的位置
        /// </summary>
        /// <param name="ID">物品ID</param>
        /// <returns>-1则没有这个物品否则返回序号</returns>
        private int GetIndexInListByID(int ID)
        {
            return checkList.ContainsKey(ID) ? checkList[ID] : -1;
        }

        /// 返回一个可用于添加的位置
        private int GetListHasNullIndex()
        {
            return isNullIndexList.Count != 0 ? isNullIndexList[0] : -1;
        }

        /// <summary>
        /// 获得真实物品列表 不包含空数据
        /// </summary>
        /// <returns></returns>
        public List<InventoryItem> GetItemList()
        {
            List<InventoryItem> resList = new List<InventoryItem>();
            // 如果置空表里面存在数据，则从list里剔除
            foreach (var n in checkList)
            {
                resList.Add(m_List[n.Value]);
            }

            return resList;
        }

        public bool IsHasItem(int itemID)
        {
            return checkList.ContainsKey(itemID);
        }

        public Dictionary<int, int>.KeyCollection GetCheckListKeys()
        {
            return checkList.Keys;
        }

        public InventoryItem GetItemData(int itemID)
        {
            if (checkList.ContainsKey(itemID))
            {
                var index = checkList[itemID];
                return m_List[index];
            }

            return new InventoryItem(itemID, 0);
        }


        /// <summary>
        /// 获得假物品列表 含空数据
        /// </summary>
        /// <returns></returns>
        public List<InventoryItem> GetItemListToTest()
        {
            return m_List;
        }

        private void AllocateSpace()
        {
            m_List = new List<InventoryItem>();
            checkList = new Dictionary<int, int>();
            isNullIndexList = new List<int>();
        }

        private void ClearData()
        {
            m_List.Clear();
            checkList.Clear();
            isNullIndexList.Clear();
        }


        public override string ToString()
        {
            string str = "DataList \n";
            foreach (var n in m_List)
            {
                str += $"{n.itemID}:{n.itemAmount} / ";
            }

            str += "\n checkListMap\n";
            foreach (var n in checkList)
            {
                str += $"{n.Key}:{n.Value} / ";
            }

            str += "\n isNullIndex\n";
            foreach (var n in isNullIndexList)
            {
                str += $"{n} / ";
            }

            return str;
        }
    }
}