using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Invector.vItemManager
{
    using Invector.vCharacterController;
    using Invector.vEventSystems;
    using vCharacterController.vActions;

    [vClassHeader("ItemManager")]
    public partial class vItemManager : vMonoBehaviour, IActionReceiver
    {
        #region Delegates

        /// <summary>
        /// Delegate to filter item usage
        /// </summary>
        /// <param name="item">target item</param>
        /// <param name="validationList">Last results of filter</param>
        public delegate void CanUseItemDelegate(vItem item, ref List<bool> validationList);

        #endregion

        #region Variables        

        [vHelpBox("Place a Inventory Prefab inside your Character, it will be auto-assigned when you enter Playmode." +
            "\nYou can find the prefab at the Inventory/Prefabs folder", vHelpBoxAttribute.MessageType.Info)]
        public vInventory inventory;

        [vHelpBox("You can find the default ItemListData at the Inventory/ItemListData folder, or create a new list at the Invector Menu", vHelpBoxAttribute.MessageType.Info)]
        public vItemListData itemListData;

        public List<ItemReference> startItems = new List<ItemReference>();
        public List<EquipPoint> equipPoints;
        public List<ApplyAttributeEvent> applyAttributeEvents;
        public bool debugMode = false;

        /// <summary>
        /// Events called when item is changed or used
        /// </summary>
        public OnHandleItemEvent onStartItemUsage, onUseItem, onUseItemFail, onAddItem, onChangeItemAmount;

        /// <summary>
        /// Events called when add or remove a itemID
        /// </summary>
        public OnHandleItemIDEvent onAddItemID, onRemoveItemID;

        /// <summary>
        /// Events called when item is Removed or Destroyed
        /// </summary>
        public OnChangeItemAmount onDestroyItem, onDropItem;

        /// <summary>
        /// Event called when inventory open or close
        /// </summary>
        public OnOpenCloseInventory onOpenCloseInventory;

        /// <summary>
        /// Events called in Equip or Unequip actions
        /// </summary>
        public OnChangeEquipmentEvent onEquipItem, onUnequipItem, onFinishEquipItem, onFinishUnequipItem;

        /// <summary>
        ///Event called when save inventory items using <seealso cref="SaveItemsExample"/>
        /// </summary>
        public UnityEngine.Events.UnityEvent onSaveItems;

        /// <summary>
        ///Event called when load inventory items using <seealso cref="LoadItemsExample"/>
        /// </summary>
        public UnityEngine.Events.UnityEvent onLoadItems;

        /// <summary>
        /// Equipments in EquipArea
        /// </summary>
        public Dictionary<vItem, vEquipment> equipments = new Dictionary<vItem, vEquipment>();

        protected GameObject equipmentContainer;
        internal List<vItem> items;
        internal bool inEquip;
        internal bool usingItem;

        private float equipTimer;
        private Animator animator;
        public vIAnimatorStateInfoController animatorStateInfos;

        [HideInInspector]
        public List<vItemType> itemsFilter = new List<vItemType>() { 0 };

        /// <summary>
        /// Event to control when use or not an item
        /// </summary>
        public event CanUseItemDelegate canUseItemDelegate;

        /// <summary>
        /// Control if it needs to play the animation to Equip or Use the item
        /// </summary>
        internal bool playItemAnimation
        {
            get
            {
                return (inventory != null && ((inventory.isOpen && inventory.playItemAnimation) || !inventory.isOpen)) && !temporarilyIgnoreItemAnimation;
            }
        }

        /// <summary>
        /// Temporarily ignore the <seealso cref="playItemAnimation"/>
        /// </summary>
        internal bool temporarilyIgnoreItemAnimation;

        #endregion

        IEnumerator Start()
        {
            // Finds a Inventory Prefab inside the character
            if (!inventory)
                inventory = transform.GetComponentInChildren<vInventory>();

            if (!inventory)
                if (debugMode) Debug.LogWarning("Missing Inventory prefab - You need to Drag and drop a Inventory Prefab inside the Character");

            if (inventory)
            {
                equipmentContainer = new GameObject("Equipment Container");
                equipmentContainer.transform.parent = transform;
                equipmentContainer.transform.localPosition = Vector3.zero;
                equipmentContainer.transform.localEulerAngles = Vector3.zero;

                // Initialize all Inventory Actions
                inventory.GetItemsHandler = GetItems;
                inventory.GetItemsAllHandler = GetAllItems;
                inventory.AddItemsHandler = AddItem;
                inventory.GetAllAmount = GetAllAmount;
                inventory.onEquipItem.AddListener(EquipItem);
                inventory.onUnequipItem.AddListener(UnequipItem);
                inventory.onDropItem.AddListener(DropItem);
                inventory.onDestroyItem.AddListener(DestroyItem);
                inventory.onUseItem.AddListener(UseItem);
                inventory.onOpenCloseInventory.AddListener(OnOpenCloseInventory);

                var melee = GetComponent<vMeleeCombatInput>();
                if (melee)
                    // Check the vMeleeCombatInput to see the conditions to lock the Inventory Input
                    inventory.IsLockedEvent = () => { return melee.lockInventory; };
            }

            // Access the Animator
            animator = GetComponent<Animator>();
            animatorStateInfos = GetComponent<vIAnimatorStateInfoController>();
            yield return new WaitForEndOfFrame();

            // Initialize Items 
            items = new List<vItem>();
            if (itemListData)
            {
                for (int i = 0; i < startItems.Count; i++)
                    AddItem(startItems[i], true);
            }
        }
        #region Generic Utils

        /// <summary>
        /// Use it to lock all the input from the vInventory
        /// </summary>
        /// <param name="value"></param>
        public virtual void LockInventoryInput(bool value)
        {
            if (inventory)
                inventory.lockInventoryInput = value;
        }

        /// <summary>
        /// Use it to trigger events when Open or Close the Inventory
        /// </summary>
        /// <param name="value"></param>
        protected virtual void OnOpenCloseInventory(bool value)
        {
            onOpenCloseInventory.Invoke(value);
        }

        /// <summary>
        /// This is just an example of saving the current items <seealso cref="vSaveLoadInventory"/>
        /// </summary>
        public void SaveItemsExample()
        {
            this.SaveInventory();
        }

        /// <summary>
        /// This is just an example of loading the saved items <seealso cref="vSaveLoadInventory"/>
        /// </summary>
        public void LoadItemsExample()
        {
            this.LoadInventory();
        }

        /// <summary>
        /// Check vAnimatorTags from the Animator
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        public virtual bool IsAnimatorTag(string tag)
        {
            if (animator == null) return false;
            if (animatorStateInfos.isValid())
            {
                if (animatorStateInfos.animatorStateInfos.HasTag(tag))
                {
                    return true;
                }
            }            
            return false;
        }

        #endregion

        #region Check Items        

        /// <summary>
        /// Check if the current Item is checked with the option <seealso cref="vItem.twoHandWeapon"/>, if so, it will Unequip the opposite hand
        /// </summary>
        /// <param name="equipPoint"></param>
        /// <param name="item"></param>
        protected virtual void CheckTwoHandItem(EquipPoint equipPoint, vItem item)
        {
            if (item == null) return;
            var opposite = equipPoints.Find(ePoint => ePoint.area != null && ePoint.equipPointName.Equals("LeftArm") && ePoint.area.currentEquippedItem != null);
            if (equipPoint.equipPointName.Equals("LeftArm"))
                opposite = equipPoints.Find(ePoint => ePoint.area != null && ePoint.equipPointName.Equals("RightArm") && ePoint.area.currentEquippedItem != null);
            else if (!equipPoint.equipPointName.Equals("RightArm"))
            {
                return;
            }
            if (opposite != null && (item.twoHandWeapon || opposite.area.currentEquippedItem.twoHandWeapon))
            {
                opposite.area.RemoveCurrentItem();
            }
        }

        /// <summary>
        /// Check if the Item List contains a Item ID
        /// </summary>
        /// <param name="id">Item id</param>
        /// <returns></returns>
        public virtual bool ContainItem(int id)
        {
            return items.Exists(i => i.id == id);
        }

        /// <summary>
        /// Check if the Item List contains a Item Name
        /// </summary>
        /// <param name="itemName">Item name</param>
        /// <returns></returns>
        public virtual bool ContainItem(string itemName)
        {
            return items.Exists(i => i.name == itemName);
        }

        /// <summary>
        /// Check if the list contains a item with a certain amount, or more
        /// </summary>
        /// <param name="id">Item id</param>
        /// <param name="amount">Item amount</param>
        /// <returns></returns>
        public virtual bool ContainItem(int id, int amount)
        {
            return GetAllAmount(id) >= amount;
        }

        /// <summary>
        /// Check if the list contains a item name with certain amount, or more
        /// </summary>
        /// <param name="itemName">Item name</param>
        /// <param name="amount">Item amount</param>
        /// <returns></returns>
        public virtual bool ContainItem(string itemName, int amount)
        {
            var item = items.Find(i => i.name == itemName && i.amount >= amount);
            return item != null ? GetAllAmount(item.id) >= amount : false;
        }

        /// <summary>
        /// Check if a EquipArea contains any item equipped
        /// <seealso cref="vInventory.equipAreas"/>
        /// </summary>
        /// <param name="indexOfArea">index of equip area</param>
        /// <returns></returns>
        public virtual bool EquipAreaHasSomeItem(int indexOfArea)
        {
            var equipArea = inventory.equipAreas[indexOfArea];
            return equipArea.equipSlots.Exists(slot => slot.item != null);
        }

        /// <summary>
        /// Check if a specific Item ID is equipped on any EquipArea
        /// <seealso cref="vInventory.equipAreas"/>
        /// </summary>
        /// <param name="id">Item id</param>
        /// <returns></returns>
        public virtual bool ItemIsInSomeEquipArea(int id)
        {
            if (!inventory || inventory.equipAreas.Length == 0) return false;

            for (int i = 0; i < inventory.equipAreas.Length; i++)
            {
                var equipArea = inventory.equipAreas[i];
                if (equipArea.equipSlots.Exists(slot => slot.item.id.Equals(id)))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Check if a specific Item Name is equipped on any EquipArea
        /// <seealso cref="vInventory.equipAreas"/>
        /// </summary>
        /// <param name="itemName">Item name</param>
        /// <returns></returns>
        public virtual bool ItemIsInSomeEquipArea(string itemName)
        {
            if (!inventory || inventory.equipAreas.Length == 0) return false;

            for (int i = 0; i < inventory.equipAreas.Length; i++)
            {
                var equipArea = inventory.equipAreas[i];
                if (equipArea.equipSlots.Exists(slot => slot.item.name.Equals(itemName)))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Check if a specific Item ID is equipped on a specific EquipArea
        /// <seealso cref="vInventory.equipAreas"/>
        /// </summary>
        /// <param name="id">Item id</param>
        /// <param name="indexOfArea">index of equip area </param>        
        /// <returns></returns>
        public virtual bool ItemIsInSpecificEquipArea(int id, int indexOfArea)
        {
            if (!inventory || inventory.equipAreas.Length == 0 || indexOfArea > inventory.equipAreas.Length - 1) return false;
            var equipArea = inventory.equipAreas[indexOfArea];
            if (equipArea.equipSlots.Exists(slot => slot.item.id.Equals(id)))
                return true;
            else
                return false;
        }

        /// <summary>
        /// Check if a specific Item Name is equipped on a specific EquipArea
        /// <seealso cref="vInventory.equipAreas"/>
        /// </summary>
        /// <param name="itemName">Item name</param>
        /// <param name="indexOfArea">index of equip area </param>        
        /// <returns></returns>
        public virtual bool ItemIsInSpecificEquipArea(string itemName, int indexOfArea)
        {
            if (!inventory || inventory.equipAreas.Length == 0 || indexOfArea > inventory.equipAreas.Length - 1) return false;
            var equipArea = inventory.equipAreas[indexOfArea];
            if (equipArea.equipSlots.Exists(slot => slot.item.name.Equals(itemName)))
                return true;
            else
                return false;
        }

        /// <summary>
        /// Check if a EquipPoint has any item equipped on it
        /// <seealso cref="vItemManager.equipPoints"/>
        /// </summary>
        /// <param name="equipPointName">EquipPoint name</param>
        /// <returns></returns>
        public virtual bool EquipPointHasSomeItem(string equipPointName)
        {
            return equipPoints.Exists(ep => ep.equipPointName.Equals(equipPointName) && ep.equipmentReference != null && ep.equipmentReference.item != null);
        }

        /// <summary>
        /// Check if a specific Item ID is equipped on any EquipPoint
        /// <seealso cref="vItemManager.equipPoints"/>
        /// </summary>
        /// <param name="id">Item id</param>
        /// <returns></returns>
        public virtual bool ItemIsInSomeEquipPont(int id)
        {
            return equipPoints.Exists(ep => ep.equipmentReference != null && ep.equipmentReference.item != null && ep.equipmentReference.item.id.Equals(id));
        }

        /// <summary>
        /// Check if a specific Item Name is equipped on any EquipPoint
        /// <seealso cref="vItemManager.equipPoints"/>
        /// </summary>
        /// <param name="itemName">Item name</param>
        /// <returns></returns>
        public virtual bool ItemIsInSomeEquipPont(string itemName)
        {
            return equipPoints.Exists(ep => ep.equipmentReference != null && ep.equipmentReference.item != null && ep.equipmentReference.item.name.Equals(itemName));
        }

        /// <summary>
        /// Check if a specific Item ID is equipped on specific EquipPoint
        /// <seealso cref="vItemManager.equipPoints"/>
        /// </summary>       
        /// <param name="id">Item id</param>
        /// <param name="equipPointName">EquipPoint name</param>
        /// <returns></returns>
        public virtual bool ItemIsInSpecificEquipPoint(int id, string equipPointName)
        {
            return equipPoints.Exists(ep => ep.equipPointName.Equals(equipPointName) && ep.equipmentReference != null && ep.equipmentReference.item != null && ep.equipmentReference.item.id.Equals(id));
        }

        /// <summary>
        /// Check if a specific Item Name is equipped on specific EquipPoint
        /// <seealso cref="vItemManager.equipPoints"/>
        /// </summary>       
        /// <param name="itemName">Item name</param>
        /// <param name="equipPointName">EquipPoint name</param>
        /// <returns></returns>
        public virtual bool ItemIsInSpecificEquipPoint(string itemName, string equipPointName)
        {
            return equipPoints.Exists(ep => ep.equipPointName.Equals(equipPointName) && ep.equipmentReference != null && ep.equipmentReference.item != null && ep.equipmentReference.item.name.Equals(itemName));
        }

        #endregion

        #region Get Items 

        /// <summary>
        /// Get all item amount with the same id from your Inventory
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public virtual int GetAllAmount(int id)
        {
            var _items = GetItems(id);
            int _amount = 0;
            for (int i = 0; i < _items.Count; i++)
            {
                _amount += _items[i].amount;
            }
            return _amount;
        }

        /// <summary>
        /// Return a list of all items that you have
        /// </summary>
        /// <returns></returns>
        public virtual List<vItem> GetItems()
        {
            return items;
        }

        /// <summary>
        /// Return a list of all items in your ItemListData
        /// </summary>
        /// <returns></returns>
        public virtual List<vItem> GetAllItems()
        {
            return itemListData ? itemListData.items : null;
        }

        /// <summary>
        /// Get a single Item with same id
        /// <seealso cref="vItemManager.items"/>
        /// </summary>
        /// <param name="id">Item id</param>
        /// <returns></returns>
        public virtual vItem GetItem(int id)
        {
            return items.Find(i => i.id == id);
        }

        /// <summary>
        /// Get a single Item with same name
        /// <seealso cref="vItemManager.items"/>
        /// </summary>
        /// <param name="itemName">Item name</param>
        /// <returns></returns>
        public virtual vItem GetItem(string itemName)
        {
            return items.Find(i => i.name == itemName);
        }

        /// <summary>
        /// Get the item from a specific equipPoint
        /// </summary>
        /// <param name="equipPointName">EquipPoint name</param>
        /// <returns>Returns the Item (if equipped) on this EquipPoint</returns>
        public virtual vItem GetItemInEquipPoint(string equipPointName)
        {
            var equipPoint = equipPoints.Find(ep => ep.equipPointName.Equals(equipPointName));
            if (equipPoint != null && equipPoint.equipmentReference != null && equipPoint.equipmentReference.item) return equipPoint.equipmentReference.item;
            else return null;
        }

        /// <summary>
        /// Get All Items with same id
        /// <seealso cref="vItemManager.items"/>
        /// </summary>
        /// <param name="id">Item id</param>
        /// <returns></returns>
        public virtual List<vItem> GetItems(int id)
        {
            var _items = items.FindAll(i => i.id == id);
            return _items;
        }

        /// <summary>
        /// Get All Items with same name
        /// <seealso cref="vItemManager.items"/>
        /// </summary>
        /// <param name="itemName">Item Name</param>
        /// <returns></returns>
        public virtual List<vItem> GetItems(string itemName)
        {
            var _items = items.FindAll(i => i.name == itemName);
            return _items;
        }

        /// <summary>
        /// Get a list of all Items equipped in the same EquipArea
        /// <seealso cref="vInventory.equipAreas"/>
        /// </summary>
        /// <param name="indexOfArea">index of equip area</param>
        /// <returns></returns>
        public virtual List<vItem> GetItemsInEquipArea(int indexOfArea)
        {
            var list = new List<vItem>();
            if (!inventory || inventory.equipAreas.Length == 0 || indexOfArea > inventory.equipAreas.Length - 1) return list;
            var equipArea = inventory.equipAreas[indexOfArea];
            var validSlot = equipArea.ValidSlots;
            for (int i = 0; i < validSlot.Count; i++)
            {
                if (validSlot[i].item != null) list.Add(validSlot[i].item);
            }
            return list;
        }

        /// <summary>
        /// Get a list of all Items equipped on all EquipAreas
        /// <seealso cref="vInventory.equipAreas"/>
        /// </summary>
        /// <returns></returns>
        public virtual List<vItem> GetAllItemInAllEquipAreas()
        {
            var list = new List<vItem>();
            if (!inventory || inventory.equipAreas.Length == 0) return list;
            for (int i = 0; i < inventory.equipAreas.Length; i++)
            {
                var equipArea = inventory.equipAreas[i];
                var validSlot = equipArea.ValidSlots;
                for (int a = 0; a < validSlot.Count; a++)
                {
                    if (validSlot[a].item != null) list.Add(validSlot[a].item);
                }
            }
            return list;
        }

        #endregion

        #region Equipment Pooling

        protected vEquipment EquipEquipment(vItem item, bool startActive = true)
        {
            if (equipments.ContainsKey(item))
            {
                if (!startActive)
                {
                    if (debugMode) Debug.Log($"<color=green>Disable Equipment {equipments[item].gameObject} </color>");
                    equipments[item].gameObject.SetActive(false);
                }
                else
                {
                    if (debugMode) Debug.Log($"<color=green>Enable Equipment {equipments[item].gameObject} </color>");
                    equipments[item].gameObject.SetActive(true);
                }


                return equipments[item];
            }
            else
            {
                if (item.originalObject)
                {
                    var equipment = item.originalObject.GetComponent<vEquipment>();

                    if (equipment != null)
                    {
                        var equipmentClone = Instantiate(item.originalObject);
                        if (!startActive)
                        {
                            if (debugMode) Debug.Log($"<color=green>Instantiate and disable Equipment {equipmentClone.gameObject} </color>");
                            equipmentClone.gameObject.SetActive(false);
                        }
                        else
                        {
                            if (debugMode) Debug.Log($"<color=green>Instantiate and enable Equipment {equipmentClone.gameObject} </color>");
                        }
                        equipmentClone.transform.SetParent(equipmentContainer.transform);
                        equipmentClone.transform.localPosition = Vector3.zero;
                        equipmentClone.transform.localEulerAngles = Vector3.zero;
                        equipment = equipmentClone.GetComponent<vEquipment>();                       
                        equipments.Add(item, equipment);
                        return equipment;
                    }
                }
            }
            return null;
        }

        protected vEquipment EquipEquipment(vItem item, Vector3 position, Quaternion rotation, Transform parent = null)
        {
            vEquipment equipment = EquipEquipment(item);
            if (equipment)
            {
                if (parent)
                {
                    equipment.transform.parent = parent;
                }
                equipment.transform.position = position;
                equipment.transform.rotation = rotation;
                equipment.OnEquip(item);
                return equipment;
            }
            return null;
        }

        protected void UnequipEquipment(vItem item)
        {
            if (equipments.ContainsKey(item))
            {
                if (debugMode) Debug.Log($"<color=red>Disable Equipment { equipments[item].gameObject} </color>");
                equipments[item].gameObject.SetActive(false);
                equipments[item].gameObject.transform.SetParent(equipmentContainer.transform);
                equipments[item].gameObject.transform.localPosition = Vector3.zero;
                equipments[item].gameObject.transform.localEulerAngles = Vector3.zero;
                equipments[item].equipPoint = null;
                equipments[item].OnUnequip(item);
            }
        }

        #endregion

        #region Add/Equip/AutoEquip Item

        /// <summary>
        /// Add a new Item Instance to the Inventory
        /// </summary>
        /// <param name="itemReference">Reference of the Item to be instantiate</param>
        /// <param name="ignoreItemAnimation">Play the Enable/Disable animation of your item, you can assign an animation to your item in the ItemListData</param>
        public virtual void AddItem(ItemReference itemReference, bool ignoreItemAnimation = false)
        {
            if (itemReference != null && itemListData != null && itemListData.items.Count > 0)
            {
                var item = itemListData.items.Find(t => t.id.Equals(itemReference.id));
                if (item)
                {
                    var sameItems = items.FindAll(i => i.stackable && i.id == item.id && i.amount < i.maxStack);
                    if (sameItems.Count == 0)
                    {
                        var _item = Instantiate(item);
                        _item.name = _item.name.Replace("(Clone)", string.Empty);

                        if (itemReference.attributes != null && _item.attributes != null && item.attributes.Count == itemReference.attributes.Count)
                        {
                            for (int i = 0; i < _item.attributes.Count; i++)
                            {
                                itemReference.attributes[i].CopyTo(_item.attributes[i]);
                            }
                        }

                        _item.amount = 0;

                        for (int i = 0; i < item.maxStack && _item.amount < _item.maxStack && itemReference.amount > 0; i++)
                        {
                            _item.amount++;
                            itemReference.amount--;
                        }
                        items.Add(_item);
                        onAddItem.Invoke(_item);
                        onAddItemID.Invoke(_item.id);
                        if (itemReference.addToEquipArea)
                        {
                            itemReference.addToEquipArea = false;
                            AutoEquipItem(_item, itemReference.indexArea, itemReference.autoEquip, ignoreItemAnimation);
                        }

                        if (itemReference.amount > 0) AddItem(itemReference);
                    }
                    else
                    {
                        var indexOffItem = items.IndexOf(sameItems[0]);

                        for (int i = 0; i < items[indexOffItem].maxStack && items[indexOffItem].amount < items[indexOffItem].maxStack && itemReference.amount > 0; i++)
                        {
                            items[indexOffItem].amount++;
                            itemReference.amount--;
                            onChangeItemAmount.Invoke(items[indexOffItem]);
                        }

                        if (itemReference.amount > 0)
                            AddItem(itemReference);
                    }
                }
            }
            inventory.UpdateInventory();
        }

        /// <summary>
        /// Automatically equip the item to empty EquiSlot of a specific EquipArea
        /// </summary>
        /// <param name="item"></param>
        /// <param name="indexArea">EquipArea (index) of the Inventory</param>
        /// <param name="ignoreItemAnimation">Ignore the Enable/Disable animation of your item</param>
        public virtual void AutoEquipItem(vItem item, int indexArea, bool autoEquip = false, bool ignoreItemAnimation = true)
        {
            if (!inventory) return;

            if (inventory.equipAreas != null && inventory.equipAreas.Length > 0 && indexArea < inventory.equipAreas.Length)
            {
                var validSlot = inventory.equipAreas[indexArea].equipSlots.Find(slot => slot.isValid && slot.item == null && slot.itemType.Contains(item.type));
                if (validSlot == null) validSlot = inventory.equipAreas[indexArea].currentEquippedSlot;
                if (validSlot && !inventory.equipAreas[indexArea].equipSlots.Exists(slot => slot.item == item))
                {
                    var indexOfSlot = inventory.equipAreas[indexArea].equipSlots.IndexOf(validSlot);
                    if (validSlot.item != item)
                        EquipItemToEquipSlot(indexArea, indexOfSlot, item, autoEquip, ignoreItemAnimation);
                }
            }
            else
            {
                if (debugMode) Debug.LogWarning("Fail to auto equip " + item.name + " on equipArea " + indexArea);
            }
        }

        /// <summary>
        /// Equip a specific Item to a specific EquipArea, this method is called internally by the Event from the Inventory
        /// </summary>
        /// <param name="equipArea"></param>
        /// <param name="item"></param>        
        protected virtual void EquipItem(vEquipArea equipArea, vItem item)
        {
            if (!item) return;
            onEquipItem.Invoke(equipArea, item);
            if (debugMode) Debug.Log($"<color=green>Start Equip {item} </color>");
            inventory.UpdateInventory();
            if (item != equipArea.currentEquippedItem)
            {
                if (debugMode) Debug.Log($"<color=green>Not Current Equip {item} </color>{equipArea.indexOfEquippedItem}", equipArea.currentEquippedItem);
                EquipEquipment(item, false);
                onFinishEquipItem?.Invoke(equipArea, item);
                if (debugMode) Debug.Log($"<color=green>Finish Equip {item} </color>");
                return;
            }

            var equipPoint = equipPoints.Find(ep => ep.equipPointName == equipArea.equipPointName);
            if (equipPoint != null && item != null && equipPoint.equipmentReference.item != item)
            {
                
                if (item.originalObject)
                {
                    var equipment = item.originalObject.GetComponentInChildren<vEquipment>();
                    if (equipment != null)
                    {                       
                        equipPoint.area = equipArea;
                        StartCoroutine(EquipItemRoutine(equipPoint, item, () => { onFinishEquipItem?.Invoke(equipArea, item); }));
                    }
                }
            }
        }

        /// <summary>
        /// Equip Item Routine
        /// </summary>
        /// <param name="equipPoint"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        IEnumerator EquipItemRoutine(EquipPoint equipPoint, vItem item, UnityEngine.Events.UnityAction onFinish)
        {
            LockInventoryInput(true);
            if (equipPoint != null) CheckTwoHandItem(equipPoint, item);
            while (inEquip || IsAnimatorTag("IsEquipping"))
            {
                yield return new WaitForEndOfFrame();
            }

            if (playItemAnimation)
            {                
                if (debugMode) Debug.Log($"<color=green>Play Equip Animation {item} </color>");
                equipTimer = item.enableDelayTime;
                animator.SetBool("FlipEquip", equipPoint.equipPointName.Contains("Left"));
                animator.CrossFade(item.EnableAnim, 0.25f);
            }

            if (!inEquip)
            {
                inEquip = true;
                inventory.canEquip = false;

                if (equipPoint != null)
                {
                    if (item.originalObject)
                    {
                        if (equipPoint.equipmentReference != null && equipPoint.equipmentReference.equipedObject != null && equipPoint.equipmentReference.item)
                        {                           
                            UnequipEquipment(equipPoint.equipmentReference.item);                       
                            equipPoint.equipmentReference.item = null;
                        }
                       
                        if (playItemAnimation && !string.IsNullOrEmpty(item.EnableAnim))
                        {
                            if (debugMode && equipTimer > 0) Debug.Log($"<color=green>In Equip delay {item} </color>");
                            while (equipTimer > 0)
                            {
                               
                                if (item == null) break;
                                yield return null;
                                equipTimer -= vTime.deltaTime;
                            }
                        }
                        inEquip = false;
                        var point = equipPoint.handler.customHandlers.Find(p => p.name == item.customHandler);
                        var equipTransform = point != null ? point : equipPoint.handler.defaultHandler;
                        var equipedObject = EquipEquipment(item, equipTransform.position, equipTransform.rotation, equipTransform);
                        equipedObject.equipPoint = equipPoint;
                        equipPoint.equipmentReference.item = item;
                        equipPoint.equipmentReference.equipedObject = equipedObject.gameObject;                      
                        
                        equipPoint.onInstantiateEquiment.Invoke(equipedObject.gameObject);                       
                    }
                    else if (equipPoint.equipmentReference != null && equipPoint.equipmentReference.equipedObject != null && equipPoint.equipmentReference.item)
                    {
                        UnequipEquipment(equipPoint.equipmentReference.item);
                        equipments[equipPoint.equipmentReference.item].equipPoint = null;
                        equipPoint.equipmentReference.item = null;
                    }
                }
                
               
            }
            LockInventoryInput(false);
            onFinish?.Invoke();
            if (debugMode) Debug.Log($"<color=green>Finish Equip {item} </color>");
            inEquip = false;
            inventory.canEquip = true;
        }

        /// <summary>
        /// Equip item to a equipArea on a specific equipSlot        
        /// </summary>
        /// <param name="indexOfArea">Index of <seealso cref="vInventory.equipAreas"/></param>
        /// <param name="indexOfSlot">Index of Slot in <seealso cref="vEquipArea"/></param>
        /// <param name="item"><seealso cref="vItem"/> to Equip</param>
        /// <param name="ignoreItemAnimation">Ignore the Enable/Disable animation of your item</param>
        public virtual void EquipItemToEquipSlot(int indexOfArea, int indexOfSlot, vItem item, bool autoEquip = false, bool ignoreItemAnimation = true)
        {
            if (!inventory) return;
            if (ignoreItemAnimation)
                temporarilyIgnoreItemAnimation = ignoreItemAnimation;
            if (inventory.equipAreas != null && indexOfArea < inventory.equipAreas.Length)
            {
                var area = inventory.equipAreas[indexOfArea];
                if (area != null)
                {
                    area.AddItemToEquipSlot(indexOfSlot, item, autoEquip);
                }
            }
            if (ignoreItemAnimation)
                temporarilyIgnoreItemAnimation = false;
        }

        /// <summary>
        /// Equip or change Item to a current equipSlot from a specific equipArea
        /// </summary>
        /// <param name="item">Item to equip</param>
        /// <param name="indexOfArea">Index of Equip area</param>
        /// <param name="ignoreItemAnimation">Ignore the Enable/Disable animation of your item</param>
        public virtual void EquipItemToCurrentEquipSlot(vItem item, int indexOfArea, bool ignoreItemAnimation = true)
        {
            if (!inventory && items.Count == 0) return;

            if (ignoreItemAnimation)
                temporarilyIgnoreItemAnimation = ignoreItemAnimation;
            if (inventory.equipAreas != null && indexOfArea < inventory.equipAreas.Length)
            {
                inventory.equipAreas[indexOfArea].AddCurrentItem(item);
            }
            if (ignoreItemAnimation)
                temporarilyIgnoreItemAnimation = false;
        }

        #endregion

        #region Unequip Item
        float unequipTimer;
        /// <summary>
        /// Unequips a specific Item from a specific EquipArea, this method is called internally on a Event from the Inventory
        /// </summary>
        /// <param name="equipArea"></param>
        /// <param name="item"></param>     
        protected virtual void UnequipItem(vEquipArea equipArea, vItem item)
        {
            if (!item) return;
            onUnequipItem.Invoke(equipArea, item);
            if (debugMode) Debug.Log($"<color=red>Start Unequip {item}</color>");
            var equipPoint = equipPoints.Find(ep => ep.equipPointName == equipArea.equipPointName &&
            ep.equipmentReference.item != null &&
            ep.equipmentReference.item == item);

            if (equipPoint != null && item != null)
            {
                equipPoint.onInstantiateEquiment.Invoke(null);
                unequipTimer = item.disableDelayTime;
                if (item.originalObject)
                {
                    var equipment = item.originalObject.GetComponentInChildren<vEquipment>();
                    if (equipment != null)
                    {
                        if (!inventory.isOpen && playItemAnimation && !inEquip && equipPoint.equipmentReference.equipedObject.activeInHierarchy)
                        {
                            if (debugMode) Debug.Log($"<color=red>Play Unequip Animation {item}</color>");
                            animator.SetBool("FlipEquip", equipArea.equipPointName.Contains("Left"));
                            animator.CrossFade(item.DisableAnim, 0.25f);
                        }
                        StartCoroutine(UnequipItemRoutine(equipPoint, item, () => { onFinishUnequipItem?.Invoke(equipArea, item); }));
                    }
                }
            }
            else if (item != null)
            {
                if (debugMode) Debug.Log($"<color=red>Finish Unequip {item}</color>");
                onFinishUnequipItem.Invoke(equipArea, item);
            }
            inventory.UpdateInventory();
        }

        /// <summary>
        /// Unequips a specific Item
        /// </summary>
        /// <param name="item"></param>    
        /// <param name="ignoreItemAnimation">Ignore the Enable/Disable animation of your item</param>
        public virtual void UnequipItem(vItem item, bool ignoreItemAnimation = true)
        {
            var equipArea = System.Array.Find(inventory.equipAreas, e => e.ValidSlots.Exists(s => s.item != null && s.item.id.Equals(item.id)));
            if (equipArea != null)
            {
                if (ignoreItemAnimation) temporarilyIgnoreItemAnimation = ignoreItemAnimation;
                UnequipItem(equipArea, item);
            }
            inventory.UpdateInventory();
            if (ignoreItemAnimation) temporarilyIgnoreItemAnimation = false;
        }

        /// <summary>
        /// Unequip item of specific area and specific slot
        /// </summary>
        /// <param name="indexOfArea">Index of Equip Area</param>
        /// <param name="indexOfSlot">Index of Slot in Equip Area</param>
        /// <param name="ignoreItemAnimation">Ignore the Enable/Disable animation of your item</param>
        public virtual void UnequipItemOfEquipSlot(int indexOfArea, int indexOfSlot, bool ignoreItemAnimation = true)
        {
            if (!inventory) return;
            if (ignoreItemAnimation)
                temporarilyIgnoreItemAnimation = ignoreItemAnimation;
            if (inventory.equipAreas != null && indexOfArea < inventory.equipAreas.Length)
            {
                var area = inventory.equipAreas[indexOfArea];
                if (area != null)
                {
                    area.RemoveItemOfEquipSlot(indexOfSlot);
                }
            }
            if (ignoreItemAnimation)
                temporarilyIgnoreItemAnimation = false;
        }

        /// <summary>
        /// Unequip current equiped item of specific area 
        /// </summary>
        /// <param name="indexOfArea">Index of Equip area</param>
        /// <param name="ignoreItemAnimation">Ignore the Enable/Disable animation of your item</param>
        public virtual void UnequipCurrentEquipedItem(int indexOfArea, bool ignoreItemAnimation = true)
        {
            if (!inventory && items.Count == 0) return;

            if (ignoreItemAnimation)
                temporarilyIgnoreItemAnimation = ignoreItemAnimation;
            if (inventory.equipAreas != null && indexOfArea < inventory.equipAreas.Length)
            {
                inventory.equipAreas[indexOfArea].RemoveCurrentItem();
            }
            if (ignoreItemAnimation)
                temporarilyIgnoreItemAnimation = false;
        }

        IEnumerator UnequipItemRoutine(EquipPoint equipPoint, vItem item, UnityEngine.Events.UnityAction onFinish)
        {
            LockInventoryInput(true);
            
            if (!inEquip)
            {
                inEquip = true;
                inventory.canEquip = false;


                if (equipPoint != null && equipPoint.equipmentReference != null && equipPoint.equipmentReference.equipedObject != null)
                {
                    if (!inventory.isOpen && playItemAnimation)
                    {
                        if (debugMode && unequipTimer >0) Debug.Log($"<color=red>In Unequip delay {item} </color>");
                        while (unequipTimer > 0 && !string.IsNullOrEmpty(item.DisableAnim))
                        {
                           
                            unequipTimer -= vTime.deltaTime;
                            yield return null;
                        }
                    }
                    if (equipPoint != null && equipPoint.equipmentReference != null && equipPoint.equipmentReference.equipedObject)
                    {                       
                        UnequipEquipment(item);                       
                        equipPoint.equipmentReference.item = null;
                    }
                }
                inEquip = false;
                inventory.canEquip = true;
            }
            else
            {

            }
            if (debugMode) Debug.Log($"<color=red>Finish Unequip {item}</color>");
            onFinish?.Invoke();
            LockInventoryInput(false);

        }

        #endregion

        #region Use Item

        /// <summary>
        /// Check if a item has any condition to be used
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool CanUseItem(vItem item)
        {
            if (canUseItemDelegate != null)
            {
                List<bool> canUse = new List<bool>();
                canUseItemDelegate.Invoke(item, ref canUse);
                return !canUse.Contains(false);
            }
            return item.canBeUsed;
        }

        /// <summary>
        /// Use a specific Item - Called internally on a Event from the Inventory
        /// </summary>
        /// <param name="item"></param>
        protected virtual void UseItem(vItem item)
        {
            if (item)
            {
                if (CanUseItem(item))
                {
                    StartCoroutine(UseItemRoutine(item));
                }
                else onUseItemFail.Invoke(item);
            }
        }

        /// <summary>
        /// Use Item Routine
        /// </summary>
        /// <param name="item"></param>      
        protected IEnumerator UseItemRoutine(vItem item)
        {
            usingItem = true;
            LockInventoryInput(true);
            onStartItemUsage.Invoke(item);
            var canUse = CanUseItem(item);

            if (canUse)
            {
                var time = item.enableDelayTime;

                if (!inventory.isOpen && playItemAnimation && !string.IsNullOrEmpty(item.EnableAnim))
                {
                    animator.SetBool("FlipAnimation", false);
                    animator.CrossFade(item.EnableAnim, 0.25f);
                    while (usingItem && time > 0 && canUse)
                    {
                        canUse = CanUseItem(item);
                        time -= vTime.deltaTime;
                        yield return null;
                    }
                }
                if (usingItem && canUse)
                {
                    if (item.destroyAfterUse) item.amount--;
                    onUseItem.Invoke(item);
                    
                    if (item.attributes != null && item.attributes.Count > 0 && applyAttributeEvents.Count > 0)
                    {
                        foreach (ApplyAttributeEvent attributeEvent in applyAttributeEvents)
                        {
                            var attributes = item.attributes.FindAll(a => a.name.Equals(attributeEvent.attribute));
                            foreach (vItemAttribute attribute in attributes)
                                attributeEvent.onApplyAttribute.Invoke(attribute.value);
                        }
                    }
                    if (item.destroyAfterUse && item.amount <= 0 && items.Contains(item))
                    {                        
                        DestroyItem(item);
                    }
                    else
                    {
                        onRemoveItemID.Invoke(item.id);
                    }
                    usingItem = false;
                    inventory.CheckEquipmentChanges();
                }
                else onUseItemFail.Invoke(item);
            }
            else onUseItemFail.Invoke(item);
            LockInventoryInput(false);
            inventory.UpdateInventory();
        }

        #endregion

        #region DestroyItem Item

        /// <summary>
        /// Destroy  all amount of specific item
        /// </summary>
        /// <param name="item"></param>
        public virtual void DestroyItem(vItem item)
        {
            DestroyItem(item, item.amount);
        }

        /// <summary>
        /// Destroy a specific amount of a specific item
        /// </summary>
        /// <param name="item"></param>
        /// <param name="amount"></param>
        public virtual void DestroyItem(vItem item, int amount )
        {
            //return;
            
            item.amount -= amount;
            onDestroyItem.Invoke(item, amount);
            if (item.amount <= 0)
            {
                var equipArea = System.Array.Find(inventory.equipAreas, e => e.ValidSlots.Exists(s => s.item != null && s.item.id.Equals(item.id)));

                if (equipArea != null)
                {
                    equipArea.UnequipItem(item);
                }
                var _itemID = item.id;
                if (items.Contains(item))
                {
                    items.Remove(item);                    
                }
                Destroy(item);
                onRemoveItemID.Invoke(_itemID);
            }
            inventory.UpdateInventory();
        }

      
        /// <summary>
        /// DestroyItem current equiped item of specific EquipArea
        /// </summary>
        /// <param name="indexOfArea">Index of Equip Area</param>
        /// <param name="ignoreItemAnimation">Ignore the Enable/Disable animation of your item</param>
        public virtual void DestroyCurrentEquipedItem(int indexOfArea, bool ignoreItemAnimation = true)
        {
            if (!inventory && items.Count == 0) return;

            if (ignoreItemAnimation)
                temporarilyIgnoreItemAnimation = ignoreItemAnimation;
            if (inventory.equipAreas != null && indexOfArea < inventory.equipAreas.Length)
            {
                var item = inventory.equipAreas[indexOfArea].currentEquippedItem;
                if (item)
                    DestroyItem(item, item.amount);
            }

            if (ignoreItemAnimation)
                temporarilyIgnoreItemAnimation = false;
        }

        #endregion

        #region Drop Item
        /// <summary>
        /// Drop all amount of specific item
        /// </summary>
        /// <param name="item"></param>
        public virtual void DropItem(vItem item)
        {
            DropItem(item, item.amount);
        }

        /// <summary>
        /// Drop a specific amount of a specific Item
        /// </summary>
        /// <param name="item"></param>
        /// <param name="amount"></param>
        public virtual void DropItem(vItem item, int amount)
        {
            item.amount -= amount;
            if (item.dropObject != null)
            {
                var dropObject = Instantiate(item.dropObject, transform.position, transform.rotation) as GameObject;
                vItemCollection collection = dropObject.GetComponent<vItemCollection>();
                if (collection != null)
                {
                    collection.items.Clear();
                    var itemReference = new ItemReference(item.id);
                    itemReference.amount = amount;
                    itemReference.attributes = new List<vItemAttribute>(item.attributes);
                    collection.items.Add(itemReference);
                }
            }
            onDropItem.Invoke(item, amount);
            if (item.amount <= 0 && items.Contains(item))
            {
                var equipArea = System.Array.Find(inventory.equipAreas, e => e.ValidSlots.Exists(s => s.item != null && s.item.id.Equals(item.id)));

                if (equipArea != null)
                {
                    equipArea.UnequipItem(item);
                }
                items.Remove(item);
                DestroyItem(item);
            }

            inventory.UpdateInventory();
        }

        /// <summary>
        /// Drop all Items from the Inventory
        /// </summary>
        public virtual void DropAllItens()
        {
            List<ItemReference> itemReferences = new List<ItemReference>();
            for (int i = 0; i < items.Count; i++)
            {
                if (itemReferences.Find(_item => _item.id == items[i].id) == null)
                {
                    var sameItens = items.FindAll(_item => _item.id == items[i].id);
                    ItemReference itemReference = new ItemReference(items[i].id);
                    for (int a = 0; a < sameItens.Count; a++)
                    {
                        if (sameItens[a].type != vItemType.Consumable)
                        {
                            var equipPoint = equipPoints.Find(ep => ep.equipmentReference != null && ep.equipmentReference.item == sameItens[a]);
                            if (equipPoint != null && equipPoint.equipmentReference.equipedObject != null)
                                UnequipItem(equipPoint.area, equipPoint.equipmentReference.item);
                        }
                        else
                        {
                            var equipArea = System.Array.Find(inventory.equipAreas, e => e.ValidSlots.Exists(s => s.item != null && s.item.id.Equals(sameItens[a].id)));

                            if (equipArea != null)
                                equipArea.UnequipItem(sameItens[a]);
                        }

                        itemReference.amount += sameItens[a].amount;
                        DestroyItem(sameItens[a]);
                    }
                    itemReferences.Add(itemReference);

                    if (items[i].dropObject)
                    {
                        var dropObject = Instantiate(items[i].dropObject, transform.position, transform.rotation) as GameObject;
                        vItemCollection collection = dropObject.GetComponent<vItemCollection>();
                        if (collection != null)
                        {
                            collection.items.Clear();
                            collection.items.Add(itemReference);
                        }
                    }
                }
            }
            items.Clear();
        }

        /// <summary>
        /// Drop the current equipped item from a specific EquipArea
        /// </summary>
        /// <param name="indexOfArea">Index of Equip Area</param>
        /// <param name="ignoreItemAnimation">Ignore the Enable/Disable animation of your item</param>
        public virtual void DropCurrentEquippedItem(int indexOfArea, bool ignoreItemAnimation = true)
        {
            if (!inventory && items.Count == 0) return;

            if (ignoreItemAnimation)
                temporarilyIgnoreItemAnimation = ignoreItemAnimation;

            if (inventory.equipAreas != null && indexOfArea < inventory.equipAreas.Length)
            {
                var item = inventory.equipAreas[indexOfArea].currentEquippedItem;
                if (item)
                    DropItem(item, item.amount);
            }

            if (ignoreItemAnimation)
                temporarilyIgnoreItemAnimation = false;
        }

        #endregion

        #region Item Collector    

        /// <summary>
        /// Add a collection of items from a <seealso cref="ItemReference"/> List 
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="ignoreItemAnimation">Ignore the Enable/Disable animation of your item</param>
        public virtual void CollectItems(List<ItemReference> collection, bool ignoreItemAnimation = true)
        {
            foreach (ItemReference reference in collection)
            {
                AddItem(reference, ignoreItemAnimation);
            }
        }

        /// <summary>
        /// Collect a list of Items and display the Items collected with a delay 
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="onCollectDelay"></param>
        /// <param name="textDelay"></param>
        /// <param name="ignoreItemAnimation">Ignore the Enable/Disable animation of your item</param>
        /// <returns></returns>
        public virtual IEnumerator CollectItemsWithDelay(List<ItemReference> collection, float onCollectDelay, float textDelay, bool ignoreItemAnimation = true)
        {
            yield return new WaitForSeconds(onCollectDelay);

            for (int i = 0; i < collection.Count; i++)
            {
                yield return new WaitForSeconds(textDelay);

                var item = itemListData.items.Find(_item => _item.id == collection[i].id);
                if (item != null && vItemCollectionDisplay.Instance != null)
                {
                    vItemCollectionDisplay.Instance.FadeText("Acquired:" + " " + collection[i].amount + " " + item.name, 4, 0.25f);
                }
                while (collection[i].addToEquipArea && (inEquip || IsAnimatorTag("IsEquipping")))
                {
                    yield return new WaitForEndOfFrame();
                }
                AddItem(collection[i], ignoreItemAnimation);
            }
        }

        /// <summary>
        /// Collect a Item and display the Items collected with a delay 
        /// </summary>
        /// <param name="itemRef"></param>
        /// <param name="onCollectDelay"></param>
        /// <param name="textDelay"></param>
        /// <param name="ignoreItemAnimation">Ignore the Enable/Disable animation of your item</param>
        /// <returns></returns>
        public virtual IEnumerator CollectItemWithDelay(ItemReference itemRef, float onCollectDelay, float textDelay, bool ignoreItemAnimation = true)
        {
            yield return new WaitForSeconds(onCollectDelay + textDelay);

            var item = itemListData.items.Find(_item => _item.id == itemRef.id);
            if (item != null && vItemCollectionDisplay.Instance != null)
            {
                vItemCollectionDisplay.Instance.FadeText("Acquired:" + " " + itemRef.amount + " " + item.name, 4, 0.25f);
            }
            while (itemRef.addToEquipArea && (inEquip || IsAnimatorTag("IsEquipping")))
            {
                yield return new WaitForEndOfFrame();
            }
            AddItem(itemRef, ignoreItemAnimation);
        }

        #endregion

        #region IActionReceiver methods

        /// <summary>
        /// Event called by <seealso cref="vActionListener"/> 
        /// </summary>
        /// <param name="action"></param>
        public virtual void OnReceiveAction(vTriggerGenericAction action)
        {
            var collection = action.GetComponentInChildren<vItemCollection>();
            if (collection != null)
            {
                if (collection.items.Count > 0)
                {
                    var itemCol = collection.items.vCopy();
                    StartCoroutine(CollectItemsWithDelay(itemCol, collection.onCollectDelay, collection.textDelay, collection.ignoreItemAnimation));
                }
            }
        }

        #endregion
    }

    [System.Serializable]
    public class ItemReference
    {
        public int id;
        public string name;
        public int amount;
        public ItemReference(int id)
        {
            this.id = id;
            this.addToEquipArea = true;
            this.autoEquip = false;
        }
        public List<vItemAttribute> attributes;
        public bool changeAttributes;
        public bool autoEquip = false;
        public bool addToEquipArea = true;
        public int indexArea;
    }

    [System.Serializable]
    public class EquipPoint
    {
        #region SeralizedProperties in CustomEditor

        [SerializeField]
        public string equipPointName;
        public EquipmentReference equipmentReference = new EquipmentReference();
        [HideInInspector]
        public vEquipArea area;
        public vHandler handler = new vHandler();
        //public Transform defaultPoint;
        //public List<Transform> customPoints = new List<Transform>();
        public OnInstantiateItemObjectEvent onInstantiateEquiment = new OnInstantiateItemObjectEvent();

        #endregion
    }

    public class EquipmentReference
    {
        public GameObject equipedObject;
        public vItem item;
    }

    [System.Serializable]
    public class ApplyAttributeEvent
    {
        [SerializeField]
        public vItemAttributes attribute;
        [SerializeField]
        public OnApplyAttribute onApplyAttribute;
    }
}