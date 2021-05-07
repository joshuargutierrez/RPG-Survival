using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Invector.vItemManager
{
    [vClassHeader("Equip Area", openClose = false)]
    public class vEquipArea : vMonoBehaviour
    {
        public delegate void OnPickUpItem(vEquipArea area, vItemSlot slot);
        public OnPickUpItem onPickUpItemCallBack;

        public vInventory inventory;
        public vItemWindow itemPicker;
        [Tooltip("Set current equiped slot when submit an slot of this area")]
        public bool setEquipSlotWhenSubmit;
        [Tooltip("Skip empty slots when switching between slots")]
        public bool skipEmptySlots;
        public List<vEquipSlot> equipSlots;
        public string equipPointName;

        public Text displayNameText;
        public Text displayTypeText;
        public Text displayAmountText;
        public Text displayDescriptionText;
        public Text displayAttributesText;

        [vHelpBox("You can ignore display Attributes using this property")]
        public List<vItemAttributes> ignoreAttributes;
        public UnityEngine.Events.UnityEvent onInitPickUpItem, onFinishPickUpItem;
        public InputField.OnChangeEvent onChangeName;
        public InputField.OnChangeEvent onChangeType;
        public InputField.OnChangeEvent onChangeAmount;
        public InputField.OnChangeEvent onChangeDescription;
        public InputField.OnChangeEvent onChangeAttributes;

        public OnChangeEquipmentEvent onEquipItem;
        public OnChangeEquipmentEvent onUnequipItem;
        public OnSelectEquipArea onSelectEquipArea;
        [HideInInspector]
        public vEquipSlot currentSelectedSlot;
        public vEquipSlot lastSelectedSlot;
        [HideInInspector]
        public int indexOfEquippedItem;
        public vItem lastEquipedItem;

        internal bool isInit;
        public void Init()
        {
            if (!isInit) Start();
        }

        void Start()
        {
            if (!isInit)
            {
                isInit = true;
                //indexOfEquipedItem = -1;
                inventory = GetComponentInParent<vInventory>();

                if (equipSlots.Count == 0)
                {
                    var equipSlotsArray = GetComponentsInChildren<vEquipSlot>(true);
                    equipSlots = equipSlotsArray.vToList();
                }
                foreach (vEquipSlot slot in equipSlots)
                {
                    slot.onSubmitSlotCallBack = OnSubmitSlot;
                    slot.onSelectSlotCallBack = OnSelectSlot;
                    slot.onDeselectSlotCallBack = OnDeselect;

                    if (slot.displayAmountText) slot.displayAmountText.text = "";
                    slot.onChangeAmount.Invoke("");
                }
            }
        }

        /// <summary>
        /// Current Equipped Slot
        /// </summary>
        public vEquipSlot currentEquippedSlot
        {
            get
            {
                return equipSlots[indexOfEquippedItem];
               
            }
        }
        /// <summary>
        /// Item in Current Equipped Slot
        /// </summary>
        public vItem currentEquippedItem
        {
            get
            {
                var validEquipSlots = ValidSlots;
                if (validEquipSlots.Count > 0 && indexOfEquippedItem >= 0 && indexOfEquippedItem < validEquipSlots.Count) return validEquipSlots[indexOfEquippedItem].item;

                return null;
            }
        }

        /// <summary>
        /// All valid slot <seealso cref="vItemSlot.isValid"/>
        /// </summary>
        public List<vEquipSlot> ValidSlots
        {
            get { return equipSlots.FindAll(slot => slot.isValid && (!skipEmptySlots || slot.item != null)); }
        }

        /// <summary>
        /// Check if Item is in Area
        /// </summary>
        /// <param name="item">item to check</param>
        /// <returns></returns>
        public bool ContainsItem(vItem item)
        {
            return ValidSlots.Find(slot => slot.item == item) != null;
        }

        /// <summary>
        /// Event called from Inventory slot UI on Submit
        /// </summary>
        /// <param name="slot"></param>
        public void OnSubmitSlot(vItemSlot slot)
        {
            lastSelectedSlot = currentSelectedSlot;
            if (itemPicker != null)
            {
                currentSelectedSlot = slot as vEquipSlot;
                if (setEquipSlotWhenSubmit)
                {
                    SetEquipSlot(equipSlots.IndexOf(currentSelectedSlot));
                }
                itemPicker.gameObject.SetActive(true);
                itemPicker.onCancelSlot.RemoveAllListeners();
                itemPicker.onCancelSlot.AddListener(CancelCurrentSlot);
                itemPicker.CreateEquipmentWindow(inventory.items, currentSelectedSlot.itemType, slot.item, OnPickItem);
                onInitPickUpItem.Invoke();
            }
        }

        /// <summary>
        /// Event called to cancel Submit action
        /// </summary>
        public void CancelCurrentSlot()
        {
            if (currentSelectedSlot == null)
                currentSelectedSlot = lastSelectedSlot;

            if (currentSelectedSlot != null)
                currentSelectedSlot.OnCancel();
            onFinishPickUpItem.Invoke();
        }

        /// <summary>
        /// Unequip Item of the Slot
        /// </summary>
        /// <param name="slot">target slot</param>
        public void UnequipItem(vEquipSlot slot)
        {
            if (slot)
            {
                vItem item = slot.item;
                if (ValidSlots[indexOfEquippedItem].item == item)
                    lastEquipedItem = item;
                slot.RemoveItem();
                onUnequipItem.Invoke(this, item);
            }
        }

        /// <summary>
        /// Unequip Item if is present in slots 
        /// </summary>
        /// <param name="item"></param>
        public void UnequipItem(vItem item)
        {
            var slot = ValidSlots.Find(_slot => _slot.item == item);
            if (slot)
            {
                if (ValidSlots[indexOfEquippedItem].item == item) lastEquipedItem = item;
                slot.RemoveItem();
                onUnequipItem.Invoke(this, item);
            }
        }

        /// <summary>
        /// Unequip <seealso cref="currentEquippedItem"/>
        /// </summary>
        public void UnequipCurrentItem()
        {
            if (currentSelectedSlot && currentSelectedSlot.item)
            {
                var _item = currentSelectedSlot.item;
                if (ValidSlots[indexOfEquippedItem].item == _item) lastEquipedItem = _item;
                currentSelectedSlot.RemoveItem();
                onUnequipItem.Invoke(this, _item);
            }
        }

        /// <summary>
        /// Event called from inventory UI when select an slot
        /// </summary>
        /// <param name="slot">target slot</param>
        public void OnSelectSlot(vItemSlot slot)
        {
            if (equipSlots.Contains(slot as vEquipSlot))
                currentSelectedSlot = slot as vEquipSlot;
            else currentSelectedSlot = null;

            onSelectEquipArea.Invoke(this);
            CreateFullItemDescription(slot);
        }

        /// <summary>
        /// Event called from inventory UI when unselect an slot
        /// </summary>
        /// <param name="slot">target slot</param>
        public void OnDeselect(vItemSlot slot)
        {
            if (equipSlots.Contains(slot as vEquipSlot))
            {
                currentSelectedSlot = null;
            }
        }

        /// <summary>
        /// Create item description
        /// </summary>
        /// <param name="slot">target slot</param>
        protected virtual void CreateFullItemDescription(vItemSlot slot)
        {
            var _name = slot.item ? slot.item.name : "";
            var _type = slot.item ? slot.item.ItemTypeText() : "";
            var _amount = slot.item ? slot.item.amount.ToString() : "";
            var _description = slot.item ? slot.item.description : "";
            var _attributes = slot.item ? slot.item.GetItemAttributesText(ignoreAttributes) : "";

            if (displayNameText) displayNameText.text = _name;
            onChangeName.Invoke(_name);

            if (displayTypeText) displayTypeText.text = _type;
            onChangeType.Invoke(_type);

            if (displayAmountText) displayAmountText.text = _amount;
            onChangeAmount.Invoke(_amount);

            if (displayDescriptionText) displayDescriptionText.text = _description;
            onChangeDescription.Invoke(_description);

            if (displayAttributesText) displayAttributesText.text = _attributes;
            onChangeAttributes.Invoke(_attributes);
        }

        /// <summary>
        /// Event called from inventory UI to open <see cref="vItemWindow"/> when submit slot
        /// </summary>
        /// <param name="slot">target slot</param>
        public void OnPickItem(vItemSlot slot)
        {
            if (!currentSelectedSlot)
                currentSelectedSlot = lastSelectedSlot;

            if (!currentSelectedSlot)
                return;

            if (currentSelectedSlot.item != null && slot.item != currentSelectedSlot.item)
            {
                currentSelectedSlot.item.isInEquipArea = false;
                var item = currentSelectedSlot.item;
                if (item == slot.item) lastEquipedItem = item;
                currentSelectedSlot.RemoveItem();
                onUnequipItem.Invoke(this, item);
            }

            if (slot.item != currentSelectedSlot.item)
            {
                if (onPickUpItemCallBack != null)
                    onPickUpItemCallBack(this, slot);
                currentSelectedSlot.AddItem(slot.item);
                onEquipItem.Invoke(this, currentSelectedSlot.item);
            }
            currentSelectedSlot.OnCancel();
            currentSelectedSlot = null;
            lastSelectedSlot = null;
            itemPicker.gameObject.SetActive(false);
            onFinishPickUpItem.Invoke();
        }

        /// <summary>
        /// Equip next slot <seealso cref="currentEquippedItem"/>
        /// </summary>
        public void NextEquipSlot()
        {
            if (equipSlots == null || equipSlots.Count == 0) return;

            lastEquipedItem = currentEquippedItem;
            var validEquipSlots = ValidSlots;
            if (indexOfEquippedItem + 1 < validEquipSlots.Count)
                indexOfEquippedItem++;
            else
                indexOfEquippedItem = 0;

            if (currentEquippedItem != null)
                onEquipItem.Invoke(this, currentEquippedItem);
            onUnequipItem.Invoke(this, lastEquipedItem);
        }

        /// <summary>
        /// Equip previous slot <seealso cref="currentEquippedItem"/>
        /// </summary>
        public void PreviousEquipSlot()
        {
            if (equipSlots == null || equipSlots.Count == 0) return;

            lastEquipedItem = currentEquippedItem;
            var validEquipSlots = ValidSlots;

            if (indexOfEquippedItem - 1 >= 0)
                indexOfEquippedItem--;
            else
                indexOfEquippedItem = validEquipSlots.Count - 1;

            if (currentEquippedItem != null)
                onEquipItem.Invoke(this, currentEquippedItem);

            onUnequipItem.Invoke(this, lastEquipedItem);
        }

        /// <summary>
        /// Equip slot <see cref="currentEquippedItem"/>
        /// </summary>
        /// <param name="indexOfSlot">index of target slot</param>
        public void SetEquipSlot(int indexOfSlot)
        {
            if (equipSlots == null || equipSlots.Count == 0) return;
            if (indexOfSlot < equipSlots.Count && indexOfSlot >= 0)
            {
                lastEquipedItem = currentEquippedItem;
                indexOfEquippedItem = indexOfSlot;
                if (currentEquippedItem != null)
                {
                    onEquipItem.Invoke(this, currentEquippedItem);
                }
                if(currentEquippedItem != lastEquipedItem)
                    onUnequipItem.Invoke(this, lastEquipedItem);
            }
        }

        /// <summary>
        /// Add an item to an slot
        /// </summary>
        /// <param name="slot">target Slot</param>
        /// <param name="item">target Item</param>
        /// 
        public void AddItemToEquipSlot(vItemSlot slot, vItem item, bool autoEquip = false)
        {
            if (slot is vEquipSlot && equipSlots.Contains(slot as vEquipSlot))
            {
                AddItemToEquipSlot(equipSlots.IndexOf(slot as vEquipSlot), item, autoEquip);
            }
        }

        /// <summary>
        /// Add an item to an slot
        /// </summary>
        /// <param name="indexOfSlot">index of target Slot</param>
        /// <param name="item">target Item</param>
        public void AddItemToEquipSlot(int indexOfSlot, vItem item, bool autoEquip = false)
        {
            if (indexOfSlot < equipSlots.Count && item != null)
            {
                var slot = equipSlots[indexOfSlot];

                if (slot != null && slot.isValid && slot.itemType.Contains(item.type))
                {
                    var sameSlot = equipSlots.Find(s => s.item == item && s != slot);

                    if (sameSlot != null)
                        RemoveItemOfEquipSlot(equipSlots.IndexOf(sameSlot));

                    if (slot.item != null && slot.item != item)
                    {
                        if (currentEquippedItem == slot.item) lastEquipedItem = slot.item;
                        slot.item.isInEquipArea = false;
                        onUnequipItem.Invoke(this, slot.item);
                    }

                    item.isInEquipArea = true;
                    slot.AddItem(item);
                    if (autoEquip)
                        SetEquipSlot(indexOfSlot);
                    else
                        onEquipItem.Invoke(this, item);
                }
            }
        }

        /// <summary>
        /// Remove item of an slot
        /// </summary>
        /// <param name="slot">target Slot</param>
        public void RemoveItemOfEquipSlot(vItemSlot slot)
        {
            if (slot is vEquipSlot && equipSlots.Contains(slot as vEquipSlot))
            {
                RemoveItemOfEquipSlot(equipSlots.IndexOf(slot as vEquipSlot));
            }
        }

        /// <summary>
        /// Remove item of an slot
        /// </summary>
        /// <param name="slot">index of target Slot</param>
        public void RemoveItemOfEquipSlot(int indexOfSlot)
        {
            if (indexOfSlot < equipSlots.Count)
            {
                var slot = equipSlots[indexOfSlot];
                if (slot != null && slot.item != null)
                {
                    var item = slot.item;
                    item.isInEquipArea = false;
                    if (currentEquippedItem == item) lastEquipedItem = currentEquippedItem;
                    slot.RemoveItem();
                    onUnequipItem.Invoke(this, item);
                }
            }
        }

        /// <summary>
        /// Add item to current equiped slot 
        /// </summary>
        /// <param name="item">target item</param>
        public void AddCurrentItem(vItem item)
        {
            if (indexOfEquippedItem < equipSlots.Count)
            {
                var slot = equipSlots[indexOfEquippedItem];
                if (slot.item != null && item != slot.item)
                {
                    if (currentEquippedItem == slot.item) lastEquipedItem = slot.item;
                    slot.item.isInEquipArea = false;
                    onUnequipItem.Invoke(this, currentSelectedSlot.item);
                }
                slot.AddItem(item);
                onEquipItem.Invoke(this, item);
            }
        }

        /// <summary>
        /// Remove current equiped Item
        /// </summary>
        public void RemoveCurrentItem()
        {
            if (!currentEquippedItem) return;
            lastEquipedItem = currentEquippedItem;
            ValidSlots[indexOfEquippedItem].RemoveItem();
            onUnequipItem.Invoke(this, lastEquipedItem);

        }
    }
}
