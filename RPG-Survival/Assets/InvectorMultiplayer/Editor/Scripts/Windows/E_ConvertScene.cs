using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using marijnz.EditorCoroutines;
using Invector.vItemManager;
using CBGames.Objects;
using Photon.Pun;
using UnityEngine.Events;
using Invector.vCharacterController.vActions;
using UnityEditor.Events;
using Invector;
using Invector.vCharacterController;
using CBGames.Core;
using System.Reflection;
using System;
using Invector.vCharacterController.AI;

namespace CBGames.Editors
{
    public class E_ConvertScene : EditorWindow
    {
        #region Editor Variables
        List<string> _previewConverts = new List<string>();
        Vector2 _scrollPos = Vector2.zero;
        Vector2 _previewScrollPos = Vector2.zero;
        Color _titleBoxColor;
        Color _convertColor;
        Color _convertSuccessColor;
        Color _convertErrorColor;
        Color _lockColor;
        Color _convertBar;
        GUISkin _skin = null;
        Editor _objectPreview;
        List<GameObject> convertables = new List<GameObject>();
        GameObject _target;
        GameObject _previousTarget;
        int _count = 0;
        float _scrollHeight = 0.0f;
        float _previewScrollHeight = 0.0f;
        string _help = "";
        bool _converting = false;

        bool _cRigidbodies = true;
        bool _cvItemCollections = true;
        bool _cvBreakableObjects = true;
        bool _cvHealthController = true;
        bool _cvTriggerGenericAction = true;
        bool _cUseItemTrigger = true;

        bool _pRigidbodies = true;
        bool _pvItemCollections = true;
        bool _pvBreakableObjects = true;
        bool _pvHealthController = true;
        bool _pvTriggerGenericAction = true;
        bool _pUseItemTrigger = true;
        
        bool _cvThrowCollectable = true;
        bool _pvThrowCollectable = true;

        bool _valueChanged = false;

        bool _convertedGenericTriggers = false;
        #endregion

