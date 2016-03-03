using UnityEngine;
using System.Collections;
using System;
using UnityEngine.Networking;
using UnityEngine.UI;


using SimpleJSON;

public class PlayerNetworkSetup : NetworkBehaviour
{

	[SerializeField] Camera FPCharacterCam;
	[SerializeField] AudioListener audioListener;

	GameManager gameManager;
	ParticipantController participantController;

	public Transform spawnPoint;
	string returnParameter;
	public TextFileReader textFileReader;
	ExperimentController expController;
	public int participant;
	public int participant_id;
	GameObject tokenBox;
	bool isHost;
	Text canvasText;
	Canvas canvasgo;

	SetupServer setupServer;

	// Use this for initialization
	void Start ()
	{
		//have paricipant number from server  in setupServer
		gameManager = GameObject.Find ("NetworkManager").GetComponent<GameManager> ();

		setupServer = GameObject.Find ("NetworkManager").GetComponent<SetupServer> ();
		textFileReader = GameObject.Find ("NetworkManager").GetComponent<TextFileReader> ();
		//has been geenrated
		if (gameManager.boxCount>-2){

			setup();
		}
	}

	public void setup ()
	{


	//	Debug.LogWarning(isLocalPlayer);
		if (isLocalPlayer){

		GameObject mainCamera = GameObject.Find ("Main Camera");
		if (mainCamera != null)
			mainCamera.SetActive (false);
			FPCharacterCam.gameObject.SetActive ( true);
		FPCharacterCam.enabled = true;
		audioListener.enabled = true;
		//setup new characgter as player
		
		

			while (textFileReader.readHost == null) {
				isHost = false;
				Debug.LogWarning ("Getting host");
			}
			participant = setupServer.participant;

			participant_id = setupServer.participant_id;

			//random -FIXME
			if (spawnPoint)
				transform.position = spawnPoint.position;
			
			callBack ();
		}

	}



	void callBack ()
	{
		//when you have the participant number setup all the variables on participatncontroller to enable them to locate objects and select
		if (isLocalPlayer) {
	
			Debug.Log ("Assigned to box " + gameManager.boxCount);
	
			//find local canvas inactive
			canvasgo = gameObject.GetComponentInChildren <Canvas> (true);
			if (canvasgo) {
				canvasgo.gameObject.SetActive (true);
				canvasText = canvasgo.transform.Find ("Text").gameObject.GetComponent<Text> ();
			}
			try {
				tokenBox = gameManager.tokenBoxes [gameManager.boxCount];
			} catch (Exception e) {
				if (gameManager.boxCount > 0) {
					if (canvasgo)
						canvasText.text = "You cannot enter this game. It is full or server is not started";
					Debug.Log (e);
					return;
				}
			}
			if (canvasgo) {
				
				//if no canvas still on fpcontroller for experimenter
				participantController = gameObject.GetComponent<ParticipantController> ();

				participantController.participant = participant;
				participantController = GetComponent<ParticipantController> ();
				participantController.box = tokenBox;
				participantController.canvasText = canvasText;
				participantController.coinManager = tokenBox.GetComponent<CoinManager> ();
		
				expController = tokenBox.GetComponent<ExperimentController> ();
				expController.participantController = participantController;
				expController.canvasText = canvasText;

				//setup authority over coinbox
				//expController.player = gameObject;

				expController.button = tokenBox.transform.Find ("Capsule");
				expController.lefthandEffector = transform.Find ("Effectors").Find ("LeftHand Effector");


				//expController.setupBox = transform.GetComponent<PlayerNetworkSetup> ();
				expController.participant_id = participant_id;
				expController.participant = participant;


			
				//first two effectors on chairbox
				participantController.walkTarget = tokenBox.transform.parent.Find ("WalkTarget").gameObject;
				participantController.sitTarget = tokenBox.transform.parent.Find ("SitTarget").gameObject;
				participantController.rearTarget = tokenBox.transform.parent.Find ("RearTarget").gameObject;
				spawnPoint=tokenBox.transform.parent.Find("SpawnPoint");

				Vector3 spawnPointV=spawnPoint.position;
				//spawnpoint set on chair/box

			
				spawnPointV.y=transform.position.y;
				transform.position = spawnPointV;
			
			
				transform.rotation = spawnPoint.rotation;

					//Cmd_Get_Authority ();

				expController._isLocalPlayer = true;
				tokenBox.GetComponent<CoinManager> ()._isLocalPlayer = true;
	


			}
		}
		SimpleMouseLook simpleMouseLook = gameObject.GetComponentInChildren<SimpleMouseLook> ();
		if (simpleMouseLook != null) {
			simpleMouseLook.enabled = true;
			simpleMouseLook._isLocalPlayer = true;
			///fixme - should come from in sml?
			Transform hips = transform.Find ("mixamorig:Hips");
			if (hips == null)
				hips = transform.Find ("Armature/mixamorig:Hips");
			if(spawnPoint!=null){
			Vector3 posspawn = spawnPoint.position;
			posspawn.y = hips.position.y;

			simpleMouseLook.dist = (simpleMouseLook.transform.position - posspawn);
			}
		} else
			Debug.LogWarning ("No Mouse Look Found");
		//Debug.LogWarning ("Mouse Look Done");
	}

	[Command]
	void Cmd_Get_Authority ()
	{

		NetworkIdentity nwI = tokenBox.gameObject.AddComponent<NetworkIdentity> ();
		nwI.AssignClientAuthority (connectionToServer);
		//NetworkServer.SpawnWithClientAuthority (this.expController.gameObject, this.connectionToClient);

	}
	//from AddPlayer when instatiate character
	[ClientRpc]
	public void	Rpc_set_prefab ()
	{
		if (isLocalPlayer){
		//instnatied prefab
		setup ();
		}
	}


}