using System.Collections.Generic;
using UnityEngine;

namespace Invector.vEventSystems
{
    public interface vIAnimatorStateInfoController
    {       
        vAnimatorStateInfos animatorStateInfos { get; }
    }
    public static class vIAnimatorStateInfoHelper
    {
        /// <summary>
        /// Register all listener to <see cref="vAnimatorTagBase"/> listener
        /// </summary>
        /// <param name="animatorStateInfos"></param>
        public static void Register(this vIAnimatorStateInfoController animatorStateInfos)
        {
            if (animatorStateInfos.isValid()) animatorStateInfos.animatorStateInfos.RegisterListener();
        }
        /// <summary>
        /// Remove all listener from <see cref="vAnimatorTagBase"/> 
        /// </summary>
        /// <param name="animatorStateInfos"></param>
        public static void UnRegister(this vIAnimatorStateInfoController animatorStateInfos)
        {
            if (animatorStateInfos.isValid()) animatorStateInfos.animatorStateInfos.RemoveListener();
        }
        /// <summary>
        /// Check if is valid 
        /// </summary>
        /// <param name="animatorStateInfos"></param>
        /// <returns></returns>
        public static bool isValid(this vIAnimatorStateInfoController animatorStateInfos)
        {
            return animatorStateInfos != null && animatorStateInfos.animatorStateInfos != null && animatorStateInfos.animatorStateInfos.animator != null;
        }

    }
    public class vAnimatorStateInfos
    {
        public bool debug;
        public Animator animator;
        public vAnimatorStateInfos(Animator animator)
        {
            this.animator = animator;           
        }

        protected int GetCurrentAnimatorLayerUsingTag(string tag)
        {
            if (HasTag(tag) && statesRunning[tag].Count > 0)
            {
                var layersWithTag = statesRunning[tag];
                int lastIndexOf = layersWithTag.Count - 1;
                int currentLayer = layersWithTag[lastIndexOf];
                return currentLayer;
            }
            else
            {
                return -1;
            }
        }

        public void RegisterListener()
        {           
            var bhv = animator.GetBehaviours<vAnimatorTagBase>();
            for (int i = 0; i < bhv.Length; i++)
            {
                bhv[i].RemoveStateInfoListener(this);
                bhv[i].AddStateInfoListener(this);
               
            }
            if (debug) Debug.Log($"Listeners Registered", animator);
        }

        public void RemoveListener()
        {
            statesRunning.Clear();
            if (animator)
            {
                var bhv = animator.GetBehaviours<vAnimatorTagBase>();
                for (int i = 0; i < bhv.Length; i++)
                {
                    bhv[i].RemoveStateInfoListener(this);
                }
                if (debug) Debug.Log($"Listeners Removed", animator );
            }
        }

        Dictionary<string, List<int>> statesRunning = new Dictionary<string, List<int>>();

        /// <summary>
        /// Add tag to the layer
        /// </summary>
        /// <param name="tag">Tag</param>
        /// <param name="layer">Animator layer</param>
        public void AddStateInfo(string tag, int layer)
        {
           // Debug.Log($"ADD {tag}  to  Layer {layer}");
            if (!statesRunning.ContainsKey(tag)) statesRunning.Add(tag, new List<int>() { layer });
            else statesRunning[tag].Add(layer);

            if (debug) Debug.Log($"<color=green>Add tag : <b><i>{tag}</i></b></color>,in the animator layer :{layer}", animator);
        }
        /// <summary>
        /// Remove Tag of the layer
        /// </summary>
        /// <param name="tag">Tag</param>
        /// <param name="layer">Animator layer</param>
        public void RemoveStateInfo(string tag, int layer)
        {
            if (statesRunning.ContainsKey(tag) && statesRunning[tag].Exists(_info => _info.Equals(layer)))
            {
                var inforef = statesRunning[tag].Find(_info => _info.Equals(layer));
                statesRunning[tag].Remove(inforef);
                if (statesRunning[tag].Count == 0)
                    statesRunning.Remove(tag);
                if (debug) Debug.Log($"<color=red>Remove tag : <b><i>{tag}</i></b></color>, in the animator layer :{layer}", animator);
            }           
        }

        /// <summary>
        /// Check If StateInfo list contains tag
        /// </summary>
        /// <param name="tag">tag to check</param>
        /// <returns></returns>
        public bool HasTag(string tag)
        {
            return statesRunning.ContainsKey(tag);
        }

        /// <summary>
        /// Check if All tags in in StateInfo List
        /// </summary>
        /// <param name="tags">tags to check</param>
        /// <returns></returns>
        public bool HasAllTags(params string[] tags)
        {
            var has = tags.Length > 0 ? true : false;
            for (int i = 0; i < tags.Length; i++)
            {
                if (!HasTag(tags[i]))
                {
                    has = false;
                    break;
                }
            }
            return has;
        }

        /// <summary>
        /// Check if StateInfo List Contains any tag
        /// </summary>
        /// <param name="tags">tags to check</param>
        /// <returns></returns>
        public bool HasAnyTag(params string[] tags)
        {
            var has = false;
            for (int i = 0; i < tags.Length; i++)
            {
                if (HasTag(tags[i]))
                {
                    has = true;
                    break;
                }
            }
            return has;
        }
        /// <summary>
        /// Get current animator state info using tag
        /// </summary>
        /// <param name="tag">tag</param>
        /// <returns>if tag exit return AnimatorStateInfo? else return null</returns>
        public AnimatorStateInfo? GetCurrentAnimatorStateUsingTag(string tag)
        {
            int currentLayer = 0;
            if (HasAnimatorLayerUsingTag(tag,out currentLayer))
            {              
                return animator.GetCurrentAnimatorStateInfo(currentLayer);
            }
            else
            {
                return null;
            }
        }
     
     
        /// <summary>
        /// Check if Animator has some Layer using specific tag, and return the layer result
        /// </summary>
        /// <param name="tag">tag to check</param>
        /// <param name="layer">Layer result</param>
        /// <returns></returns>
        public bool HasAnimatorLayerUsingTag(string tag,out int layer)
        {            
            layer = GetCurrentAnimatorLayerUsingTag(tag);
            return layer!=-1;
        }
    }
}
