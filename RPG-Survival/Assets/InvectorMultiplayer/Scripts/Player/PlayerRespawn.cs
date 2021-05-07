using CBGames.Core;
using Photon.Pun;
using System.Collections;
using UnityEngine;
using System.Linq;
using Invector.vCamera;
using Invector;
using Invector.vCharacterController;
using Invector.vItemManager;
using System.Collections.Generic;

namespace CBGames.Player
{
    #region Spawn Settings
    public enum RespawnType { ClosestPoint, LastSavePoint, Random, TeamStatic, TeamDynamic }
    [System.Serializable]
    public class SpawnTeam
    {
        [Tooltip("The name of your team.")]
        public string teamName = "New Team";
        [Tooltip("Only matters if you have selected \"TeamStatic\" as your respawn type. " +
            "The tag that will be found in your scene to determine where you should spawn.")]
        public string tag = "SpawnPoint";
    }
    #endregion

    [AddComponentMenu("CB GAMES/UI/Player/Player Respawn")]
    public class PlayerRespawn : MonoBehaviour
    {
        [Tooltip("How long to wait before your player will respawn")]
        [SerializeField] protected float respawnDelay = 5.0f;
        [Tooltip("The prefab to display when doing a countdown. Does not have to come from the \"Resources\" folder.\n\n" +
            "This object needs to implement a function called \"StartCounting\". The component \"Counter\" has " +
            "this built in if you would like to make use of it.")]
        public GameObject visualCountdown = null;
        [Tooltip("What type of respawning system are you using?\n\n" +
            "ClosestPoint = Will find the closest respawn point to where you died and use that.\n\n" +
            //"TeamBased = Find the highest concentration of people on your team and spawn at the respawn point closest to them.\n\n" +
            "LastSavePoint = Respawn at whatever is currently set in the respawnPoint transform field.\n\n" +
            "Random = Randomly choose a respawn point and respawn there.\n" +
            "TeamStatic = Find a random spawn point according to your teams tag and spawn there.\n" +
            "TeamDynamic = Find the spawn point that most of your team is next to and spawn there.")]
        [SerializeField] protected RespawnType respawnType = RespawnType.ClosestPoint;
        [Tooltip("Mostly here for debugging purposes. The point where the player will respawn. Is auto selected based on the RespawnType.")]
        public Transform respawnPoint = null;
        [Tooltip("Whether or not you want to send a message via the ChatBox (as a BroadCast Message event) " +
            "when this player dies. Remember that you need to do something with it by opening the UnityEvent " +
            "found on the chatbox under the events tab.")]
        [SerializeField] protected bool broadcastDeathMessage = false;
        [Tooltip("The message that will be broadcast via the chatbox as a BroadCast Message event. " +
            "Remember that you need to do something with it by opening the UnityEvent found on the chatbox " +
            "under the events tab.\n\n" +
            "\"{Nickname}\" will be replaced with the photon player's name.\n\n" +
            "\"{Damager}\" will be replaced with the source player name (if a player, otherwise the transform name) that dealt the killing blow.\n\n" +
            "\"{DamageType}\" will be replaced with the damage type that was last recorded.")]
        [TextArea(2, 10)]
        [SerializeField] protected string deathMessage = "{Nickname} has been killed by {Damager} by using {DamageType}.";
        [Tooltip("If you want a verbose log of what is happening in this script to help you debug things.")]
        [SerializeField] protected bool debugging = false;
        [Tooltip("The team names and the tags that will be used as respawn points for that team.")]
        [SerializeField] protected SpawnTeam[] teams = new SpawnTeam[] { };

        protected bool isRespawning = false;

        /// <summary>
        /// Calls the base `Respawn` function. This is used to get the photon
        /// view component from the owning player before calling `Respawn`.
        /// </summary>
        /// <param name="keepItems">bool type, keep the items of the player that is respawning</param>
        /// <param name="lastDamager">Gameobject type, the gameobject of the thing that dealt the last bit of damage</param>
        /// <param name="lastDamageType">string type, the type of damage that was received.</param>
        public virtual void Respawn(bool keepItems, GameObject lastDamager=null, string lastDamageType=null)
        {
            Respawn(
                NetworkManager.networkManager.GetYourPlayer().transform.GetComponent<PhotonView>(), 
                keepItems,
                lastDamager,
                lastDamageType
            );
        }

        /// <summary>
        /// Calls the `RespawnAction` IEnumerator to start the respawning process.
        /// </summary>
        /// <param name="playerView">PhotonView type, the photonview of the player respawning</param>
        /// <param name="keepItems">bool type, keep the items of the player that is respawning</param>
        /// <param name="lastDamager">Gameobject type, the gameobject of the thing that dealt the last bit of damage</param>
        /// <param name="lastDamageType">string type, the type of damage that was received.</param>
        public virtual void Respawn(PhotonView playerView, bool keepItems, GameObject lastDamager = null, string lastDamageType = "")
        {
            if (isRespawning == true) return;
            isRespawning = true;
            StartCoroutine(RespawnAction(playerView, keepItems, lastDamager, lastDamageType));
        }

