using UnityEngine;
using Photon.Pun;
using Invector.vCharacterController;            
using Invector.vMelee;
using Invector.vItemManager;
using Invector.vCharacterController.vActions;
using Invector;
using System.Collections.Generic;
using CBGames.Inspector;
using CBGames.Core;
using System.Reflection;
using System.Collections;
using CBGames.Objects;
using CBGames.UI;

#region Shooter Template
using Invector.vShooter;
using Invector.IK;
using System.Threading;
using Invector.vCamera;
#endregion

namespace CBGames.Player
{
    [System.Serializable]
    public class SerializedDamage
    {
        public int damageValue = 15;
        public float staminaBlockCost = 5;
        public float staminaRecoveryDelay = 1;
        public bool ignoreDefense;
        public bool activeRagdoll;
        public int senderViewId;
        public int receiverViewId;
        public Vector3 hitPosition;
        public bool hitReaction = true;
        public int recoil_id = 0;
        public int reaction_id = 0;
        public string damageType;

        public SerializedDamage(vDamage damage)
        {
            this.damageValue = damage.damageValue;
            this.staminaBlockCost = damage.staminaBlockCost;
            this.staminaRecoveryDelay = damage.staminaRecoveryDelay;
            this.ignoreDefense = damage.ignoreDefense;
            this.activeRagdoll = damage.activeRagdoll;
            this.senderViewId = GetViewId(damage.sender);
            this.receiverViewId = GetViewId(damage.receiver); ;
            this.hitPosition = damage.hitPosition;
            this.hitReaction = damage.hitReaction;
            this.recoil_id = damage.recoil_id;
            this.reaction_id = damage.reaction_id;
            this.damageType = damage.damageType;
        }

        int GetViewId(Transform target)
        {
            int id = 9999999;
            if (target && !target.GetComponent<PhotonView>())
            {
                if (target.root.GetComponent<PhotonView>())
                {
                    id = target.root.GetComponent<PhotonView>().ViewID;
                }
            }
            else if (target && target.GetComponent<PhotonView>())
            {
                id = target.GetComponent<PhotonView>().ViewID;
            }

            return id;
        }

        public vDamage TovDamage()
        {
            vDamage damage = new vDamage();

            damage.damageValue = this.damageValue;
            damage.staminaBlockCost = this.staminaBlockCost;
            damage.staminaRecoveryDelay = this.staminaRecoveryDelay;
            damage.ignoreDefense = this.ignoreDefense;
            damage.activeRagdoll = this.activeRagdoll;
            damage.sender = (this.senderViewId != 9999999 && PhotonView.Find(this.senderViewId)) ? PhotonView.Find(this.senderViewId).transform : null;
            damage.receiver = (this.receiverViewId != 9999999 && PhotonView.Find(this.receiverViewId)) ? PhotonView.Find(this.receiverViewId).transform : null;
            damage.hitPosition = this.hitPosition;
            damage.hitReaction = this.hitReaction;
            damage.recoil_id = this.recoil_id;
            damage.reaction_id = this.reaction_id;
            damage.damageType = this.damageType;

            return damage;
        }
    }

    [AddComponentMenu("CB GAMES/Player/Sync Player")]
    [RequireComponent(typeof(PhotonView))]
    [DisallowMultipleComponent]
    public class SyncPlayer : MonoBehaviourPunCallbacks, IPunObservable
    {
        #region Sync Components
        #region Position/Rotation
        protected bool sendPosRot = true;
        protected Vector3 velocity = Vector3.zero;
        protected Vector3 lastPos = Vector3.zero;
        protected Vector3 realPos = Vector3.zero;
        protected Quaternion realRot = Quaternion.identity;
        #endregion

        #region Animations
        protected bool sendAnimations = true;
        protected Dictionary<string, AnimatorControllerParameterType> animParams = new Dictionary<string, AnimatorControllerParameterType>();
        #pragma warning disable 0414
        protected RigidbodyConstraints originalContraints;
        protected bool contraintsSet = false;
        #pragma warning restore 0414
        protected bool _ragdolled = false;
        protected int arrowCount = 0;
        #endregion

        #region Uncatagorized
        protected PhotonView view;
        protected Animator animator;
        protected vLadderAction ladderAction;
        protected vThirdPersonController tpc;
        protected int hitDirectionHash;
        protected int reactionIDHash;
        protected int triggerReactionHash = -1;
        protected int triggerResetStateHash = -1;
        protected int recoilIDHash;
        protected int triggerRecoilHash = -1;
        protected List<int> triggerNames = new List<int>();
        protected vItemListData itemData;
        protected vInventory inventory;

        protected BindingFlags flags;
        protected bool _sentRolling = false;
        protected bool _cursorLocked = false;

        protected vWeaponHolderManager whm;
        [Tooltip("Hidden variable. The item dictionary of the player. This is used to save it to the `PlayerData`.")]
        [HideInInspector]
        public Dictionary<int, GameObject> itemDict = new Dictionary<int, GameObject>();

        protected string lastDamageType;
        protected GameObject lastDamageObject;
        protected GameObject lastDamageSender;
        protected vMeleeManager meleeManager = null;
        #endregion
        #endregion

        #region EditorOnly Variables
        [HideInInspector] public bool showBonesToSync = false;
        #endregion

        #region Modifiables

        #region Animations
        [Tooltip("This will sync the bone positions. Makes it so players on the network can see where this player is looking.")]
        [SerializeField] protected bool _syncAnimations = true;
        #endregion

        #region Position/Rotation Data
        [Tooltip("How fast to move to the new position when the networked equivalent of this player receives an update from the server. High values will 'snap' to position, while too low values will feel sluggish or unresponsive. This is something to play with until you get right.")]
        public float _positionLerpRate = 5.0f;
        [Tooltip("How fast to move to the new rotation when the networked equivalent of this player receives an update from the server. High values will 'snap' to position, while too low values will feel sluggish or unresponsive. This is something to play with until you get right.")]
        public float _rotationLerpRate = 5.0f;
        #endregion

        #region Tagging/Layers/Teams
        [TagSelector, Tooltip("If this is not a locally controlled version of this player change the objects tag to be this. " +
            "Does not change things tagged 'Weapon'.")]
        public string noneLocalTag = "Enemy";
        [LayerSelector, Tooltip("If this is not a locally controlled version of this player change the objects layer to be this. " +
            "Only changes the root (this) and the hips bone.")]
        public int _nonAuthoritativeLayer = 9;
        
        #endregion

        #region Hidden
        [Tooltip("The name of the team you're currently on. Set this by calling \"SetTeamName\" function on the " +
            "NetworkManager component.")]
        public string teamName = "";
        #endregion
        #endregion