        [MenuItem("CB Games/Convert/Scene", false, 0)]
        public static void CB_ConvertScene()
        {
            EditorWindow window = GetWindow<E_ConvertScene>(true);
            window.maxSize = new Vector2(500, 570);
            window.minSize = window.maxSize;
        }
        private void OnEnable()
        {
            if (!_skin) _skin = E_Helpers.LoadSkin(E_Core.e_guiSkinPath);

            //Set title bar colors
            _lockColor = new Color32(158, 158, 158, 200); //dark blue
            _convertBar = new Color32(95, 165, 245, 255); //dark blue
            _titleBoxColor = new Color32(1, 16, 51, 255); //Lighter Blue
            _convertColor = new Color32(173, 127, 0, 255); //Darker Yellow
            _convertSuccessColor = new Color32(8, 156, 0, 255); //Light Green
            _convertErrorColor = new Color32(222, 11, 0, 255); //Red

            //Make window title
            this.titleContent = new GUIContent("Scene Conversion", null, "Convert a objects in the scene to support multiplayer.");

            GetSceneObjects();
            _scrollHeight = 35 * convertables.Count;
            _scrollPos = new Vector2(0, _scrollHeight);

            //Set starting help text
            _help = "This is a list of objects that will be converted to support multiplayer. " +
                "Select the object name button to see a preview of the target and what will be converted " +
                "on that target. If you wish to remove an object from being converted click the 'X' " +
                "button next to the object name to remove it from the list. Once you are satistfied with " +
                "your selections click the 'CONVERT ALL' button to begin the conversion process.\n\n " +
                "Found objects in this scene: " + convertables.Count;

            _target = null;
            if (_convertedGenericTriggers == true)
            {
                if (EditorUtility.DisplayDialog("Friendly Reminder",
                        "You have converted some vTriggerGenericAction's in this scene. Not everything " +
                        "is able to be auto converted by default. Look through these objects and make sure " +
                        "the unity events that you want to be triggered over the network are being run by " +
                        "the \"CallNetworkEvents\" component." +
                        "",
                                    "Thanks For The Info"))
                {
                    _convertedGenericTriggers = false;
                }
            }
        }
        private void OnGUI()
        {
            CBColorHolder _orglabel = new CBColorHolder(EditorStyles.label);
            CBColorHolder _orgfoldout = new CBColorHolder(EditorStyles.foldout);
            CBColorHolder _skinHolder = new CBColorHolder(_skin.label);
            //Apply the gui skin
            GUI.skin = _skin;

            //Draw title bar
            EditorGUI.DrawRect(new Rect(0, 0, position.width, position.height), E_Colors.e_c_blue_5);
            EditorGUILayout.Space();
            EditorGUI.DrawRect(new Rect(5, 5, position.width - 10, 40), E_Colors.e_c_blue_4);
            EditorGUILayout.LabelField("Convert Scene Objects For Multiplayer Support", _skin.label);
            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal(_skin.box, GUILayout.ExpandHeight(false));
            EditorGUILayout.BeginVertical(GUILayout.ExpandHeight(false));

            //Draw Helpful Text
            EditorGUILayout.LabelField(_help, _skin.textField);

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();

            //Draw Selection Booleans
            EditorGUILayout.BeginVertical(_skin.box, GUILayout.Width(100));
            EditorGUILayout.BeginHorizontal(GUILayout.Width(100));
            CBEditor.SetColorToEditorStyle(_skinHolder, _skinHolder);
            _cRigidbodies = EditorGUILayout.ToggleLeft("Rigidbodies", _cRigidbodies, GUILayout.Width(150));
            _cvBreakableObjects = EditorGUILayout.ToggleLeft("vBreakableObjects", _cvBreakableObjects, GUILayout.Width(163));
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal(GUILayout.Width(100));
            _cvItemCollections = EditorGUILayout.ToggleLeft("vItemCollections", _cvItemCollections, GUILayout.Width(150));
            _cvHealthController = EditorGUILayout.ToggleLeft("vHealthControllers", _cvHealthController, GUILayout.Width(163));
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal(GUILayout.Width(100));
            _cvTriggerGenericAction = EditorGUILayout.ToggleLeft("vTriggerGenericActions", _cvTriggerGenericAction, GUILayout.Width(150));
            _cvThrowCollectable = EditorGUILayout.ToggleLeft("vThrowCollectables", _cvThrowCollectable, GUILayout.Width(150));
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal(GUILayout.Width(100));
            _cUseItemTrigger = EditorGUILayout.ToggleLeft("UseItemEventTriggers", _cUseItemTrigger, GUILayout.Width(150));
            CBEditor.SetColorToEditorStyle(_orglabel, _orgfoldout);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();

            if (_cRigidbodies != _pRigidbodies || _cvItemCollections != _pvItemCollections || _cvBreakableObjects != _pvBreakableObjects ||
                _cvHealthController != _pvHealthController || _cvTriggerGenericAction != _pvTriggerGenericAction || 
                _cvThrowCollectable != _pvThrowCollectable || _cUseItemTrigger != _pUseItemTrigger)
            {
                _pRigidbodies = _cRigidbodies;
                _pvItemCollections = _cvItemCollections;
                _pvBreakableObjects = _cvBreakableObjects;
                _pvHealthController = _cvHealthController;
                _pvThrowCollectable = _cvThrowCollectable;
                _pvTriggerGenericAction = _cvTriggerGenericAction;
                _pUseItemTrigger = _cUseItemTrigger;
                _valueChanged = true;
            }
            if (_valueChanged == true)
            {
                _valueChanged = false;
                OnEnable();
            }

            //Draw Selectable List
            _count = 0;
            EditorGUILayout.BeginHorizontal(_skin.box, GUILayout.Width(325));
            EditorGUILayout.BeginVertical(GUILayout.Height(300));
            _scrollPos = GUI.BeginScrollView(new Rect(10, 260, 300, position.height - 290), _scrollPos, new Rect(0, 0, 230, _scrollHeight), false, true);
            for (int i = 0; i < convertables.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                if (GUI.Button(new Rect(5, _count, 29, 30), "X", _skin.button) && _converting == false)
                {
                    convertables.RemoveAt(i);
                }
                if (GUI.Button(new Rect(40, _count, 230, 30), convertables[i].name, _skin.button) && _converting == false)
                {
                    PreviewObject(convertables[i]);
                }
                EditorGUILayout.EndHorizontal();
                _count += 35;

            }
            GUI.EndScrollView();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();

            //Draw Target Preview
            if (_target)
            {
                _objectPreview.OnInteractivePreviewGUI(new Rect(position.width - 170, 162, 150, 150), "window");
                EditorGUI.DrawRect(new Rect(position.width - 170, 315, 150, 214), E_Colors.e_c_blue_3);
                _previewScrollPos = GUI.BeginScrollView(new Rect(position.width - 168, 312, 167, 210), _previewScrollPos, new Rect(0, 0, 150, _previewScrollHeight), false, true);
                for (int i = 0; i < _previewConverts.Count; i++)
                {
                    EditorGUI.LabelField(new Rect(0, (i * 30), 150, 40), _previewConverts[i], _skin.textField);
                }
                GUI.EndScrollView();
            }

            //Draw Convert button
            if (GUI.Button(new Rect(position.width - 170, position.height - 38, 167, 30), "CONVERT ALL", _skin.button) && _converting == false)
            {
                if (EditorUtility.DisplayDialog("Start Converting Scene Objects?",
                    "It's important to note that this does not make a copy of these " +
                    "objects but edits them in place. If this is not something that you " +
                    "want to do then stop this operation now by selecting the 'Cancel' " +
                    "button. If you're unsure what this could do simply copy this scene " +
                    "and run this operation on the copied scene.",
                    "Start Conversion", "Cancel"))
                {
                    this.StartCoroutine(ConvertAll());
                }
            }
            if (_converting == true)
            {
                EditorGUI.DrawRect(new Rect(0, 50, position.width, position.height - 30), _lockColor);
                EditorGUI.DrawRect(new Rect(0, position.height / 2 - 50, position.width, 100), _convertBar);
                EditorGUI.LabelField(new Rect(0, position.height / 2 - 50, position.width, 100), "Converting Objects, please wait...", _skin.GetStyle("Label"));
                EditorGUI.LabelField(new Rect(100, position.height / 2 + 10, position.width, 100), "Important Note: You can refresh this window by clicking into it.", _skin.GetStyle("TextField"));
            }
        }

