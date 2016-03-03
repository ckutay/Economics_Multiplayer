using UnityEngine;
using System.Collections;
using System;
using UnityEngine.VR;
using SimpleJSON;
using UnityEngine.Networking;
using System.Collections.Generic;
using UnityEngine.UI;

public class ExperimentController : NetworkBehaviour
{
	//set in playernetworkcontroller

	public int participant_id;
	public int participant;

	//find in game
	public TextFileReader textFileReader;

	//move hand to enter coins - effector from Playernetworksetup
	public Transform lefthandEffector;
	public bool enter;
	public Transform button;

	GameObject[] gos;
	public float transPos;

	bool urlReturn;
	string url;

	public enum runState
	{
		start,
		wait,
		ask,
		answer,
		result,
		end}

	;


	public bool isHost;


	int returnInt;
	float returnFloat;
	//public PlayerNetworkSetup setupBox;
	public GameManager gameManager;
	public CoinManager coinManager;
	public GameObject box;
	public Text canvasText;
	string returnString;
	string message = "";
	//set in playernetworksetup to create network authority
	public ParticipantController participantController;
	public bool _isLocalPlayer;
	//Shared from host
	//[HideInInspector]
	[SyncVar]public int stage_number = 0;
	[SyncVar]public runState mode = runState.wait;
	public ExperimentController[] tokenBoxes;

	//fixme
	string result;
	// Use this for initialization
	void Start ()
	{
		//start at stage 0

		urlReturn = true;

		coinManager = (CoinManager)GetComponent<CoinManager> ();

		//reduce error in scene setup
		gameManager = GameObject.Find ("NetworkManager").GetComponent<GameManager> ();
		textFileReader = GameObject.Find ("NetworkManager").GetComponent<TextFileReader> ();
		button=transform.Find("Capsule");

	}

	void setupHost ()
	{


		if (isHost && tokenBoxes.Length == 0) {
			List <ExperimentController> list = new List<ExperimentController> ();
			foreach (GameObject box in gameManager.tokenBoxes) {

				list.Add (box.GetComponent<ExperimentController> ());
			}
			tokenBoxes = list.ToArray ();
		}
		//Debug.Log(gameManager);
		
	}

	void Update ()
	{
		
		
		if (urlReturn) {
			// url calls in rest of update do not work
			if (isHost && _isLocalPlayer) {
				//find next step
				updateMove ();
		
			}

			//not working in start unless set to active later  as no tokenbox- then not visible for collection tokenboxes FIXME
	
			//if (isHost)setupHost();
	

			if (_isLocalPlayer) {

				switch (mode) {
				case runState.start:
					coinManager.result = false;
					canvasText.text = "Wait for others to join you team";
				//show this before starts
			
					break;

				case runState.wait:
			//wait for nest step - need signal that all have done it from api - FIXME


					break;
				case runState.ask:
					
				//set up to change coins
					coinManager._isLocalPlayer = true;

				//assign coin box to player
					//nothing to braodcaset
				
					Vector3 target = button.transform.position;
					lefthandEffector.rotation=Quaternion.Euler(-18f,-15f,40f);
					lefthandEffector.transform.position = target;
					button.GetComponent<ClearButton> ()._isLocalPlayer = true;
				//when asked for coin number, send it	
					canvasText.text = message + " You have selected " + coinManager.currentCoins + " coins";
					if (coinManager.isFinished & _isLocalPlayer) {
						//cannot enter anymore
						coinManager.SetToClear ();
						coinManager.isFinished = false;
					
						//send in result to ZTree
						canvasText.text = "Wait for others to finish";
						url = textFileReader.IP_Address + "/experiments/results?experiment_id=" + textFileReader.experiment_id + "&stage_number=" + stage_number + "&participant_id=" + participant_id + "&round_id=1&name=CoinEffort&value=" + coinManager.currentCoins;
			
						callServer (url, "", "", mode);
						url = "";
						mode = runState.wait;
						

					}

					break;

				case runState.answer:
			//when get response from team input, display it - FIXME
					coinManager.result = true;
				//force go to next stage?

			
					//get result for previous stage for each participant
					url = textFileReader.IP_Address + "/experiments/results?experiment_id=" + textFileReader.experiment_id + "&stage_number=" + (stage_number - 1) + "&round_id=1&name=Result&participant_id=" + participant_id;
					string find = "Results";
					callServer (url, find, "", mode);


				
					break;
				case runState.end:
					canvasText.text = message + ((coinManager.maxCoins + 1 - coinManager.currentCoins).ToString ());
					message = null;
					isHost = false;
					gameManager.boxCount = -1;

		
			//fix me
		

					break;
				}

			}

			//works when authority given during assigning box to player
			if (isHost) {
				try {

					if (_isLocalPlayer) {
						Cmd_change_currentStage (stage_number, mode);
					}
				} catch {
				}
			}
		
		
		}
	}

