using UnityEngine;
using System.Collections;
using System;
using UnityEngine.Networking;
using SimpleJSON;
using UnityEngine.UI;

public class AddPlayer : NetworkBehaviour
{
	//not auot player add for different prefabs
	public TextFileReader textFileReader;
	GameManager gameManager;
	NetworkManager networkManager;
	CommonNetwork commonNetwork;
	SetupServer setupServer;
	[SerializeField] Camera FPCharacterCam;
	[SerializeField] AudioListener audioListener;

	public NetworkConnection conn;
	//	NetworkIdentity defaultLocalPlayer;


	void Start ()
	{
		if (isLocalPlayer) {
			gameManager = GameObject.Find ("NetworkManager").GetComponent<GameManager> ();
			networkManager = GameObject.Find ("NetworkManager").GetComponent<NetworkManager> ();
			setupServer = GameObject.Find ("NetworkManager").GetComponent<SetupServer> ();
			commonNetwork = GameObject.Find ("NetworkManager").GetComponent<CommonNetwork> ();
			//Debug.LogWarning(NetworkTransport.IsStarted);
			if (isLocalPlayer) {
				

				//have not updated count yet
				if (gameManager.boxCount >= commonNetwork.max_participants) {
					FPCharacterCam.gameObject.SetActive (true);
					FPCharacterCam.enabled = true;
					audioListener.enabled = true;
					//add wrning to default player
					Canvas canvasgo = gameObject.GetComponentInChildren <Canvas> (true);
					if (canvasgo) {
						canvasgo.gameObject.SetActive (true);
						canvasgo.enabled = true;
						Text canvasText = canvasgo.transform.Find ("Text").gameObject.GetComponent<Text> ();
						canvasText.text = "You cannot join this game as the server is full";
					}
			
				} else if (gameManager.boxCount > -2) {
					//if has received box count

					//prefabs stored on GameManager but also registered on Newtwork Manager

					Cmd_Spawn_Prefab (1 + gameManager.boxCount);
			
					//is destroyed after this
					//FIXME - needed?
					//PlayerNetworkSetup playerNetworkSetup = GetComponent<PlayerNetworkSetup> ();
					//playerNetworkSetup.Rpc_set_prefab ();
				}

			}
		}
	}

	[Command]
	public void Cmd_Spawn_Prefab (int boxCount)
	{
		//boxCount plus one to include experimenter
		//place at 1.2 height
		Debug.Log(boxCount);

		GameManager.singleton.ServerRespawn(this, boxCount);

	/*
		playerPrefab = gameManager.playerPrefabs [boxCount];
	
		GameObject newPlayer = Instantiate<GameObject> (playerPrefab);
		Destroy(gameObject);

		bool added = NetworkServer.ReplacePlayerForConnection (this.connectionToClient, newPlayer, 0);

		Debug.LogWarning (added);
		//setup up new player
	
		PlayerNetworkSetup playerNetworkSetup = newPlayer.GetComponent<PlayerNetworkSetup> ();
		playerNetworkSetup.Rpc_set_prefab ();
		//setup instantiated prefab

*/

	}



}
