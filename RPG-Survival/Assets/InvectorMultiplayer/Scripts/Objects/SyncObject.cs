using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using CBGames.Core;
using Invector.vItemManager;

namespace CBGames.Objects {
    [AddComponentMenu("CB GAMES/Player/Sync Equipped Items")]
    public class SyncObject : MonoBehaviour
    {
        public class ChildrenStates
        {
            public string name = null;
            public GameObject gameObject = null;
            public bool active = true;

            public ChildrenStates(string inputName, GameObject newChild, bool activeState)
            {
                this.name = inputName;
                this.gameObject = newChild;
                this.active = activeState;
            }
        }

        [Tooltip("The photon view to use when making rpc calls. If not populated will use the root object or if this object contains a photonview component will use it.")]
        public PhotonView view = null;
        [Space(10)]
        [Tooltip("When this object is enabled, sync across the network using the selected PhotonView.")]
        public bool syncEnable = true;
        [Tooltip("When this object is disabled, sync across the network using the selected PhotonView.")]
        public bool syncDisable = true;
        [Tooltip("When this object is destroyed, sync across the network using the selected PhotonView.")]
        public bool syncDestroy = true;
        [Tooltip("Important Note: Used ONLY for items!  \n\n When child objects are added or removed, sync across the network using the selected PhotonView. This will only sync immediate child transforms of this object, no nested transforms. This is a limitation of the unity engine.\n\n Will sync: Enables, Destorys, Disables, and Instantiate items from the item data.")]
        public bool syncImmediateChildren = true;
        [Space(10)]
        [Tooltip("NOTE: This only matters when \"syncImmediateChildren\" is enabled \n\nIs a left handed weapon? If the correct one is not selected could produce incorrect positioning when syncing across the network.")]
        public bool isLeftHanded = false;
        [Tooltip("NOTE: This only matters when \"syncImmediateChildren\" is enabled \n\nIs this a weapon holder? If the correct one is not selected could produce incorrect positioning when syncing across the network.")]
        public bool isWeaponHolder = false;
        [SerializeField] protected bool debugging = false;

        protected List<ChildrenStates> children = new List<ChildrenStates>();
        protected List<int> destroyed = new List<int>();
        protected List<int> instantiated = new List<int>();
        protected int[] toParentTree = null;
        protected int[] treeToThis = null;