        #region Initializations 
        /// <summary>
        /// Sets the `itemData` variable and disables the enable switching on the 
        /// death camera by calling the `EnableSwitching` function.
        /// </summary>
        protected virtual void Awake()
        {
            ladderAction = GetComponent<vLadderAction>();
            tpc = GetComponent<vThirdPersonController>();
            GetComponent<vThirdPersonInput>().cc = tpc;
            if (GetComponent<vItemManager>())
            {
                itemData = GetComponent<vItemManager>().itemListData;
            }
            else
            {
                Debug.LogWarning("You don't have a vItemManager component attached. That means you won't be able to sync items across the network.", gameObject);
            }
            flags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance;
            if (tpc && !tpc.animator)
            {
                tpc.Init();
            }
            if (FindObjectOfType<DeathCamera>())
            {
                FindObjectOfType<DeathCamera>().EnableSwitching(false);
            }
        }

        /// <summary>
        /// Builds the item dictionary used to spawn weapons over the network based on 
        /// the weaponds id. Also disables/enables a series of components on this player
        /// if you're a networked player of the owner player. Disables the death camera
        /// and builds the animator parameters to sync over the network. Also sets the
        /// layers and tags of the player via the `SetLayer` and `SetTags` functions.
        /// Finally enables/disables the triggers on the body parts via the `SetBodyParts`
        /// function.
        /// </summary>
        protected virtual void Start()
        {
            realPos = this.transform.position;
            realRot = this.transform.rotation;
            animator = GetComponent<Animator>();
            view = GetComponent<PhotonView>();
            whm = (GetComponent<vWeaponHolderManager>()) ? GetComponent<vWeaponHolderManager>() : null;
            
            if (GetComponent<vHitDamageParticle>()) GetComponent<vHitDamageParticle>().enabled = true;
            if (GetComponent<vMeleeManager>())
            {
                meleeManager = GetComponent<vMeleeManager>();
            }

            if (view.IsMine == false)
            {
                if (GetComponentInChildren<vHUDController>())
                {
                    GetComponentInChildren<vHUDController>().gameObject.SetActive(false);
                }
                tpc.isImmortal = true;
                BuildItemDictionary();

                if (GetComponent<vItemManager>()) GetComponent<vItemManager>().enabled = false;
                if (GetComponent<vWeaponHolderManager>()) GetComponent<vWeaponHolderManager>().enabled = false;
                //if (GetComponent<vGenericAction>()) GetComponent<vGenericAction>().enabled = false;
                //if (GetComponent<vGenericAnimation>()) GetComponent<vGenericAnimation>().enabled = false;
                if (GetComponent<vLockOn>()) GetComponent<vLockOn>().enabled = false;
                if (GetComponent<vDrawHideMeleeWeapons>()) GetComponent<vDrawHideMeleeWeapons>().enabled = false;
                if (GetComponentInChildren<vOpenCloseInventoryTrigger>())
                {
                    GetComponentInChildren<vOpenCloseInventoryTrigger>().gameObject.SetActive(false);
                }
                StartCoroutine(WaitToDisable());

                if (!string.IsNullOrEmpty(noneLocalTag))
                {
                    this.tag = noneLocalTag;
                }
                SetLayer();
                SetTags(animator.GetBoneTransform(HumanBodyBones.Hips).transform);
                #region Shooter Template
                if (GetComponent<vShooterManager>())
                {
                    if (GetComponent<vShooterManager>().ignoreTags.Contains("Player"))
                    {
                        GetComponent<vShooterManager>().ignoreTags.Remove("Player");
                    }
                }
                #endregion
            }
            else
            {
                #region Shooter Template
                SetBodyParts(true);
                #endregion
                EnableDeathCamera(false);
                if (animator.GetValidParameter("HitDirection") == null || animator.GetValidParameter("ReactionID") == null || 
                    animator.GetValidParameter("TriggerReaction") == null || animator.GetValidParameter("ResetState") == null ||
                    animator.GetValidParameter("RecoilID") == null || animator.GetValidParameter("TriggerRecoil") == null)
                {
                    Debug.LogError("You appear to be using the basic controller. This controller is " +
                        "missing animation parameters that are required for this script to work. Please " +
                        "change this to a melee controller at the very LEAST.");
                }
                if (FindObjectOfType<PlayerRespawn>())
                {
                    FindObjectOfType<PlayerRespawn>().SetRespawnState(false);
                }

                GetComponent<PhotonView>().RPC("SetTeam", RpcTarget.AllBuffered, NetworkManager.networkManager.teamName);

                if (GetComponentInChildren<vInventory>())
                {
                    inventory = GetComponentInChildren<vInventory>();
                }
            }

            hitDirectionHash = animator.GetValidParameter("HitDirection").nameHash;
            reactionIDHash = animator.GetValidParameter("ReactionID").nameHash;
            triggerReactionHash = (animator.GetValidParameter("TriggerReaction") != null) ? animator.GetValidParameter("TriggerReaction").nameHash : -1;
            triggerResetStateHash = (animator.GetValidParameter("ResetState") != null) ? animator.GetValidParameter("ResetState").nameHash : -1;
            recoilIDHash = animator.GetValidParameter("RecoilID").nameHash;
            triggerRecoilHash = (animator.GetValidParameter("TriggerRecoil") != null) ? animator.GetValidParameter("TriggerRecoil").nameHash : -1;

            BuildAnimatorParamsDict();
        }
        protected virtual IEnumerator WaitToDisable()
        {
            yield return new WaitForFixedUpdate();
            if (GetComponentInChildren<vInventory>())
            {
                GetComponentInChildren<vInventory>().gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Set the layers to the none owner layers.
        /// </summary>
        protected virtual void SetLayer()
        {
            gameObject.layer = _nonAuthoritativeLayer;
            animator.GetBoneTransform(HumanBodyBones.Hips).transform.parent.gameObject.layer = _nonAuthoritativeLayer;
        }

        /// <summary>
        /// Sets the tags to be the none owner tags on all transforms of this
        /// object.
        /// </summary>
        /// <param name="target"></param>
        protected virtual void SetTags(Transform target)
        {
            target.tag = noneLocalTag;
            foreach (Transform child in target)
            {
                if ((child.tag == "Untagged" || child.tag == "Player") && 
                    child.tag != "Weapon")
                {
                    child.tag = noneLocalTag;
                }
                SetTags(child);
            }
        }
      #region Shooter Template
        /// <summary>
        /// Turns the triggers on/off for the body parts of the player based on the input value.
        /// </summary>
        /// <param name="isTrigger">bool type, enable the triggers for all the body parts?</param>
        protected virtual void SetBodyParts(bool isTrigger)
        {
            SetBodyPart((animator.GetBoneTransform(HumanBodyBones.Hips)) ? animator.GetBoneTransform(HumanBodyBones.Hips).transform : null, isTrigger);

            SetBodyPart((animator.GetBoneTransform(HumanBodyBones.LeftUpperLeg)) ? animator.GetBoneTransform(HumanBodyBones.LeftUpperLeg).transform : null, isTrigger);
            SetBodyPart((animator.GetBoneTransform(HumanBodyBones.LeftLowerLeg)) ? animator.GetBoneTransform(HumanBodyBones.LeftLowerLeg).transform : null, isTrigger);
            SetBodyPart((animator.GetBoneTransform(HumanBodyBones.LeftUpperArm)) ? animator.GetBoneTransform(HumanBodyBones.LeftUpperArm).transform : null, isTrigger);
            SetBodyPart((animator.GetBoneTransform(HumanBodyBones.LeftLowerArm)) ? animator.GetBoneTransform(HumanBodyBones.LeftLowerArm).transform : null, isTrigger);
            SetBodyPart((animator.GetBoneTransform(HumanBodyBones.LeftShoulder)) ? animator.GetBoneTransform(HumanBodyBones.LeftShoulder).transform : null, isTrigger);
            SetBodyPart((animator.GetBoneTransform(HumanBodyBones.LeftHand)) ? animator.GetBoneTransform(HumanBodyBones.LeftHand).transform : null, isTrigger);
            SetBodyPart((animator.GetBoneTransform(HumanBodyBones.LeftFoot)) ? animator.GetBoneTransform(HumanBodyBones.LeftFoot).transform : null, isTrigger);
            //SetBodyPart(animator.GetBoneTransform(HumanBodyBones.LeftToes).transform);

            SetBodyPart((animator.GetBoneTransform(HumanBodyBones.RightUpperLeg)) ? animator.GetBoneTransform(HumanBodyBones.RightUpperLeg).transform : null, isTrigger);
            SetBodyPart((animator.GetBoneTransform(HumanBodyBones.RightLowerLeg)) ? animator.GetBoneTransform(HumanBodyBones.RightLowerLeg).transform : null, isTrigger);
            SetBodyPart((animator.GetBoneTransform(HumanBodyBones.RightUpperArm)) ? animator.GetBoneTransform(HumanBodyBones.RightUpperArm).transform : null, isTrigger);
            SetBodyPart((animator.GetBoneTransform(HumanBodyBones.RightLowerArm)) ? animator.GetBoneTransform(HumanBodyBones.RightLowerArm).transform : null, isTrigger);
            SetBodyPart((animator.GetBoneTransform(HumanBodyBones.RightShoulder)) ? animator.GetBoneTransform(HumanBodyBones.RightShoulder).transform : null, isTrigger);
            SetBodyPart((animator.GetBoneTransform(HumanBodyBones.RightHand)) ? animator.GetBoneTransform(HumanBodyBones.RightHand).transform : null, isTrigger);
            SetBodyPart((animator.GetBoneTransform(HumanBodyBones.RightFoot)) ? animator.GetBoneTransform(HumanBodyBones.RightFoot).transform : null, isTrigger);
            //SetBodyPart((animator.GetBoneTransform(HumanBodyBones.RightToes)) ? animator.GetBoneTransform(HumanBodyBones.RightToes).transform : null);

            SetBodyPart((animator.GetBoneTransform(HumanBodyBones.Spine)) ? animator.GetBoneTransform(HumanBodyBones.Spine).transform : null, isTrigger);
            SetBodyPart((animator.GetBoneTransform(HumanBodyBones.UpperChest)) ? animator.GetBoneTransform(HumanBodyBones.UpperChest).transform : null, isTrigger);
            SetBodyPart((animator.GetBoneTransform(HumanBodyBones.Chest)) ? animator.GetBoneTransform(HumanBodyBones.Chest).transform : null, isTrigger);
            SetBodyPart((animator.GetBoneTransform(HumanBodyBones.Neck)) ? animator.GetBoneTransform(HumanBodyBones.Neck).transform : null, isTrigger);
            SetBodyPart((animator.GetBoneTransform(HumanBodyBones.Head)) ? animator.GetBoneTransform(HumanBodyBones.Head).transform : null, isTrigger);
            SetBodyPart((animator.GetBoneTransform(HumanBodyBones.Hips)) ? animator.GetBoneTransform(HumanBodyBones.Hips).transform : null, isTrigger);
            
        }

        /// <summary>
        /// Used by the `SetBodyParts` function. Will set the target transform to enable or 
        /// disable it's trigger if it has one.
        /// </summary>
        /// <param name="target">Transform type, the transform to target</param>
        /// <param name="isTrigger">bool type, enable or disable the trigger.</param>
        protected virtual void SetBodyPart(Transform target = null, bool isTrigger = true)
        {
            if (target == null) return;
            if (target.GetComponent<vHitBox>()) return;
            if (target.GetComponent<Collider>())
            {
                target.GetComponent<Collider>().enabled = true;
                target.GetComponent<Collider>().isTrigger = isTrigger;
            }
            //target.tag = "Untagged";
        }
        #endregion

        /// <summary>
        /// Make sure the network versions allowed to hit the `Player` Layer and tag and the 
        /// owner player is NOT allowed to hit the `Player` Layer and tag. These settings are 
        /// set in the `vMeleeManager` component. Called via `Start`.
        /// </summary>
        protected virtual void ModifyHitTags()
        {
            if (GetComponent<vMeleeManager>())
            {
                if (_nonAuthoritativeLayer != LayerMask.NameToLayer("Player") && 
                    !GetComponent<vMeleeManager>().hitProperties.hitDamageTags.Contains("Player"))
                {
                    if (!string.IsNullOrEmpty(NetworkManager.networkManager.teamName) && (
                        teamName != NetworkManager.networkManager.teamName ||
                        teamName == NetworkManager.networkManager.teamName && NetworkManager.networkManager.allowTeamDamaging == true
                        ) || string.IsNullOrEmpty(NetworkManager.networkManager.teamName))
                    {
                        GetComponent<vMeleeManager>().hitProperties.hitDamageTags.Add("Player");
                    }
                }
            }
        }

        /// <summary>
        /// Populates the `animParams` variable which is used to know what animator parameters 
        /// to watch and send updates over the network for. Calls via `Start` function.
        /// </summary>
        protected virtual void BuildAnimatorParamsDict()
        {
            if (GetComponent<Animator>())
            {
                foreach (var param in GetComponent<Animator>().parameters)
                {
                    if (param.type != AnimatorControllerParameterType.Trigger) //Syncing triggers this way is unreliable, send trigger events via RPC
                    {
                        animParams.Add(param.name, param.type);
                    }
                }
            }
        }

        /// <summary>
        /// Populates the `itemDict` parameter so when an item is equipped/unequipped it will
        /// know the GameObject to spawn based on that items Id.
        /// </summary>
        protected virtual void BuildItemDictionary()
        {
            if (itemData != null)
            {
                foreach (vItem item in itemData.items)
                {
                    itemDict.Add(item.id, item.originalObject);
                }
            }
        }
        #endregion

        #region Server Sync Logic
        public virtual void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) //this function called by Photon View component
        {
            try
            {
                if (PhotonNetwork.InRoom == false) return;
                if (stream.IsWriting)
                {
                    //Send Player Position, rotation, velocity
                    stream.SendNext(sendPosRot);
                    if (sendPosRot == true)
                    {
                        stream.SendNext(transform.position);
                        stream.SendNext(transform.rotation);
                        stream.SendNext((realPos - lastPos) * Time.deltaTime);
                    }

                    stream.SendNext(sendAnimations);
                    if (sendAnimations == true)
                    {
                        //Set player animations
                        if (_syncAnimations == true)
                        {
                            //Send Player Animations
                            foreach (var item in animParams)
                            {
                                switch (item.Value)
                                {
                                    case AnimatorControllerParameterType.Bool:
                                        stream.SendNext(animator.GetBool(item.Key));
                                        if (item.Key == "IsBlocking") PhotonNetwork.SendAllOutgoingCommands();
                                        break;
                                    case AnimatorControllerParameterType.Float:
                                        stream.SendNext((animator.GetFloat(item.Key) < 0.05f && animator.GetFloat(item.Key) > -0.05f) ? 0 : animator.GetFloat(item.Key));
                                        break;
                                    case AnimatorControllerParameterType.Int:
                                        stream.SendNext(animator.GetInteger(item.Key));
                                        break;
                                }
                            }
                        }

                        if (animator)
                        {
                            for (int i = 0; i < animator.layerCount; i++)
                            {
                                stream.SendNext(animator.GetLayerWeight(i));
                            }
                        }
                    }
                }
                else if (stream.IsReading) //Network player copies receiving data from server
                {
                    //Receive Player Position and rotation
                    sendPosRot = (bool)stream.ReceiveNext();
                    if (sendPosRot == true)
                    {
                        this.realPos = (Vector3)stream.ReceiveNext();
                        this.realRot = (Quaternion)stream.ReceiveNext();
                        this.velocity = (Vector3)stream.ReceiveNext();
                        float lag = Mathf.Abs((float)(PhotonNetwork.Time - info.SentServerTime));
                        this.realPos += (velocity * lag);
                    }

                    sendAnimations = (bool)stream.ReceiveNext();
                    if (sendAnimations == true)
                    {
                        //Recieve player animations updates
                        if (_syncAnimations == true)
                        {
                            //Receive Player Animations
                            foreach (var item in animParams)
                            {
                                switch (item.Value)
                                {
                                    case AnimatorControllerParameterType.Bool:
                                        animator.SetBool(item.Key, (bool)stream.ReceiveNext());
                                        break;
                                    case AnimatorControllerParameterType.Float:
                                        animator.SetFloat(item.Key, (float)stream.ReceiveNext());
                                        break;
                                    case AnimatorControllerParameterType.Int:
                                        animator.SetInteger(item.Key, (int)stream.ReceiveNext());
                                        break;
                                }
                            }
                        }
                        if (animator)
                        {
                            for (int i = 0; i < animator.layerCount; i++)
                            {
                                animator.SetLayerWeight(i, (float)stream.ReceiveNext());
                            }
                        }
                    }
                }
            }
            catch { }
        }

        #region Enable/Disable Network Sending
        /// <summary>
        /// Send the position and rotation of the owner player over the network so the 
        /// network players will set their position and rotation based on what they
        /// receive.
        /// </summary>
        /// <param name="isEnabled">bool type, allow sending the position and rotation over the network?</param>
        public virtual void EnableSendingPosRot(bool isEnabled)
        {
            if (photonView.IsMine == true)
            {
                sendPosRot = isEnabled;
            }
        }

        /// <summary>
        /// Make it so the owner player can send his animation parameters over the network.
        /// The network players will set their animation parameters based on what they 
        /// recieve from the owner player.
        /// </summary>
        /// <param name="isEnabled">bool type, allow the owner player to send their animator parameters</param>
        public virtual void EnableSendingAnimations(bool isEnabled)
        {
            if (photonView.IsMine == true)
            {
                sendAnimations = isEnabled;
            }
        }
        #endregion

        #region Bow
        /// <summary>
        /// Returns the id that the next arrow should use.
        /// </summary>
        /// <returns>The arrow id to use</returns>
        public virtual int GetArrowId()
        {
            arrowCount += 1;
            return GetComponent<PhotonView>().ViewID + arrowCount;
        }
        #endregion

        #region Damage Sync
        /// <summary>
        /// Used to set the damage to zero if the sender is on the same team and you disallow team damaging.
        /// This function is meant to be called by the OnReceiveDamage unityevent on the `vThirdPersonController`.
        /// If damage is received then it is replicated to all networked players
        /// </summary>
        /// <param name="damage"></param>
        public virtual void OnReceiveDamage(vDamage damage)
        {
            if (damage.sender != null && damage.sender.GetComponentInParent<SyncPlayer>() && !string.IsNullOrEmpty(NetworkManager.networkManager.teamName))
            {
                if (NetworkManager.networkManager.allowTeamDamaging == false && 
                    NetworkManager.networkManager.teamName == damage.sender.GetComponentInParent<SyncPlayer>().teamName)
                {
                    damage.ReduceDamage(100);
                    return;
                }
            }
            lastDamageType = damage.damageType;
            lastDamageSender = (damage.sender == null) ? null : damage.sender.gameObject;

            if (GetComponent<PhotonView>().IsMine == true)
            {
                view.RPC("ReceiveDamage", RpcTarget.Others, JsonUtility.ToJson(new SerializedDamage(damage)), tpc.currentHealth);
            }
        }

        /// <summary>
        /// Calls the `Respawn` function with a null gameobject
        /// </summary>
        public virtual void Respawn()
        {
            Respawn(null);
        }

        /// <summary>
        /// Calls the `SavePlayer` function in the network manager and also calls 
        /// the `Respawn` function on the `PlayerRespawn` component.
        /// </summary>
        /// <param name="go">Placed here for calling with UnityEvents. Otherwise not used.</param>
        public virtual void Respawn(GameObject go)
        {
            if (GetComponent<PhotonView>().IsMine == true)
            {
                if (GetComponent<vItemManager>())
                {
                    NetworkManager.networkManager.SavePlayer(GetComponent<vThirdPersonController>());
                }
                if (NetworkManager.networkManager.transform.GetComponent<PlayerRespawn>())
                {
                    NetworkManager.networkManager.transform.GetComponent<PlayerRespawn>().Respawn(
                        true,
                        lastDamageSender,
                        lastDamageType
                    );
                }
            }
        }
        #endregion

        #region vThirdPersonController
        /// <summary>
        /// Makes the other networked players jump in response to this owners command.
        /// Meant to be used with the OnJump UnityEvent in the `vThirdPersonController`.
        /// </summary>
        public virtual void Jump()
        {
            if (tpc.input.sqrMagnitude < 0.1f)
            {
                view.RPC("NetworkJump", RpcTarget.Others, "Jump", 0.1f);
            }
            else
            {
                view.RPC("NetworkJump", RpcTarget.Others, "JumpMove", 0.1f);
            }
            StartCoroutine(WaitForLand());
        }
        protected virtual IEnumerator WaitForLand()
        {
            yield return new WaitUntil(() => tpc.isJumping == false);
            view.RPC("UseRigidBodyGravity", RpcTarget.Others, true);

        }
        #endregion

        #region vLadderAction
        /// <summary>
        /// Makes the networked players play the enter ladder animation.
        /// </summary>
        public virtual void EnterLadder()
        {
            vTriggerLadderAction tla = (vTriggerLadderAction)ladderAction.GetType().GetField("targetLadderAction", flags).GetValue(ladderAction);
            view.RPC("NetworkEnterLadder", RpcTarget.OthersBuffered, (tla) ? tla.playAnimation : null);
        }

        /// <summary>
        /// Makes the networked players play the exit ladder animation
        /// </summary>
        public virtual void ExitLadder()
        {
            vTriggerLadderAction tla = (vTriggerLadderAction)ladderAction.GetType().GetField("targetLadderAction", flags).GetValue(ladderAction);
            view.RPC("NetworkExitLadder", RpcTarget.OthersBuffered, (tla) ? tla.exitAnimation : null);
        }
        #endregion

        #region Dropped Items
        /// <summary>
        /// Makes all network players drop and item with the specified amount. Also
        /// Send a drop command to the data channel on the `Chatbox` so when other 
        /// players enter this unity scene that item is dropped for their version
        /// of the game as well.
        /// </summary>
        /// <param name="item">vItem type, the item to drop</param>
        /// <param name="value">int type, the amount of this item to drop</param>
        public virtual void OnDropItem(vItem item, int value)
        {
            List<ItemReference> irs = new List<ItemReference>();

            ItemReference ir = new ItemReference(item.id);
            ir.amount = item.amount;
            ir.attributes = item.attributes;

            irs.Add(ir);
            ItemWrapper wrapper = new ItemWrapper(irs);

            GetComponent<PhotonView>().RPC(
                "PlayerDropItem",
                RpcTarget.MasterClient, 
                item.dropObject.name, 
                JsonUtility.ToJson(transform.position),
                JsonUtility.ToJson(transform.rotation),
                0,
                JsonUtility.ToJson(wrapper)
            );
        }
        #endregion

        #region Ragdoll
        /// <summary>
        /// Turns on ragdoll effects for the networked players.
        /// </summary>
        /// <param name="damage">Placed here to be used with UnityEvents, otherwise not used.</param>
        public virtual void NetworkActiveRagdoll(vDamage damage = null)
        {
            _ragdolled = true;
            #region Shooter Template
            SetBodyParts(false);
            #endregion
            GetComponent<PhotonView>().RPC("SetActiveRagdoll", RpcTarget.Others, true);
        }
        /// <summary>
        /// Turns off ragdoll effects for the networked players.
        /// </summary>
        protected virtual void NetworkDisableRagdoll()
        {
           #region Shooter Template
            SetBodyParts(false);
            #endregion
            GetComponent<PhotonView>().RPC("SetActiveRagdoll", RpcTarget.Others, false);
        }
        #endregion

        #region DeathCam
        /// <summary>
        /// Allows/Disallows switching on the `DeathCamera` component by calling `EnableSwitching`
        /// with the specified value.
        /// </summary>
        /// <param name="isEnabled">bool type, allow switching the camera or not.</param>
        public virtual void EnableDeathCamera(bool isEnabled)
        {
            if (GetComponent<PhotonView>().IsMine == true && FindObjectOfType<DeathCamera>())
            {
                FindObjectOfType<DeathCamera>().EnableSwitching(isEnabled);
            }
        }

        /// <summary>
        /// Turn on the death camera by calling `EnableDeathCamera` with a true value.
        /// </summary>
        /// <param name="temp">Placed here for UnityEvent activation. Otherwise not used.</param>
        public virtual void DeadEnableDeathCam(GameObject temp = null)
        {
            EnableDeathCamera(true);
        }
        #endregion

        #region RPCs

        #region Team Logic
        [PunRPC]
        protected virtual void SetTeam(string networkTeamName)
        {
            teamName = networkTeamName;
            if (GetComponent<PhotonView>().IsMine == false)
            {
                ModifyHitTags();
            }
        }
        #endregion

        #region Animations
        [PunRPC]
        protected virtual void SetTriggers(int[] names)
        {
            if (GetComponent<Animator>())
            {
                foreach (int name in names)
                {
                    GetComponent<Animator>().SetTrigger(name);
                }
            }
        }
        [PunRPC]
        protected virtual void SetTriggers(string[] names)
        {
            if (GetComponent<Animator>())
            {
                foreach (string name in names)
                {
                    GetComponent<Animator>().SetTrigger(name);
                }
            }
        }
        [PunRPC]
        protected virtual void ResetTriggers(string[] names)
        {
            if (GetComponent<Animator>())
            {
                foreach (string name in names)
                {
                    GetComponent<Animator>().ResetTrigger(name);
                }
            }
        }
        [PunRPC]
        protected virtual void CrossFadeInFixedTime(string name, float time)
        {
            if (GetComponent<Animator>())
            {
                GetComponent<Animator>().CrossFadeInFixedTime(name, time);
            }
        }
        [PunRPC]
        protected virtual void NetworkJump(string animName, float fadeTime)
        {
            UseRigidBodyGravity(false);
            CrossFadeInFixedTime(animName, fadeTime);
        }
        #endregion

        #region Rigidbodies
        [PunRPC]
        protected virtual void UseRigidBodyGravity(bool enabled)
        {
            GetComponent<Rigidbody>().useGravity = enabled;
        }
        [PunRPC]
        protected virtual void SetActiveRagdoll(bool isActive)
        {
          #region Shooter Template
            SetBodyParts(false);
            #endregion
            if (isActive == true)
            {
                tpc.EnableRagdoll();
            }
            else
            {
                tpc.ResetRagdoll();
            }
        }
        #endregion

        #region Ladders
        [PunRPC]
        protected virtual void NetworkEnterLadder(string animName = null)
        {
            UseRigidBodyGravity(false);
            if (!string.IsNullOrEmpty(animName))
                CrossFadeInFixedTime(animName, 0.1f);
        }
        [PunRPC]
        protected virtual void NetworkExitLadder(string animName = null)
        {
            UseRigidBodyGravity(true);
            if (!string.IsNullOrEmpty(animName))
                CrossFadeInFixedTime(animName, 0.1f);
        }
        #endregion

        #region Object States
        [PunRPC]
        protected virtual void NetworkSetActive(int[] treeToParent, int[] enableChildIndexs, bool enabled)
        {
            if (enableChildIndexs.Length < 1 || enableChildIndexs.Length < 1) return;
            Transform parentTransform = StaticMethods.FindTargetChild(treeToParent, transform.root);
            if (!parentTransform || parentTransform.childCount < 1) return;
            foreach (int index in enableChildIndexs)
            {
                if (parentTransform.childCount > index)
                {
                    parentTransform.GetChild(index).gameObject.SetActive(enabled);
                }
            }
        }
        [PunRPC]
        protected virtual void Item_NetworkDestroy(int[] treeToParent, int[] destroyAtIndex)
        {
            if (destroyAtIndex.Length < 1) return;
            Transform parentTransform = StaticMethods.FindTargetChild(treeToParent, transform);
            if (parentTransform && parentTransform.childCount < 1) return;
            foreach (int index in destroyAtIndex)
            {
                Transform currentTarget = parentTransform.GetChild(index);
                if (currentTarget == null)
                {
                    continue;
                }
                else
                {
                    Destroy(currentTarget.gameObject);
                }
            }
        }
        [PunRPC]
        protected virtual void NetworkInstantiate(int[] treeToHandler, int[] itemIds, bool isLeft, bool isHolder)
        {
            if (itemIds.Length < 1) return;
            Transform targetHandler = StaticMethods.FindTargetChild(treeToHandler, transform);
            if (!targetHandler) return;
            foreach (int itemId in itemIds)
            {
                vItem foundItem = itemData.items.Find(h => itemId == h.id);
                if (foundItem && foundItem.originalObject)
                {
                    var instantiate_item = Instantiate(foundItem.originalObject, targetHandler.position, targetHandler.rotation) as GameObject;
                    instantiate_item.transform.SetParent(targetHandler.transform);
                    instantiate_item.transform.localPosition = Vector3.zero;
                    instantiate_item.transform.localEulerAngles = Vector3.zero;
                    if (isLeft && isHolder == false)
                    {
                        var scale = instantiate_item.transform.localScale;
                        scale.x *= -1;
                        instantiate_item.transform.localScale = scale;
                    }
                    break;
                }
            }
        }
        [PunRPC]
        protected virtual void MasterClientInstantiate(string prefabName, string position, string rotation)
        {
            if (PhotonNetwork.IsMasterClient == true)
            {
                NetworkManager.networkManager.NetworkInstantiatePersistantPrefab(
                    prefabName,
                    JsonUtility.FromJson<Vector3>(position),
                    JsonUtility.FromJson<Quaternion>(rotation)
                );
            }
        }
        #endregion

        #region Damage
        [PunRPC]
        protected virtual void ReceiveDamage(string serializedDamage, float ownerCurrentHealth)
        {
            SerializedDamage deserializedDamage = JsonUtility.FromJson<SerializedDamage>(serializedDamage);
            vDamage damage = deserializedDamage.TovDamage();

            triggerNames.Clear();
            if (animator != null && animator.enabled && !damage.activeRagdoll && tpc.currentHealth > 0)
            {
                if (damage.hitReaction)
                {
                    if (triggerReactionHash != -1)
                    {
                        triggerNames.Add(triggerReactionHash);
                    }
                    if (triggerResetStateHash != -1)
                    {
                        triggerNames.Add(triggerResetStateHash);
                    }
                }
                else
                {
                    if (triggerRecoilHash != -1)
                    {
                        triggerNames.Add(triggerRecoilHash);
                    }
                    if (triggerResetStateHash != -1)
                    {
                        triggerNames.Add(triggerResetStateHash);
                    }
                }
                SetTriggers(triggerNames.ToArray());
            }

            tpc.ChangeHealth((int)Mathf.Round(ownerCurrentHealth));
            tpc.TakeDamage(damage);
        }
        [PunRPC]
        protected virtual void SendDamage(int viewId, string recieveDamage, int damageMultiplier, int senderView)
        {
            PhotonView foundView = PhotonView.Find(viewId);
            if (foundView && foundView.IsMine == true && !string.IsNullOrEmpty(recieveDamage))
            {
                vDamage damage = JsonUtility.FromJson<vDamage>(recieveDamage);
                damage.damageValue = (damageMultiplier > 0) ? damage.damageValue * damageMultiplier : damage.damageValue;
                PhotonView sender = PhotonView.Find(senderView);
                damage.sender = (sender) ? sender.transform : null;
                if (foundView.transform.GetComponent<vThirdPersonController>())
                {
                    foundView.transform.GetComponent<vThirdPersonController>().TakeDamage(damage);
                }
            }
        }
        #endregion

        #region NetworkManager Actions
        [PunRPC]
        protected virtual void UpdateEntrancePoint(string entrancePointName)
        {
            NetworkManager.networkManager.SetSpawnAtPoint(entrancePointName);
        }
        #endregion

        #region Position/Rotation Settings
        [PunRPC]
        protected virtual void SetPositionRotation(string rotation, string position)
        {
            transform.position = JsonUtility.FromJson<Vector3>(position);
            transform.rotation = JsonUtility.FromJson<Quaternion>(rotation);
        }
        #endregion

        #region Items
        //[PunRPC]
        //void ReceiveSetLeftWeapon(int[] treeToWeapon)
        //{
        //    if (treeToWeapon.Length > 0)
        //    {
        //        Transform weapon = StaticMethods.FindTargetChild(treeToWeapon, transform);
        //        if (!weapon || !weapon.gameObject) return;
        //        if (!meleeManager) meleeManager = GetComponent<vMeleeManager>();
        //        meleeManager.SetLeftWeapon(weapon.gameObject);
        //    }
        //    else
        //    {
        //        GameObject empty = null;
        //        if (!meleeManager) meleeManager = GetComponent<vMeleeManager>();
        //        meleeManager.SetLeftWeapon(empty);
        //    }
        //}
        //[PunRPC]
        //void ReceiveSetRightWeapon(int[] treeToWeapon)
        //{
        //    if (treeToWeapon.Length > 0)
        //    {
        //        Transform weapon = StaticMethods.FindTargetChild(treeToWeapon, transform);
        //        if (!weapon || !weapon.gameObject) return;
        //        if (!meleeManager) meleeManager = GetComponent<vMeleeManager>();
        //        meleeManager.SetRightWeapon(weapon.gameObject);
        //    }
        //    else
        //    {
        //        GameObject empty = null;
        //        if (!meleeManager) meleeManager = GetComponent<vMeleeManager>();
        //        meleeManager.SetRightWeapon(empty);
        //    }
        //}
        [PunRPC]
        protected virtual void PlayerDropItem(string prefabName, string position, string rotation, int group, string serializedWrapper)
        {
            if (PhotonNetwork.IsMasterClient == true)
            {
                // Place Item in world with saved data as a scene object
                ItemWrapper wrapper = JsonUtility.FromJson<ItemWrapper>(serializedWrapper);
                object[] data = new object[1];
                data[0] = serializedWrapper;
                GameObject droppedItem = PhotonNetwork.InstantiateRoomObject(
                    prefabName,
                    JsonUtility.FromJson<Vector3>(position), 
                    JsonUtility.FromJson<Quaternion>(rotation), 
                    (byte)group,
                    data
                );

                //Update every elses database
                droppedItem.gameObject.GetComponent<SyncItemCollection>().UpdateDatabase(wrapper.items);
            }
        }
        #endregion

        #endregion

        #region Shooter Template
        [PunRPC]
        protected virtual void SetAnimatorLayerWeights(int layer, float weight)
        {
            GetComponent<Animator>().SetLayerWeight(layer, weight);
        }

        [PunRPC]
        protected virtual void DestroyArrow(int arrowView)
        {
            foreach (ArrowView arrow in FindObjectsOfType<ArrowView>())
            {
                if (arrow.viewId == arrowView)
                {
                    Destroy(arrow.transform.GetComponent<SyncItemCollection>().GetHolder().gameObject);
                    break;
                }
            }
        }
        [PunRPC]
        protected virtual void SetArrowPositionRotation(int arrowView, int[] tree, string position, string rotation)
        {
            Transform targetParent = StaticMethods.FindTargetChild(tree, transform);
            foreach (ArrowView arrow in FindObjectsOfType<ArrowView>())
            {
                if (arrow.viewId == arrowView)
                {
                    arrow.transform.SetParent(targetParent);
                    arrow.transform.position = JsonUtility.FromJson<Vector3>(position);
                    arrow.transform.rotation = JsonUtility.FromJson<Quaternion>(rotation);
                    break;
                }
            }
        }

        [PunRPC]
        protected virtual void ShooterWeaponShotWithPosition(int[] treeToWeapon, Vector3 aimPos)
        {
            Transform weaponTransform = StaticMethods.FindTargetChild(treeToWeapon, transform.root);
            weaponTransform.gameObject.GetComponent<MP_ShooterWeapon>().RecieveNetworkShot(aimPos);
        }
        [PunRPC]
        protected virtual void ShooterWeaponShot(int[] treeToWeapon)
        {
            Transform weaponTransform = StaticMethods.FindTargetChild(treeToWeapon, transform.root);
            weaponTransform.gameObject.GetComponent<MP_ShooterWeapon>().RecieveNetworkShot();
        }
        [PunRPC]
        protected virtual void ShooterWeaponEmptyClip(int[] treeToWeapon)
        {
            Transform weaponTransform = StaticMethods.FindTargetChild(treeToWeapon, transform.root);
            weaponTransform.gameObject.GetComponent<MP_ShooterWeapon>().RecieveNetworkEmptyClip();
        }
        [PunRPC]
        protected virtual void ShooterWeaponOnFinishReload(int[] treeToWeapon)
        {
            Transform weaponTransform = StaticMethods.FindTargetChild(treeToWeapon, transform.root);
            weaponTransform.gameObject.GetComponent<MP_ShooterWeapon>().RecieveNetworkOnFinishReload();
        }
        [PunRPC]
        protected virtual void ShooterWeaponOnFullPower(int[] treeToWeapon)
        {
            Transform weaponTransform = StaticMethods.FindTargetChild(treeToWeapon, transform.root);
            weaponTransform.gameObject.GetComponent<MP_ShooterWeapon>().RecieveNetworkOnFullPower();
        }
        [PunRPC]
        protected virtual void ShooterWeaponOnFinishAmmo(int[] treeToWeapon)
        {
            Transform weaponTransform = StaticMethods.FindTargetChild(treeToWeapon, transform.root);
            weaponTransform.gameObject.GetComponent<MP_ShooterWeapon>().RecieveNetworkOnFinishAmmo();
        }
        [PunRPC]
        protected virtual void ShooterWeaponReload(int[] treeToWeapon)
        {
            Transform weaponTransform = StaticMethods.FindTargetChild(treeToWeapon, transform.root);
            weaponTransform.gameObject.GetComponent<MP_ShooterWeapon>().RecieveNetworkReload();
        }
        [PunRPC]
        protected virtual void ShooterWeaponOnEnableAim(int[] treeToWeapon)
        {
            Transform weaponTransform = StaticMethods.FindTargetChild(treeToWeapon, transform.root);
            weaponTransform.gameObject.GetComponent<MP_ShooterWeapon>().RecieveNetworkOnEnableAim();
        }
        [PunRPC]
        protected virtual void ShooterWeaponOnDisableAim(int[] treeToWeapon)
        {
            Transform weaponTransform = StaticMethods.FindTargetChild(treeToWeapon, transform.root);
            weaponTransform.gameObject.GetComponent<MP_ShooterWeapon>().RecieveNetworkOnDisableAim();
        }
        [PunRPC]
        protected virtual void ShooterWeaponOnChangerPowerCharger(int[] treeToWeapon, float amount)
        {
            Transform weaponTransform = StaticMethods.FindTargetChild(treeToWeapon, transform.root);
            weaponTransform.gameObject.GetComponent<MP_ShooterWeapon>().RecieveNetworkOnChangerPowerCharger(amount);
        }
        #endregion

/*        #region Swimming Addon
        [PunRPC]
        protected virtual void InvokeOnEnterWater()
        {
            GetComponent<MP_vSwimming>().NetworkOnEnterWater.Invoke();
        }
        [PunRPC]
        protected virtual void InvokeOnExitWater()
        {
            GetComponent<MP_vSwimming>().NetworkOnExitWater.Invoke();
        }
        [PunRPC]
        protected virtual void InvokeOnAboveWater()
        {
            GetComponent<MP_vSwimming>().NetworkOnAboveWater.Invoke();
        }
        [PunRPC]
        protected virtual void InvokeOnUnderWater()
        {
            GetComponent<MP_vSwimming>().NetworkOnUnderWater.Invoke();
        }
        [PunRPC]
        protected virtual void NetworkWaterRingEffect(float height, string rotation)
        {
            var newPos = new Vector3(transform.position.x, height, transform.position.z);
            Instantiate(
                GetComponent<MP_vSwimming>().waterRingEffect, 
                newPos, 
                JsonUtility.FromJson<Quaternion>(rotation)
            ).transform.SetParent(vObjectContainer.root, true);
        }
        [PunRPC]
        protected virtual void NetworkWaterImpactEffect(string position, string rotation)
        {
            Instantiate(
                GetComponent<MP_vSwimming>().impactEffect, 
                JsonUtility.FromJson<Vector3>(position),
                JsonUtility.FromJson<Quaternion>(rotation)
            );
        }
        [PunRPC]
        protected virtual void NetworkWaterDropsEffect(string position, string rotation)
        {
            GameObject myWaterDrops = Instantiate(
                GetComponent<MP_vSwimming>().waterDrops, 
                JsonUtility.FromJson<Vector3>(position),
                JsonUtility.FromJson<Quaternion>(rotation)
            ) as GameObject;
            myWaterDrops.transform.parent = transform;
        }
        #endregion*/

/*        #region Zipline Addon
        [PunRPC]
        protected virtual void FreezeRigidbodyContraints(bool enableFreeze)
        {
            if (contraintsSet == false)
            {
                originalContraints = GetComponent<Rigidbody>().constraints;
                contraintsSet = true;
            }
            GetComponent<Rigidbody>().constraints = (enableFreeze == true) ? RigidbodyConstraints.FreezeAll : originalContraints;
        }
        [PunRPC]
        protected virtual void NetworkOnZiplineEnter()
        {
            GetComponent<MP_vZipline>().onZiplineEnter.Invoke();
        }
        [PunRPC]
        protected virtual void NetworkOnZiplineExit()
        {
            GetComponent<MP_vZipline>().onZiplineExit.Invoke();
        }
        [PunRPC]
        protected virtual void NetworkOnZiplineUsing()
        {
            GetComponent<MP_vZipline>().onZiplineUsing.Invoke();
        }
        #endregion*/

        #endregion

        #region Heartbeat Actions
        /// <summary>
        /// Sets the networked players position/rotation based on the received values from
        /// the owner player. Also is responsible for playing the "Roll" animation if the 
        /// owner player enters the roll state. Finally checks if the owner has entered a
        /// ragdoll stat, if so enables/disables ragdoll for the networked player based on
        /// the setting the owner currently is in.
        /// 
        /// This is also responsible for showing or hiding the cursor if the inventory is
        /// open or not.
        /// </summary>
        protected virtual void Update()
        {
            if (photonView.IsMine == false && PhotonNetwork.OfflineMode == false)
            {
                if (sendPosRot == true)
                {
                    float distance = Vector3.Distance(transform.position, realPos);
                    if (distance < 2f)
                    {
                        transform.position = Vector3.Lerp(transform.position, realPos, Time.deltaTime * _positionLerpRate);
                        transform.rotation = Quaternion.Slerp(transform.rotation, realRot, Time.deltaTime * _rotationLerpRate);
                    }
                    else
                    {
                        transform.position = this.realPos;
                        transform.rotation = this.realRot;
                    }
                }
            }
            else if (photonView.IsMine == true && PhotonNetwork.OfflineMode == false)
            {
                lastPos = realPos;
                if (tpc.isRolling == true && _sentRolling == false)
                {
                    _sentRolling = true;
                    if (sendAnimations == true)
                    {
                        view.RPC("CrossFadeInFixedTime", RpcTarget.Others, "Roll", 0.1f);
                    }
                }
                else if (tpc.isRolling == false && _sentRolling == true)
                {
                    _sentRolling = false;
                }
                //if (_watchMeleeManager == true)
                //{
                //    if (prevLeftWeapon != meleeManager.leftWeapon)
                //    {
                //        prevLeftWeapon = meleeManager.leftWeapon;
                //        if (meleeManager.leftWeapon != null)
                //        {
                //            photonView.RPC("ReceiveSetLeftWeapon", RpcTarget.Others, StaticMethods.BuildChildTree(transform, meleeManager.leftWeapon.transform));
                //        }
                //    }
                //    if (prevRightWeapon != meleeManager.rightWeapon)
                //    {
                //        prevRightWeapon = meleeManager.rightWeapon;
                //        if (meleeManager.rightWeapon != null)
                //        {
                //            photonView.RPC("ReceiveSetRightWeapon", RpcTarget.Others, StaticMethods.BuildChildTree(transform, meleeManager.rightWeapon.transform));
                //        }
                //    }
                //}
            }
            if (_ragdolled == true)
            {
                if (tpc.ragdolled == false)
                {
                    _ragdolled = false;
                    NetworkDisableRagdoll();
                }
            }
            if (inventory)
            {
                if (inventory.isOpen == true && _cursorLocked == false)
                {
                    _cursorLocked = true;
                    if (GetComponent<vMeleeCombatInput>())
                    {
                        GetComponent<vMeleeCombatInput>().SetLockAllInput(_cursorLocked);
                        GetComponent<vMeleeCombatInput>().SetLockCameraInput(_cursorLocked);
                    }
                    Cursor.visible = true;
                    Cursor.lockState = CursorLockMode.None;
                }
                else if (inventory.isOpen == false && _cursorLocked == true)
                {
                    _cursorLocked = false;
                    if (GetComponent<vMeleeCombatInput>())
                    {
                        GetComponent<vMeleeCombatInput>().SetLockAllInput(_cursorLocked);
                        GetComponent<vMeleeCombatInput>().SetLockCameraInput(_cursorLocked);
                    }
                    Cursor.visible = false;
                    Cursor.lockState = CursorLockMode.Locked;
                }
            }
        }
        #endregion

        #region Location Saving
        protected virtual void OnDestroy()
        {
            NetworkManager.networkManager.spawnAtLoc = transform.position;
            NetworkManager.networkManager.spawnAtRot = transform.rotation;
        }
        #endregion
    }
}
