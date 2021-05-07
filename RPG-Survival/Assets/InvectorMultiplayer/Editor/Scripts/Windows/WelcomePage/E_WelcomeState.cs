#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;
using System.IO;

namespace CBGames.Editors
{
    [InitializeOnLoad]
    [Serializable]
    public class E_WelcomeState : ScriptableObject
    {
        private static E_WelcomeState welcomePageInstance;
        [SerializeField] private bool alreadyDisplayed = true;

        static E_WelcomeState()
        {
            EditorApplication.update += OnInit;
        }
        static void OnInit()
        {
            EditorApplication.update -= OnInit;
            if (WelcomePageInstance) { }
        }

        public static E_WelcomeState WelcomePageInstance
        {
            get
            {
                if (welcomePageInstance == null)
                {
                    welcomePageInstance = Resources.Load<E_WelcomeState>("WelcomeStatus");
                    if (welcomePageInstance == null)
                    {
                        welcomePageInstance = CreateInstance<E_WelcomeState>();
                        if (!Directory.Exists("Assets/InvectorMultiplayer/Editor/Scripts/Windows/Resources/"))
                        {
                            Directory.CreateDirectory("Assets/InvectorMultiplayer/Editor/Scripts/Windows/Resources/");
                        }
                        AssetDatabase.CreateAsset(welcomePageInstance, "Assets/InvectorMultiplayer/Editor/Scripts/Windows/Resources/WelcomeStatus.asset");
                        E_Welcome.CB_OpenWelcomePage();
                    }
                }
                return welcomePageInstance;
            }
        }
        public static bool DisplayWelcomeScreen
        {
            get { return WelcomePageInstance.alreadyDisplayed; }
            set
            {
                if (value != WelcomePageInstance.alreadyDisplayed)
                {
                    WelcomePageInstance.alreadyDisplayed = value;
                    SaveSettings();
                }
            }
        }

        public static void SaveSettings()
        {
            if (!AssetDatabase.Contains(WelcomePageInstance))
            {
                var copy = CreateInstance<E_WelcomeState>();
                EditorUtility.CopySerialized(WelcomePageInstance, copy);
                welcomePageInstance = Resources.Load<E_WelcomeState>("vEditorStartupPrefs");
                if (welcomePageInstance == null)
                {
                    AssetDatabase.CreateAsset(copy, "Assets/InvectorMultiplayer/Editor/Scripts/Windows/Resources/WelcomeState.asset");
                    AssetDatabase.Refresh();
                    welcomePageInstance = copy;

                    return;
                }
                EditorUtility.CopySerialized(copy, welcomePageInstance);
            }
            EditorUtility.SetDirty(WelcomePageInstance);
        }

        
    }
}
#endif