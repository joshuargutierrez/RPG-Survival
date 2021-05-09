using System.Collections.Generic;
using UnityEngine;

namespace CBGames.Core
{
    [CreateAssetMenu(menuName = "CB Games/Scene", fileName = "Scene")]
    public class DatabaseScene : ScriptableObject
    {
        public string sceneName = "";
        public string path = "";
        public int index = 0;
        public bool enabled = false;
        public List<string> entrancePoints = null;

        public DatabaseScene(string inputName, string path, int inputIndex, bool inputEnabled, List<string> inputEntrancePoints = null)
        {
            this.name = inputName;
            this.index = inputIndex;
            this.enabled = inputEnabled;
            if (inputEntrancePoints != null)
            {
                this.entrancePoints = inputEntrancePoints;
            }
        }
    }
}