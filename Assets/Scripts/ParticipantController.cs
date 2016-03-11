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
	float startHeight = -1.3f;
	Vector3 sitTargetV;
	Transform lookAtEffector;
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

	ExperimentController exp_cont;

	// Use this for initialization
	void Start ()
	{
		


		animator = GetComponent<Animator> ();
		gameManager = GameObject.Find ("NetworkManager").GetComponent<GameManager> ();
		rearBone = transform.Find ("mixamorig:Hips");
		if (rearBone == null)
			rearBone = transform.Find ("Armature/mixamorig:Hips");
		//to focus on box
		lookAtEffector=transform.GetComponentInChildren <SimpleMouseLook>().transform;
		Debug.Log (lookAtEffector);
		mode = modes.start;
	


		head = gameObject;


	}
	
	// Update is called once per frame
	void Update ()
	{
		if (isLocalPlayer) {

			switch (mode) {
			case modes.start:
				animator.SetBool ("Sit", true);
				mode = modes.sitting;
				if (sitTarget != null)
					sitTargetV = sitTarget.transform.position; 
				sitTargetV.y = startHeight;
				//break;
		//	alternate walk start
				if (walkTarget != null) {
					//start walk
					animator.SetFloat ("Speed", 1);
					target = walkTarget.transform.position;
					animator.SetFloat ("Speed", 1);
					mode = modes.walk;
				}

				break;
			case modes.stand:
			//end of game
				animator.SetFloat ("Speed", 0);
				animator.SetBool ("Sit", false);

			//	exp_cont.isHost = false;
			
				break;
			case modes.walk:
			//walking use walkTarget for direction the sittarget

				relativePos = target - transform.position;
			//relativePos.y=transform.position.y;
				relrotation = Quaternion.LookRotation (relativePos);
				transform.rotation = Quaternion.Lerp (transform.rotation, relrotation, .5f);	
		
				if (Vector3.Distance (transform.position, target) < 1f) {
					//move between two effectors, first beside seat, next in front.
					//find target for sit or you are sitting
					//Debug.Log("ChangeDir");
					if (walkTarget == null)
						mode = modes.sit;
					
					animator.SetBool ("Sit", true);
					target = sitTargetV;
					walkTarget = null;

				}

				break;
			case modes.sit:
			//sitting down
				//look at box to help participant

				animator.SetBool ("Sit", true);
				animator.SetFloat ("Speed", 0);
				canvasText.text = "You will contribute effort in the form of coins";

			// use box target to back of chair for walk direction

			//FIXME set standing at sittarget position
				if (sitTarget != null) {

					transform.position = sitTargetV;
					transform.rotation = sitTarget.transform.rotation;
				}
				if (rearBone != null & rearTarget != null& 	!animator.IsInTransition (0)) {



				

					//transform.position += relativePos;
					rearBone.transform.position = Vector3.Lerp (rearBone.transform.position, rearTarget.transform.position, .5f);

					//Debug.Log(Vector3.Distance (rearBone.transform.position, sitTarget.transform.position));
					if (Vector3.Distance (rearBone.transform.position, rearTarget.transform.position) < 1f) {
						mode = modes.sitting;
						rearBone.transform.position = rearTarget.transform.position;
					}
				} else {
					transform.position = sitTargetV;
					transform.rotation = sitTarget.transform.rotation;
					mode = modes.sitting;
				}
				break;
			case modes.sitting:
				if (sitTarget != null) {
					//in case start from here
					transform.position = sitTargetV;
					transform.rotation = sitTarget.transform.rotation;
				}


				if (coinManager == null) {
					coinManager = box.GetComponent<CoinManager> ();
				}

				exp_cont = coinManager.GetComponent<ExperimentController> ();
			

				//Debug.LogWarning(Vector3.Distance (rearBone.transform.position, sitTarget.transform.position));
			
					//has sat
				lookAtEffector.position =  Vector3.Lerp (lookAtEffector.position ,box.transform.position, .5f);
					mode = modes.run;

				break;
			case modes.run:
				
				rearBone.transform.position = rearTarget.transform.position;
				if (animator.GetCurrentAnimatorStateInfo(0).IsName("sitting_idle")){
					if (exp_cont.mode!=ExperimentController.runState.wait)exp_cont.ikActive=true;
				}
				break;

			}
		}
	}


}