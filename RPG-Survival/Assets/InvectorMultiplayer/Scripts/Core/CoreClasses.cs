using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Runtime.Serialization.Formatters.Binary;
using Invector.vCharacterController;
using System.IO;
using CBGames.Player;
using UnityEngine.SceneManagement;
using Invector.vItemManager;

namespace CBGames.Core
{
    public class StringIntClass
    {
        public string name = null;
        public int id = 0;
        private int v;

        public StringIntClass(string name, int v)
        {
            this.name = name;
            this.v = v;
        }
    }
    public class StaticMethods
    {
        public static string RandomHash(int length)
        {
            string glyphs = "abcdefghijklmnopqrstuvwxyz0123456789";
            string hash = "";
            for (int i = 0; i < length; i++)
            {
                hash += glyphs[Random.Range(0, glyphs.Length)];
            }
            return hash;
        }

        public static byte[] SerializeObject(object target)
        {
            var binFormatter = new BinaryFormatter();
            var mStream = new MemoryStream();
            binFormatter.Serialize(mStream, target);
            return mStream.ToArray();
        }
        public static object DeSerializeObject(byte[] serializedObject)
        {
            var mStream = new MemoryStream();
            var binFormatter = new BinaryFormatter();
            mStream.Write(serializedObject, 0, serializedObject.Length);
            mStream.Position = 0;

            return binFormatter.Deserialize(mStream);
        }

        public static Transform FindTargetChild(int[] tree, Transform startingTransform)
        {
            try
            {
                if (tree.Length < 1) return null;
                Transform target = startingTransform;
                foreach (int childIndex in tree.Reverse())
                {
                    target = target.GetChild(childIndex);
                    if (!target) return target;
                }
                return target;
            }
            catch
            {
                return null;
            }
        }

        public static int[] BuildChildTree(Transform parentTransform, Transform startingTransform, bool debugging = false)
        {
            List<int> tempList = new List<int>();
            Transform target = startingTransform;
            while (target.GetInstanceID() != parentTransform.GetInstanceID())
            {
                if (debugging == true) Debug.Log(target.name, target);
                tempList.Add(target.GetSiblingIndex());
                target = target.parent;
            }
            return tempList.ToArray();
        }

        public static void SetUIOpacity(float amount, List<Image> images = null, List<Text> texts = null)
        {
            if (images != null)
            {
                foreach (Image image in images)
                {
                    Color tmp = image.color;
                    tmp.a = amount;
                    image.color = tmp;
                }
            }
            if (texts != null)
            {
                foreach (Text text in texts)
                {
                    Color tmp = text.color;
                    tmp.a = amount;
                    text.color = tmp;
                }
            }
        }
    }

    public class SaveLogic
    {
        public static PlayerData SavePlayerData(vThirdPersonController controller)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            string path = Application.persistentDataPath + "/player." + controller.gameObject.name.Replace("(Clone)", "").Replace("Instance", "");
            FileStream stream = new FileStream(path, FileMode.Create);
            PlayerData data = new PlayerData(controller);
            formatter.Serialize(stream, data);
            stream.Close();

            return data;
        }

        public static PlayerData LoadPlayerData(string playerName)
        {
            string path = Application.persistentDataPath + "/player." + playerName;
            if (File.Exists(path))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                FileStream stream = new FileStream(path, FileMode.Open);
                PlayerData data = (PlayerData)formatter.Deserialize(stream);
                stream.Close();
                return data;
            }
            else
            {
                Debug.LogError("PlayerData file not found at: " + path);
                return null;
            }
        }
    }

    [System.Serializable] public class IntUnityEvent : UnityEvent<int> { };
    [System.Serializable] public class FloatUnityEvent : UnityEvent<float> { };
    [System.Serializable] public class StringUnityEvent : UnityEvent<string> { };
    [System.Serializable] public class PhotonPlayerEvent : UnityEvent<Photon.Realtime.Player> { };
    [System.Serializable] public class PlayerListEvent : UnityEvent<PlayerListInfo> { };
    [System.Serializable] public class SceneEvent : UnityEvent<Scene> { };

    public class BroadCastMessage
    {
        public string speaker = "";
        public string message = "";

        public BroadCastMessage(string inputSpeaker, string inputMessage)
        {
            this.speaker = inputSpeaker;
            this.message = inputMessage;
        }
    }

    [System.Serializable]
    public class PlayerListInfo
    {
        public string userId;
        public int sceneIndex;
        public bool inSession;

        public PlayerListInfo(string inputUserId, bool inputStatus, int inputSceneIndex=99999)
        {
            this.userId = inputUserId;
            this.inSession = inputStatus;
            this.sceneIndex = inputSceneIndex;
        }
    }
    
    [System.Serializable] public class BroadCastUnityEvent : UnityEvent<BroadCastMessage> { };

    public class SentChatMessage
    {
        public string playerName = "";
        public string message = "";

        public SentChatMessage(string inputName, string inputMessage)
        {
            this.playerName = inputName;
            this.message = inputMessage;
        }
    }
    [System.Serializable] public class SentChatUnityEvent : UnityEvent<SentChatMessage> { };
    public enum PhotonChatRegions { US, EU, Asia};

    public class LobbyItem
    {
        public string displayName = "";
        public string rawRoomName = "";
        public string sceneName = "";
        public int playerCount = 0;
        public bool isVisible = false;
        public bool isOpen = false;

        public LobbyItem(string inputDisplayName, string inputRawRoomName, string inputSceneName, int inputPlayerCount, bool inputVisible, bool inputOpen)
        {
            displayName = inputDisplayName;
            rawRoomName = inputRawRoomName;
            sceneName = inputSceneName;
            playerCount = inputPlayerCount;
            isVisible = inputVisible;
            isOpen = inputOpen;
        }
    }
}