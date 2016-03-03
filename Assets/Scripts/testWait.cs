using UnityEngine;
using System.Collections;

public class testWait : MonoBehaviour {

	// Use this for initialization
	void Start () {
		Debug.Log (System.DateTime.Now.ToString());
		StartCoroutine (WaitTest());
		Debug.Log ("Endf");

		Debug.Log (System.DateTime.Now.ToString());
		StartCoroutine(WaitForSeconds(10));
			Debug.Log ("End");

			Debug.Log (System.DateTime.Now.ToString());
	}
	IEnumerator WaitTest(){
		Debug.Log (System.DateTime.Now.ToString());
		yield return StartCoroutine( WaitForSeconds (10));
		Debug.Log ("Waited");
		Debug.Log (System.DateTime.Now.ToString());

	}
	IEnumerator WaitForSeconds (int num)
	{

		yield return new WaitForSeconds (num);

	}
	// Update is called once per frame
	void Update () {
	
	}
}
