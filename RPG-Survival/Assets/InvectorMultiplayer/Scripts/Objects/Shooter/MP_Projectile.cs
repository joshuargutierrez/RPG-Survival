using CBGames.Core;
using CBGames.Player;
using Invector;
using UnityEngine;

namespace CBGames.Objects
{
    [AddComponentMenu("CB GAMES/Weapons/MP Projectile")]
    public class MP_Projectile : MonoBehaviour
    {
        /// <summary>
        /// Will make the damage zero if you're hitting a teammate and team damage is 
        /// set to false in the NetworkManager.
        /// </summary>
        /// <param name="damage">vDamage type, Contains sender and receiver to determine teams.</param>
        public void TeamDamageCheck(vDamage damage)
        {
            if (damage.receiver.GetComponentInParent<SyncPlayer>() && 
                damage.receiver.GetComponentInParent<SyncPlayer>().teamName == damage.sender.GetComponentInParent<SyncPlayer>().teamName &&
                NetworkManager.networkManager.allowTeamDamaging == false)
            {
                damage.hitReaction = false;
                damage.activeRagdoll = false;
                damage.damageValue = 0;
                damage.ReduceDamage(100);
            }
        }
    }
}