using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Invector.vItemManager
{
    /// <summary>
    /// Equipments of the Inventory that needs to be instantiated
    /// </summary>
    [vClassHeader("Equipment", openClose = false, helpBoxText = "Use this component if you also use the ItemManager in your Character")]
    public partial class vEquipment : vMonoBehaviour
    {    
        public OnHandleItemEvent onEquip, onUnequip;
        
        public EquipPoint equipPoint { get; set; }

        /// <summary>
        /// Event called when equipment is destroyed
        /// </summary>
        public virtual void OnDestroy()
        {

        }

        /// <summary>
        /// Item representing the equipment
        /// </summary>
        public vItem referenceItem;

        //{
        //    get;
        //    protected set;
        //}

        /// <summary>
        /// Event called when the item is equipped
        /// </summary>
        /// <param name="item">target item</param>
        public virtual void OnEquip(vItem item)
        {           
            referenceItem = item;          
            onEquip.Invoke(item);
        }

        /// <summary>
        /// Event called when the item is unquipped
        /// </summary>
        /// <param name="item">target item</param>
        public virtual void OnUnequip(vItem item)
        {          
            onUnequip.Invoke(item);
        }
    }
}