        //Responsible for collecting all of the objects in the scene that need to be converted
        private void GetSceneObjects()
        {
            convertables.Clear();
            if (_cRigidbodies == true)
            {
                foreach (Rigidbody rb in GameObject.FindObjectsOfType<Rigidbody>())
                {
                    if (!rb.gameObject.GetComponent<vThirdPersonController>() && !rb.GetComponent<PhotonRigidbodyView>() &&
                        !rb.GetComponentInParent<v_AIController>())
                    {
                        convertables.Add(rb.gameObject);
                    }
                }
            }
            if (_cvItemCollections == true)
            {
                foreach (vItemCollection ic in GameObject.FindObjectsOfType<vItemCollection>())
                {
                    if (!ic.gameObject.GetComponent<SyncItemCollection>())
                    {
                        convertables.Add(ic.gameObject);
                    }
                }
            }
            if (_cvBreakableObjects == true)
            {
                foreach (vBreakableObject bo in GameObject.FindObjectsOfType<vBreakableObject>())
                {
                    if (!bo.gameObject.GetComponent<NetworkBreakObject>())
                    {
                        convertables.Add(bo.gameObject);
                    }
                }
            }
            if (_cvHealthController == true)
            {
                foreach (vHealthController hc in GameObject.FindObjectsOfType<vHealthController>())
                {
                    if (!hc.GetComponent<SyncHealthController>() && !hc.GetComponent<vThirdPersonController>() &&
                        !hc.GetComponent<v_AIController>())
                    {
                        convertables.Add(hc.gameObject);
                    }
                }
            }
            if (_cvTriggerGenericAction == true)
            {
                foreach (vTriggerGenericAction ga in GameObject.FindObjectsOfType<vTriggerGenericAction>())
                {
/*                    #region Shooter Template
                    if (ga.gameObject.GetComponent<vThrowCollectable>()) continue;
                    #endregion*/
                    if (ga.gameObject.GetComponent<vItemCollection>()) continue;
                    if (ga.gameObject.GetComponent<CallNetworkEvents>()) continue;
                    convertables.Add(ga.gameObject);
                }
            }
            
/*            #region Shooter Template
            if (_cvThrowCollectable == true)
            {
                foreach (vThrowCollectable tc in GameObject.FindObjectsOfType<vThrowCollectable>())
                {
                    if (!tc.GetComponent<CallNetworkEvents>())
                    {
                        convertables.Add(tc.gameObject);
                    }
                }
            }
            if (_cUseItemTrigger == true)
            {
                foreach (UseItemEventTrigger tc in GameObject.FindObjectsOfType<UseItemEventTrigger>())
                {
                    if (!tc.GetComponent<CallNetworkEvents>())
                    {
                        convertables.Add(tc.gameObject);
                    }
                }
            }
            #endregion*/
        }

