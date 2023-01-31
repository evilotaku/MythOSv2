using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DatabaseSystem.Managers
{
    public abstract class DataManager<T, D> : MonoBehaviour
    {
        #region Variables
        protected Dictionary<T, D> dataDictionary = default;
        #endregion

        #region Public Methods
        public Dictionary<T, D> GetAllDataObjects()
        {
            return dataDictionary.ToDictionary(k => k.Key, v => v.Value);
        }
        public D GetDataObject(T id)
        {
            if(dataDictionary.ContainsKey(id))
            {
                return dataDictionary[id];
            }

            return default;
        }
        #endregion

        #region Protected Methods
        protected virtual bool TryPutDataItem(T id, D data)
        {
            if (dataDictionary.ContainsKey(id))
            {
                return false;
            }

            dataDictionary.Add(id, data);
            return true;
        }
        protected virtual bool TryRemoveItem(T id, D data)
        {
            if (!dataDictionary.ContainsKey(id))
            {
                return false;
            }

            dataDictionary.Remove(id);
            return true;
        }
        #endregion
    }
}
