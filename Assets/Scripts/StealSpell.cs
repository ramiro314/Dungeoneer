/*
Efecto de StealSpell
*/
using UnityEngine;
using UnityEngine.Networking;

public class StealSpell : ItemEffect
{
    public GameObject projectilePrefab;

    public override void UseItem()
    {
        // Spawn projectile
        GameObject projectile = (GameObject)Instantiate(projectilePrefab, caster.transform.position,
            Quaternion.LookRotation(Vector3.forward));
        projectile.GetComponent<StealProjectile>().caster = caster;
        NetworkServer.Spawn(projectile);
    }
}
