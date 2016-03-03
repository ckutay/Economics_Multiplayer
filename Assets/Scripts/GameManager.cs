using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.UI;

public class GameManager : NetworkBehaviour {
	 
	public GameObject[] playerPrefabs;
	static public GameManager singleton;
	public GameObject [] tokenBoxes;
[SyncVar] 
	public int boxCount=-2; 

	Text canvasText;
	Text canvasTextUp;
	public string message="";


	void Awake()
	{
		singleton = this;
	}
	// Use this for initialization
	void Start () 
	{
		//setup is is -1 when assigned expereimenter
		boxCount = -2;
		//get all the canvas texts - 1 per participant
	//	canvasText = GameObject.FindGameObjectWithTag("Canvas").transform.GetChild (1).gameObject.GetComponent<Text>();
	//	canvasTextUp = GameObject.FindGameObjectWithTag("Canvas").transform.GetChild (2).gameObject.GetComponent<Text>();
	}
	
	// Update is called once per frame
	void Update () {
	
	//	canvasText.text = message;
	//	canvasTextUp.text= message;
	}
	void OnGUI()
	{
		//GUILayout.Label(boxCount.ToString());
	}



	public void Spawn()
	{

		//	GameObject.FindObjectOfType<AddPlayer>().Cmd_Spawn_Prefab();
	}

	// called on the server
	public void ServerRespawn(AddPlayer addPlayer, int boxCount)
	{
		//zero is expereimenter prefab
		GameObject playerPrefab = playerPrefabs[boxCount];
		Destroy (addPlayer.gameObject);
		Vector3 pos = new Vector3 (0, -1.2f, 0);
		GameObject newPlayer = Instantiate<GameObject >( playerPrefab);

		newPlayer.transform.position=pos;
		bool added = NetworkServer.ReplacePlayerForConnection(addPlayer.connectionToClient, newPlayer,0);






	}




}
