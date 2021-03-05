using Invector.vCharacterController;
using UnityEngine;
namespace Invector.vItemManager
{
    public class vToolbarSelector : MonoBehaviour
    {
        public GenericInput selectRightInput;
        public GenericInput selectLeftInput;
        public vMasterWindow masterWindow;
        public vToolbarButton[] selectables;
        public int index;
        public delegate void OnSelectToolbar(vToolbarButton toolbarButton);
        public event OnSelectToolbar onSelect;
        private void GetMasterWindow()
        {
            if (!masterWindow) masterWindow = transform.parent.GetComponentInParent<vMasterWindow>();
        }
        private void Awake()
        {
            for (int i = 0; i < selectables.Length; i++)
            {
                onSelect += selectables[i].OnSelectTool;
                int val = i;
                selectables[i].onSelect.AddListener(() => { SelectToolbar(val); });
            }
            GetMasterWindow();
        }
        public void Update()
        {
            if (selectRightInput.GetButtonDown())
            {
                SelectRight();
            }
            else if (selectLeftInput.GetButtonDown())
            {
                SelectLeft();
            }
        }
        public void SelectRight()
        {
            index = (index + 1) % selectables.Length;
            onSelect(selectables[index]);
        }
        public void SelectLeft()
        {
            index--; if (index < 0) index = selectables.Length - 1;
            onSelect(selectables[index]);
        }
        public void SelectToolbar(int index)
        {
            this.index = index;
            onSelect(selectables[this.index]);
            masterWindow.SetCurrentWindow(selectables[this.index].targetWindow);
        }
    }
}