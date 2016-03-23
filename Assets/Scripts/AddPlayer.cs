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
	CommonNetwork commonNetwork;
	[SerializeField] Camera FPCharacterCam;
	[SerializeField] AudioListener audioListener;

	public NetworkConnection conn;
	//	NetworkIdentity defaultLocalPlayer;


	void Start ()
	{
		if (isLocalPlayer) {
			gameManager = GameObject.Find ("NetworkManager").GetComponent<GameManager> ();
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

					//prefabs stored on GameManager but also registered on Network Manager - add 

					Cmd_Spawn_Prefab (gameManager.boxCount);
			
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

		//Debug.Log(boxCount);

		GameManager.singleton.ServerRespawn(this, boxCount);

	

	}



}
