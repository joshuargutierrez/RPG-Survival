using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomProperty
{
    [Tooltip("CONST string that is used by other classes when making the password key. Used in photon RoomProperty definitions.")]
    public const string Password = "PW";
    [Tooltip("CONST string that is used by other classes when making the room name key. Used in photon RoomProperty definitions.")]
    public const string RoomName = "RN";
    [Tooltip("CONST string that is used by other classes when making the room type key. Used in photon RoomProperty definitions.")]
    public const string RoomType = "RT";


    [Tooltip("CONST string that is used by other classes when making a private room key. Used in photon RoomProperty definitions.")]
    public const string PrivateRoomType = "PRIVATE";
    [Tooltip("CONST string that is used by other classes when making a public room key. Used in photon RoomProperty definitions.")]
    public const string PublicRoomType = "PUBLIC";
}