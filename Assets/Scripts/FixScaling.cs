using UnityEngine;
using UnityEngine.Networking;

public class FixScaling : NetworkBehaviour
{

    [SyncVar]
    public Vector3 scale;

	void Start ()
	{
	    transform.localScale = scale;
	    GetComponent<Renderer>().material.mainTextureScale = new Vector2(scale.x, scale.y);
	}

}
