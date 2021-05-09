using CBGames.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CBGames.UI
{
    [AddComponentMenu("CB GAMES/UI/Chatbox/Broadcast Receiver")]
    public class BroadcastReceiver : MonoBehaviour
    {
        [Tooltip("The ScrollView object in this UI.")]
        [SerializeField] private GameObject scrollView = null;
        [Tooltip("The content object of the scrollView.")]
        [SerializeField] private GameObject content = null;
        [Tooltip("How long to have the message be visible before it is removed.")]
        [SerializeField] private float messageDestroyTime = 15.0f;
        [Space(10)]
        [Header("MESSAGE TYPES")]
        [Tooltip("The prefab that will be instantiated as a child of the content when " +
            "this receive a \"DEATH\" broadcast message.")]
        [SerializeField] private GameObject deathMessage = null;

        private bool isEnabled = false;

        /// <summary>
        /// Disables the scrollview
        /// </summary>
        protected virtual void Awake()
        {
            scrollView.SetActive(false);
        }

        /// <summary>
        /// Perform certain actions based on the type of `BroadCastMessage` that is 
        /// received from the data channel on the ChatBox.
        /// </summary>
        /// <param name="message">BroadCastMessage type, the message and the type of message</param>
        public virtual void ReceiveBroadCastMessage(BroadCastMessage message)
        {
            isEnabled = true;
            scrollView.SetActive(true);
            if (message.speaker == "DEATH")
            {
                InstantiateDeathMessage(message.message);
            }
        }

        /// <summary>
        /// Adds a death message object to the message view.
        /// </summary>
        /// <param name="message">string type, the message to display</param>
        protected virtual void InstantiateDeathMessage(string message)
        {
            GameObject msgObject = (GameObject)Instantiate(deathMessage);
            if (msgObject.GetComponent<Text>())
            {
                msgObject.GetComponent<Text>().text = message;
            }
            else
            {
                msgObject.GetComponentInChildren<Text>().text = message;
            }
            msgObject.transform.SetParent(content.transform);
            msgObject.transform.localScale = new Vector3(1, 1, 1);
            Destroy(msgObject, messageDestroyTime);
        }

        /// <summary>
        /// Enables/Disables the scroll view based on how many messages there are 
        /// in the view.
        /// </summary>
        protected virtual void Update()
        {
            if (isEnabled == true)
            {
                if (content.transform.childCount < 1)
                {
                    isEnabled = false;
                    scrollView.SetActive(false);
                }
            }
        }
    }
}