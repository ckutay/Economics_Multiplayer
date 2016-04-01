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
	Quaternion transformRotation;
	public Vector3 target;
	GameManager gameManager;
	public Transform rearBone;
	public CoinManager coinManager;
	public modes mode = modes.start;

	Animator animator;
	public NetworkConnection conn;
	GameObject head;

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
		try {
			lookAtEffector = GetComponentInChildren<SimpleMouseLook> ().transform;
		} catch {
			lookAtEffector = transform.GetChild (0).GetChild (0).GetComponent<SimpleMouseLook> ().transform;
		}
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
				if (sitTarget == null)
					sitTargetV = sitTarget.transform.position; 
				sitTargetV.y = startHeight;
				transform.position = sitTargetV;
				break;
			case modes.walk:
			//walking use walkTarget for direction the sittarget

				relativePos = target - transform.position;
			//relativePos.y=transform.position.y;
				transformRotation = Quaternion.LookRotation (relativePos);
				transform.rotation = Quaternion.Lerp (transform.rotation, transformRotation, .5f);	
		
				if ((Vector3.Distance (transform.position, target) < 1f) && (transform.rotation.eulerAngles.y - transformRotation.eulerAngles.y < 1f)) {
					//move between two effectors, first beside seat, next in front.
					//find target for sit or you are sitting
					if (walkTarget == null) {
						mode = modes.sit;
						animator.SetFloat ("Speed", 0);
					} else {
						transformRotation = sitTarget.transform.rotation;
						animator.SetBool ("Sit", true);
						sitTargetV.y = startHeight;
						target = sitTargetV;
						walkTarget = null;
					}
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
			
				sitTargetV.y = startHeight;
				transform.position = sitTargetV;



				//got to sitting if finished sit motion
				if (rearBone != null & rearTarget != null) {


					//transform.position += relativePos;
					transform.rotation = Quaternion.Slerp (transform.rotation, sitTarget.transform.rotation, Time.time * 1);
				}
					//Debug.Log(Vector3.Distance (rearBone.transform.position, sitTarget.transform.position));

				if (animator.GetCurrentAnimatorStateInfo (0).IsName ("sitting_idle"))
					mode = modes.sitting;

	
				break;
			case modes.sitting:
				
				//controler sits over centre of seat
				sitTargetV = rearTarget.transform.position;
				sitTargetV.y = startHeight;
				transform.rotation=sitTarget.transform.rotation;
				if (rearTarget != null) {
					transform.position = sitTargetV;
				}

				if (coinManager == null) {
					coinManager = box.GetComponent<CoinManager> ();
				}

				//start experiment when sitting
				if (animator.GetCurrentAnimatorStateInfo (0).IsName ("sitting_idle")) {
					mode = modes.run;
					exp_cont = coinManager.GetComponent<ExperimentController> ();
					exp_cont.ikActive = true;
				
				}
				lookAtEffector.position = box.transform.position;
				break;
			case modes.run:
				//rearBone.transform.position = rearTarget.transform.position;
				break;

			}
		}
	}


}