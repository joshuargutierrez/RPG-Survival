using UnityEngine;
using Photon.Pun;
using ExitGames.Client.Photon;
using CBGames.Core;

namespace CBGames.Testing
{
    [AddComponentMenu("CB GAMES/Testing/Network Control Tester")]
    public class NetworkControl : MonoBehaviour
    {
        [Tooltip("Position of the window")]
        protected Rect WindowRect = new Rect(Screen.width-270, 0, 200, 200);
        [Tooltip("The id of the unity window, must be unique or it causes issues.")]
        protected int WindowId = 909;
        [Tooltip("Being the network simulation based on the settings? " +
            "Enables and disables the simulation. A sudden, big change of network " +
            "conditions might result in disconnects.")]
        [SerializeField] protected bool simulate = false;
        [Tooltip("The amount of lag to simulate over the network. " +
            "Adds a fixed delay to all outgoing and incoming messages. In milliseconds")]
        [SerializeField] protected float lagAmount = 0.0f;
        [Tooltip("The amount of network jitter to simulate over the network. " +
            "Adds a random delay of \"up to X milliseconds\" per message")]
        [SerializeField] protected float jitterAmount = 0.0f;
        [Tooltip("How many lost network packets you want to simulate. Drops the set " +
            "percentage of messages. You can expect less than 2% drop in the internet today.")]
        [SerializeField] protected float lossPercent = 0.0f;

        protected PhotonPeer Peer;

        protected virtual void Start()
        {
            Peer = PhotonNetwork.NetworkingClient.LoadBalancingPeer;
        }

        /// <summary>
        /// Use to automatically update the network settings according to the settings you apply 
        /// in realtime.
        /// </summary>
        protected virtual void Update()
        {
            Peer.IsSimulationEnabled = simulate;
            Peer.NetworkSimulationSettings.IncomingLag = (int)lagAmount;
            Peer.NetworkSimulationSettings.OutgoingLag = (int)lagAmount;
            Peer.NetworkSimulationSettings.IncomingJitter = (int)jitterAmount;
            Peer.NetworkSimulationSettings.OutgoingJitter = (int)jitterAmount;
            Peer.NetworkSimulationSettings.IncomingLossPercentage = (int)lossPercent;
            Peer.NetworkSimulationSettings.OutgoingLossPercentage = (int)lossPercent;
        }

        /// <summary>
        /// Will disconnect from photon.
        /// </summary>
        public void DisconnectFromPhoton()
        {
            NetworkManager.networkManager.Disconnect();
        }

        /// <summary>
        /// Leaves your current room.
        /// </summary>
        public void LeaveRoom()
        {
            Debug.Log("CALLED LEAVE ROOM");
            NetworkManager.networkManager.LeaveRoom();
        }

        /// <summary>
        /// Sets the Network Managers reconnect value.
        /// </summary>
        /// <param name="isEnabled">reconnect is true or false</param>
        public void SetReconnect(bool isEnabled)
        {
            NetworkManager.networkManager.reconnect = isEnabled;
        }

        /// <summary>
        /// Produces a visual interface at runtime which makes this useable with builds as well.
        /// </summary>
        protected virtual void OnGUI()
        {
            if (!PhotonNetwork.IsConnected)
            {
                WindowRect = GUILayout.Window(WindowId, WindowRect, NotConnected, "Network Contol Simulator");
            }
            else if (PhotonNetwork.CurrentRoom == null || !PhotonNetwork.IsConnected)
            {
                WindowRect = GUILayout.Window(WindowId, WindowRect, NotInRoom, "Network Contol Simulator");
            }
            else
            {
                if (this.Peer != null)
                {
                    WindowRect = GUILayout.Window(WindowId, WindowRect, DrawConnectedToRoomWindow, "Network Contol Simulator");
                }
                else
                {
                    WindowRect = GUILayout.Window(WindowId, WindowRect, DrawConnectedToRoomWindow, "Network Contol Simulator");
                }
            }
        }

        void NotConnected(int windowId)
        {
            GUILayout.Label("Not connected to photon.");
        }

        void NotInRoom(int windowId)
        {
            GUILayout.Label("Not connected a photon room.");
        }

        void WaitForPeer(int windowId)
        {
            GUILayout.Label("Waiting for Photon Load Balancing Peer.");
        }

        void DrawConnectedToRoomWindow(int windowId)
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Disconnect"))
            {
                DisconnectFromPhoton();
            }
            if (GUILayout.Button("Leave Room"))
            {
                LeaveRoom();
            }
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Enable Reconnect"))
            {
                SetReconnect(true);
            }
            if (GUILayout.Button("Disable Reconnect"))
            {
                SetReconnect(false);
            }
            GUILayout.EndHorizontal();

            if (PhotonNetwork.CurrentRoom != null)
            {
                GUILayout.Label(string.Format("Photon Room Name: {0}", PhotonNetwork.CurrentRoom.Name));
            }
            else
            {
                GUILayout.Label(string.Format("Photon Room Name: {0}", "No Connected"));
            }
            GUILayout.Space(5);
            GUILayout.Label(string.Format("Round Trip Time:{0,4}, Variance: +/-{1,3}", Peer.RoundTripTime, Peer.RoundTripTimeVariance));

            // Enable or Disables Simulation
            simulate = GUILayout.Toggle(simulate, "Simulate");

            // Lag Simulator
            GUILayout.Label("Lag Amount: " + lagAmount);
            lagAmount = GUILayout.HorizontalSlider(lagAmount, 0, 500);

            // Jitter Simulator
            GUILayout.Label("Jitter Amount: " + jitterAmount);
            jitterAmount = GUILayout.HorizontalSlider(jitterAmount, 0, 100);

            // Loss Simulator
            GUILayout.Label("Loss Amount: " + lossPercent);
            lossPercent = GUILayout.HorizontalSlider(lossPercent, 0, 20);
        }
    }
}