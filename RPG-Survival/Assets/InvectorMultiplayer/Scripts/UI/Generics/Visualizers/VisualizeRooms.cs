using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CBGames.UI
{
    [AddComponentMenu("CB GAMES/UI/Generics/Visualizers/Visualize Rooms")]
    public class VisualizeRooms : MonoBehaviour
    {
        [Tooltip("The gameobject that will act as the parent. Will replace all child objects according to results.")]
        [SerializeField] protected Transform parentObj = null;
        [Tooltip("Each room found will spawn this as a child of the parentObj.")]
        [SerializeField] protected GameObject roomButton = null;
        [Tooltip("Watch for any room changes and auto update this list.")]
        [SerializeField] protected bool autoUpate = false;
        [Tooltip("If this is true it will display each sub photon room as part of the session. Basically each Unity" +
            "scene that a connected player is in.")]
        public bool canDisplaySessionRooms = false;
        [Tooltip("If this is true it will only display the master session room. As in any photom room with an '_' in" +
            "the name will not be displayed.")]
        public string onlyDisplaySessionRooms = "";
        [Tooltip("If this has any value in it that means if the photom room doesn't have this value int it will not be" +
            "displayed.")]
        [SerializeField] protected string filterRooms = "";
        [Tooltip("Enable this if you want to have verbose logging to the console. Meant for debugging purposes.")]
        [SerializeField] protected bool debugging = false;

        protected UICoreLogic logic;
        protected Dictionary<string, RoomInfo> _roomList = new Dictionary<string, RoomInfo>();

        protected virtual void Start()
        {
            logic = FindObjectOfType<UICoreLogic>();
        }

        /// <summary>
        /// Calls the to set the list of rooms to be displayed manually.
        /// </summary>
        /// <param name="roomList">Dictionary<string, RoomInfo> type, a list of rooms to be displayed</param>
        public virtual void ManullayUpdateList(Dictionary<string, RoomInfo> roomList)
        {
            if (debugging == true) Debug.Log("Manually updating list...");
            _roomList = roomList;
            RefreshList();
        }

        /// <summary>
        /// Dynamically gets the room list that is saved in the `UICoreLogic` component and calls `RefreshList`
        /// with this new list.
        /// </summary>
        public virtual void GetRoomListFromUI()
        {
            _roomList = logic.GetRoomList();
            if (debugging == true) Debug.Log("Found: " + _roomList.Count + " rooms.");
            RefreshList();
        }

        /// <summary>
        /// Calls the `WaitForChange` IEnumerator
        /// </summary>
        public virtual void WaitForListChange()
        {
            StartCoroutine(WaitForChange());
        }

        /// <summary>
        /// Waits until the `UICoreLogic`'s room list is greater than zero. As soon as it is it calls the
        /// `GetRoomListFromUI` function.
        /// </summary>
        /// <returns></returns>
        protected virtual IEnumerator WaitForChange()
        {
            if (debugging == true) Debug.Log("Waiting for a room list update");
            logic = (logic == null) ? FindObjectOfType<UICoreLogic>() : logic;
            yield return new WaitUntil(() => logic.GetRoomList().Count > 0);
            GetRoomListFromUI();
        }

        /// <summary>
        /// If the previous count of the number of rooms is different it will call the `GetRoomListFromUI`
        /// function.
        /// </summary>
        protected virtual void Update()
        {
            if (autoUpate == false) return;
            if (_roomList.Count != logic.GetRoomList().Count)
            {
                GetRoomListFromUI();
            }
        }

        /// <summary>
        /// Sets the `filterRooms` parameter value to be whatever you pass in. Then calls the `WaitForListChange`
        /// function.
        /// </summary>
        /// <param name="filter">string type, the value that the room names must have</param>
        public virtual void SetFilter(string filter)
        {
            if (debugging == true) Debug.Log("Setting filter: " + filter);
            filterRooms = filter;
            WaitForListChange();
        }

        /// <summary>
        /// Destroys all child objects first then loops through all found rooms and spawns a new child object
        /// for each found room that matches the set criteria.
        /// </summary>
        protected virtual void RefreshList()
        {
            if (debugging == true) Debug.Log("Refreshing room list...");
            foreach (Transform child in parentObj)
            {
                Destroy(child.gameObject);
            }
            List<string> lockedRooms = new List<string>();
            foreach(KeyValuePair<string, RoomInfo> room in _roomList)
            {
                if (!string.IsNullOrEmpty(filterRooms) && !room.Value.Name.Contains(filterRooms) ||
                    canDisplaySessionRooms == false && room.Value.Name.Contains("_") ||
                    !string.IsNullOrEmpty(onlyDisplaySessionRooms) && 
                    canDisplaySessionRooms == true && room.Value.Name.Contains("_") && 
                    room.Value.Name.Split('_')[0] != onlyDisplaySessionRooms
                )
                {
                    if (debugging == true) Debug.Log("Skipping room because of filter value");
                    continue;
                }
                else if (room.Value.IsVisible == false)
                {
                    if (debugging == true) Debug.Log("Skipping room because invisible.");
                    continue;
                }

                if (room.Value.Name.Contains("_") && canDisplaySessionRooms == true)
                {
                    if (debugging == true) Debug.Log("Creating session room button.");
                    GenerateRoomButton(room.Value.Name, room.Value);
                }
                else
                {
                    if (debugging == true) Debug.Log("Creating room button.");
                    GenerateRoomButton(room.Value.Name, room.Value);
                }
            }
        }

        /// <summary>
        /// Sets the room button values based on the room info. Also makes sure its set properly as 
        /// a child object and its scale is correct.
        /// </summary>
        /// <param name="roomName">string type, the name of the room to display</param>
        /// <param name="roomInfo">RoomInfo type, the information used to join the room if this is clicked</param>
        protected virtual void GenerateRoomButton(string roomName, RoomInfo roomInfo)
        {
            GameObject roomBtn = Instantiate(roomButton);
            roomBtn.transform.SetParent(parentObj);
            roomBtn.transform.localScale = new Vector3(1, 1, 1);
            roomBtn.transform.localPosition = Vector3.zero;
            if (roomBtn.GetComponent<RoomButton>())
            {
                roomBtn.GetComponent<RoomButton>().SetRoomValues(roomInfo);
                roomBtn.GetComponent<RoomButton>().SetRoomName(roomName);
            }
        }
    }
}