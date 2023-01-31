using UnityEngine;

namespace DatabaseSystem.ScriptableObjects
{
    [CreateAssetMenu(menuName = "Demo/Items/Item Data")]
    public class ItemData : ScriptableObject
    {
        #region Variables
        [SerializeField] private int id = default;
        [SerializeField] private string displayName = default;
        [SerializeField] private int levelRequired = 1;
        [SerializeField] private Sprite icon = default;

        public int Id
        {
            get
            {
                return id;
            }
        }
        public string DisplayName
        {
            get
            {
                return displayName;
            }
        }
        public int LevelRequired
        {
            get
            {
                return levelRequired;
            }
        }
        public Sprite Icon
        {
            get
            {
                return icon;
            }
        }
        #endregion
    }
}
