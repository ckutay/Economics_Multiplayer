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
	[SyncVar]public int round_id;
	public float transPos;
	//watiing for url output
	bool urlReturn;
	//first run of answer etc
	bool update = true;
	string url;

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

	//public PlayerNetworkSetup setupBox;
	public GameManager gameManager;
	public CoinManager coinManager;

	public int boxCount;
	public Text canvasText;

	string oldMessage;

	//set in playernetworksetup to create network authority
	public ParticipantController participantController;
	public ExperimentNetworking experimentNetworking;
	public bool _isLocalPlayer;
	//Shared from host
	//[HideInInspector]

	[SyncVar] public int stage_number = 0;
	[SyncVar]public runState mode = runState.wait;
	//stage number when result recorded
	int resultStage = 0;
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
		experimentNetworking = GetComponent<ExperimentNetworking> ();
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
		round_id = gameManager.round_id;
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
	

			if (_isLocalPlayer & coinManager.player) {
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
					//enable ik and sync from participantcontroller when sitting
					//ikActive = true;
					//send to server then SyncVar
					if (ikActive) {
						coinManager.player.Cmd_ikActive (boxCount, true);

						//set up to change coins
						coinManager._isLocalPlayer = true;
							
						Vector3 target = button.transform.position;
						lefthandEffector.rotation = coinManager.gameObject.transform.parent.rotation * Quaternion.Euler (-18f, -15f, 40f);
						//need to add rotation of chair

						lefthandEffector.transform.position = target;
						button.GetComponent<ClearButton> ()._isLocalPlayer = true;
						//send coin number	
						canvasText.text = experimentNetworking.message + " You have selected " + coinManager.currentCoins + " coins";
						if (coinManager.isFinished & _isLocalPlayer) {
							round_id = gameManager.round_id;
							//cannot enter anymore

							button.GetComponent<ClearButton> ()._isLocalPlayer = false;
							resultStage = stage_number;
							//send in result to ZTree
							canvasText.text = "Wait for others to finish";

							//enter result
							url = textFileReader.IP_Address + "/experiments/results?experiment_id=" + textFileReader.experiment_id + "&stage_number=" + stage_number + "&participant_id=" + participant_id + "&round_id=" + round_id.ToString () + "&name=CoinEffort&value=" + coinManager.currentCoins;
							StartCoroutine (experimentNetworking.FetchStage (url, "", "", mode));
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
						url = textFileReader.IP_Address + "/experiments/results?experiment_id=" + textFileReader.experiment_id + "&stage_number=" + (resultStage) + "&round_id=" + round_id.ToString () + "&name=Result&participant_id=" + participant_id;
						//gets result and displays to local canvasText.text
						StartCoroutine (experimentNetworking.FetchStage (url, "Results", "", mode));
						update = false;
					}
						//has wait at end to stop going to end screen too quick
					if (experimentNetworking.resultMessage != "")
						canvasText.text = experimentNetworking.message + coinManager.currentCoins.ToString ();

						//FIXME should go to wait, but get message change
						//mode = runState.wait;
					

					break;
				case runState.end:
					
					StartCoroutine (resultShow (experimentNetworking.resultMessage));
				
					participantController.mode = ParticipantController.modes.stand;
					//gameManager.boxCount = -1;


					break;
				}
				//Debug.LogWarning (message);
			}
			//update effort until end
			if (mode != runState.end & mode!=runState.answer)
				effortCoins = coinManager.currentCoins;
			//works when syncvar gives new braodcast message to player
		
			if (_isLocalPlayer & oldMessage != experimentNetworking.message & !experimentNetworking.message.Equals ("")) {
				showMessage (experimentNetworking.message);
				oldMessage = experimentNetworking.message;
			}
		
		}

	}

	IEnumerator resultShow (string _message)
	{
		//make sure see return message before final result

		yield return StartCoroutine (WaitForSeconds (1f));
		//wait before send result
		int resultCoins =	coinManager.currentCoins;
		canvasText.text = _message + resultCoins.ToString ();
		yield return true;

	}


	//deals with group messages, not individual ones
	//they are done under modes
	void showMessage (string _message)
	{
		//update if not blank
		if (!_message.Equals (""))
			canvasText.text = _message;

	}

	void updateMove ()
	{
		//runs on host localplayer only
		//do not broadcast unless change as some are in waiting
		int _stage_number = stage_number;
	

		//returned data from url
		if (experimentNetworking.urlReturn) {

			if (mode != runState.end) {
				
				url = textFileReader.IP_Address + "/experiments/stages?experiment_id=" + textFileReader.experiment_id + "&stage_number=" + stage_number + "&round_id=" + round_id.ToString ();
				//Debug.Log (url);

				StartCoroutine (experimentNetworking.FetchStage (url, "type_stage", "stage_number", mode));
				url = "";

				//move on to next one
				if (experimentNetworking.returnInt > 0)
					stage_number = experimentNetworking.returnInt;
				
				if (experimentNetworking.returnString == "Request") {
					

					mode = runState.ask;

				} else if (experimentNetworking.returnString == "Response") {
					

					mode = runState.answer;


				} else if (experimentNetworking.returnString == "Wait") {

					mode = runState.wait;

				} else if (experimentNetworking.returnString == "End") {

					mode = runState.end;

				}

			}


			try {

				//fixMe is this a problem??
				if (_stage_number != stage_number & coinManager.player) {
					coinManager.player.Cmd_change_currentStage (stage_number, mode);
				
				}
			} catch {
			}


		
			experimentNetworking.callUpdate ();

		}
	}

	IEnumerator WaitForSeconds (float num)
	{

		yield return new WaitForSeconds (num);

	}



}