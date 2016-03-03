using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.UI;
using System;

public class GameManager : NetworkBehaviour {
	 
	public GameObject[] playerPrefabs;
	static public GameManager singleton;
	public GameObject [] tokenBoxes;
[SyncVar] 
	public int boxCount=-2; 

	Text canvasText;
	Text canvasTextUp;
	public string message="";


	void Awake()
	{
		singleton = this;
	}
	// Use this for initialization
	void Start () 
	{
		//setup is is -1 when assigned expereimenter
		boxCount = -2;
		//get all the canvas texts - 1 per participant
	//	canvasText = GameObject.FindGameObjectWithTag("Canvas").transform.GetChild (1).gameObject.GetComponent<Text>();
	//	canvasTextUp = GameObject.FindGameObjectWithTag("Canvas").transform.GetChild (2).gameObject.GetComponent<Text>();
	}
	
	// Update is called once per frame
	void Update () {
	
	//	canvasText.text = message;
	//	canvasTextUp.text= message;
	}
	void OnGUI()
	{
		//GUILayout.Label(boxCount.ToString());
	}



	public void Spawn()
	{

		//	GameObject.FindObjectOfType<AddPlayer>().Cmd_Spawn_Prefab();
	}

	// called on the server
	public void ServerRespawn(AddPlayer addPlayer, int boxCount)
	{
		//zero is expereimenter prefab
		GameObject playerPrefab = playerPrefabs[boxCount];
		Destroy (addPlayer.gameObject);
		Vector3 pos = new Vector3 (0, -1.2f, 0);
		GameObject newPlayer = Instantiate<GameObject >( playerPrefab);

		newPlayer.transform.position=pos;
		bool added = NetworkServer.ReplacePlayerForConnection(addPlayer.connectionToClient, newPlayer,0);


	}
	//called form coin manager as has no authority
	[Command]
	public void Cmd_Update_Coins(CoinManager CM, int _currentCoins){

		CM.currentCoins = _currentCoins;
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
	public void Cmd_change_currentStage (ExperimentController exc_cont, int _stage_number, ExperimentController.runState _mode)
	{

		foreach (ExperimentController  exp_conts in exc_cont.tokenBoxes) {

			exp_conts.stage_number = _stage_number;

			exp_conts.mode = _mode;

		}

	}

}
