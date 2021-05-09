using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Events;
using UnityEngine;
namespace Invector.vItemManager
{
    using vShooter;
    public partial class vItemManagerUtilities
    {
        partial void _InitItemManager(vItemManager itemManager)
        {
            CreateShooterPoints(itemManager, itemManager.GetComponent<vShooterManager>());
        }

        static void CreateShooterPoints(vItemManager itemManager, vShooterManager shooterManager)
        {
            if (!shooterManager)
            {
                return;
            }

            var animator = itemManager.GetComponent<Animator>();

            #region LeftEquipPoint

            var equipPointL = itemManager.equipPoints.Find(p => p.equipPointName == "LeftArm");
            if (equipPointL == null)
            {
                EquipPoint pointL = new EquipPoint();
                pointL.equipPointName = "LeftArm";
                if (shooterManager)
                {
#if UNITY_EDITOR
                    UnityEventTools.AddPersistentListener<GameObject>(pointL.onInstantiateEquiment, shooterManager.SetLeftWeapon);
#else
                                    pointL.onInstantiateEquiment.AddListener(shooterManager.SetLeftWeapon);
#endif
                }

                //if (animator)
                //{
                //    var defaultEquipPointL = new GameObject("defaultHandler");
                //    var parent = animator.GetBoneTransform(HumanBodyBones.LeftHand);
                //    defaultEquipPointL.transform.SetParent(parent);
                //    defaultEquipPointL.transform.localPosition = Vector3.zero;
                //    defaultEquipPointL.transform.forward = itemManager.transform.forward;
                //    defaultEquipPointL.gameObject.tag = "Ignore Ragdoll";
                //    pointL.handler = new vHandler();
                //    pointL.handler.defaultHandler = defaultEquipPointL.transform;
                //}

                itemManager.equipPoints.Add(pointL);
            }
            else
            {
                if (equipPointL.handler.defaultHandler == null)
                {
                    if (animator)
                    {
                        var parent = animator.GetBoneTransform(HumanBodyBones.LeftHand);
                        var defaultPoint = parent.Find("defaultEquipPoint");

                        if (defaultPoint)
                        {
                            equipPointL.handler.defaultHandler = defaultPoint;
                        }
                        else
                        {
                            var _defaultPoint = new GameObject("defaultEquipPoint");
                            _defaultPoint.transform.SetParent(parent);
                            _defaultPoint.transform.localPosition = Vector3.zero;
                            _defaultPoint.transform.forward = itemManager.transform.forward;
                            _defaultPoint.gameObject.tag = "Ignore Ragdoll";
                            equipPointL.handler.defaultHandler = _defaultPoint.transform;
                        }
                    }
                }

                bool containsListener = false;
                for (int i = 0; i < equipPointL.onInstantiateEquiment.GetPersistentEventCount(); i++)
                {
                    if (equipPointL.onInstantiateEquiment.GetPersistentTarget(i).GetType().Equals(typeof(vShooterManager)) && equipPointL.onInstantiateEquiment.GetPersistentMethodName(i).Equals("SetLeftWeapon"))
                    {
                        containsListener = true;
                        break;
                    }
                }

                if (!containsListener && shooterManager)
                {
#if UNITY_EDITOR
                    UnityEventTools.AddPersistentListener<GameObject>(equipPointL.onInstantiateEquiment, shooterManager.SetLeftWeapon);
#else
                    equipPointL.onInstantiateEquiment.AddListener(shooterManager.SetLeftWeapon);
#endif
                }
            }
            #endregion

            #region RightEquipPoint

            var equipPointR = itemManager.equipPoints.Find(p => p.equipPointName == "RightArm");
            if (equipPointR == null)
            {
                EquipPoint pointR = new EquipPoint();
                pointR.equipPointName = "RightArm";
                if (shooterManager)
                {
#if UNITY_EDITOR
                    UnityEventTools.AddPersistentListener<GameObject>(pointR.onInstantiateEquiment, shooterManager.SetRightWeapon);
#else
                                    pointR.onInstantiateEquiment.AddListener(shooterManager.SetRightWeapon);
#endif
                }

                //if (animator)
                //{
                //    var defaultEquipPointR = new GameObject("defaultHandler");
                //    var parent = animator.GetBoneTransform(HumanBodyBones.RightHand);
                //    defaultEquipPointR.transform.SetParent(parent);
                //    defaultEquipPointR.transform.localPosition = Vector3.zero;
                //    defaultEquipPointR.transform.forward = itemManager.transform.forward;
                //    defaultEquipPointR.gameObject.tag = "Ignore Ragdoll";
                //    pointR.handler = new vHandler();
                //    pointR.handler.defaultHandler = defaultEquipPointR.transform;
                //}
                
                itemManager.equipPoints.Add(pointR);
            }
            else
            {
                if (equipPointR.handler.defaultHandler == null)
                {
                    if (animator)
                    {
                        var parent = animator.GetBoneTransform(HumanBodyBones.RightHand);
                        var defaultPoint = parent.Find("defaultEquipPoint");
                        if (defaultPoint)
                        {
                            equipPointR.handler.defaultHandler = defaultPoint;
                        }
                        else
                        {
                            var _defaultPoint = new GameObject("defaultEquipPoint");
                            _defaultPoint.transform.SetParent(parent);
                            _defaultPoint.transform.localPosition = Vector3.zero;
                            _defaultPoint.transform.forward = itemManager.transform.forward;
                            _defaultPoint.gameObject.tag = "Ignore Ragdoll";
                            equipPointR.handler.defaultHandler = _defaultPoint.transform;
                        }
                    }
                }

                bool containsListener = false;
                for (int i = 0; i < equipPointR.onInstantiateEquiment.GetPersistentEventCount(); i++)
                {
                    if (equipPointR.onInstantiateEquiment.GetPersistentTarget(i).GetType().Equals(typeof(vShooterManager)) && equipPointR.onInstantiateEquiment.GetPersistentMethodName(i).Equals("SetRightWeapon"))
                    {
                        containsListener = true;
                        break;
                    }
                }

                if (!containsListener && shooterManager)
                {
#if UNITY_EDITOR
                    UnityEventTools.AddPersistentListener<GameObject>(equipPointR.onInstantiateEquiment, shooterManager.SetRightWeapon);
#else
                    equipPointR.onInstantiateEquiment.AddListener(shooterManager.SetRightWeapon);
#endif
                }
            }
            #endregion
        }
    }
}