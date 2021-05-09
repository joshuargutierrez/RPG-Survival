using Invector.vCharacterController;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Invector.vItemManager
{
    [vClassHeader("Control Area By Input", "Use to select an EquipArea when you press a Input")]
    public class vControlAreaByInput : vMonoBehaviour
    {
        public List<SlotsSelector> slotsSelectors;
        public vEquipArea equipArea;
        public vInventory inventory;

        public delegate void OnSelectSlot(int indexOfSlot);
        public event OnSelectSlot onSelectSlot;

        void Start()
        {
            inventory = GetComponentInParent<vInventory>();

            for (int i = 0; i < slotsSelectors.Count; i++)
            {
                onSelectSlot += slotsSelectors[i].Select;
            }

            onSelectSlot?.Invoke(0);
        }

        void Update()
        {
            if (!inventory || !equipArea || inventory.lockInventoryInput) return;

            for (int i = 0; i < slotsSelectors.Count; i++)
            {
                if (slotsSelectors[i].input.GetButtonDown() && (inventory && !inventory.IsLocked() && !inventory.isOpen && inventory.canEquip))
                {
                    if (slotsSelectors[i].indexOfSlot < equipArea.equipSlots.Count && slotsSelectors[i].indexOfSlot >= 0)
                    {
                        equipArea.SetEquipSlot(slotsSelectors[i].indexOfSlot);
                        onSelectSlot?.Invoke(slotsSelectors[i].indexOfSlot);
                    }
                }

                if (slotsSelectors[i].equipDisplay != null && slotsSelectors[i].indexOfSlot < equipArea.equipSlots.Count && slotsSelectors[i].indexOfSlot >= 0)
                {
                    if (equipArea.equipSlots[slotsSelectors[i].indexOfSlot].item != slotsSelectors[i].equipDisplay.item)
                    {
                        slotsSelectors[i].equipDisplay.AddItem(equipArea.equipSlots[slotsSelectors[i].indexOfSlot].item);
                    }
                    else if (equipArea.equipSlots[slotsSelectors[i].indexOfSlot].item == null && slotsSelectors[i].equipDisplay.hasItem)
                    {
                        slotsSelectors[i].equipDisplay.RemoveItem();
                    }
                }
            }
        }

        [System.Serializable]
        public class SlotsSelector
        {
            public GenericInput input;
            public int indexOfSlot;
            public vEquipmentDisplay equipDisplay;
            public bool selected;
            public void Select(int indexOfSlot)
            {
                if (this.indexOfSlot != indexOfSlot && selected)
                {
                    equipDisplay.onDeselect.Invoke();
                    selected = false;
                }
                else if (this.indexOfSlot == indexOfSlot && !selected)
                {
                    equipDisplay.onSelect.Invoke();
                    selected = true;
                }
            }
        }
    }
}