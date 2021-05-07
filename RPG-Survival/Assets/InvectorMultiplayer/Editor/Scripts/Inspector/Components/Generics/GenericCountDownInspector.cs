using UnityEngine;
using UnityEditor;
using CBGames.Editors;
using CBGames.UI;

namespace CBGames.Inspector
{
    [CustomEditor(typeof(GenericCountDown), true)]
    public class GenericCountDownInspector : BaseEditor
    {
        #region Properties
        SerializedProperty useRoomOwnerShip;
        SerializedProperty ifIsOwner;
        SerializedProperty startTime;
        SerializedProperty countSpeed;
        SerializedProperty startType;
        SerializedProperty numberType;
        SerializedProperty texts;
        SerializedProperty soundSource;
        SerializedProperty tickClip;
        SerializedProperty OnStartCounting;
        SerializedProperty OnStopCounting;
        SerializedProperty OnNumberChange;
        SerializedProperty OnZero;
        SerializedProperty _time;
        SerializedProperty syncWithPhotonServer;
        SerializedProperty debugging;
        #endregion

        protected override void OnEnable()
        {
            #region Properties
            debugging = serializedObject.FindProperty("debugging");
            useRoomOwnerShip = serializedObject.FindProperty("useRoomOwnerShip");
            ifIsOwner = serializedObject.FindProperty("ifIsOwner");
            startTime = serializedObject.FindProperty("startTime");
            countSpeed = serializedObject.FindProperty("countSpeed");
            startType = serializedObject.FindProperty("startType");
            numberType = serializedObject.FindProperty("numberType");
            texts = serializedObject.FindProperty("texts");
            soundSource = serializedObject.FindProperty("soundSource");
            tickClip = serializedObject.FindProperty("tickClip");
            OnStartCounting = serializedObject.FindProperty("OnStartCounting");
            OnStopCounting = serializedObject.FindProperty("OnStopCounting");
            OnNumberChange = serializedObject.FindProperty("OnNumberChange");
            OnZero = serializedObject.FindProperty("OnZero");
            _time = serializedObject.FindProperty("_time");
            syncWithPhotonServer = serializedObject.FindProperty("syncWithPhotonServer");
            #endregion

            base.OnEnable();
        }

        public override void OnInspectorGUI()
        {
            #region Core
            base.OnInspectorGUI();
            GenericCountDown countdown = (GenericCountDown)target;
            DrawTitleBar(
                "Generic Count Down",
                "This component will execute a series of UnityEvents based on the countdown time.", 
                E_Core.h_uiIcon
            );
            #endregion

            #region Properties
            EditorGUILayout.PropertyField(useRoomOwnerShip);
            if (useRoomOwnerShip.boolValue == true)
            {
                EditorGUILayout.PropertyField(ifIsOwner);
            }
            EditorGUILayout.PropertyField(startType);
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(startTime);
            EditorGUILayout.PropertyField(syncWithPhotonServer);
            if (syncWithPhotonServer.boolValue == false)
            {
                EditorGUILayout.PropertyField(countSpeed);
            }
            EditorGUILayout.PropertyField(numberType);
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(texts, true);
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(soundSource);
            EditorGUILayout.PropertyField(tickClip);
            GUI.enabled = false;
            EditorGUILayout.PropertyField(_time);
            GUI.enabled = true;
            EditorGUILayout.PropertyField(debugging);
            EditorGUILayout.Space();
            countdown.showUnityEvents = EditorGUILayout.Foldout(countdown.showUnityEvents, "Unity Events");
            if (countdown.showUnityEvents)
            {
                GUI.skin = _original;
                CBEditor.SetColorToEditorStyle(_originalHolder, _originalFoldout);
                EditorGUILayout.PropertyField(OnStartCounting);
                EditorGUILayout.PropertyField(OnStopCounting);
                EditorGUILayout.PropertyField(OnNumberChange);
                EditorGUILayout.PropertyField(OnZero);
                GUI.skin = _skin;
                CBEditor.SetColorToEditorStyle(_skinHolder, _skinHolder);
            }
            #endregion

            #region Core
            EndInspectorGUI(typeof(GenericCountDown));
            #endregion
        }
    }
}