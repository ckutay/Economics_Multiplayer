/*using UnityEngine;
using System.Collections;


public class NetworkCharatcer : Photon.MonoBehaviour {


    Vector3 realPosition = Vector3.zero;
    Quaternion realRotation = Quaternion.identity;
    float lastUpdateTime;
	Animator anim;

	// Use this for initialization
	void Start () 
	{
		anim = GetComponent<Animator> ();

	}
	
	// Update is called once per frame
	void FixedUpdate () {
        // PhotonView photonView = GetComponent<PhotonView>();
        if (photonView.isMine)
        {
            // do nothing
        }
        else
        {
            transform.position = Vector3.Lerp(transform.position, realPosition, 0.1f);
            transform.rotation = Quaternion.Lerp(transform.rotation, realRotation, 0.1f);
        }
		/*if (transform.position != realPosition) {
			anim.SetBool ("Running", true);		
		}
		else
		{
			anim.SetBool ("Running", false);	
		}
	}

	// send/recieve information from other clients
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.isWriting)
        {
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);
			stream.SendNext (anim.GetFloat("Forward"));
			//stream.SendNext (anim.GetBool ("m_IsGrounded"));
			//stream.SendNext (anim.GetFloat("Jump"));
			stream.SendNext (anim.GetFloat("Turn"));  //m_IsGrounded

        }
        else
        {
            realPosition = (Vector3)stream.ReceiveNext();
            realRotation = (Quaternion)stream.ReceiveNext();
			anim.SetFloat ("Forward", (float)stream.ReceiveNext ());
			//anim.SetBool ("m_IsGrounded", (bool)stream.ReceiveNext ());
			//anim.SetFloat ("Jump", (float)stream.ReceiveNext ());
			anim.SetFloat ("Turn", (float)stream.ReceiveNext ());
        }
    }
}
*/