        private void PreviewObject(GameObject obj)
        {
            Selection.activeObject = obj;
            _target = obj;
            _objectPreview = Editor.CreateEditor(_target);
            _previewConverts.Clear();
            if (_target.GetComponent<Rigidbody>() && !_target.GetComponent<vBreakableObject>() && _cRigidbodies == true)
            {
                _previewConverts.Add("* Add Photon Rigidbody component");
            }
            if (_target.GetComponent<Invector.vCamera.vThirdPersonCamera>())
            {
                _previewConverts.Add("* vThirdPersonCamera -> MP_vThirdPersonCamera");
            }
            if (_target.GetComponent<vItemCollection>() && _cvItemCollections == true)
            {
                _previewConverts.Add("* Add PhotonView component");
                _previewConverts.Add("* Add SyncItemCollection component");
            }
            if (_target.GetComponent<vBreakableObject>() && _cvBreakableObjects == true)
            {
                _previewConverts.Add("* Add PhotonView component");
                _previewConverts.Add("* Add NetworkBreakObject component");
            }
            if (_target.GetComponent<vHealthController>() && _cvHealthController == true)
            {
                _previewConverts.Add("* Add PhotonView component");
                _previewConverts.Add("* Add SyncHealthController component");
                _previewConverts.Add("* Add SyncHealthController event to vHealthController");
            }
            if (_target.GetComponent<vTriggerGenericAction>() && _cvTriggerGenericAction == true)
            {
                _previewConverts.Add("* Add PhotonView component");
                _previewConverts.Add("* Add CallNetworkEvents component");
                _previewConverts.Add("* Copy All UnityEvents From vTriggerGenericAction -> CallNetworkEvents");
            }
/*            #region Shooter Template
            if (_target.GetComponent<UseItemEventTrigger>() && _cUseItemTrigger == true)
            {
                _previewConverts.Add("* Add PhotonView component");
                _previewConverts.Add("* Add CallNetworkEvents component");
                _previewConverts.Add("* Copy OnUse -> CallNetworkEvents");
            }
            if (_target.GetComponent<vThrowCollectable>() && _cvThrowCollectable == true)
            {
                _previewConverts.Add("* Add PhotonView component");
                _previewConverts.Add("* Add CallNetworkEvents component");
                _previewConverts.Add("* Copy OnCollectObject -> CallNetworkEvents");
            }
            #endregion*/
            _previewScrollHeight = _previewConverts.Count * 40;
        }

        IEnumerator ConvertAll()
        {
            _converting = true;
            if (!FindObjectOfType<NetworkManager>())
            {
                if (EditorUtility.DisplayDialog("Missing Network Manager",
                    "This scene is missing a network manager. Some of the settings depend on the network manager. " +
                    "Please add one to the scene and try again.",
                    "Cancel Conversion"))
                {
                    yield return null;
                }
            }
            foreach (GameObject convertable in convertables)
            {
                C_Rigidbodies(convertable);
                C_vItemCollection(convertable);
                C_vBreakableObject(convertable);
                C_vHealthController(convertable);
                C_vTriggerGenericActions(convertable);

/*                #region Shooter Template
                C_vThrowCollectable(convertable);
                C_UseItemEventTrigger(convertable);
                #endregion*/
            }
            _converting = false;
            if (EditorUtility.DisplayDialog("Scene Conversion Completed!",
                    "The listed objects have been converted. This window will remain up so you can select each object " +
                    "and view the modified components. " +
                    "You can close this window at anytime.",
                    "Okay"))
            {
            }
            OnEnable();
            yield return null;
        }

