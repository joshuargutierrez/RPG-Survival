using Photon.Pun;
using UnityEngine;

namespace CBGames.Player
{
    [AddComponentMenu("CB GAMES/Player/MP Components/MP vSimpleInput")]
    public class MP_vSimpleInput : vSimpleInput
    {
        /// <summary>
        /// Calls the `NetworkOnPressInput` when the owner player presses 
        /// the selected input button. It triggers the `OnPressInput` 
        /// UnityEvent for all the network players as well as the owner.
        /// </summary>
        protected virtual void Update()
        {
            if (GetComponent<PhotonView>().IsMine)
            {
                if (input.GetButtonDown() && gameObject.activeSelf)
                {
                    if (disableThisObjectAfterInput)
                    {
                        this.gameObject.SetActive(false);
                    }

                    GetComponent<PhotonView>().RPC("NetworkOnPressInput", RpcTarget.All);
                }
            }
        }
        [PunRPC]
        void NetworkOnPressInput()
        {
            OnPressInput.Invoke();
        }
    }
}