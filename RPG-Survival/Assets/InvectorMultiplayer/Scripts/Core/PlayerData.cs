using Invector.vCharacterController;
using Invector.vItemManager;
using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CBGames.Player
{
    [Serializable]
    public class PlayerData
    {
        #region Serializable Classes
        [Serializable]
        public class PlayerHealth
        {
            public float current = 100;
            public int max = 100;
            public float recovery = 0;
            public float recoveryDelay = 0;
        }

        [Serializable]
        public class PlayerStamina
        {
            public float max = 200;
            public float recovery = 1.2f;
            public float sprint = 30f;
            public float jump = 30f;
            public float roll = 25f;
        }

        [Serializable]
        public class PlayerRoll
        {
            public float speed = 0.0f;
            public float rotationSpeed = 10.0f;
            public bool useGravity = true;
            public float useGravityTime = 10.0f;
            public float timeToRollAgain = 0.75f;
        }

        [Serializable]
        public class PlayerJump
        {
            public float timer = 0.3f;
            public float height = 4f;
            public float airSpeed = 5f;
            public float airSmooth = 2f;
            public float extraGravity = -10f;
            public float limitFallVelocity = -15f;
            public float ragdollVelocity = -15f;

            public float fallDamageMinHeight = 6f;
            public float fallDamageMinVerticalVelocity = -10f;
            public float fallDamage = 10f;
        }

        [Serializable]
        public class PlayerGrounded
        {
            public float minDistance = 0.25f;
            public float maxDistance = 0.5f;
            public float slideDownVelocity = 7f;
            public float slideSidewaysVelocity = 5f;
            public float slideEnterTime = 0.1f;
            public float stepOffsetMaxHeight = 0.5f;
            public float stepOffsetMinHeight = 0;
            public float stepOffsetDistance = 0.1f;
        }

        [Serializable]
        public class PlayerInventory
        {
            public string items;
        }

        [Serializable]
        public class InventoryWrapper
        {
            public WrapperItems wrapper = new WrapperItems();
        }
        [Serializable]
        public class WrapperItems
        {
            public List<vItem> items = new List<vItem>();
            public List<InventoryPlacement> placement = new List<InventoryPlacement>();
        }
        [Serializable]
        public class InventoryPlacement
        {
            public int areaIndex = 0;
            public int slotIndex = 0;
            public vItem item = null;

            public InventoryPlacement(int inputAreaIndex, int inputSlotIndex, vItem inputItem)
            {
                this.areaIndex = inputAreaIndex;
                this.slotIndex = inputSlotIndex;
                this.item = inputItem;
            }
        }
        #endregion

        [Tooltip("The nickname of your the photon room.")]
        public string characterName = "";
        [Tooltip("The actual gameobject name of this character.")]
        public string character = "";

        [Tooltip("All of the health data related to this character.")]
        public PlayerHealth health = new PlayerHealth();
        [Tooltip("All of the stamina data related to this character.")]
        public PlayerStamina stamina = new PlayerStamina();
        [Tooltip("All of the roll settings on this character.")]
        public PlayerRoll roll = new PlayerRoll();
        [Tooltip("All of the jump settings on this character.")]
        public PlayerJump jump = new PlayerJump();
        [Tooltip("All of the group settings on this character.")]
        public PlayerGrounded ground = new PlayerGrounded();
        [Tooltip("The inventory data from this characters inventory.")]
        public PlayerInventory inventory = new PlayerInventory();

        /// <summary>
        /// Takes a simple character controller inputs and extracts all of the 
        /// following information from that character: health info, stamina info,
        /// roll settings, jump settings, group settings, and inventory data.
        /// 
        /// It will then save that information into easily serializable classes
        /// that can be stored into a binary file. It's all self contained 
        /// within this class.
        /// </summary>
        /// <param name="controller">The vThirdPersonController to scan</param>
        public PlayerData(vThirdPersonController controller)
        {
            characterName = PhotonNetwork.NickName;
            character = controller.gameObject.name.Replace("(Clone)", "").Replace("Instance", "").Trim();

            #region Health
            health.current = controller.currentHealth;
            health.max = controller.MaxHealth;
            health.recovery = controller.healthRecovery;
            health.recoveryDelay = controller.healthRecoveryDelay;
            #endregion

            #region Stamina
            stamina.max = controller.maxStamina;
            stamina.recovery = controller.staminaRecovery;
            stamina.sprint = controller.sprintStamina;
            stamina.jump = controller.jumpStamina;
            stamina.roll = controller.rollStamina;
            #endregion

            #region Roll
            roll.speed = controller.rollSpeed;
            roll.rotationSpeed = controller.rollRotationSpeed;
            roll.useGravity = controller.rollUseGravity;
            roll.useGravityTime = controller.rollUseGravityTime;
            roll.timeToRollAgain = controller.timeToRollAgain;
            #endregion

            #region Jump
            jump.timer = controller.jumpTimer;
            jump.height = controller.jumpHeight;
            jump.airSpeed = controller.airSpeed;
            jump.airSmooth = controller.airSmooth;
            jump.extraGravity = controller.extraGravity;
            jump.limitFallVelocity = controller.limitFallVelocity;
            jump.ragdollVelocity = controller.ragdollVelocity;
            jump.fallDamageMinHeight = controller.fallMinHeight;
            jump.fallDamageMinVerticalVelocity = controller.fallMinVerticalVelocity;
            jump.fallDamage = controller.fallDamage;
            #endregion

            #region Ground
            ground.minDistance = controller.groundMinDistance;
            ground.maxDistance = controller.groundMaxDistance;
            ground.slideDownVelocity = controller.slideDownVelocity;
            ground.slideSidewaysVelocity = controller.slideSidewaysVelocity;
            ground.slideEnterTime = controller.slidingEnterTime;
            ground.stepOffsetMaxHeight = controller.stepOffsetMaxHeight;
            ground.stepOffsetMinHeight = controller.stepOffsetMinHeight;
            ground.stepOffsetDistance = controller.stepOffsetDistance;
            #endregion

            #region Inventory
            if (controller.gameObject.GetComponent<vItemManager>())
            {
                this.inventory.items = ConvertToStringArray(controller.gameObject.GetComponent<vItemManager>(), controller.gameObject.GetComponent<vItemManager>().items);
            }
            #endregion
        }

        #region Helper Serialization Methods
        string ConvertToStringArray(vItemManager manager, List<vItem> items)
        {
            InventoryWrapper topKey = new InventoryWrapper();
            topKey.wrapper.items = items;
            for (int areaIndex = 0; areaIndex < manager.inventory.equipAreas.Length; areaIndex++)
            {
                for (int slotIndex = 0; slotIndex < manager.inventory.equipAreas[areaIndex].equipSlots.Count; slotIndex++)
                {
                    topKey.wrapper.placement.Add(new InventoryPlacement(
                        areaIndex,
                        slotIndex,
                        manager.inventory.equipAreas[areaIndex].equipSlots[slotIndex].item
                    ));
                }
            }
            return JsonUtility.ToJson(topKey);
        }

        public InventoryWrapper ConvertBackToWrapper(string serializedItems)
        {
            InventoryWrapper topKey = JsonUtility.FromJson<InventoryWrapper>(serializedItems);
            return topKey;
        }

        #endregion
    }
}