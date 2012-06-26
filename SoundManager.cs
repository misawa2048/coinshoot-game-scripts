using UnityEngine;
using System.Collections;

namespace SoundManage{
	public class SoundData {
		public string soundstr="";
		public GameObject parent=null;
		public int soundTime=0;
		public int elapsedTime=0;
		public bool isPrePrio=false;
		public bool isTrigger=false;
		public bool isDone=false;
	}
}
public class SoundManager : MonoBehaviour {
	ArrayList queueList;
	ArrayList soundList;
	// Use this for initialization
	void Start () {
		queueList = new ArrayList();
		soundList = new ArrayList();
	}
	
	// Update is called once per frame
	void Update () {
		if(soundList.Count<32){
			foreach(SoundManage.SoundData data in queueList){
				bool isInUse=false;
				foreach(SoundManage.SoundData srcData in soundList){
					if(  (data.soundstr == srcData.soundstr) ){
						if(srcData.elapsedTime < 2){
							isInUse=true;
							break;
						}
					}
					if(  (data.soundstr == srcData.soundstr)&&(data.parent == srcData.parent)){
						if(srcData.elapsedTime < 10){
							srcData.elapsedTime=0;
							isInUse=true;
						}
						break;
					}
				}
				if(!isInUse){
					soundList.Add(data);
				}
			}
			queueList.Clear();
		}
		for( int ii  =0; ii < soundList.Count; ++ii){
			SoundManage.SoundData srcData = (SoundManage.SoundData)(soundList[ii]);
			srcData.elapsedTime++;
			if(srcData.soundTime > srcData.elapsedTime){
				if(!srcData.isDone){
					srcData.isDone = true;
					Camera.main.GetComponent<AudioSource>().Play();
				}
			}else{
				soundList.Remove(srcData);
			}
		}

	}
	
	public bool queue (SoundManage.SoundData data){
		data.soundTime = 30;
		data.elapsedTime = 0;
		data.isPrePrio = true;
		data.isTrigger = true;
		queueList.Add(data);
		return true;
	}
}
