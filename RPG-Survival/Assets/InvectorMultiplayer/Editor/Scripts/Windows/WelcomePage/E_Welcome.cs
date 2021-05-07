#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace CBGames.Editors
{
    public class E_Welcome : EditorWindow
    {
        Vector2 _minrect = new Vector2(500, 510);
        Vector2 _maxrect = new Vector2(500, 510);
        GUISkin _skin = null;
        bool showGetStarted = true;
        bool showDocs = false;
        bool showHelp = false;


        [MenuItem("Window/Reset/Reset Welcome Page")]
        public static void ResetWelcomePage()
        {
            AssetDatabase.DeleteAsset("Assets/InvectorMultiplayer/Editor/Scripts/Windows/Resources/WelcomeStatus.asset");
        }

        [MenuItem("CB Games/Welcome Page", false, 300)]
        public static void CB_OpenWelcomePage()
        {
            GetWindow<E_Welcome>(true);
        }

        [MenuItem("CB Games/Open Documentation Website", false, 300)]
        public static void CB_OpenDocumentationWebsite()
        {
            Application.OpenURL("https://wesleywh.github.io/InvectorMultiplayerAddOnDocs/");
        }

        private void OnEnable()
        {
            if (!_skin) _skin = E_Helpers.LoadSkin(E_Core.e_guiSkinPath);
            this.minSize = _minrect;
            this.maxSize = _maxrect;
            titleContent = new GUIContent("Welcome To The Invector Multiplayer Add-On");
        }

        private void OnGUI()
        {
            GUI.skin = _skin;
            EditorGUI.DrawRect(new Rect(0, 0, position.width, position.height), E_Colors.e_c_blue_3);
            EditorGUI.DrawRect(new Rect(5, 5, position.width - 10, 110), E_Colors.e_c_blue_5);
            GUI.DrawTexture(new Rect(80, -30, 350, 200), E_Helpers.LoadImage(new Vector2(1024, 1024), E_Core.e_invectorMPTitle));

            for (int i = 0; i < 19; i++)
            {
                EditorGUILayout.Space();
            }
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Getting Started"))
            {
                showGetStarted = true;
                showDocs = false;
                showHelp = false;
            }
            else if (GUILayout.Button("Documentation"))
            {
                showGetStarted = false;
                showDocs = true;
                showHelp = false;
            }
            else if (GUILayout.Button("Additional Help"))
            {
                showGetStarted = false;
                showDocs = false;
                showHelp = true;
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal(GUI.skin.box, GUILayout.ExpandHeight(true));
            EditorGUILayout.BeginVertical(GUILayout.ExpandHeight(true));
            if (showGetStarted)
            {
                EditorGUILayout.LabelField("How Do I Use This Add-On?", GUI.skin.label);
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("0. Watch the \"Full Setup Video v4\" found on my YouTube channel.", GUI.skin.textArea);
                EditorGUILayout.LabelField("1. Open a scene you want to convert to support multiplayer.", GUI.skin.textArea);
                EditorGUILayout.LabelField("2. Activate all the add-ons and templates you currently have in your project by going to " +
                    "CB Games > Enable Support > ... Then select every add-on you wish to enable.", GUI.skin.textArea);
                EditorGUILayout.LabelField("3. Check the status of the add-ons (if they're enabled or not) by going to CB Games > Check File Status'", GUI.skin.textArea);
                EditorGUILayout.LabelField("4. With your scene open and all your add-ons activated with the Invector Multiplayer Add-On go to CB Games > Main Menu.", GUI.skin.textArea);
                EditorGUILayout.LabelField("5. Follow the detailed instructions on the main menu and every selected sub menu item.", GUI.skin.textArea);
                EditorGUILayout.LabelField("7. After running through the entire main menu run the \"Scene Tests\" and follow the instructions given " +
                    "to you there to put the final touchs on your scene.", GUI.skin.textArea);
                EditorGUILayout.LabelField("8. Look at all the other cool menu items available to you that are not presented to you in the main menu window.", GUI.skin.textArea);
            }
            if (showDocs)
            {
                EditorGUILayout.LabelField("Where Can I Find Documentation?", GUI.skin.label);
                EditorGUILayout.LabelField("Documentation currently mostly consist of YouTube videos. You " +
                    "can go straight to my YouTube channel by clicking on CB Games > YouTube Tutorials.\n\n" +
                    "However there is a WIP documentation website that can be found here: \n\n" +
                    "https://wesleywh.github.io/InvectorMultiplayerAddOnDocs/" +
                    "\n or \nby clicking on the \"CB Games/Open Documentation Website\" menu item.", GUI.skin.textArea);
                EditorGUILayout.LabelField("Where Can I Find Current Tasks?", GUI.skin.label);
                EditorGUILayout.LabelField("All tasks for this project can be found on my private github page. " +
                    "Login to the Invector Multiplayer Add-On Discord team:\n\n" +
                    "discord.gg/ERzKPSx" +
                    "\n\n" +
                    "and look at the \"task-list\" channel " +
                    "at the bottom to learn how you can gain access to this as well as wip code.", GUI.skin.textArea);
            }
            if (showHelp)
            {
                EditorGUILayout.LabelField("How Can I Get Help If I Get Stuck?", GUI.skin.label);
                EditorGUILayout.LabelField("The fastest way to get help is to join the Discord team and ask for help " +
                    "in the multiplayer-help channel. Here is a link you can use to join the discord team: \n\n" +
                    "discord.gg/ERzKPSx" +
                    "\n\nA slower method is to email me directly at wes@cyberbulletgames.com.", GUI.skin.textArea);
            }
            if (E_WelcomeState.DisplayWelcomeScreen) { };
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
        }
    }
}
#endif