	void updateMove ()
	{
		if (isHost & message != null)
			Cmd_broadcast (message);
		if (isHost & urlReturn) {
			string url;


			if (mode != runState.end) {
				
				url = textFileReader.IP_Address + "/experiments/stages?experiment_id=" + textFileReader.experiment_id + "&stage_number=" + stage_number + "&round_id=1";
				//Debug.Log (url);
				callServer (url, "type_stage", "stage_number", mode);

				//move on to next one
				if (returnInt > 0)
					stage_number = returnInt;
				
				if (returnString == "Request") {
					

					mode = runState.ask;

				} else if (returnString == "Response") {
					

					mode = runState.answer;


				} else if (returnString == "Wait") {

					mode = runState.wait;

				} else if (returnString == "End") {

					mode = runState.end;
				}

			}


		}
	}
	[Command]
	void Cmd_broadcast (string message)
	{
		//send message to all players - use synvar on script on Canvas??

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


	void callServer (string url, string find, string findInt, runState mode)
	{

		StartCoroutine (FetchWWW (url, find, findInt, mode));


	}

	[Command]
	void Cmd_change_currentStage (int _stage_number, runState _mode)
	{
		foreach (ExperimentController  expCont in tokenBoxes) {
			
			Rpc_change_currentStage (_stage_number, _mode);

		}

	}

	[ClientRpc] 
	void Rpc_change_currentStage (int _stage_number, runState _mode)
	{
		stage_number = _stage_number;

		mode = _mode;
	}

	IEnumerator FetchWWW (string url, string find, string findInt, runState _mode)
	{
		
		urlReturn = false;
		//Debug.Log (url);

		yield return StartCoroutine (WaitForSeconds (.1f));
		WWW www = new WWW (url);
		url = "";
		coinManager.result = false;
		yield return StartCoroutine (WaitForRequest (www));
		//go to next step when done
		urlReturn = true;
		// StringBuilder sb = new StringBuilder();
		string result = www.text;
		JSONNode node = JSON.Parse (result);
			
		if (node != null) {
			try {
				message = node ["message"];
			} catch {
				//message = null;
			}
		
			//Debug.Log (message);
		
			if (find.Length != 0) {

				returnString = node [find];
				returnFloat = 0;
				if (find == "Results") {
					//hack to get results into message- the time delay
					//mens you cannot pick this up in the state machine


					if (float.TryParse (returnString, out returnFloat)) {
						//get back result from group
						//when get result show it
					

						//FIXME
						if (returnFloat > 0 && _mode == runState.answer) {


							coinManager.currentCoins -= (int)returnFloat;
							//display results - no entered coins
							coinManager.result = true;
							canvasText.text = message + returnFloat.ToString ();


							//stop broadcast
							message = null;
						

							//show for 2 sec
							urlReturn = false;
		
							yield return StartCoroutine (WaitForSeconds (2f));
							urlReturn = true;
						}
							
						yield return true;
					
						//message for localplayer/tokenbox only
					}

					yield return true;
				} else if (Int32.TryParse (node [findInt], out returnInt)) {
					
					//Debug.Log(returnInt);
					yield return true;
				}

				yield return true;
			} else {

				if (Int32.TryParse (node [findInt], out returnInt))
					yield return true;
				
			}
		} else {
			//Debug.LogWarning ("No node on api read for " + find + " or " + findInt);
			//canvas.message = "Errer in stages for experiment: " + node;
			yield return true;

		}


	}

	IEnumerator setupWait (float num)
	{
		yield return WaitForSeconds (num);


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