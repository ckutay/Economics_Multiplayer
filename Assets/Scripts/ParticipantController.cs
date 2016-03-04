using UnityEngine;
using System.Collections;
using System;
using UnityEngine.UI;

using UnityEngine.Networking;

public class ParticipantController :NetworkBehaviour
{
	//data is inserted from playernetworksetup when it starts

	public int participant;
	public int participant_id;
	float transPos;

	public enum modes
	{
		start,
		stand,
		walk,
		wait,
		sitting,
		sit,
		run}

	;

	bool update = true;
	//targets assigned in PlyaerNetworksetup
	public GameObject box;

	public GameObject walkTarget;
	public GameObject sitTarget = null;
	public GameObject rearTarget;
	 
	public Vector3 target;
	GameManager gameManager;
	public Transform rearBone;
	public CoinManager coinManager;
	public modes mode = modes.start;

	Animator animator;
	public NetworkConnection conn;
	GameObject head;
	Quaternion relrotation;
	Vector3 relativePos;
	//from playernewtork setup
	public Text canvasText;


	// Use this for initialization
	void Start ()
	{
		
		transPos = transform.position.y;//save for use
	
		animator = GetComponent<Animator> ();
		gameManager = GameObject.Find ("NetworkManager").GetComponent<GameManager> ();
		rearBone = transform.Find ("mixamorig:Hips");
		if (rearBone == null)
			rearBone = transform.Find ("Armature/mixamorig:Hips");

		mode = modes.start;
	


		head = gameObject;


	}
	
	// Update is called once per frame
	void Update ()
	{
		if (isLocalPlayer) {

			switch (mode) {
			case modes.start:

		//	mode = modes.sitting;
				if (walkTarget != null) {
					//start walk
					animator.SetFloat ("Speed", 1);
					target = walkTarget.transform.position;
					animator.SetFloat ("Speed", 1);
					mode = modes.walk;
				}

				break;
			case modes.stand:
			//stop walk
				animator.SetFloat ("Speed", 0);
				if (box != null)
					mode = modes.start;

			
				break;
			case modes.walk:
			//walking use walkTarget for direction the sittarget

				relativePos = target - transform.position;
			//relativePos.y=transform.position.y;
				relrotation = Quaternion.LookRotation (relativePos);
				transform.rotation = Quaternion.Lerp (transform.rotation, relrotation, .5f);	
				Debug.LogWarning (walkTarget);
				Debug.LogWarning (Vector3.Distance (transform.position, target));
				if (Vector3.Distance (transform.position, target) < 1f) {
					//move between two effectors, first beside seat, next in front.
					//find target for sit
					if (walkTarget == null)
						mode = modes.sit;
					target = sitTarget.transform.position;
					walkTarget = null;


				}

				break;
			case modes.sit:
			//sitting down
				canvasText.text = "You will contribute effort in the form of coins";

			// use box target to back of chair for walk direction
				animator.SetBool ("Sit", true);
				animator.SetFloat ("Speed", 0);
			//FIXME
				Vector3 sitTargetV = sitTarget.transform.position;
				sitTargetV.y = transform.position.y;
				transform.position = sitTargetV;

				relativePos = sitTargetV - transform.position;
				relativePos.y = transform.position.y;
				relrotation = Quaternion.LookRotation (relativePos);
				transform.rotation = Quaternion.Lerp (transform.rotation, relrotation, .5f);
				//rotation
			

				transform.LookAt (box.transform);
				mode = modes.sitting;
	

				break;
			case modes.sitting:
				

				if (rearBone != null & rearTarget != null) {

					relativePos = rearTarget.transform.position - rearBone.transform.position;
					//relativePos.y = transform.position.y;

					transform.position += relativePos;
					if (Vector3.Distance (rearBone.transform.position, rearTarget.transform.position) < .5f) {

						animator.SetBool ("Sit", true);
						animator.SetFloat ("Speed", 0);
						//go to experiment controller
						mode = modes.run;

						//setup up experiment controller once
						ExperimentController exp_cont = null;
						//error if no coinmanager
						if (box != null) {
							coinManager = box.GetComponent<CoinManager> ();
						} else {

							//move to exact place
							rearBone.transform.position =rearTarget.transform.position;
							transform.LookAt(box.transform);
							//enable ik
							transform.GetComponent<IKBody> ().ikActive = true;
						
							exp_cont = coinManager.GetComponent<ExperimentController> ();
							//start experiemnt
							exp_cont.mode = ExperimentController.runState.wait;
							exp_cont.box = box;
							//button = exp_cont.button;
							//link to canvas - done in playernetwork setup
							//	exp_cont.canvasText=canvasText;

						}
			
					}
		
				}

				break;
			case modes.run:

				break;

			}
		}
	}



}