        void C_vBreakableObject(GameObject target)
        {
            if (_cvBreakableObjects == false) return;
            if (target.GetComponent<vBreakableObject>())
            {
                E_Helpers.SetObjectIcon(target, E_Core.h_genericIcon);
                if (!target.GetComponent<NetworkBreakObject>())
                {
                    target.AddComponent<NetworkBreakObject>();
                }
                bool sync = !FindObjectOfType<NetworkManager>().syncScenes;
                target.GetComponent<NetworkBreakObject>().GetType().GetField("syncCrossScenes", E_Helpers.allBindings).SetValue(target.GetComponent<NetworkBreakObject>(), sync);
                if (sync == true)
                {
                    target.GetComponent<NetworkBreakObject>().GetType().GetField("holder", E_Helpers.allBindings).SetValue(target.GetComponent<NetworkBreakObject>(), target.transform);
                }
                if (!E_PlayerEvents.HasUnityEvent(target.GetComponent<vBreakableObject>().OnBroken, "BreakObject", target.GetComponent<NetworkBreakObject>()))
                {
                    UnityEventTools.AddPersistentListener(target.GetComponent<vBreakableObject>().OnBroken, target.GetComponent<NetworkBreakObject>().BreakObject);
                }
            }
        }
        void C_vItemCollection(GameObject target)
        {
            if (_cvItemCollections == false) return;
            if (!target.GetComponent<vItemCollection>()) return;
            E_Helpers.SetObjectIcon(target, E_Core.h_genericIcon);
            if (!target.GetComponent<PhotonView>())
            {
                target.AddComponent<PhotonView>();
            }
            if (!target.transform.GetComponentInParent<PhotonView>() && !target.GetComponent<PhotonView>())
            {
                target.AddComponent<PhotonView>();
            }
            if (!target.GetComponent<SyncItemCollection>())
            {
                target.AddComponent<SyncItemCollection>();
            }
            bool useTarget = (target.GetComponent<vItemCollection>().onPressActionInputWithTarget.GetPersistentEventCount() > 0);

            // Copy Original Values to MP component
            target.GetComponent<SyncItemCollection>().onPressActionDelay = target.GetComponent<vItemCollection>().onPressActionDelay;
            target.GetComponent<SyncItemCollection>().OnPressActionInput = target.GetComponent<vItemCollection>().OnPressActionInput;
            target.GetComponent<SyncItemCollection>().onPressActionInputWithTarget = target.GetComponent<vItemCollection>().onPressActionInputWithTarget;
            
            target.GetComponent<SyncItemCollection>().GetType().GetField("syncCreateDestroy", E_Helpers.allBindings).SetValue(target.GetComponent<SyncItemCollection>(), false);
            target.GetComponent<SyncItemCollection>().GetType().GetField("skipStartCheck", E_Helpers.allBindings).SetValue(target.GetComponent<SyncItemCollection>(), target.GetComponent<vItemCollection>().items.Count > 0);

            // Clear Original Values
            target.GetComponent<vItemCollection>().onPressActionDelay = 0.0f;
            target.GetComponent<vItemCollection>().OnPressActionInput = new UnityEvent();
            target.GetComponent<vItemCollection>().onPressActionInputWithTarget = new OnDoActionWithTarget();

            // Set Scene Sync Options
            target.GetComponent<SyncItemCollection>().GetType().GetField("syncCrossScenes", E_Helpers.allBindings).SetValue(target.GetComponent<SyncItemCollection>(), true);
            target.GetComponent<SyncItemCollection>().GetType().GetField("holder", E_Helpers.allBindings).SetValue(target.GetComponent<SyncItemCollection>(), target.transform);

            if (target.GetComponent<vItemCollection>().destroyAfter == true)
            {
                target.GetComponent<SyncItemCollection>().OnSceneEnterUpdate = new UnityEvent();
                UnityEventTools.AddBoolPersistentListener(target.GetComponent<SyncItemCollection>().OnSceneEnterUpdate, target.SetActive, false);
            }

            // Set OpenChest Listener on original component
            if (useTarget == true)
            {
                UnityEventTools.AddPersistentListener(target.GetComponent<vItemCollection>().onPressActionInputWithTarget, target.GetComponent<SyncItemCollection>().Collect);
            }
            else
            {
                UnityEventTools.AddPersistentListener(target.GetComponent<vItemCollection>().OnPressActionInput, target.GetComponent<SyncItemCollection>().Collect);
            }
            //if (!E_PlayerEvents.HasUnityEvent(target.GetComponent<SyncItemCollection>().OnSceneEnterUpdate, "SetActive", target))
            //{
            //    UnityEventTools.AddBoolPersistentListener(target.GetComponent<SyncItemCollection>().OnSceneEnterUpdate, target.SetActive, false);
            //}
        }
        void C_Rigidbodies(GameObject target)
        {
            if (_cRigidbodies == false) return;
            if (target.GetComponent<Rigidbody>())
            {
                E_Helpers.SetObjectIcon(target, E_Core.h_genericIcon);
                if (target.GetComponent<vBreakableObject>()) return;
                if (!target.GetComponent<PhotonRigidbodyView>())
                {
                    target.AddComponent<PhotonRigidbodyView>();
                }
                if (!target.GetComponent<PhotonView>())
                {
                    target.AddComponent<PhotonView>();
                }
                List<Component> observables = target.GetComponent<PhotonView>().ObservedComponents;
                if (observables == null || !observables.Contains(target.GetComponent<PhotonRigidbodyView>()))
                {
                    observables = new List<Component>();
                    observables.Add(target.GetComponent<PhotonRigidbodyView>());
                }
                target.GetComponent<PhotonView>().ObservedComponents = observables;
                target.GetComponent<PhotonView>().Synchronization = ViewSynchronization.ReliableDeltaCompressed;
            }
        }
        void C_vHealthController(GameObject target)
        {
            if (_cvHealthController == false) return;
            if (target.GetComponent<vHealthController>() && !target.GetComponent<vThirdPersonController>())
            {
                E_Helpers.SetObjectIcon(target, E_Core.h_genericIcon);
                if (!target.GetComponent<SyncHealthController>())
                {
                    target.AddComponent<SyncHealthController>();
                }
                if (!E_PlayerEvents.HasUnityEvent(target.GetComponent<vHealthController>().onReceiveDamage, "SendDamageOverNetwork", target.GetComponent<SyncHealthController>()))
                {
                    UnityEventTools.AddPersistentListener(target.GetComponent<vHealthController>().onReceiveDamage, target.GetComponent<SyncHealthController>().SendDamageOverNetwork);
                }
            }
        }
        void C_vTriggerGenericActions(GameObject target)
        {
            if (!target.GetComponent<vTriggerGenericAction>()) return;
            if (target.GetComponent<vItemCollection>()) return;
            
/*            #region Shooter Template
            if (target.GetComponent<vThrowCollectable>()) return;
            #endregion*/

            _convertedGenericTriggers = true;
            if (!target.GetComponent<CallNetworkEvents>())
            {
                target.AddComponent<CallNetworkEvents>();
            }
            target.GetComponent<CallNetworkEvents>().GetType().GetField("holder", E_Helpers.allBindings).SetValue(target.GetComponent<CallNetworkEvents>(), target.transform);

            //No Input UnityEvents
            if (!E_PlayerEvents.HasUnityEvent(target.GetComponent<vTriggerGenericAction>().OnPressActionInput, "CallNetworkInvoke1", target.GetComponent<CallNetworkEvents>()))
            {
                target.GetComponent<CallNetworkEvents>().GetType().GetField("NetworkInvoke1", E_Helpers.allBindings).SetValue(target.GetComponent<CallNetworkEvents>(), target.GetComponent<vTriggerGenericAction>().OnPressActionInput);
                target.GetComponent<vTriggerGenericAction>().OnPressActionInput = new UnityEvent();
                UnityEventTools.AddPersistentListener(target.GetComponent<vTriggerGenericAction>().OnPressActionInput, target.GetComponent<CallNetworkEvents>().CallNetworkInvoke1);
            }
            if (!E_PlayerEvents.HasUnityEvent(target.GetComponent<vTriggerGenericAction>().OnCancelActionInput, "CallNetworkInvoke2", target.GetComponent<CallNetworkEvents>()))
            {
                target.GetComponent<CallNetworkEvents>().GetType().GetField("NetworkInvoke2", E_Helpers.allBindings).SetValue(target.GetComponent<CallNetworkEvents>(), target.GetComponent<vTriggerGenericAction>().OnCancelActionInput);
                target.GetComponent<vTriggerGenericAction>().OnCancelActionInput = new UnityEvent();
                UnityEventTools.AddPersistentListener(target.GetComponent<vTriggerGenericAction>().OnCancelActionInput, target.GetComponent<CallNetworkEvents>().CallNetworkInvoke2);
            }
            if (!E_PlayerEvents.HasUnityEvent(target.GetComponent<vTriggerGenericAction>().OnFinishActionInput, "CallNetworkInvoke3", target.GetComponent<CallNetworkEvents>()))
            {
                target.GetComponent<CallNetworkEvents>().GetType().GetField("NetworkInvoke3", E_Helpers.allBindings).SetValue(target.GetComponent<CallNetworkEvents>(), target.GetComponent<vTriggerGenericAction>().OnFinishActionInput);
                target.GetComponent<vTriggerGenericAction>().OnFinishActionInput = new UnityEvent();
                UnityEventTools.AddPersistentListener(target.GetComponent<vTriggerGenericAction>().OnFinishActionInput, target.GetComponent<CallNetworkEvents>().CallNetworkInvoke3);
            }
            if (!E_PlayerEvents.HasUnityEvent(target.GetComponent<vTriggerGenericAction>().OnStartAnimation, "CallNetworkInvoke4", target.GetComponent<CallNetworkEvents>()))
            {
                target.GetComponent<CallNetworkEvents>().GetType().GetField("NetworkInvoke4", E_Helpers.allBindings).SetValue(target.GetComponent<CallNetworkEvents>(), target.GetComponent<vTriggerGenericAction>().OnStartAnimation);
                target.GetComponent<vTriggerGenericAction>().OnStartAnimation = new UnityEvent();
                UnityEventTools.AddPersistentListener(target.GetComponent<vTriggerGenericAction>().OnStartAnimation, target.GetComponent<CallNetworkEvents>().CallNetworkInvoke4);
            }
            if (!E_PlayerEvents.HasUnityEvent(target.GetComponent<vTriggerGenericAction>().OnEndAnimation, "CallNetworkInvoke5", target.GetComponent<CallNetworkEvents>()))
            {
                target.GetComponent<CallNetworkEvents>().GetType().GetField("NetworkInvoke5", E_Helpers.allBindings).SetValue(target.GetComponent<CallNetworkEvents>(), target.GetComponent<vTriggerGenericAction>().OnEndAnimation);
                target.GetComponent<vTriggerGenericAction>().OnEndAnimation = new UnityEvent();
                UnityEventTools.AddPersistentListener(target.GetComponent<vTriggerGenericAction>().OnEndAnimation, target.GetComponent<CallNetworkEvents>().CallNetworkInvoke5);
            }

            ////GameObject Input UnityEvents
            //if (!E_PlayerEvents.HasUnityEvent(target.GetComponent<vTriggerGenericAction>().onPressActionInputWithTarget, "CallGameObjectInvoke1", target.GetComponent<CallNetworkEvents>()))
            //{
            //    target.GetComponent<CallNetworkEvents>().GetType().GetField("NetworkGameObjectInvoke1", E_Helpers.allBindings).SetValue(target.GetComponent<CallNetworkEvents>(), target.GetComponent<vTriggerGenericAction>().onPressActionInputWithTarget);
            //    target.GetComponent<vTriggerGenericAction>().onPressActionInputWithTarget = new OnDoActionWithTarget();
            //    UnityEventTools.AddPersistentListener(target.GetComponent<vTriggerGenericAction>().onPressActionInputWithTarget, target.GetComponent<CallNetworkEvents>().CallGameObjectInvoke1);
            //}
            //if (!E_PlayerEvents.HasUnityEvent(target.GetComponent<vTriggerGenericAction>().OnPlayerEnter, "CallGameObjectInvoke2", target.GetComponent<CallNetworkEvents>()))
            //{
            //    target.GetComponent<CallNetworkEvents>().GetType().GetField("NetworkGameObjectInvoke2", E_Helpers.allBindings).SetValue(target.GetComponent<CallNetworkEvents>(), target.GetComponent<vTriggerGenericAction>().OnPlayerEnter);
            //    target.GetComponent<vTriggerGenericAction>().OnPlayerEnter = new OnDoActionWithTarget();
            //    UnityEventTools.AddPersistentListener(target.GetComponent<vTriggerGenericAction>().OnPlayerEnter, target.GetComponent<CallNetworkEvents>().CallGameObjectInvoke2);
            //}
            //if (!E_PlayerEvents.HasUnityEvent(target.GetComponent<vTriggerGenericAction>().OnPlayerStay, "CallGameObjectInvoke3", target.GetComponent<CallNetworkEvents>()))
            //{
            //    target.GetComponent<CallNetworkEvents>().GetType().GetField("NetworkGameObjectInvoke3", E_Helpers.allBindings).SetValue(target.GetComponent<CallNetworkEvents>(), target.GetComponent<vTriggerGenericAction>().OnPlayerStay);
            //    target.GetComponent<vTriggerGenericAction>().OnPlayerStay = new OnDoActionWithTarget();
            //    UnityEventTools.AddPersistentListener(target.GetComponent<vTriggerGenericAction>().OnPlayerStay, target.GetComponent<CallNetworkEvents>().CallGameObjectInvoke3);
            //}
            //if (!E_PlayerEvents.HasUnityEvent(target.GetComponent<vTriggerGenericAction>().OnPlayerExit, "CallGameObjectInvoke4", target.GetComponent<CallNetworkEvents>()))
            //{
            //    target.GetComponent<CallNetworkEvents>().GetType().GetField("NetworkGameObjectInvoke4", E_Helpers.allBindings).SetValue(target.GetComponent<CallNetworkEvents>(), target.GetComponent<vTriggerGenericAction>().OnPlayerExit);
            //    target.GetComponent<vTriggerGenericAction>().OnPlayerExit = new OnDoActionWithTarget();
            //    UnityEventTools.AddPersistentListener(target.GetComponent<vTriggerGenericAction>().OnPlayerExit, target.GetComponent<CallNetworkEvents>().CallGameObjectInvoke4);
            //}

            ////Float Input UnityEvents
            //if (!E_PlayerEvents.HasUnityEvent(target.GetComponent<vTriggerGenericAction>().OnUpdateButtonTimer, "CallFloatInvoke1", target.GetComponent<CallNetworkEvents>()))
            //{
            //    target.GetComponent<CallNetworkEvents>().GetType().GetField("NetworkSingleInvoke1", E_Helpers.allBindings).SetValue(target.GetComponent<CallNetworkEvents>(), target.GetComponent<vTriggerGenericAction>().OnUpdateButtonTimer);
            //    target.GetComponent<vTriggerGenericAction>().OnUpdateButtonTimer = new vTriggerGenericAction.OnUpdateValue();
            //    UnityEventTools.AddPersistentListener(target.GetComponent<vTriggerGenericAction>().OnUpdateButtonTimer, target.GetComponent<CallNetworkEvents>().CallFloatInvoke1);
            //}
        }
        
/*        #region Shooter Template
        void C_vThrowCollectable(GameObject target)
        {
            if (!target.GetComponent<vThrowCollectable>()) return;
            if (!target.GetComponent<CallNetworkEvents>())
            {
                target.AddComponent<CallNetworkEvents>();
            }
            target.GetComponent<CallNetworkEvents>().GetType().GetField("NetworkInvoke1", E_Helpers.allBindings).SetValue(target.GetComponent<CallNetworkEvents>(), target.GetComponent<vThrowCollectable>().onCollectObject);
            target.GetComponent<CallNetworkEvents>().GetType().GetField("holder", E_Helpers.allBindings).SetValue(target.GetComponent<CallNetworkEvents>(), target.transform);
            target.GetComponent<vThrowCollectable>().onCollectObject = new UnityEvent();

            UnityEventTools.AddPersistentListener(target.GetComponent<vThrowCollectable>().onCollectObject, target.GetComponent<CallNetworkEvents>().CallNetworkInvoke1);
            if (target.GetComponent<vThrowCollectable>().destroyAfter == true)
            {
                UnityEventTools.AddBoolPersistentListener((UnityEventBase)target.GetComponent<CallNetworkEvents>().GetType().GetField("NetworkInvoke1", E_Helpers.allBindings).GetValue(target.GetComponent<CallNetworkEvents>()), target.SetActive, false);
            }
            if (!E_PlayerEvents.HasUnityEvent(target.GetComponent<vThrowCollectable>().onCollectObject, "CallNetworkInvoke1", target.GetComponent<CallNetworkEvents>()))
            {
                UnityEventTools.AddPersistentListener(target.GetComponent<vThrowCollectable>().onCollectObject, target.GetComponent<CallNetworkEvents>().CallNetworkInvoke1);
            }
        }
        void C_UseItemEventTrigger(GameObject target)
        {
            if (!target.GetComponent<UseItemEventTrigger>()) return;
            if (!target.GetComponent<CallNetworkEvents>())
            {
                target.AddComponent<CallNetworkEvents>();
            }
            target.GetComponent<CallNetworkEvents>().GetType().GetField("holder", E_Helpers.allBindings).SetValue(target.GetComponent<CallNetworkEvents>(), target.transform);

            if (!E_PlayerEvents.HasUnityEvent(target.GetComponent<UseItemEventTrigger>().itemEvent.onUse, "CallNetworkInvoke1", target.GetComponent<CallNetworkEvents>()))
            {
                target.GetComponent<CallNetworkEvents>().GetType().GetField("NetworkInvoke1", E_Helpers.allBindings).SetValue(target.GetComponent<CallNetworkEvents>(), target.GetComponent<UseItemEventTrigger>().itemEvent.onUse);
                target.GetComponent<UseItemEventTrigger>().itemEvent.onUse = new UnityEvent();
                UnityEventTools.AddPersistentListener(target.GetComponent<UseItemEventTrigger>().itemEvent.onUse, target.GetComponent<CallNetworkEvents>().CallNetworkInvoke1);
            }
        }
        #endregion*/
    }
}
