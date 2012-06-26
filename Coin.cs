using UnityEngine;
using System.Collections;
using SoundManage;

public class Coin: MonoBehaviour {

	public GameObject pinObj;
	public GameObject ghostObj;

	private int		objTimer;
	private Vector3 pinPos;
	private Vector3 oldPos;
	private Vector3 spdVec;
	private const float acc = 0.95f;
	private GameObject usePinObj;
	private bool isHitOld;
	
	// Use this for initialization
	void Start () {
		objTimer = 0;
		pinPos = transform.position;
		oldPos = transform.position;
		spdVec = Vector3.zero;
		usePinObj = null;
		isHitOld = false;
	}
	
	// Update is called once per frame
	void Update () {
		objTimer++;
	}
	void LateUpdate(){
		transform.position += spdVec;
		spdVec *= acc;
		if(spdVec.sqrMagnitude < 0.00001f){
			spdVec = Vector3.zero;
		}
		bool isHit = hitUpdae();
		soundUpdate(isHit?1:0);
		oldPos = transform.position;
	}
	
	void OnMouseDown(){
		gameObject.tag="Player";
		pinPos = transform.position;
		GameObject camBaseObj = GameObject.FindGameObjectWithTag("tagCameraBase");
		if(camBaseObj!=null){
			camBaseObj.transform.position = gameObject.transform.position;
		}
		if(usePinObj==null){
			usePinObj = (GameObject)Instantiate(pinObj);
		}
		usePinObj.renderer.enabled = true;
		usePinObj.transform.position = pinPos;
	}
	void OnMouseUp(){
		gameObject.tag="tagCoin";
		if(usePinObj!=null){
			usePinObj.renderer.enabled=false;
		}
		addSpdVec( (pinPos-usePinObj.transform.position)*0.1f );
		spdVec.y = 0.0f;
		Destroy(usePinObj);
		GameObject[] gos = GameObject.FindGameObjectsWithTag("tagCoin");
		foreach(GameObject go in gos){
			go.GetComponent<LineRenderer>().enabled=false;
		}
	}
	
	void OnMouseDrag(){
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		RaycastHit hit;
		int mask = (1<<LayerMask.NameToLayer("floorLayer"));
		bool result = Physics.Raycast(ray,out hit,100.0f,mask);
		if( result ){
			Vector3 resPos = hit.point;
			resPos.y += 0.1f;
			usePinObj.transform.position = resPos;
			GameObject camBaseObj = GameObject.FindGameObjectWithTag("tagCameraBase");
			if(camBaseObj!=null){
//				camBaseObj.transform.LookAt(resPos);
			}
		}
#if true
		if((objTimer & 0x0f) == 0){
			GameObject tmpGhostObj = (GameObject)Instantiate(ghostObj);
			tmpGhostObj.transform.position = transform.position;
			tmpGhostObj.SendMessage("addSpdVec",(pinPos-usePinObj.transform.position)*0.1f);
		}
#endif
		Vector3 outPos = usePinObj.transform.position;
		crossNearCheck(out outPos, "tagCoin");
	}
	
	//-----------------------------------------------------------------------------
	//-----------------------------------------------------------------------------
	public Vector3 addSpdVec(Vector3 speed){
		spdVec += speed;
		return(spdVec);
	}
	
	//-----------------------------------------------------------------------------
	// サウンド更新 :発音したらtrue
	//-----------------------------------------------------------------------------
	bool soundUpdate(int soundId=0){
		bool ret = false;
		if(soundId!=0){
			GameObject camObj = GameObject.FindGameObjectWithTag("tagCameraBase");
			if(camObj!=null){
				SoundManage.SoundData data =  new SoundData();
				data.parent=gameObject;
				data.soundstr = "coinHit";
				SoundManager smg = camObj.GetComponent<SoundManager>();
				smg.SendMessage("queue",data);
				ret = true;
			}
		}
		return ret;
	}
	
	//-----------------------------------------------------------------------------
	// 衝突ステップ :衝突があったらtrue
	//-----------------------------------------------------------------------------
	bool hitUpdae(){
		bool ret = false;
		GameObject[] gos = GameObject.FindGameObjectsWithTag("tagCoin");
		Vector2 p0 = new Vector2(oldPos.x,oldPos.z);
		Vector2 p1 = new Vector2(transform.position.x, transform.position.z);
		float mr = gameObject.transform.localScale.x*0.5f; // 基準反半径 
		float hitBreak = 1.9f; // ぶつかったときの減衰係数 
		foreach(GameObject go in gos){
//			if(go == this.gameObject)	break; // 熱交換は各一回 
			if(go == this.gameObject)	continue;
			
			float tr = go.transform.localScale.x*0.5f; // 基準半径 
			float massRate = mr / (mr+tr); // 自分と相手の簡易質量比率
			Vector2 p = new Vector2(go.transform.position.x, go.transform.position.z);
			Vector2 tgtPoint = nearestPointOnLine(p0,p1,p,true); // 直線上で最も近い場所 
			if( (p - tgtPoint).sqrMagnitude < (mr+tr)*(mr+tr) ){ // 接触の可能性あり
				if(true){ // 線分の内側(nearestPointOnLine(,,,false)かつ線分の内側)
					float chipRate = (p - tgtPoint).magnitude / (mr+tr); // かすり率(=1-めり込み率) 
					// (直線上で点にもっとも近い点から、接触したときの中心までの距離)/接触限界距離
					float hitRateOnLine = Mathf.Sqrt(1 - chipRate*chipRate);
					// 接触直後の直線上の位置 
					Vector2 hitFront = tgtPoint - (p1-p0) * hitRateOnLine;
					// 止まっていたオブジェクトに与えるエネルギー 
					Vector2 hitPow1 = (p-hitFront).normalized * spdVec.magnitude * (1-chipRate) * massRate * hitBreak;
					go.SendMessage("addSpdVec",new Vector3(hitPow1.x,0.0f,hitPow1.y));
//					go.GetComponent<Coin>().addSpdVec(new Vector3(hitPow1.x,0.0f,hitPow1.y));
					Vector2 hitPow0 = -(p-hitFront).normalized * spdVec.magnitude * (1-chipRate) * (1-massRate) * hitBreak;
					addSpdVec(new Vector3(hitPow0.x,0.0f,hitPow0.y));
					ret = true;
				}
			}
		}
		return ret;
	}

