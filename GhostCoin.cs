using UnityEngine;
using System.Collections;

public class GhostCoin : MonoBehaviour {

	private int objTimer;
	private Vector3 spdVec;
	private const float acc = 0.95f;
	// Use this for initialization
	void Start () {
		objTimer=0;
	}
	
	// Update is called once per frame
	void Update () {
		objTimer++;
		if(objTimer>60){
			Destroy(gameObject);
		}else{
			transform.position += spdVec;
			spdVec *= acc;
		}
	}
	
	//-----------------------------------------------------------------------------
	//-----------------------------------------------------------------------------
	public Vector3 addSpdVec(Vector3 speed){
		spdVec += speed;
		return(spdVec);
	}
}
