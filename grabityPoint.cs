using UnityEngine;
using System.Collections;

public class grabityPoint : MonoBehaviour {
	private bool isSelected;

	// Use this for initialization
	void Start () {
		isSelected = false;
	}
	
	// Update is called once per frame
	void Update () {
		if(isSelected){
			this.transform.Rotate(1,2,3);
			GameObject[] gos = GameObject.FindGameObjectsWithTag("Player");
			foreach( GameObject go in gos ){
				go.SendMessage("sweepTo",this.gameObject.transform.position);
			}
		}
	}
	
	void OnMouseDown () {
#if false
		isSelected = !isSelected;
#else
		if(!isSelected){
			isSelected = true;
			GameObject[] gos = GameObject.FindGameObjectsWithTag("GravityPoint");
			foreach( GameObject go in gos ){
				if (go != this.gameObject){
					go.SendMessage("unselect");
				}
			}
		}
#endif
	}
	
	public void unselect () {
		isSelected = false;
		transform.rotation = Quaternion.identity;
	}
}