	//-----------------------------------------------------------------------------
	//-----------------------------------------------------------------------------
	void crossNearCheck(out Vector3 outPos, string tagName){
		Vector3 pinPos = usePinObj.transform.position;
		GameObject[] gos = GameObject.FindGameObjectsWithTag(tagName);
		if(gos.Length > 1){
			Vector2 p2 = new Vector2(transform.position.x,transform.position.z);
			Vector2 p3 = new Vector2(pinPos.x,pinPos.z);
			for(int ii = 0; ii < gos.Length; ++ii){
				for(int jj = 0; jj < gos.Length; ++jj){
					if(ii<jj){
						Vector2 p0 = new Vector2(gos[ii].transform.position.x,gos[ii].transform.position.z);
						Vector2 p1 = new Vector2(gos[jj].transform.position.x,gos[jj].transform.position.z);
					
						Vector2 resPoint;
						if(intersection(out resPoint,p0,p1,p2,p3,true)){
							LineRenderer line = gos[ii].GetComponent<LineRenderer>();
							line.enabled=true;
							line.SetPosition(0,gos[ii].transform.position);
							line.SetPosition(1,gos[jj].transform.position);
		//					usePinObj.transform.position = new Vector3(resPoint.x,transform.position.y,resPoint.y);
						}
					}
				}
			}
		}
		outPos = pinPos;
	}
	
	//-----------------------------------------------------------------------------
	//! 点にもっとも近い直線上の点(isSegmentがtrueで線分判定)
	//-----------------------------------------------------------------------------
	Vector2 nearestPointOnLine(Vector2 p1, Vector2 p2, Vector2 p, bool isSegment=true){
	    Vector2 d = p2 - p1;
	    if (d.sqrMagnitude == 0)    return p1;
	    float t = (d.x * (p - p1).x + d.y * (p - p1).y) / d.sqrMagnitude;
		if(isSegment){
		    if (t < 0)    return p1;
		    if (t > 1)    return p2;
		}
		Vector2 c = new Vector2( (1 - t) * p1.x + t * p2.x, (1 - t) * p1.y + t * p2.y);
	    return c;
	}
	
	//-----------------------------------------------------------------------------
	//! 直線と点の距離(isSegmentがtrueで線分判定)
	//-----------------------------------------------------------------------------
	float lineToPointDistance(Vector2 p1, Vector2 p2, Vector2 p, bool isSegment=true){
		return ( p - nearestPointOnLine(p1,p2,p,isSegment) ).magnitude;
	}
	
	//-----------------------------------------------------------------------------
	//! 線分の交差チェック   交差したらtrue
	//-----------------------------------------------------------------------------
	bool crossCheck(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4)	{
		if(((p1.x-p2.x)*(p3.y-p1.y)+(p1.y-p2.y)*(p1.x-p3.x))*((p1.x-p2.x)*(p4.y-p1.y)+(p1.y-p2.y)*(p1.x-p4.x))<0){
			if(((p3.x-p4.x)*(p1.y-p3.y)+(p3.y-p4.y)*(p3.x-p1.x))*((p3.x-p4.x)*(p2.y-p3.y)+(p3.y-p4.y)*(p3.x-p2.x))<0){
				return(true);
			}
		}
		return(false);
	}

	//-----------------------------------------------------------------------------
	//! 直線と直線の交点(isSegmentがtrueで線分判定):nullなら交差しない
	//-----------------------------------------------------------------------------
	bool intersection(out Vector2 ret, Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4, bool isSegment=true){
		bool result = false;
		ret = Vector2.zero;
		float bs = ( p2.x - p1.x )*( p4.y - p3.y ) - ( p2.y - p1.y )*( p4.x - p3.x );
		if( bs != 0 ){	// !平行
			Vector2 ac = p3 - p1;
			float h1 = ( ( p2.y - p1.y ) * ac.x - ( p2.x - p1.x ) * ac.y ) / bs;
			float h2 = ( ( p4.y - p3.y ) * ac.x - ( p4.x - p3.x ) * ac.y ) / bs;
			if( (!isSegment) || ((h1>=0)&&(h1<=1)&&(h2>=0)&&(h2<=1)) ){
				ret = p1 + h2 * ( p2 - p1 );
				result = true;
			}
		}
		return result;
	}
}
