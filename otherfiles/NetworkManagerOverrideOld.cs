using UnityEngine;
using System.Collections;
using System;
using UnityEngine.Networking;
using SimpleJSON;
using System.Linq;

public class NetworkManagerOverrideOld: NetworkManager
{

	public GameObject[] playerPrefabs;

	GameObject prefab;

	int participant = -1;
	public bool isHost;

	//will make this isHost
	bool hosting;

	public override void OnServerAddPlayer (NetworkConnection conn, short playerControllerId)
	{
		//System.

		int part = gameObject.GetComponent<SetupServer> ().participant;
		Debug.LogWarning ("Adding");
		Debug.LogWarning (part);
		//Random r = new System.Random();
		//participant = r.Next(1,4);
		if (part < 0) {//first prefab is experimetner
			Debug.LogWarning ("orig");
			// For default type use default implementation
			// (spawnes prefab configured in inspector)
			base.OnServerAddPlayer (conn, 1);
		} else {
			Debug.LogWarning ("add");
			playerPrefab = Instantiate (playerPrefabs [part])  ;
			base.OnServerAddPlayer(conn, playerControllerId);
		}
		
	}

	public override void OnClientConnect (NetworkConnection conn)
	{
		//get host to select default player as experimenter

		int part = gameObject.GetComponent<SetupServer> ().participant;
		Debug.LogWarning (part);
		base.OnClientConnect (conn);
	

		//mass participant value as message??
		if (part < 0) {
			Debug.LogWarning ("host");
		
			OnServerAddPlayer (conn, (short)part);
		} else {
			Debug.LogWarning ("adding");
			ClientScene.AddPlayer (conn, (short)part);
				
		}

	}

	public override void OnServerError (NetworkConnection conn, int errorCode)
	{

		Debug.Log (errorCode);
	}

}
