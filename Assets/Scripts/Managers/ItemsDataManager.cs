using DatabaseSystem.ScriptableObjects;
using System.Collections.Generic;
using UnityEngine;

namespace DatabaseSystem.Managers
{
    public class ItemsDataManager : DataManager<int, ItemData>
    {
        #region Variables
        [SerializeField] private string resourcesItemsFolder = default;
        #endregion

        #region Private Methods
        private void LoadFromResources()
        {
            dataDictionary = new Dictionary<int, ItemData>();
            ItemData[] itemsFromResources = Resources.LoadAll<ItemData>(resourcesItemsFolder);
            foreach (var itemData in itemsFromResources)
            {
                TryPutDataItem(itemData.Id, itemData);
            }
        }
        #endregion

        #region Public Methods
        public void Initialize()
        {
            LoadFromResources();
        }
        #endregion
    }
}