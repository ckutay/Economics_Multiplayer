using UnityEngine;
using System.Collections;

public class testing : MonoBehaviour {
	public bool server;

	int maxClients = 5;

	string remoteIP = "10.248.156.97";
	public int remotePort = 25000;

	void connectToServer() {
		Network.Connect(remoteIP, remotePort);  
	}

	void disconnectFromServer() {
		Network.Disconnect();
	}
	void startServer() {
		
		Network.InitializeServer(maxClients, remotePort, false);    

	}

	void stopServer() {
		Network.Disconnect();
	}
	void Start(){
		

	}
	public void Update(){


	}
	void OnGUI ()
	{
		if (Network.peerType == NetworkPeerType.Disconnected) {
			GUILayout.Label("Network server is not running.");
			if (GUILayout.Button ("Start/Join Server"))
			{               
				if (server){
					startServer();
				}else{
					connectToServer();
				}

				Debug.Log(Network.connections);
			}
		}
		else {
			if (Network.peerType == NetworkPeerType.Connecting)
				GUILayout.Label("Network server is starting up...");
			else { 
				GUILayout.Label("Network server is running.");          
				showServerInformation();    
				showClientInformation();
			}
			if (GUILayout.Button ("Stop Server"))
			{               
				stopServer();   
			}
		}
	
	}

	void showClientInformation() {
		GUILayout.Label("Clients: " + Network.connections.Length + "/" + maxClients);
		foreach( NetworkPlayer p in Network.connections) {
			GUILayout.Label(" Player from ip/port: " + p.ipAddress + "/" + p.port); 
		}
	}

	void showServerInformation() {
		GUILayout.Label("IP: " + Network.player.ipAddress + " Port: " + Network.player.port);  
	}
}
