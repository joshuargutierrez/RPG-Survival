using UnityEngine;
using UnityEngine.UI;

namespace Invector.vCharacterController
{
    public class vThrowUI : MonoBehaviour
    {        
        public Text maxThrowCount;
        public Text currentThrowCount;       

        internal virtual void UpdateCount(vThrowManager throwManager)
        {
            currentThrowCount.text = throwManager.currentThrowObject.ToString();
            maxThrowCount.text = throwManager.maxThrowObjects.ToString();
        }
    }
}