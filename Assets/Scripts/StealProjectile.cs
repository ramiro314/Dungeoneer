using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityStandardAssets.Characters.ThirdPerson;

public class StealProjectile : NetworkBehaviour
{
    public MyThirdPersonUserControl caster;
    public int rotationSpeed;
    public float maxSize;
    public float growthSpeed;

    private bool grow;
    private bool returnToPlayer;

	// Use this for initialization
	void Start () {
	    transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
	    returnToPlayer = false;
	    grow = true;
	}

    private void Update()
    {
        transform.Rotate(0, rotationSpeed * Time.deltaTime, 0);
        if (grow)
        {
            transform.localScale += new Vector3(growthSpeed, growthSpeed, growthSpeed) * Time.deltaTime;
            if (transform.localScale.x > maxSize)
            {
                grow = false;
                returnToPlayer = true;
            }
        }
        else
        {
            transform.localScale -= new Vector3(growthSpeed, growthSpeed, growthSpeed) * Time.deltaTime;
            if (transform.localScale.x < 0.1f)
            {
                Destroy(this);
            }
        }
        if (returnToPlayer)
        {
            float returnSpeed = caster.m_Character.m_MoveSpeedMultiplier + caster.m_Character.m_MoveSpeedMultiplier * 0.5f;
            float step = returnSpeed * Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, caster.transform.position, step);
        }
    }
}
