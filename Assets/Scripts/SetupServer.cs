using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using SimpleJSON;
using System;

public class SetupServer : NetworkBehaviour
{
	//gets data from ZTree server for Host_IP, Port, max_participants and participant number
	//The latter is used to setup player prefab, so rest of code ahs to check this has been done

	string find;
	string findInt;
	public string IP_Address = null;
	TextFileReader textFileReader;
	GameManager gameManager;
	//get from textfilereader
	public int experiment_id;
	//get from server
	public int max_participants;
	public int participant=0;
	public int participant_id;
	public string Host_IP;
	public int Port;
	bool server = true;
	bool update = true;
	//is this the first running system - otherwise do not need to setup server
	bool isHost;



	NetworkManager networkManager;
	// Use this for initialization
	void Start ()
	{
		
		//get components to address 
		networkManager = GetComponent<NetworkManager> ();
		DontDestroyOnLoad (transform.gameObject);
		textFileReader = GetComponent<TextFileReader> ();
		gameManager = GetComponent<GameManager> ();

	
	}
	
	// Update is called once per frame
	void Update ()
	{
		if (update) {
			//get setup values for server request
			IP_Address = textFileReader.IP_Address;
		
			experiment_id = textFileReader.experiment_id;
			isHost = textFileReader.isHost;
			//set up for expereimenter as ishost
			//GetComponent<NetworkManagerOverride> ().isHost = isHost;

		
	
		}
	}

	void OnGUI ()
	{

		//our own HUD
		if (!NetworkClient.active && !NetworkServer.active) {


			//reset values on server to set this machine as hosting
			//option only if set in textfile as host
			if (isHost) {
				if (GUILayout.Button ("Reset ZTree server for hosting from this machine")) {

					find = "Host_IP";

					string url = "/experiments/setup?experiment_id=" + experiment_id + "&Host_IP=" + Network.player.ipAddress;
					//StartCoroutine (FetchHost_IP (url, find, ""));
					findInt = "Port";
					url = "/experiments/setup?experiment_id=" + experiment_id + "&Port=11000";
					StartCoroutine (FetchHost_IP (url, "", findInt));
					//reset participants
					url = "/experiments/participant?experiment_id=" + experiment_id + "&participant=-1";
					StartCoroutine (FetchHost_IP (url, "", ""));
					find = "Host_IP";
					url = "/experiments/setup?experiment_id=" + experiment_id + "&Host_IP";
					StartCoroutine (FetchHost_IP (url, find, ""));
					find = "Port";
					url = "/experiments/setup?experiment_id=" + experiment_id + "&Port";
					StartCoroutine (FetchHost_IP (url, "", ""));
					//remove befor production - reset expereiment results - also remvoe from ztree FIXME
					find = "results";
					url = "/experiments/delete_results?experiment_id=" + experiment_id + "&round_id=1";
					StartCoroutine (FetchHost_IP (url, "", ""));


				}

				//GUILayout.Label ("Network server is not running.");

				//textfile sets up server to avoid oculus need to select as server

				server = GUILayout.Toggle (server, "Run as host or client");
				//find host IP and max_participant number
			
				if (GUILayout.Button ("Start/Join Server")) { 
					//set up your IP
				
					StartCoroutine(setupLink ());
				}

			}else if (textFileReader.readHost != null) {
				
				server = false;
				StartCoroutine(setupLink ());
			}
			//auto start as client if textfilereader found isHost not true

		}
	}
	IEnumerator WaitForSeconds (float num)
	{

		yield return new WaitForSeconds (num);

	}
	IEnumerator setupLink ()
	{
		
		//find varibles for link
		find = "Host_IP";
		string url = "/experiments/setup?experiment_id=" + experiment_id + "&Host_IP";

		yield return StartCoroutine (FetchHost_IP (url, find, ""));

		findInt = "Port";
		url = "/experiments/setup?experiment_id=" + experiment_id + "&Port";
		yield return StartCoroutine (FetchHost_IP (url, "", findInt));
	

		findInt = "max_participants";
		url = "/experiments/setup?experiment_id=" + experiment_id + "&max_participants";
		yield return StartCoroutine (FetchHost_IP (url, "", findInt));
		findInt="";


		//now can set up
		networkManager.networkAddress = Host_IP;
		networkManager.networkPort = Port;
		//can start after set up
		find="participant";
		if (server) {
			//different comment for participant = experimenters as do not add to ecperiment listmax
		
			 url = textFileReader.IP_Address + "/experiments/participant?participant=0&experiment_id=" + textFileReader.experiment_id;
			//Debug.Log(textFileReader.IP_Address+"/experiments/participant?participant=1&experiment_id="+textFileReader.experiment_id);
			yield return StartCoroutine (FetchParticipant (url));
			networkManager.StartHost ();


		} else {


			url = textFileReader.IP_Address + "/experiments/participant?participant=1&experiment_id=" + textFileReader.experiment_id;
			//Debug.Log(textFileReader.IP_Address+"/experiments/participant?participant=1&experiment_id="+textFileReader.experiment_id);
			yield return StartCoroutine (FetchParticipant (url));
			networkManager.StartClient ();
		}

		//Debug.LogWarning(NetworkTransport.IsStarted);
	}


	void showServerInformation ()
	{
		//display link info
		GUILayout.Label ("IP: " + networkManager.networkAddress + " Port: " + networkManager.networkPort);  
	}
	IEnumerator FetchParticipant (string url)
	{
		//Debug.LogWarning(url);
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
				participant = Convert.ToInt32 ((Math.Ceiling (node ["participant"].AsFloat)) - 1);

				participant_id = Convert.ToInt32 (Math.Ceiling (node ["participant_id"].AsFloat));
			
				gameManager.boxCount = participant;
			
				//setup as host

			} else yield break;


		} else {

			Debug.Log ("No Node Network Setup for new participant");
			yield break;
		}



	}

	IEnumerator FetchHost_IP (string url, string find, string findInt)
	{
		
		//get IP and Port numbers - slowly
		yield return StartCoroutine (WaitForSeconds (.1f));
	
		WWW www = new WWW (IP_Address + url);
		yield return StartCoroutine (WaitForRequest (www));
		// StringBuilder sb = new StringBuilder();
		string result = www.text;
		//Debug.Log (result);

		JSONNode node = JSON.Parse (result);
		//Debug.Log (node);
		if (node != null) {
			if (find.Length != 0) {
				//collect string values
				Host_IP = node [find];



			} else if (findInt.Length != 0) {
				//collect integer values
				int resultant;
				if (Int32.TryParse (node [findInt], out resultant)) {

					if (findInt == "Port")
						Port = resultant;
					else if (findInt == "max_participants")
						max_participants = resultant;
					else
						Debug.LogWarning ("incorrect call");

				} else {
					yield break;
				}
			}
		} else {
			
			Debug.LogWarning ("no node for " + find + " or " + findInt);

			yield break;
		}


	}



	public IEnumerator WaitForRequest (WWW www)
	{
		yield return www;

	}

}
