using Photon.Realtime;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace CBGames.Core
{
    public class CoreDelegates : MonoBehaviour
    {
        public delegate void PlayerDelegate(Photon.Realtime.Player player);
        public delegate void DisconnectCauseDelegate(DisconnectCause cause);
        public delegate void RoomFailure(short returnCode, string message);
        public delegate void RoomListUpdate(Dictionary<string, RoomInfo> roomList);
        public delegate void BasicDelegate();
        public delegate void StringDelegate(string input);
        public delegate void ChatSubChannels(string[] channels, bool[] results);
        public delegate void ChatChannels(string[] channels);
        public delegate void ChatUserChannel(string channel, string user);
        public delegate void SentChatMessageDelegate(SentChatMessage message);
        public delegate void ChatDataMessage(Type type, string incomingData);
    }
}