using System;
using UnityEngine;
using System.Collections;
using System.Net;
using System.Text;

public class TestDungeon : MonoBehaviour {

	// Use this for initialization
	void Start () {
	    Stage s = new Stage(41, 41);
	    Dungeon d = new Dungeon();
	    d.generate(s);
	    StringBuilder dString = new StringBuilder();
	    for (int w = 0; w < d.stage.width; w++)
	    {
	        for (int h = 0; h < d.stage.height; h++)
	        {
	            dString.Append(d.stage.tiles[w, h].type.name[0]);
	        }
	        dString.AppendLine();
	    }
	    Debug.Log(dString.ToString());
	    Debug.Log("Dungeon Finished.");
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
