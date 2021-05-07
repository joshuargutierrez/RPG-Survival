using Invector.vItemManager;
using System.Collections.Generic;
using UnityEngine;

namespace CBGames.Core {
    [CreateAssetMenu(menuName = "CB Games/Scene Database", fileName = "ScenesDatabase")]
    public class SceneDatabase : ScriptableObject
    {
        [Tooltip("Scriptable object. Parameter that is used by many Core objects " +
            "like UICoreLogic, Chatbox, and NetworkManager. The has a list of all the " +
            "unity scenes and transition points within those scenes.")]
        public List<DatabaseScene> storedScenesData;
    }
    [System.Serializable]
    public class ItemWrapper
    {
        public List<ItemReference> items = new List<ItemReference>();

        public ItemWrapper(List<ItemReference> inputItems)
        {
            this.items = inputItems;
        }
    }
}