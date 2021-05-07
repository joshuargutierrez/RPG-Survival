using CBGames.Core;
using Invector.vMelee;
using Photon.Pun;
using UnityEngine;

[AddComponentMenu("CB GAMES/Player/MP Components/MP vMeleeManager")]
public class MP_vMeleeManager : vMeleeManager
{
    #region Weapon Placements
    public override void SetLeftWeapon(GameObject weapon)
    {
        base.SetLeftWeapon(weapon);
        if (GetComponent<PhotonView>().IsMine == true)
        {
            GetComponent<PhotonView>().RPC(
                "MeleeManager_ReceiveSetLeftWeapon",
                RpcTarget.Others,
                (weapon) ? StaticMethods.BuildChildTree(transform, weapon.transform, false) : new int[] { }
            );
        }
    }
    public override void SetRightWeapon(GameObject weapon)
    {
        base.SetRightWeapon(weapon);
        if (GetComponent<PhotonView>().IsMine == true)
        {
            GetComponent<PhotonView>().RPC(
                "MeleeManager_ReceiveSetRightWeapon",
                RpcTarget.Others,
                (weapon) ? StaticMethods.BuildChildTree(transform, weapon.transform, false) : new int[] { }
            );
        }
    }
    public override void SetLeftWeapon(vMeleeWeapon weapon)
    {
        base.SetLeftWeapon(weapon);
        if (GetComponent<PhotonView>().IsMine == true)
        {
            GetComponent<PhotonView>().RPC(
                "MeleeManager_ReceiveSetLeftWeapon",
                RpcTarget.Others,
                (weapon) ? StaticMethods.BuildChildTree(transform, weapon.transform, false) : new int[] { }
            );
        }
    }
    public override void SetRightWeapon(vMeleeWeapon weapon)
    {
        base.SetRightWeapon(weapon);
        if (GetComponent<PhotonView>().IsMine == true)
        {
            GetComponent<PhotonView>().RPC(
                "MeleeManager_ReceiveSetLeftWeapon",
                RpcTarget.Others,
                (weapon) ? StaticMethods.BuildChildTree(transform, weapon.transform, false) : new int[] { }
            );
        }
    }
    #endregion

    #region RPCs
    [PunRPC]
    void MeleeManager_ReceiveSetLeftWeapon(int[] treeToWeapon)
    {
        if (treeToWeapon.Length > 0)
        {
            Transform weapon = StaticMethods.FindTargetChild(treeToWeapon, transform);
            SetLeftWeapon(weapon.gameObject);
        }
        else
        {
            GameObject empty = null;
            SetLeftWeapon(empty);
        }
    }
    [PunRPC]
    void MeleeManager_ReceiveSetRightWeapon(int[] treeToWeapon)
    {
        if (treeToWeapon.Length > 0)
        {
            Transform weapon = StaticMethods.FindTargetChild(treeToWeapon, transform);
            SetRightWeapon(weapon.gameObject);
        }
        else
        {
            GameObject empty = null;
            SetLeftWeapon(empty);
        }
    }
    #endregion
}