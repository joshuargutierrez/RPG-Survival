using UnityEngine;

namespace CBGames.Core
{
    public partial class PhotonEventCodes : MonoBehaviour
    {
        public const byte CB_EVENT_READYUP = 1;
        public const byte CB_EVENT_TEAMCHANGE = 2;
        public const byte CB_EVENT_STARTSESSION = 3;
        public const byte CB_EVENT_AUTOSPAWN = 4;
        public const byte CB_EVENT_PLAYERLEFT = 5;
        public const byte CB_EVENT_KICKPLAYER = 6;
        public const byte CB_EVENT_MAPCHANGE = 7;
        public const byte CB_EVENT_RANDOMSCENELIST = 8;
        public const byte CB_EVENT_SCENEVOTE = 9;
        public const byte CB_EVENT_STARTCOUNTDOWN = 10;
        public const byte CB_EVENT_PLAYERJOINED = 11;
        public const byte CB_EVENT_ROUNDTIME = 12;
        public const byte CB_EVENT_VOICEVIEW = 13;
        /*
         * Probably not a good idea to add things to this file if you're planning 
         * on updating constantly. If you do add things to this file and plan on
         * merging your changes with future updates start with the highest allowed
         * value and work your way down from there as to not clash with upcoming 
         * future values.
         * 
         * Since this is a partial class it's better to make a seperate file that
         * you maintane that can still make use of this class across multiple 
         * files. Then you can add your own byte values but still reference them
         * via this class.
         */
    }
}