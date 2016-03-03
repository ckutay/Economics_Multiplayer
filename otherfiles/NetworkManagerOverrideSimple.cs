using UnityEngine;
using System.Collections;
using System;
using UnityEngine.Networking;
using SimpleJSON;
using System.Linq;


public class NetworkManagerOverrideSimple: NetworkManager
{

	public GameObject[] playerPrefabs;



	public override void OnStartServer()
	{
		

	
		base.OnStartServer();
	}

	public override void OnClientConnect(NetworkConnection conn)
	{
		int prefabIndex = gameObject.GetComponent<SetupServer> ().participant;
		if (prefabIndex>=0)playerPrefab = playerPrefabs[prefabIndex];
		client.RegisterHandler(MsgTypes.PlayerPrefab, OnRequestPrefab);
		Debug.LogWarning(playerPrefab.name + " registered!");
		base.OnClientConnect(conn);

	}



	public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId)
	{
		int prefabIndex = gameObject.GetComponent<SetupServer> ().participant;
		if(prefabIndex < 0)
		{
			// For default type use default implementation
			// (spawnes prefab configured in inspector)
			base.OnServerAddPlayer(conn, playerControllerId);
		}
		else  // (Used 1 as id of my prefab type)
		{
			NetworkServer.AddPlayerForConnection(conn,
				Instantiate(playerPrefabs[prefabIndex]), playerControllerId);
		}

	}
}
