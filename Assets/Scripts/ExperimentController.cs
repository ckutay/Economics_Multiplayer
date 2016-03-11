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
	//watiing for url output
	bool urlReturn;
	//first run of answer etc
	bool update = true;
	string url;
	int resultCoins = -1;

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
	//returns from url
	string returnString;
	int returnInt;
	float returnFloat;
	//public PlayerNetworkSetup setupBox;
	public GameManager gameManager;
	public CoinManager coinManager;

	public int boxCount;
	public Text canvasText;

	[SyncVar] public string message = "";
	[SyncVar] public string resultMessage = "";
	string oldMessage;

	//set in playernetworksetup to create network authority
	public ParticipantController participantController;
	public bool _isLocalPlayer;
	//Shared from host
	//[HideInInspector]

	[SyncVar] public int stage_number = 0;
	[SyncVar]public runState mode = runState.wait;
	//stage number when result recorded
	int resultStage=0;
	public ExperimentController[] tokenBoxes;
	IKBody ikBody = null;
	//fixme
	string result;
	bool start;
	// Use this for initialization
	void Start ()
	{
		//start at stage 0

		urlReturn = true;
		update = true;

		coinManager = (CoinManager)GetComponent<CoinManager> ();

		//reduce error in scene setup
		gameManager = GameObject.Find ("NetworkManager").GetComponent<GameManager> ();
		boxCount = gameManager.boxCount;
		textFileReader = GameObject.Find ("NetworkManager").GetComponent<TextFileReader> ();
		button = transform.Find ("Capsule");
	
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
		//wait for updates from api

		if (urlReturn) {
			// url calls in rest of update do not work
			if (isHost && _isLocalPlayer) {
				//find next step and message
				updateMove ();
				//message etc is send on Command on server and to all players
				//the syncvar to localplayer
			}


			//not working in start unless set to active later  as no tokenbox- then not visible for collection tokenboxes FIXME
			if (start)
				setupHost ();
	

			if (_isLocalPlayer) {
				//set from the server syncvar

				if (ikBody != null & coinManager.player != null)
					ikBody.ikActive = ikActive;
				else
					//get component if null
					ikBody = coinManager.player.gameObject.GetComponent<IKBody> ();
			
				switch (mode) {
				case runState.start:
					//show this at start

					coinManager.result = false;
					canvasText.text = "Wait for others to join you";
			
					break;

				case runState.wait:
					//wait for next step - need signal that all have done it from api - FIXME?

					if (coinManager.isFinished) {
						waiting = true;
						coinManager.isFinished = false;
					}
					//don't change message so not rewritten
					break;
				case runState.ask:
					//enable ik and sync
					//set if playercontroller is sitting
					//ikActive = true;
					//send to server then SyncVar
					coinManager.player.Cmd_ikActive (boxCount, true);

					//set up to change coins
					coinManager._isLocalPlayer = true;
					if (ikActive) {
						Vector3 target = button.transform.position + new Vector3 (0, .5f, 0);
						//compendate for rotation
						lefthandEffector.rotation = coinManager.gameObject.transform.parent.rotation * Quaternion.Euler (-18f, -15f, 40f);
						//need to add rotation of chair

						lefthandEffector.transform.position = target;
						button.GetComponent<ClearButton> ()._isLocalPlayer = true;
						//send coin number	
						canvasText.text = message + " You have selected " + coinManager.currentCoins + " coins";
						if (coinManager.isFinished & _isLocalPlayer) {
							//cannot enter anymore
							effortCoins = coinManager.currentCoins;
							button.GetComponent<ClearButton> ()._isLocalPlayer = false;
							resultStage = stage_number;
							//send in result to ZTree
							canvasText.text = "Wait for others to finish";
							url = textFileReader.IP_Address + "/experiments/results?experiment_id=" + textFileReader.experiment_id + "&stage_number=" + stage_number + "&participant_id=" + participant_id + "&round_id=1&name=CoinEffort&value=" + coinManager.currentCoins;
							StartCoroutine (FetchStage (url, "", "", mode));
							//not playing anymore
							ikActive = false;
							coinManager.player.Cmd_ikActive (boxCount, false);
							url = "";
							mode = runState.wait;
							//cannot use button
							button.GetComponent<ClearButton> ().SetToClear (false);
						}
					}

					break;

				case runState.answer:
					//call for result once per participant
					if (update) {
						//get result for previous stage for each participant
						url = textFileReader.IP_Address + "/experiments/results?experiment_id=" + textFileReader.experiment_id + "&stage_number=" + (resultStage) + "&round_id=1&name=Result&participant_id=" + participant_id;
						//gets result and displays to local canvasText.text
						StartCoroutine (FetchStage (url, "Results", "", mode));
						//has wait at end to stop going to end screen too quick
						url = "";
						update = false;
						//FIXME should go to wait, but get message change
						//mode = runState.wait;
					}

					break;
				case runState.end:
					//broadcast the local change

					//resultCoins = -1;
					if ( !resultMessage.Equals ("")) {
						canvasText.text = resultMessage+ resultCoins.ToString() ;
						//stop overwrite
						message = "";
						resultMessage = "";
					}
				
					//no more mesages sent
				
					participantController.mode = ParticipantController.modes.stand;
					//gameManager.boxCount = -1;


					break;
				}
				//Debug.LogWarning (message);
			}

			//works when syncvar gives new braodcast message to player
		
			if (_isLocalPlayer & oldMessage != message & !message.Equals ("")) {
				showMessage (message);
				oldMessage = message;
			}
		
		}

	}

	//IEnumerator resultMessage(string _message){
	//make sure see return message before final result
	//	yield return StartCoroutine (WaitForSeconds (.1f));
	//	canvasText.text = _message;
	//reset in class
	//	message = "";

	//}


	//deals with group messages, not individual ones
	//they are done under modes
	void showMessage (string _message)
	{
		//update if not blank
		if (!_message.Equals(""))canvasText.text = _message;

	}

	void updateMove ()
	{
		//runs on host localplayer only
		//do not broadcast unless change as some are in waiting
		int _stage_number = stage_number;
	
		string _message = message;
	
		if (urlReturn) {



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


				//if (_stage_number!=stage_number){
				coinManager.player.Cmd_change_currentStage (stage_number, mode);
				
				//}
			} catch {
			}


		}

		if ( message != _message) {
			//send update of result Message too for when it comes in
			//empy message not displayed
			coinManager.player.Cmd_broadcast (message, resultMessage);

		}
	}




	IEnumerator FetchStage (string _url, string find, string findInt, runState _mode)
	{
		if (_isLocalPlayer) {
			urlReturn = false;
			//Debug.LogWarning (url);

			yield return StartCoroutine (WaitForSeconds (.5f));
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
					if (node ["type_stage"] == "End") {
						resultMessage = message;
					}
				} catch {
					//message = null;
					//yield return false;
				}
		
				//Debug.Log (message);
		
				if (find.Length != 0) {

					returnString = node [find];
					returnFloat = 0;
					//	Debug.LogWarning (node);
					if (find == "Results") {
						//hack to get results into message- the time delay
						//mens you cannot pick this up in the state machine


						if (float.TryParse (returnString, out returnFloat)) {
							//get back result from group
							//when get result show it`
					

							//FIXME
							if (returnFloat > 0 && !message.Equals ("")) {
								//set to display result only - return then total
                                //store for a while then display total
								resultCoins =	coinManager.maxCoins + 1 - effortCoins + (int)returnFloat;
								coinManager.result = true;
						
								coinManager.currentCoins -= (int)returnFloat;

					
						
						
								//display results - no entered coins show anymore - fixit

								canvasText.text = message + returnFloat.ToString ();
								//	Debug.LogWarning (message + returnFloat.ToString ());
								//stop broadcast
								message = "";
								//delay display of final message

								yield return StartCoroutine (WaitForSeconds (1f));
							//broadcast new message
                                Debug.Log(resultCoins);

								resultCoins =	coinManager.maxCoins + 1 - effortCoins + (int)returnFloat;
								coinManager.player.Cmd_Set_Text (boxCount, message, resultMessage + resultCoins.ToString ());
								//wait before get result and update message
								if ( !resultMessage.Equals ("")) {
									canvasText.text = resultMessage+ resultCoins.ToString() ;
									//stop overwrite
									message = "";
									resultMessage = "";
								}
							}
							
							yield return true;
					
							//message for localplayer/tokenbox only
						}

						yield return true;
					} else if (Int32.TryParse (node [findInt], out returnInt)) {
					
						//Debug.Log(returnInt);
						yield return true;
					}

					yield  return true;
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
		yield break;
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