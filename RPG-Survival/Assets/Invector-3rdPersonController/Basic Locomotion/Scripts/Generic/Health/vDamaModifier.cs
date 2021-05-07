using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Invector
{  
    /// <summary>
    /// Damage Modifier. You can use this with <see cref="vIDamageReceiver.onStartReceiveDamage"/> to modificate the damage result
    /// </summary>   
    [System.Serializable]
    public class vDamageModifier
    {
        public string name = "MyModifier";
        [SerializeField,Tooltip("List of Damage type that this can modify")] protected List<string> damageTypes = new List<string>();
        [SerializeField,Tooltip("Modifier value")] protected int value;
        [SerializeField,Tooltip("true: Reduce a percentage of damage value\nfalse: Reduce da damage value directly")] protected bool percentage;

        /// <summary>
        /// Apply modifier to damage 
        /// </summary>
        /// <param name="damage">Damage to modify</param>
        public virtual void ApplyModifier(vDamage damage)
        {
            ///Apply modifier conditions            
            if (damage.damageValue > 0 && damageTypes.Contains(damage.damageType))
            {
                int modifier = 0;
                if (percentage)
                {
                    modifier = damage.damageValue / 100 * value;  ///Calculate Percentage of the damage
                }
                else
                {
                    modifier = value;/// default value
                }
                ///apply modifier to damage value
                damage.damageValue -= value;
            }
        }
    }
}