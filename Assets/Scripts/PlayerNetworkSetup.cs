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
	int boxCount;
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
				boxCount=gameManager.boxCount;
				///boxCount=1;
				tokenBox= gameManager.tokenBoxes [boxCount];

			} catch (Exception e) {
				if (gameManager.boxCount > 0) {
					if (canvasgo)
						canvasText.text = "You cannot enter this game. It is full or server is not started";
					Debug.Log (e);
					return;
				}
			}

			//this is a player so set up experiment controller
			if (canvasgo) {
				
				//if no canvas still on fpcontroller for experimenter
				participantController = gameObject.GetComponent<ParticipantController> ();

				participantController.participant = participant;
				participantController = GetComponent<ParticipantController> ();
				participantController.box = tokenBox;
				participantController.canvasText = canvasText;
				participantController.coinManager = tokenBox.GetComponent<CoinManager> ();
				participantController.coinManager.boxCount = boxCount;
				expController = tokenBox.GetComponent<ExperimentController> ();
				expController.participantController = participantController;
				expController.canvasText = canvasText;
				expController.boxCount = boxCount;
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


				expController._isLocalPlayer = true;
				tokenBox.GetComponent<CoinManager> ().player = this;
				tokenBox.GetComponent<CoinManager> ()._isLocalPlayer = true;
				tokenBox.GetComponent<CoinManager> ().SetToClear ();
	


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

	//Called from playernetworksetup for assigning cotnrol of token box
	public void Cmd_Get_Authority (int _boxCount)
	{
		GameObject box = gameManager.tokenBoxes [_boxCount].gameObject;
	
		NetworkIdentity nwI = tokenBox.AddComponent<NetworkIdentity> ();
		nwI.localPlayerAuthority = true;
		NetworkManager networkManager= GameObject.Find ("NetworkManager").GetComponent<NetworkManager> ();
	
		GameObject newBox = Instantiate (box);
	
		NetworkServer.SpawnWithClientAuthority ( newBox, connectionToClient);
		 gameManager.tokenBoxes [boxCount]=newBox;
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
	//called form coin manager as has no authority
	[Command]
	public void Cmd_Update_Coins(int _boxCount, int _currentCoins){

		gameManager.tokenBoxes[_boxCount].GetComponent<CoinManager>().currentCoins = _currentCoins;
	}
	//caleld form experiment controller to send update messages from ZTree
	[Command]
	public void Cmd_broadcast (string message)
	{
		//send message to all players - use synvar on script on Canvas??
		GameObject[] gos;
		gos = GameObject.FindGameObjectsWithTag ("Player");
		//update as player enters
		foreach (GameObject go in gos) {

			try {
				Transform tran = go.transform.Find ("FPCharacterCam").Find ("Canvas");

				tran = tran.Find ("Text");

				if (tran != null)
					tran.gameObject.GetComponent<Text> ().text = message;

			} catch (Exception e) {

				Debug.LogWarning (e);
			}

		}


	}
	//called form expereiment controlle r to update stage from Ztree
	[Command]
	public void Cmd_change_currentStage ( int _stage_number, ExperimentController.runState _mode)
	{

		foreach (GameObject  exp_conts in gameManager.tokenBoxes) {
			ExperimentController exp_cont = exp_conts.GetComponent<ExperimentController> ();
			exp_cont.stage_number = _stage_number;

			exp_cont.mode = _mode;

		}

	}
	[Command]
	public void Cmd_ikActive(int _boxCount, bool _ikActive){

		gameManager.tokenBoxes[_boxCount].GetComponent<ExperimentController>().ikActive = _ikActive;

	}

}