        /// <summary>
        /// Will set the death message based on the inputs and broadcasts that message via the 
        /// Chatbox's data channel to everyone in the session, but only if you allow this. Enables
        /// the respawn visual component and starts the countdown. After waiting for the `respawnDelay`
        /// time it calls `NetworkInstantiatePrefab` from the NetworkManager to instantiate your
        /// new player and sets all of its needed values at a target respawn point. It will then 
        /// destroy the old player based on the photonView.
        /// </summary>
        /// <param name="playerView">PhotonView type, the photonview of the player respawning</param>
        /// <param name="keepItems">bool type, keep the items of the player that is respawning</param>
        /// <param name="lastDamager">Gameobject type, the gameobject of the thing that dealt the last bit of damage</param>
        /// <param name="lastDamageType">string type, the type of damage that was received.</param>
        protected virtual IEnumerator RespawnAction(PhotonView playerView, bool keepItems, GameObject lastDamager = null, string lastDamageType = "")
        {
            GameObject respawnVisual = null;
            if (broadcastDeathMessage == true)
            {
                deathMessage = deathMessage.Replace("{Nickname}", playerView.Owner.NickName);
                if (string.IsNullOrEmpty(lastDamageType))
                {
                    deathMessage = deathMessage.Replace("{DamageType}", "a unknown damage type");
                }
                else
                {
                    deathMessage = deathMessage.Replace("{DamageType}", lastDamageType);
                }
                if (lastDamager != null)
                {
                    if (lastDamager.tag == "Player")
                    {
                        deathMessage = deathMessage.Replace("{Damager}", "themself");
                    }
                    else if (lastDamager.GetComponent<PhotonView>() || lastDamager.transform.GetComponentInParent<PhotonView>())
                    {
                        string playerName;
                        if (lastDamager.GetComponent<PhotonView>())
                        {
                            playerName = lastDamager.GetComponent<PhotonView>().Owner.NickName;
                        }
                        else
                        {
                            playerName = lastDamager.transform.GetComponentInParent<PhotonView>().Owner.NickName;
                        }
                        deathMessage = deathMessage.Replace("{Damager}", playerName);
                    }
                    else
                    {
                        deathMessage = deathMessage.Replace("{Damager}", lastDamager.name.Replace("(Clone)", ""));
                    }
                }
                else
                {
                    deathMessage = deathMessage.Replace("{Damager}", "a unknown damager");
                }
                BroadCastMessage message = new BroadCastMessage(
                    "DEATH",
                    deathMessage
                );
                NetworkManager.networkManager.GetChabox().BroadcastData(
                    NetworkManager.networkManager.GetChatDataChannel(),
                    message
                );
            }
            if (visualCountdown != null)
            {
                respawnVisual = Instantiate(visualCountdown) as GameObject;
                respawnVisual.SendMessage("StartCounting", respawnDelay);
            }
            respawnPoint = SelectRespawnPoint(playerView);
            yield return new WaitForSeconds(respawnDelay);
            if (respawnVisual != null)
            {
                Destroy(respawnVisual);
            }
            PhotonNetwork.Destroy(playerView);
            if (!playerView.GetComponentInChildren<vInventory>() && FindObjectOfType<vInventory>())
            {
                Destroy(FindObjectOfType<vInventory>().gameObject);
            }
            yield return new WaitForEndOfFrame();
            GameObject newPlayer = NetworkManager.networkManager.NetworkInstantiatePrefab(
                NetworkManager.networkManager.playerPrefab.name,
                respawnPoint.position,
                respawnPoint.rotation,
                0
            );
            FindObjectOfType<vThirdPersonCamera>().currentTarget = (newPlayer.GetComponentInChildren<vLookTarget>()) ? newPlayer.GetComponentInChildren<vLookTarget>().transform : newPlayer.transform;
            newPlayer.GetComponent<vThirdPersonController>().isDead = false;
            if (FindObjectOfType<vGameController>())
            {
                vGameController gc = FindObjectOfType<vGameController>();
                if (gc != null)
                {
                    gc.currentPlayer = newPlayer;
                }
            }
            yield return new WaitForFixedUpdate();
            if (keepItems == true)
            {
                NetworkManager.networkManager.LoadPlayerData(
                    playerView.Owner.NickName, 
                    newPlayer.GetComponent<vThirdPersonController>(), 
                    false, 
                    true, 
                    true
                );
            }
        }
        
        /// <summary>
        /// Will return a target respawn point based on the settings in this component.
        /// </summary>
        /// <param name="targetView">The player's PhotonView that needs to select a respwn point</param>
        /// <returns>Transform of the target respawn point.</returns>
        protected virtual Transform SelectRespawnPoint(PhotonView targetView)
        {
            Transform retVal = null;
            switch(respawnType)
            {
                case RespawnType.ClosestPoint:
                    GameObject closest = GameObject.FindGameObjectsWithTag("RespawnPoint").OrderBy(
                        (g) =>
                        {
                            return (g.transform.position - targetView.transform.position).sqrMagnitude;
                        }
                    ).First();
                    retVal = closest.transform;
                    break;
                case RespawnType.LastSavePoint:
                    retVal = respawnPoint;
                    break;
                case RespawnType.Random:
                    GameObject[] respawnPoints = GameObject.FindGameObjectsWithTag("RespawnPoint");
                    retVal = respawnPoints[Random.Range(0, respawnPoints.Length)].transform;
                    break;
                case RespawnType.TeamStatic:
                    retVal = SelectStaticTeamSpawn(NetworkManager.networkManager.teamName);
                    break;
                case RespawnType.TeamDynamic:
                    retVal = SelectDynamicTeamSpawn(NetworkManager.networkManager.teamName);
                    break;
            }
            return retVal;
        }

