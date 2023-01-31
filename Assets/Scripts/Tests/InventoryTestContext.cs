using DatabaseSystem.Managers;
using UnityEngine;

namespace DatabaseSystem.Tests
{
    public class InventoryTestContext : MonoBehaviour
    {
        #region Variables
        [SerializeField] private ItemsDataManager itemsDataManager;
        #endregion

        #region Unity Methods
        private void Awake()
        {
            itemsDataManager.Initialize();
        }

        private void Start()
        {
            foreach (var item in itemsDataManager.GetAllDataObjects())
            {
                Debug.Log($"Added new item to backpack - id:[{item.Key}] name:[{item.Value.DisplayName}]");
            }
        }
        #endregion
    }
}