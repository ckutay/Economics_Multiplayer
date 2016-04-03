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
	float startHeight = -1.0f;
	Vector3 sitTargetV;
	Transform lookAtEffector;
	float tableHeight=1.3f;
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

	float diff = 1;
	//targets assigned in PlyaerNetworksetup
	public GameObject box;

	public GameObject walkTarget;
	public GameObject sitTarget = null;
	public GameObject rearTarget;
	Quaternion transformRotation;
	public Vector3 target;

	PlayerNetworkSetup playerNetwork;
	public Transform rearBone;
	public CoinManager coinManager;
	public modes mode = modes.start;

	Animator animator;
	public NetworkConnection conn;


	Vector3 relativePos;
	//from playernewtork setup
	public Text canvasText;

	ExperimentController exp_cont;
	Transform leftHandEffector;
	Transform rightHandEffector;
	// Use this for initialization
	void Start ()
	{
		startHeight = transform.position.y;
		leftHandEffector=GetComponent<IKBody>().leftHandObj;
		rightHandEffector=GetComponent<IKBody>().rightHandObj;
		playerNetwork = transform.GetComponent<PlayerNetworkSetup> ();
		animator = GetComponent<Animator> ();
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

	}
	
	// Update is called once per frame
	void Update ()
	{
		if (isLocalPlayer) {

			switch (mode) {
			case modes.start:
				
				if (sitTarget != null)
					sitTargetV = sitTarget.transform.position; 
				sitTargetV.y = startHeight;
				

				//break;
		//	alternate walk start
				if (walkTarget != null) {
					//start walk
					animator.SetFloat ("Speed", 1);
					target = walkTarget.transform.position;
				
					mode = modes.walk;

				} else {
					animator.SetBool ("Sit", true);
					mode = modes.sitting;
				}
				break;
			case modes.stand:
			//end of game
				animator.SetFloat ("Speed", 0);
				animator.SetBool ("Sit", false);
			
				transform.position = sitTargetV;
				break;
			case modes.walk:
			//walking use walkTarget for direction the sittarget

				relativePos = target - transform.position;
				relativePos.y = 0f;
				transformRotation = Quaternion.LookRotation (relativePos);
				transform.rotation = Quaternion.Slerp (transform.rotation, transformRotation, Time.time * 1);	
			//	Debug.LogWarning (Vector3.Distance (transform.position, target));

				if ((Vector3.Distance (transform.position, target) < diff) && (transform.rotation.eulerAngles.y - transformRotation.eulerAngles.y < diff)) {
					//move between two effectors, first beside seat, next in front.
					//find target for sit or you are sitting
					if (walkTarget == null) {
						mode = modes.sit;
						sitTargetV.y = startHeight;
						transform.position = sitTargetV;
						animator.SetFloat ("Speed", 0);
					} else {
						
						if(rearTarget!=null)diff = 0.4f;

						target = sitTargetV;
						walkTarget = null;
					}
				}

				break;
			case modes.sit:
			//sitting down
				//look at box to help participant
				//put at target
				//set active wihthand higher

			
				animator.SetBool ("Sit", true);
				animator.SetFloat ("Speed", 0);
				canvasText.text = "You will contribute effort in the form of coins";

				transformRotation = sitTarget.transform.rotation;

					//transform.position += relativePos;
				transform.rotation = Quaternion.Slerp (transform.rotation, transformRotation, Time.time * 1);
				if (transform.rotation.eulerAngles.y - transformRotation.eulerAngles.y < .1f) {
					//go to sitting if finished sit motion
					if (animator.GetCurrentAnimatorStateInfo (0).IsName ("sitting_idle"))
						mode = modes.sitting;
				
				}


	
				break;
			case modes.sitting:
				
				//controler sits over centre of seat
				bool continues = false;
				//finish rotation
				transform.rotation = sitTarget.transform.rotation;
				if (rearTarget != null) {
					sitTargetV = rearTarget.transform.position;
					sitTargetV.y = startHeight;
					transform.position = Vector3.Slerp (transform.position, sitTargetV, Time.time * 1);
					if (Vector3.Distance (transform.position, sitTargetV) < 0.3f) {
						transform.position = sitTargetV;
						continues=true;
					}
				} else continues=true;
			if (continues) {
					if (coinManager == null) {
						coinManager = box.GetComponent<CoinManager> ();
					}

					//start experiment when sitting
					if (animator.GetCurrentAnimatorStateInfo (0).IsName ("sitting_idle")) {
						mode = modes.run;
						exp_cont = coinManager.GetComponent<ExperimentController> ();

						exp_cont.ikActive = true;
						exp_cont.mode = ExperimentController.runState.wait;
				
					
						Vector3 pos= leftHandEffector.position;
						pos.y*=tableHeight;
						leftHandEffector.position=pos;
						pos= rightHandEffector.position;
						pos.y*=tableHeight;
						rightHandEffector.position=pos;
					}
					lookAtEffector.position = box.transform.position;
					playerNetwork.FPCharacterCam.transform.LookAt (lookAtEffector.position);
				}
				break;
			case modes.run:
				
				break;

			}
		}
	}


}