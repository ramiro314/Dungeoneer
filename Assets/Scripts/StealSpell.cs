using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.UI;

public class StealSpell : ItemEffect
{
    public GameObject projectilePrefab;

    public override void UseItem()
    {
        GameObject projectile = (GameObject)Instantiate(projectilePrefab, caster.transform.position,
            Quaternion.LookRotation(Vector3.forward));
        projectile.GetComponent<StealProjectile>().caster = caster;
        NetworkServer.Spawn(projectile);
    }
}