        /// <summary>
        /// Enable or disable this `isRespawning` variable.
        /// </summary>
        /// <param name="isEnabled">bool type, is respawning?</param>
        public void SetRespawnState(bool isEnabled)
        {
            isRespawning = isEnabled;
        }

        /// <summary>
        /// Selects and random "Team" spawn point. These spawn point names are defined
        /// in the `NetworkManager` component.
        /// </summary>
        /// <param name="teamName">string type, the team spawn point name to randomly pick</param>
        /// <returns>Transform of the target respawn point</returns>
        public virtual Transform SelectStaticTeamSpawn(string teamName)
        {
            if (debugging == true) Debug.Log("Selecting static team spawn point...");
            if (string.IsNullOrEmpty(teamName))
            {
                if (debugging == true) Debug.Log("Selecting random point, team name is empty.");
                GameObject[] respawnPoints = GameObject.FindGameObjectsWithTag("RespawnPoint");
                return respawnPoints[Random.Range(0, respawnPoints.Length)].transform;
            }

            List<GameObject> spawns = new List<GameObject>();
            foreach (SpawnTeam team in teams)
            {
                if (team.teamName == teamName)
                {
                    spawns.AddRange(GameObject.FindGameObjectsWithTag(team.tag));
                    break;
                }
            }
            Transform selectedSpawn = spawns[Random.Range(0, spawns.Count)].transform;
            if (debugging == true) Debug.Log("Selected " + selectedSpawn.name + " spawn point.");
            return selectedSpawn;
        }

        /// <summary>
        /// Returns a random point that one of your team mates is at.
        /// </summary>
        /// <param name="teamName">string type, the name of the team to try and find a point for</param>
        /// <returns>Transform point of a found team member</returns>
        public virtual Transform SelectDynamicTeamSpawn(string teamName)
        {
            if (debugging == true) Debug.Log("Selecting a Dynamic Team Spawn point...");
            if (string.IsNullOrEmpty(teamName))
            {
                if (debugging == true) Debug.Log("Selecting random point, team name is empty.");
                GameObject[] respawnPoints = GameObject.FindGameObjectsWithTag("RespawnPoint");
                return respawnPoints[Random.Range(0, respawnPoints.Length)].transform;
            }
                         
            vThirdPersonController[] controllers = FindObjectsOfType<vThirdPersonController>();
            List<Transform> teamMembers = new List<Transform>();
            List<GameObject> spawnPoints = new List<GameObject>();
            spawnPoints.AddRange(GameObject.FindGameObjectsWithTag(NetworkManager.networkManager.spawnPointsTag));
            foreach (vThirdPersonController controller in controllers)
            {
                if (!controller.transform.GetComponent<SyncPlayer>()) continue;
                if (controller.transform.GetComponent<SyncPlayer>().teamName == teamName &&
                    controller.transform.GetComponent<PhotonView>().IsMine == false)
                {
                    teamMembers.Add(controller.transform);
                }
            }

            if (teamMembers.Count < 1)
            {
                if (debugging == true) Debug.Log("No other team members, selecting random point...");
                return spawnPoints[Random.Range(0, spawnPoints.Count)].transform;
            }
            else
            {
                if (debugging == true) Debug.Log("Found " + teamMembers.Count + " selecting closets spawn point...");
                Dictionary<Transform, int> spawnPointCounter = new Dictionary<Transform, int>();
                foreach (Transform teamMember in teamMembers)
                {
                    Transform minDistSpawn = null;
                    float minDist = Mathf.Infinity;
                    Vector3 currentPos = transform.position;
                    foreach (GameObject spawnPoint in spawnPoints)
                    {
                        float dist = Vector3.Distance(teamMember.position, spawnPoint.transform.position);
                        if (dist < minDist)
                        {
                            minDistSpawn = spawnPoint.transform;
                            minDist = dist;
                        }
                    }
                    if (!spawnPointCounter.ContainsKey(minDistSpawn))
                    {
                        spawnPointCounter.Add(minDistSpawn, 1);
                    }
                    else
                    {
                        spawnPointCounter[minDistSpawn] += 1;
                    }
                }
                Transform closetSpawnPoint = spawnPointCounter.Aggregate((l, r) => l.Value > r.Value ? l : r).Key;
                if (debugging == true) Debug.Log("Selecting " + closetSpawnPoint.name + " spawn point.", closetSpawnPoint);
                return closetSpawnPoint;
            }
        }
    }
}