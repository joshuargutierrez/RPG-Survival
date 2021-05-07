using UnityEditor;
using System.IO;
using UnityEngine;

namespace CBGames.Editors
{
    public class E_DeletePhotonDemos : EditorWindow
    {
        [MenuItem("Window/Photon Unity Networking/Delete all demo directories")]
        public static void CB_MENU_DeletePhotonDemos()
        {
            if (EditorUtility.DisplayDialog("Purge Photon Demo Directories?",
                            "Would you like to cleanup your project by deleting all of the Photon Demo directories in your project?",
                                        "Yes","No"))
            {
                if (E_Helpers.DirExists(string.Format("Assets{0}Photon{0}PhotonChat{0}Demos", Path.DirectorySeparatorChar)))
                {
                    Debug.Log("Deleting PhotonChat Demo dir...");
                    E_Helpers.DeleteDir(string.Format("Assets{0}Photon{0}PhotonChat{0}Demos", Path.DirectorySeparatorChar));
                    Debug.Log("Success!");
                }
                if (E_Helpers.DirExists(string.Format("Assets{0}Photon{0}PhotonRealtime{0}Demos", Path.DirectorySeparatorChar)))
                {
                    Debug.Log("Deleting PhotonRealtime Demo dir...");
                    E_Helpers.DeleteDir(string.Format("Assets{0}Photon{0}PhotonRealtime{0}Demos", Path.DirectorySeparatorChar));
                    Debug.Log("Success!");
                }
                if (E_Helpers.DirExists(string.Format("Assets{0}Photon{0}PhotonUnityNetworking{0}Demos", Path.DirectorySeparatorChar)))
                {
                    Debug.Log("Deleting PhotonUnityNetworking Demo dir...");
                    E_Helpers.DeleteDir(string.Format("Assets{0}Photon{0}PhotonUnityNetworking{0}Demos", Path.DirectorySeparatorChar));
                    Debug.Log("Success!");
                }
                if (E_Helpers.DirExists(string.Format("Assets{0}Photon{0}PhotonVoice{0}Demos", Path.DirectorySeparatorChar)))
                {
                    Debug.Log("Deleting PhotonVoice Demo dir...");
                    E_Helpers.DeleteDir(string.Format("Assets{0}Photon{0}PhotonVoice{0}Demos", Path.DirectorySeparatorChar));
                    Debug.Log("Success!");
                }
                AssetDatabase.Refresh();
            }
        }
    }
}