        /// <summary>
        /// Checks to see if the passed in GameObject is is currently in this components
        /// known list of gameObjects.
        /// </summary>
        /// <param name="go">The gameobject to check if its in the list</param>
        /// <returns>True or False, is already in the known list.</returns>
        protected virtual bool IsInChildrenStatsList(GameObject go)
        {
            foreach (ChildrenStates child in children)
            {
                if (child.gameObject.GetInstanceID() == go.GetInstanceID())
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Checks to see if the passed in GameObject is is currently in this components
        /// known list of gameObjects.
        /// </summary>
        /// <param name="go">The gameobject to check if its in the list</param>
        /// <returns>True or False, is already in the known list.</returns>
        protected virtual bool GOInList(GameObject go, List<ChildrenStates> children)
        {
            return children.Exists(x => x.gameObject == go);
        }

        /// <summary>
        /// Calls to add the `SyncObject` component to the passed in gameobject.
        /// Also sets the settings as though this is a child of this component.
        /// </summary>
        /// <param name="child">The GameObject to add the `SyncObject` component to</param>
        protected virtual void AddThisComponent(GameObject child)
        {
            if (!child.gameObject.GetComponent<SyncObject>())
            {
                child.gameObject.AddComponent<SyncObject>();
                child.gameObject.GetComponent<SyncObject>().syncDestroy = true;
                child.gameObject.GetComponent<SyncObject>().syncDisable = true;
                child.gameObject.GetComponent<SyncObject>().syncEnable = true;
                child.gameObject.GetComponent<SyncObject>().syncImmediateChildren = false;
                child.gameObject.GetComponent<SyncObject>().isLeftHanded = isLeftHanded;
                child.gameObject.GetComponent<SyncObject>().isWeaponHolder = isWeaponHolder;
                child.gameObject.GetComponent<SyncObject>().debugging = debugging;
            }
        }

        /// <summary>
        /// Builds the index tree to the root of this tree and finds the parent `PhotonView` component.
        /// </summary>
        protected virtual void Awake()
        {
            if (!view)
            {
                if (GetComponent<PhotonView>())
                {
                    view = GetComponent<PhotonView>();
                }
                else if (transform.GetComponentInParent<PhotonView>())
                {
                    view = transform.GetComponentInParent<PhotonView>();
                }
            }
            if (view.IsMine == true)
            {
                toParentTree = StaticMethods.BuildChildTree(transform.root, transform.parent, debugging);
                treeToThis = StaticMethods.BuildChildTree(transform.root, transform, debugging);
            }
        }

        /// <summary>
        /// Will call the `NetworkSetActive` RPC which will enable this object for everyone on
        /// the network, not including yourself.
        /// </summary>
        protected virtual void OnEnable()
        {
            if (view.IsMine == true && syncEnable == true && toParentTree.Length > 0)
            {
                if (debugging == true) Debug.Log("ENABLE MESSAGE SENT");
                view.RPC("NetworkSetActive", RpcTarget.OthersBuffered, toParentTree, new int[1] { transform.GetSiblingIndex() }, true);
            }
        }

        /// <summary>
        /// Will call the `NetworkSetActive` RPC which will disable this object for everyone on
        /// the network, not including yourself.
        /// </summary>
        protected virtual void OnDisable()
        {
            if (view.IsMine == true && syncDisable == true)
            {
                if (toParentTree != null && toParentTree.Length > 0)
                {
                    if (debugging == true) Debug.Log("DISABLE MESSAGE SENT");
                    view.RPC("NetworkSetActive", RpcTarget.OthersBuffered, toParentTree, new int[1] { transform.GetSiblingIndex() }, false);
                }
            }
        }

        /// <summary>
        /// Calls the `Item_NetworkDestroy` RPC which destroys this object for everyone on the network, 
        /// not including yourself.
        /// </summary>
        protected virtual void OnDestroy()
        {
            if (view.IsMine == true && syncDestroy == true)
            {
                if (toParentTree != null && toParentTree.Length > 0)
                {
                    if (debugging == true) Debug.Log("DESTROY MESSAGE SENT");
                    view.RPC("Item_NetworkDestroy", RpcTarget.OthersBuffered, (object)toParentTree, new int[1] { transform.GetSiblingIndex() });
                }
            }
        }

        /// <summary>
        /// Called when a child object is added or removed from this object. Will detect
        /// if the object is added or removed and call the `Item_NetworkDestroy` or 
        /// `NetworkSetActive` RPC to enable/disable or destroy this object over the 
        /// network. This makes the networked players mimic what the owner's state 
        /// currently is.
        /// </summary>
        protected virtual void OnTransformChildrenChanged()
        {
            if (view.IsMine == true && syncImmediateChildren == true)
            {
                List<ChildrenStates> current = new List<ChildrenStates>();
                Transform child = null;
                for (int i = 0; i < transform.childCount; i++)
                {
                    child = transform.GetChild(i);
                    current.Add(new ChildrenStates(
                        child.name,
                        child.gameObject,
                        child.gameObject.activeInHierarchy
                    ));
                    if (!IsInChildrenStatsList(child.gameObject))
                    {
                        AddThisComponent(child.gameObject);

                        string nameToCheck = child.name.Replace("(Clone)", "");
                        foreach (vItem item in view.transform.GetComponent<vItemManager>().items)
                        {
                            if (item.originalObject && item.originalObject.name.Trim() == nameToCheck.Trim())
                            {
                                instantiated.Add(item.id);
                            }
                        }
                    }
                }
                //Find recently destroyed children transforms
                int index = 0;
                foreach (ChildrenStates target in children)
                {
                    if (!GOInList(target.gameObject, current))
                    {
                        destroyed.Add(index);
                    }
                    index += 1;
                }
                children = current;

                if (destroyed.Count > 0)
                {
                    if (treeToThis.Length > 0)
                    {
                        if (debugging == true) Debug.Log("DESTROY CHILD MESSAGE SENT");
                        view.RPC("Item_NetworkDestroy", RpcTarget.OthersBuffered, (object)treeToThis, (object)destroyed.ToArray());
                    }
                }
                if (instantiated.Count > 0 && treeToThis.Length > 0)
                {
                    if (debugging == true) Debug.Log("INSTANTIATE CHILD MESSAGE SENT");
                    treeToThis = StaticMethods.BuildChildTree(transform.root, transform, debugging);
                    view.RPC("NetworkInstantiate", RpcTarget.OthersBuffered, (object)treeToThis, (object)instantiated.ToArray(), isLeftHanded, isWeaponHolder);
                }
                destroyed.Clear();
                instantiated.Clear();
            }
        }
    }
}
