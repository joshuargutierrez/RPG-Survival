using Invector.vItemManager;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class vItemDisplay3D : MonoBehaviour
{
    [System.Serializable]
    public class vDisplay
    {
        public int itemId;
        public GameObject itemModel;
    }
    public GameObject currentItemModel;
    public List<vDisplay> displays;
    public void Display(vItemSlot slot)
    {
       if(slot) Display(slot.item);
    }

    public void Display(int id)
    {
        vDisplay display = displays.Find(d => d.itemId.Equals(id));
        if(display!=null)
        {
            if (currentItemModel) currentItemModel.SetActive(false);
            display.itemModel.SetActive(true);
            currentItemModel = display.itemModel;
        }
    }

    public void Display(vItem item)
    {
        if(item)Display(item.id);
    }
}
