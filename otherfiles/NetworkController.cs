using UnityEngine;
using System.Collections;
using System;
using UnityEngine.Networking;
using SimpleJSON;
using System.Linq;


public class NetworkController : NetworkManager
{
	public short playerPrefabIndex;

	public override void OnStartServer()
	{
		NetworkServer.RegisterHandler(MsgTypes.PlayerPrefab, OnResponsePrefab);
		base.OnStartServer();
	}

	public override void OnClientConnect(NetworkConnection conn)
	{
		client.RegisterHandler(MsgTypes.PlayerPrefab, OnRequestPrefab);
		base.OnClientConnect(conn);
	}

	private void OnRequestPrefab(NetworkMessage netMsg)
	{
		MsgTypes.PlayerPrefabMsg msg = new MsgTypes.PlayerPrefabMsg();
		msg.controllerID = netMsg.ReadMessage<MsgTypes.PlayerPrefabMsg>().controllerID;
		msg.prefabIndex = playerPrefabIndex;
		client.Send(MsgTypes.PlayerPrefab, msg);
	}

	private void OnResponsePrefab(NetworkMessage netMsg)
	{
		MsgTypes.PlayerPrefabMsg msg = netMsg.ReadMessage<MsgTypes.PlayerPrefabMsg>();  
		playerPrefab = spawnPrefabs[msg.prefabIndex];
		base.OnServerAddPlayer(netMsg.conn, msg.controllerID);
		Debug.Log(playerPrefab.name + " spawned!");
	}

	public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId)
	{
		MsgTypes.PlayerPrefabMsg msg = new MsgTypes.PlayerPrefabMsg();
		msg.controllerID = playerControllerId;
		NetworkServer.SendToClient(conn.connectionId, MsgTypes.PlayerPrefab, msg);
	}
}