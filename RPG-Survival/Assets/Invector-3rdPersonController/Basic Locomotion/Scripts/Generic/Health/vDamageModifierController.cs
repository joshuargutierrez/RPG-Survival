using System.Collections.Generic;
using UnityEngine;
namespace Invector
{
    [vClassHeader("Damage Modifier Controller",openClose = false,useHelpBox =true,helpBoxText ="Needs a HealthController component")]
    public class vDamageModifierController : vMonoBehaviour
    {      
        [Tooltip("Modifier List")]
        public List<vDamageModifier> modifiers;
        protected virtual void Awake()
        {
            var healthController = GetComponent<vHealthController>();
            if (healthController) healthController.onStartReceiveDamage.AddListener(ApplyModifiers);
        }
        /// <summary>
        /// Apply All Modifiers
        /// </summary>
        /// <param name="damage">Damage to modify</param>
        protected virtual void ApplyModifiers(vDamage damage)
        {          
            for (int i = 0; i < modifiers.Count; i++)
            {
                modifiers[i].ApplyModifier(damage);
            }
        }
    }    
}
 