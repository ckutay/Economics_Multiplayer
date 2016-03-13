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
	[SyncVar]public bool ikActive;

	public float transPos;

	bool urlReturn;
	string url;
	int resultCoins=-1;

	int effortCoins;
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
	public bool waiting;
	string resultMessage="";
	int returnInt;
	float returnFloat;
	//public PlayerNetworkSetup setupBox;
	public GameManager gameManager;
	public CoinManager coinManager;

	public int boxCount;
	public Text canvasText;
	string returnString;
	[SyncVar] string message = "";
	//set in playernetworksetup to create network authority
	public ParticipantController participantController;
	public bool _isLocalPlayer;
	//Shared from host
	//[HideInInspector]
	[SyncVar] public int stage_number = 0;
	[SyncVar]public runState mode = runState.wait;
	public ExperimentController[] tokenBoxes;
	IKBody ikBody=null ;
	//fixme
	string result;
	bool start;
	// Use this for initialization
	void Start ()
	{
		//start at stage 0

		urlReturn = true;

		coinManager = (CoinManager)GetComponent<CoinManager> ();

		//reduce error in scene setup
		gameManager = GameObject.Find ("NetworkManager").GetComponent<GameManager> ();
		boxCount = gameManager.boxCount;
		textFileReader = GameObject.Find ("NetworkManager").GetComponent<TextFileReader> ();
		button=transform.Find("Capsule");
	
		start = true;

	}

	void setupHost ()
	{
		

			

		if (isHost && tokenBoxes.Length == 0) {
			List <ExperimentController> list = new List<ExperimentController> ();
			foreach (GameObject box in gameManager.tokenBoxes) {

				list.Add (box.GetComponent<ExperimentController> ());
			}
			tokenBoxes = list.ToArray ();

		//Debug.Log(gameManager);
		}
		start = false;
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
			if(start)	setupHost();
	

			if (_isLocalPlayer) {
				//set from the server syncvar

				if (ikBody!=null & coinManager.player!=null)	ikBody.ikActive = ikActive;
				else ikBody = coinManager.player.gameObject.GetComponent<IKBody> ();
			
				switch (mode) {
				case runState.start:

					coinManager.result = false;
					canvasText.text = "Wait for others to join you";
				//show this before starts

					break;

				case runState.wait:
			//wait for nest step - need signal that all have done it from api - FIXME
					if (coinManager.isFinished) {
						waiting = true;
						coinManager.isFinished = false;
					}

					break;
				case runState.ask:
					//enable ik and sync
					ikActive = true;
					coinManager.player.Cmd_ikActive (boxCount, true);

				//set up to change coins
					coinManager._isLocalPlayer = true;

				//assign coin box to player
					//nothing to braodcaset
				
					Vector3 target = button.transform.position;
					lefthandEffector.rotation =coinManager.gameObject.transform.parent.rotation* Quaternion.Euler (-18f, -15f, 40f);
					//need to add rotation of chair

					lefthandEffector.transform.position = target;
					button.GetComponent<ClearButton> ()._isLocalPlayer = true;
				//when asked for coin number, send it	
					canvasText.text = message + " You have selected " + coinManager.currentCoins + " coins";
					if (coinManager.isFinished & _isLocalPlayer) {
						//cannot enter anymore
						effortCoins = coinManager.currentCoins;
						button.GetComponent<ClearButton> ()._isLocalPlayer = false;
					
						//send in result to ZTree
						canvasText.text = "Wait for others to finish";
						url = textFileReader.IP_Address + "/experiments/results?experiment_id=" + textFileReader.experiment_id + "&stage_number=" + stage_number + "&participant_id=" + participant_id + "&round_id=1&name=CoinEffort&value=" + coinManager.currentCoins;
						StartCoroutine (FetchStage (url, "","", mode));
					

						url = "";
						mode = runState.wait;
						

					}

					break;

				case runState.answer:
					//get result for previous stage for each participant
					url = textFileReader.IP_Address + "/experiments/results?experiment_id=" + textFileReader.experiment_id + "&stage_number=" + (stage_number - 1) + "&round_id=1&name=Result&participant_id=" + participant_id;

					StartCoroutine (FetchStage (url, "Results", "", mode));
					url = "";

				
			
				

				
					ikActive = false;
					coinManager.player.Cmd_ikActive (boxCount, false);
					//mode = runState.wait;

					break;
				case runState.end:
					//get last message
					//wait before get result and update message
					Debug.LogWarning("got to end");
					Debug.LogWarning(resultCoins);
				
					if (resultCoins >= 0 & !resultMessage.Equals (""))
						canvasText.text =message + resultCoins.ToString ();
					
					resultCoins = -1;

				
					//no more mesages sent
				
					participantController.mode = ParticipantController.modes.stand;
					//gameManager.boxCount = -1;

		
			//fix me

				

					break;
				}
				//Debug.LogWarning (message);
			}

			//works when authority given during assigning box to player
		
		
		
		}

	}

	//IEnumerator resultMessage(string _message){
		//make sure see return message before final result
	//	yield return StartCoroutine (WaitForSeconds (.1f));
	//	canvasText.text = _message;
		//reset in class
	//	message = "";

	//}
	void updateMove ()
	{
		//runs on host localplayer only
		//do not broadcast unless change as some are in waiting
		int _stage_number = stage_number;
	
		string _message = message;
	
		if (urlReturn ) {



			if (mode != runState.end) {
				
				url = textFileReader.IP_Address + "/experiments/stages?experiment_id=" + textFileReader.experiment_id + "&stage_number=" + stage_number + "&round_id=1";
				//Debug.Log (url);

				StartCoroutine (FetchStage (url, "type_stage", "stage_number", mode));
				url = "";

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


				try {


					if (_stage_number!=stage_number){
						coinManager.player.Cmd_change_currentStage ( stage_number, mode);
					Debug.LogWarning(stage_number);
					Debug.LogWarning(mode);
					}
				} catch {
				}


		}

		if (message != null & message != _message) {

			coinManager.player.Cmd_broadcast (message);

		}
	}




	IEnumerator FetchStage (string _url, string find, string findInt, runState _mode)
	{
		if (_isLocalPlayer) {
			urlReturn = false;
			//Debug.LogWarning (url);

			yield return StartCoroutine (WaitForSeconds (.1f));
			WWW www = new WWW (_url);
	
			yield return StartCoroutine (WaitForRequest (www));
			//go to next step when done
			urlReturn = true;
			// StringBuilder sb = new StringBuilder();
			string result = www.text;
			JSONNode node = JSON.Parse (result);
			
			if (node != null) {
				try {
					//get stage message
					message = node ["message"];
					if (node["type_stage"]=="End"){
						resultMessage=message;
					}
				} catch {
					//message = null;
				}
		
				//Debug.Log (message);
		
				if (find.Length != 0) {

					returnString = node [find];
					returnFloat = 0;
					Debug.LogWarning (node);
					if (find == "Results") {
						//hack to get results into message- the time delay
						//mens you cannot pick this up in the state machine


						if (float.TryParse (returnString, out returnFloat)) {
							//get back result from group
							//when get result show it`
					

							//FIXME
							if (returnFloat > 0 && !message.Equals("")) {
								resultCoins =	coinManager.maxCoins + 1 - effortCoins + (int)returnFloat;
								//set to display result only
								coinManager.result = true;
						
								coinManager.currentCoins -= (int)returnFloat;

					
						
						
								//display results - no entered coins show anymore - fixit

								canvasText.text = message + returnFloat.ToString ();
								Debug.LogWarning (message + returnFloat.ToString ());
								//stop broadcast
								message = "";

						
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