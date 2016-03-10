using UnityEngine;
using System.Collections;
using System;
using UnityEngine.Networking;
using UnityEngine.UI;


using SimpleJSON;

public class PlayerNetworkSetupError : NetworkBehaviour
{

	[SerializeField] Camera FPCharacterCam;
	[SerializeField] AudioListener audioListener;
	//GameObject networkManageGO;
	GameManager gameManager;
	ParticipantController participantController;


	public Transform spawnPoint;
	string returnParameter;
	public TextFileReader textFileReader;
	SetupServer setupServer;
	ExperimentController expController;
	public int participant;
	public int participant_id;
	GameObject tokenBox;
	//set on prefab
	public bool isHost = false;

	public Transform hips;
	Text canvasText;
	Canvas canvasgo;


	[SyncVar] GameObject playerPrefab;
 GameObject[] playerPrefabs;


	// Use this for initialization
	void Start ()
	{
		
		//have paricipant number from server  in setupServer
		if (isLocalPlayer) {

		
			//StartCoroutine()
			GameObject mainCamera = GameObject.Find ("Main Camera");
			if (mainCamera != null)
				mainCamera.SetActive (false);
			FPCharacterCam.enabled = true;
			audioListener.enabled = true;
		
			gameManager = GameObject.Find ("NetworkManager").GetComponent<GameManager> ();


			textFileReader = GameObject.Find ("NetworkManager").GetComponent<TextFileReader> ();
			setupServer = GameObject.Find ("NetworkManager").GetComponent<SetupServer> ();
			//find local canvas inactive
			canvasgo = gameObject.GetComponentInChildren <Canvas> (true);
			if (canvasgo) {
				canvasgo.gameObject.SetActive (true);
				canvasText = canvasgo.transform.Find ("Text").gameObject.GetComponent<Text> ();
			}
			participantController = gameObject.GetComponent<ParticipantController> ();
			string find="participant";
			while (textFileReader.readHost==null){
				isHost=false;
			}
			int part=1;
			if(textFileReader.isHost && textFileReader.readHost!=null){
				isHost=true;
				part=0;
			}
			string url = textFileReader.IP_Address + "/experiments/participant?participant="+part+"&experiment_id=" + textFileReader.experiment_id;
			//Debug.Log(textFileReader.IP_Address+"/experiments/participant?participant=1&experiment_id="+textFileReader.experiment_id);
			StartCoroutine (FetchParticipant (url));

			Debug.LogWarning(gameManager.boxCount);
			Debug.LogWarning(participant);
		
				CmdSpawn();


			callBack();
		}


	}

	void Update ()
	{
		
	}



	void callBack ()
	{
		//when you have the participant number setup all the variables on participatncontroller to enable them to locate objects and select

	
		//Debug.LogWarning ("Assigned to box " + gameManager.boxCount);
		//setup simplemousellok first

		try {
			tokenBox = gameManager.tokenBoxes [gameManager.boxCount];
		} catch (Exception e) {

			//adding experimenter
		
			if (gameManager.boxCount > 0) {
				canvasText.text = "You cannot enter this game. It is full or server is not started";
			
				Debug.Log (e);
				return;
			}
		}
		//participant=setupServer.participant;

		//participant_id=setupServer.participant_is;
		Debug.LogWarning("Setting up experiment");
		//random -FIXME
		if (spawnPoint)
			transform.position = spawnPoint.position;
		


		if (participantController) {
			participantController.participant = participant;
			participantController = GetComponent<ParticipantController> ();
			participantController.box = tokenBox;
			participantController.canvasText = canvasText;
			participantController.coinManager = tokenBox.GetComponent<CoinManager> ();
			//clear box
			participantController.coinManager.SetToClear ();

			expController = tokenBox.GetComponent<ExperimentController> ();
			expController.participantController = participantController;
			expController.canvasText = canvasText;
			expController.button = tokenBox.transform.Find ("Capsule");
			expController.lefthandEffector = transform.Find ("Effectors").Find ("LeftHand Effector");

			//expController.setupBox = transform.GetComponent<PlayerNetworkSetup> ();
			expController.participant_id = participant_id;
			expController.participant = participant;

			//host assigned to game box, not player
			//expController.isHost = isHost;
			expController._isLocalPlayer = true;
			//first two effectors on chairbox
			participantController.walkTarget = tokenBox.transform.parent.Find ("WalkTarget").gameObject;
			participantController.sitTarget = tokenBox.transform.parent.Find ("SitTarget").gameObject;
	
			//spawnpoint set on chair/box
	
			spawnPoint = tokenBox.transform.parent.Find ("SpawnPoint");

			transform.position = spawnPoint.position;
			//rotation not happening?
			transform.rotation = spawnPoint.rotation;
		
		//	tokenBox.GetComponent<CoinManager> ().isLocalPlayer = true;
		}
	
		//enable mouse control if non vr
		SimpleMouseLook simpleMouseLook = gameObject.GetComponentInChildren<SimpleMouseLook> ();

		if (simpleMouseLook != null) {
			simpleMouseLook.enabled = true;
			simpleMouseLook._isLocalPlayer = true;
			///fixme - should come from in sml?

			if (hips == null) {
				hips = transform.Find ("mixamorig:Hips");
				hips = transform.Find ("Armature/mixamorig:Hips");
			}
			Vector3 posspawn = spawnPoint.position;
			posspawn.y = hips.position.y;

			simpleMouseLook.dist = (simpleMouseLook.transform.position - posspawn);
		} else
			Debug.LogWarning ("No SimpleMouseLook");
		

	}
	[Command]
	void CmdSpawn()
	{
		Debug.LogWarning("adding player");
		playerPrefab=playerPrefabs[gameManager.boxCount];

		playerPrefab = (GameObject)Instantiate(playerPrefab, transform.position + new Vector3(0,1,0), Quaternion.identity);
		NetworkServer.SpawnWithClientAuthority(playerPrefab, base.connectionToClient);

	
	//	NetworkServer.AddPlayerForConnection(conn, player, 0);

		Debug.LogWarning("adding player");
	}



	//generic function to  FIX if
	IEnumerator FetchParticipant (string url)
	{
		Debug.Log (url);
		yield return StartCoroutine (WaitForSeconds (.1f));
		//simple function for participant call only
		WWW www = new WWW (url);

		yield return StartCoroutine (WaitForRequest (www));
		// StringBuilder sb = new StringBuilder();
		string result = www.text;
		//Debug.Log(result);
		JSONNode node = JSON.Parse (result);
		//Debug.Log (node);
		if (node != null) {
			if ((node ["participant"] != null) & (Convert.ToInt32 (Math.Ceiling (node ["participant"].AsFloat)) >= 0)) {
				participant = Convert.ToInt32 (Math.Ceiling (node ["participant"].AsFloat)) - 1;

				participant_id = Convert.ToInt32 (Math.Ceiling (node ["participant_id"].AsFloat));
				
				gameManager.boxCount = participant;
				//setup as host

			} else {

				canvasText.text = "Please join another experiment this one is full or server not started";
				yield break;
			}
			//only called on localplayer??
			callBack ();
		} else {

			Debug.Log ("No Node Network Setup for new participant");
			canvasText.text = "Please join another experiment this one is full, or server not started";
			yield break;
		}


	}

	public IEnumerator WaitForRequest (WWW www)
	{
		yield return www;

	}

	IEnumerator WaitForSeconds (float num)
	{

		yield return new WaitForSeconds (num);

	}


}