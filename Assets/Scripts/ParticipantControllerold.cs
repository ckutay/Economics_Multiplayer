﻿using UnityEngine;
using System.Collections;
using System;
using SimpleJSON;

using UnityEngine.VR;

public class ParticipantControllerOld : MonoBehaviour
{
	//public Transform headTransform;
	public TextFileReader textFileReader;
	Animator animator;
	string returnString;
	int returnInt;


	public enum modes
	{
		start,
		walk,
		wait,
		sitting,
		sit,
		ask,
		answer,
		result,
		finish}

	;

	int stage_number;
	int stage_id;
	GameManager gameManager;
	public GameObject box;


	public GameObject walkTarget;
	public GameObject sitTarget;
	bool update = true;
	public modes mode = modes.start;

	// begins at this value

	// 3.0 seconds or however long to wait

	Quaternion oldRotation;
	Quaternion rotation;
	Vector3 relativePos;
	CoinManager coinManager;
	Vector3 lookPos;
	// Use this for initialization
	void Start ()
	{
		
		stage_number = 0;
		animator = GetComponent<Animator> ();
		gameManager = GameObject.Find ("NetworkManager").GetComponent<GameManager> ();
	}
	
	// Update is called once per frame
	void Update ()
	{
		Vector3 sitPos;
		string url;
		string message = "";

		//"mixamorig:Hips"
		Transform rearBone = transform.GetChild (1);
		switch (mode) {
		case modes.start:

			//start walk
			if (walkTarget != null) {
				//comment out to prevent going into walk mode
				mode = modes.walk;
			}
			//relativePos = box.transform.position - transform.position;
			//oldRotation = Quaternion.LookRotation (relativePos);

		

			break;
		
		case modes.walk:
			//walk to effector near chair, when near go to sit
			animator.SetFloat ("Speed", 1);
			relativePos = walkTarget.transform.position - transform.position;

			rotation = Quaternion.LookRotation (relativePos);
			transform.rotation = Quaternion.Slerp (transform.rotation, rotation, Time.deltaTime);	

			if (Vector3.Distance (transform.position, walkTarget.transform.position) < 1f) {

				mode = modes.sit;
			}



			break;
		case modes.sit:
			
			//move slowly to box then look at box as sitting - then only done once, not pulling head in sitting pose
		
			lookPos = box.transform.position;
			lookPos.y = 0;
		
			//move to point between chair and box
		
			relativePos =   box.transform.position -transform.position ;
			rotation = Quaternion.LookRotation (relativePos);
			transform.rotation = Quaternion.Slerp (transform.rotation, rotation, Time.deltaTime);	
	       //still walking but move to the box

			if (Vector3.Distance (transform.position, box.transform.position) < 1f) {

				mode = modes.sitting;
				//move head to look at box but not hold there
				//do this here as set up box now and does once
			
				if (VRSettings.enabled == false) {
					transform.LookAt (box.transform);
					Transform cam = transform.GetChild (0);
					//IK ik = cam.GetComponent<IK> ();
					//ik.ikActive = true;
					cam.LookAt (lookPos);
				}

			}
			break;
		case modes.sitting:
			//stop and sit
			animator.SetFloat ("Speed", 0);
			animator.SetBool ("Sit", true);

	
			//move Hips to effector on back of seat - not working
			relativePos =   sitTarget.transform.position-transform.position;
			rotation = Quaternion.LookRotation (relativePos);
			//animator.MatchTarget(sitTarget.transform.position, rotation, AvatarTarget.Body, 
			//	new MatchTargetWeightMask(Vector3.one, 1f), 0.141f, 0.78f);
			
			//maybe move to wait - FIXME
		
				//do once
				if (update) {
					message = "These are your coins to contribute as your effort";
					displayMessage (message);

					//find what first step in experiment is
					stage_number++;
					url = textFileReader.IP_Address + "/experiments/stages?experiment_id=" + textFileReader.experiment_id + "&stage_number=" + stage_number;
					Debug.Log (url);
					callServer (url, "type_stage", "");
				
				}
				updateMove ();

			break;
		case modes.wait:
			//wait for nest step - need signal that all have done it from api - FIXME
			if (update) {
				update = false;
				stage_number++;
				message = "Wait for others to finish";
				url = textFileReader.IP_Address + "/experiments/stages?experiment_id=" + textFileReader.experiment_id + "&stage_number=" + stage_number;
				callServer (url, "type_stage", "");

			}
			//when get next step do it
			updateMove ();
			break;
		case modes.ask:
			//when asked for coin number, send it
			coinManager = box.GetComponent<CoinManager> ();
			if (coinManager.isFinished) {
				stage_number++;
			//	url = textFileReader.IP_Address + "/experiments/stages?experiment_id=" + textFileReader.experiment_id + "&stage_number=" + stage_number + "&participant_id=" + textFileReader.participant_id + "&name=CoinEffort&valueString=" + coinManager.currentCoins;
			//	callServer (url, "type_stage", "");
				update = true;
				mode = modes.wait;
			}

			break;

		case modes.answer:
			//when get response form team input, display it - FIXME
			if (update) {
				update = false;
				animator.SetFloat ("Speed", 0);
				stage_number++;
				url = textFileReader.IP_Address + "/experiments/stages?experiment_id=" + textFileReader.experiment_id + "&stage_number=" + stage_number + "&name=Result";
				callServer (url, "type_stage", "");

				url = textFileReader.IP_Address + "/experiments/stages?experiment_id=" + textFileReader.experiment_id + "&stage_number=" + stage_number;
				callServer (url, "type_stage", "");
			}
			//when get next step do it
			updateMove ();
			break;
		}

	}

	void updateMove ()
	{

		//depending on test step, do it
		if (returnString == "Request") {
			update = true;
			mode = modes.ask;
		} else if (returnString == "Receive") {
			update = true;
			mode = modes.answer;
		} else if (returnString == "Wait") {
			update = true;
			mode = modes.wait;
		}
	}


	void displayMessage (string message)
	{
		gameManager.message = message;
	}


	void callServer (string url, string find, string findInt)
	{

		StartCoroutine (FetchWWW (url, find, findInt));


	}

	IEnumerator FetchWWW (string url, string find, string findInt)
	{

		WWW www = new WWW (url);
		yield return StartCoroutine (WaitForRequest (www));
		// StringBuilder sb = new StringBuilder();
		string result = www.text;
		JSONNode node = JSON.Parse (result);
		if (node != null) {
			//Debug.Log(url);
			if (find.Length != 0) {
			
				returnString = node [find];
				yield return true;
			} else {

				if (Int32.TryParse (node [findInt], out returnInt))
					yield return true;
			
			}
		} else {
			Debug.Log ("No node on api read for " + find + " or " + findInt);
			yield break;
	
		}
	}

	public IEnumerator WaitForRequest (WWW www)
	{
		yield return www;

	}
}