/*
Network.Spawn ignora scales cuando instancia objectos en los clientes.
Este script toma el scale del objecto en el servidor y lo aplica en los clientes.
Tambien escala el material de forma tal que coincida con el objeto.
*/
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
