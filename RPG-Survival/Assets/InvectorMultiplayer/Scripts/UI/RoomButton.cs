using CBGames.Core;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace CBGames.UI
{
    [AddComponentMenu("CB GAMES/UI/Uncatagorized/Room Button")]
    public class RoomButton : MonoBehaviour
    {
        [Tooltip("The text that will display what the name of this room is.")]
        public Text roomName = null;
        [Tooltip("The text that will display the current number of players in this room")]
        public Text numberOfPlayers = null;
        [Tooltip("The text that will display the max number of players in this room")]
        public Text maxNumOfPlayers = null;
        [Tooltip("The text that will display if a player is in a lobby or not")]
        public Text isOpen = null;
        [Tooltip("The database that holds all the information about all given scenes.")]
        public SceneDatabase database;
        [Tooltip("The password required for this room. Do not set here unless used for testing.")]
        [SerializeField] protected string _password = "";
        [Tooltip("The scene index to load when this room button is clicked.")]
        public int indexToLoad = 0;

        protected string roomJoinName = "";
        protected List<LobbyItem> scenes = new List<LobbyItem>();
        protected RoomInfo _roomInfo;
        protected UICoreLogic logic;

        protected virtual void Start()
        {
            logic = FindObjectOfType<UICoreLogic>();    
        }

        /// <summary>
        /// Sets the display values based on the input values.
        /// </summary>
        /// <param name="inputRoomName">string type, the name of the room to join</param>
        /// <param name="numOfPlayers">int type, the number of players currently in this room.</param>
        /// <param name="inputIsOpen">bool type, is this room currently joinable?</param>
        /// <param name="roomDisplayName">string type, the name of the room to display</param>
        public virtual void SetValues(string inputRoomName, int numOfPlayers, bool inputIsOpen, string roomDisplayName = null)
        {
            if (roomName != null) roomName.text = inputRoomName;
            if (numberOfPlayers != null) numberOfPlayers.text = numberOfPlayers.ToString();
            if (isOpen != null) isOpen.text = (inputIsOpen == true) ? "OPEN" : "CLOSED";
        }

        /// <summary>
        /// Set the name of the photon room to join when clicking this room button based 
        /// on the `inputRoomName`
        /// </summary>
        /// <param name="inputRoomName">string type, the photon room name to join.</param>
        public virtual void SetRoomName(string inputRoomName)
        {
            if (roomName != null)
            {
                roomName.text = inputRoomName;
            }
            roomJoinName = inputRoomName;
        }

        /// <summary>
        /// Set the display values based on the input values.
        /// </summary>
        /// <param name="room">RoomInfo type, extracts room info to display based on this</param>
        /// <param name="openText">string type, the string to display if the room is open</param>
        /// <param name="closedText">string type, the string to display if the room is closed</param>
        public virtual void SetRoomValues(RoomInfo room, string openText = "OPEN", string closedText = "CLOSED")
        {
            _roomInfo = room;
            roomJoinName = room.Name;
            if (roomName != null)
            {
                roomName.text = room.Name;
            }
            if (numberOfPlayers != null)
            {
                numberOfPlayers.text = room.PlayerCount.ToString(); ;
            }
            if (maxNumOfPlayers != null)
            {
                maxNumOfPlayers.text = room.MaxPlayers.ToString();
            }
            if (isOpen != null)
            {
                isOpen.text = (room.IsOpen == true) ? openText : closedText;
            }
            if (room.CustomProperties.ContainsKey(RoomProperty.Password))
            {
                _password = (string)room.CustomProperties[RoomProperty.Password];
            }
        }

        /// <summary>
        /// Calls `UICoreLogic`'s `JoinRoom` function with this set room name.
        /// </summary>
        public virtual void JoinLobby()
        {
            logic.JoinRoom(roomJoinName);
        }

        /// <summary>
        /// Calls the `NetworkManager`'s `JoinRoom` function with the saved room name.
        /// If the current scene index also doesn't match it calls the `NetworkLoadLevel`
        /// function from the `NetworkManager` as well.
        /// </summary>
        public virtual void JoinRoom()
        {
            if (scenes.Count > 1 && FindObjectOfType<ExampleUI>())
            {
                FindObjectOfType<ExampleUI>().DisplayRoomOptions(scenes);
            }
            else
            {
                try
                {
                    NetworkManager.networkManager.JoinRoom(roomJoinName);
                    if (SceneManager.GetActiveScene().buildIndex != indexToLoad)
                    {
                        NetworkManager.networkManager.NetworkLoadLevel(indexToLoad, null, false);
                    }
                }
                catch
                {
                    Debug.LogError("RoomButton - Failed To Load Room: " + roomJoinName +" or join scene index with: "+ indexToLoad);
                }
            }
        }

        /// <summary>
        /// Sets the values of this component based on this input.
        /// </summary>
        /// <param name="sceneItem">LobbyItem type, Extracts the photon room name from this</param>
        public virtual void AddAvailableScene(LobbyItem sceneItem)
        {
            scenes.Add(sceneItem);
            if (scenes.Count == 1)
            {
                roomJoinName = sceneItem.rawRoomName;
                if (string.IsNullOrEmpty(sceneItem.sceneName) || sceneItem.sceneName.Contains("Lobby"))
                {
                    indexToLoad = database.storedScenesData.Find(x => x.index == FindObjectOfType<ExampleUI>().lobbyIndex).index;
                }
                else
                {
                    indexToLoad = database.storedScenesData.Find(x => x.sceneName == sceneItem.sceneName).index;
                }
                SetValues(scenes[0].displayName, scenes[0].playerCount, scenes[0].isVisible, scenes[0].rawRoomName);
            }
            SetTotalPlayerCount();
        }

        /// <summary>
        /// Sets the `numberOfPlayers` string value to be the total number of players currently in
        /// this photon room.
        /// </summary>
        public virtual void SetTotalPlayerCount()
        {
            if (numberOfPlayers == null) return;
            int total = 0;
            foreach(LobbyItem scene in scenes)
            {
                total += scene.playerCount;
            }
            numberOfPlayers.text = total.ToString();
        }
    }
}