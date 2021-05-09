using UnityEditor;
using UnityEngine;

namespace CBGames.Editors
{
    public class E_Youtube : EditorWindow
    {
        [MenuItem("CB Games/Youtube Tutorials", false, 300)]
        public static void CB_YouTube()
        {
            Application.OpenURL("https://www.youtube.com/channel/UCnmqZ8pqQlB9zXlGqjTK_Ag/playlists");
        }
    }
}