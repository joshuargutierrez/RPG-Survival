/*
using Photon.Pun;
using Invector.vCharacterController.vActions;
using Invector.vCharacterController;
using UnityEngine;

namespace CBGames.Player
{
    [AddComponentMenu("CB GAMES/Player/MP Components/MP vGenericAction")]
    public class MP_vGenericAction : vGenericAction
    {
        //protected override void Start()
        //{
        //    if (GetComponent<PhotonView>().IsMine == true && PhotonNetwork.IsConnected == true)
        //    {
        //        base.Start();
        //    }
        //    else
        //    {
        //        enabled = false;
        //    }
        //}

        /// <summary>
        /// Is used to call `AnimatorActionState` RPC which will have the network player
        /// mimic the action stats of the owner player.
        /// </summary>
        public override void TriggerAnimation()
        {
            if (playingAnimation || actionStarted) return;

            if (triggerAction.animatorActionState != 0)
            {
                tpInput.cc.SetActionState(triggerAction.animatorActionState);
                GetComponent<PhotonView>().RPC("AnimatorActionState", RpcTarget.Others, triggerAction.animatorActionState);
            }

            // trigger the animation behaviour & match target
            if (!string.IsNullOrEmpty(triggerAction.playAnimation))
            {
                if (!actionStarted)
                {
                    actionStarted = true;
                    playingAnimation = true;
                    tpInput.cc.animator.CrossFadeInFixedTime(triggerAction.playAnimation, 0.1f);    // trigger the action animation clip
                    if (!string.IsNullOrEmpty(triggerAction.customCameraState))
                        tpInput.ChangeCameraState(triggerAction.customCameraState, true);           // change current camera state to a custom
                }
            }
            else
            {
                actionStarted = true;
            }

            animationBehaviourDelay = 0.2f;

            if (!string.IsNullOrEmpty(triggerAction.playAnimation) && playingAnimation == false)
            {
                GetComponent<PhotonView>().RPC("CrossFadeInFixedTime", RpcTarget.Others, triggerAction.playAnimation, 0.1f);
            }

        }

        /// <summary>
        /// Calls `AnimatorActionState` RPC which will have the network player reset
        /// ther action stat when the owner player does.
        /// </summary>
        public override void ResetActionState()
        {
            if (triggerAction && triggerAction.resetAnimatorActionState)
            {
                tpInput.cc.SetActionState(0);
                GetComponent<PhotonView>().RPC("AnimatorActionState", RpcTarget.Others, 0);
            }
        }

        /// <summary>
        /// Calls the `AnimatorActionState` RPC which will have the network player 
        /// follow the action stat of the owner player.
        /// </summary>
        public override void ResetTriggerSettings(bool removeTrigger = true)
        {
            GetComponent<PhotonView>().RPC("AnimatorActionState", RpcTarget.Others, 0);
            if (debugMode)
            {
                Debug.Log($"<b>GenericAction: </b>Reset Trigger Settings ");
            }

            // reset player gravity and collision
            EnablePlayerGravityAndCollision();
            // reset the Animator parameter ActionState back to 0 
            ResetActionState();
            // reset the CameraState to the Default state
            if (triggerAction != null && !string.IsNullOrEmpty(triggerAction.customCameraState)) tpInput.ResetCameraState();
            // remove the collider from the actions list
            if (triggerAction != null && actions.ContainsKey(triggerAction._collider) && removeTrigger)
            {
                actions.Remove(triggerAction._collider);
            }
            triggerAction = null;
            doingAction = false;
            actionStarted = false;
        }

    /// <summary>
    /// Calls `GenericAction_EndAction` RPC which will have the network player
    /// end its action when the owner player does.
    /// </summary>
    protected override void EndAction()
        {
            if (GetComponent<PhotonView>().IsMine == true)
            {
                GetComponent<PhotonView>().RPC("GenericAction_EndAction", RpcTarget.Others);
            }
            OnEndAction.Invoke(triggerAction);

            var trigger = triggerAction;
            if (trigger != null)
            {
                // triggers the OnEndAnimation Event
                trigger.OnEndAnimation.Invoke();
            }
            // reset GenericAction variables so you can use it again
            ResetTriggerSettings();

            // Destroy trigger affter reset all settings
            if (trigger != null && trigger.destroyAfter) StartCoroutine(DestroyActionDelay(trigger));

            //DebugAction("End Action ");
        }

        [PunRPC]
        void AnimatorActionState(int state)
        {
            tpInput = (!tpInput) ? GetComponent<vThirdPersonInput>() : tpInput;
            if (tpInput)
            {
                tpInput.cc.SetActionState(state);
            }
        }
        [PunRPC]
        void GenericAction_EndAction()
        {
            EndAction();
        }
    }
}
*/
