using UnityEngine;
using System.Collections;
using System;
using UnityEngine.Networking;
using SimpleJSON;
using System.Linq;
public class MsgTypes
{
	public const short PlayerPrefab = MsgType.Highest + 1;

	public class PlayerPrefabMsg : MessageBase
	{
		public short controllerID;    
		public short prefabIndex;
	}
}

public class NetworkManagerOverride: NetworkManager
{

	public GameObject[] playerPrefabs;



	public override void OnStartServer()
	{
		
		NetworkServer.RegisterHandler(MsgTypes.PlayerPrefab, OnResponsePrefab);
	
		base.OnStartServer();
	}

	public override void OnClientConnect(NetworkConnection conn)
	{
		int prefabIndex = gameObject.GetComponent<SetupServer> ().participant;
		if (prefabIndex>=0)playerPrefab = playerPrefabs[prefabIndex];
		client.RegisterHandler(MsgTypes.PlayerPrefab, OnRequestPrefab);
		Debug.LogWarning(playerPrefab.name + " registered!");
		base.OnClientConnect(conn);

	}

	private void OnRequestPrefab(NetworkMessage netMsg)
	{
		MsgTypes.PlayerPrefabMsg msg = new MsgTypes.PlayerPrefabMsg();
		msg.controllerID = netMsg.ReadMessage<MsgTypes.PlayerPrefabMsg>().controllerID;
		msg.prefabIndex = gameObject.GetComponent<SetupServer> ().participant;
		Debug.LogWarning(msg.prefabIndex + " selected!");
		Debug.LogWarning(playerPrefab.name + " requested!");
		client.Send(MsgTypes.PlayerPrefab, msg);
		ClientScene.AddPlayer(netMsg.conn,(short)0);
	}

	private void OnResponsePrefab(NetworkMessage netMsg)
	{
		MsgTypes.PlayerPrefabMsg msg = netMsg.ReadMessage<MsgTypes.PlayerPrefabMsg>();  
		if (msg.prefabIndex>=0)playerPrefab = playerPrefabs[msg.prefabIndex];
		base.OnServerAddPlayer(netMsg.conn, msg.controllerID);
		Debug.LogWarning(playerPrefab.name + " spawned!");
	}

	public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId)
	{
		MsgTypes.PlayerPrefabMsg msg = new MsgTypes.PlayerPrefabMsg();
		msg.controllerID = playerControllerId;
		NetworkServer.SendToClient(conn.connectionId, MsgTypes.PlayerPrefab, msg);

	}
}
