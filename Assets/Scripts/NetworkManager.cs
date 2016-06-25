using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class NetworkManager : MonoBehaviour
{
	[SerializeField]
	Text connectionText;
	[SerializeField]
	Transform[] spawnPoints;
	[SerializeField]
	Camera sceneCamera;

	//UI lobby
	[SerializeField] InputField username;
	[SerializeField] InputField roomname;
	[SerializeField] InputField roomlist;

	//UI Info Console
	Queue<string> messages;
	public Text messageWindow;
	const int messageCount = 5;
	PhotonView photonView;

	//UI ScoreTab
	public ScoreManager sm;
	int lastChangeCounter;
	public Text usernameScore;
	public Text Kills;
	public Text Assists;
	public Text Deaths;

	//General state check
	public bool isJoinedRoom=false;
	public bool isJoinedLobby=false;

	//OFfLINE
	public GameObject FPSPlayer;
	GameObject player;

	void Start()
	{
		//offline init
		PhotonNetwork.offlineMode =true;

		//general networking init
		photonView = GetComponent<PhotonView> ();
		PhotonNetwork.logLevel = PhotonLogLevel.Full;
		//PhotonNetwork.ConnectUsingSettings("0.3");

		//console message init
		messages = new Queue<string>(messageCount);
		lastChangeCounter = sm.counter;

	}

	void Update()
	{
		if(isJoinedRoom==false) {
			connectionText.text = PhotonNetwork.connectionStateDetailed.ToString ();
			return;
		}

		//score updates
		if (lastChangeCounter == sm.counter)
			return;
		lastChangeCounter = sm.counter;

		//photonView.RPC("AddScore_RPC", PhotonTargets.All);
		AddScore();
	}

	void OnJoinedLobby()
	{
		isJoinedLobby = true;
	}

	void OnReceivedRoomListUpdate()
	{	//update room list
		roomlist.text = "";
		RoomInfo[] rooms = PhotonNetwork.GetRoomList ();
		foreach (RoomInfo room in rooms) {
			roomlist.text += room.name + '\n';
		}
	}
	public void JoinRoom()
	{	PhotonNetwork.player.name = username.text;
		
		//offline
		PhotonNetwork.JoinRoom(roomname.text);

		//online
		//RoomOptions ro = new RoomOptions() { isVisible = true, maxPlayers = 10 };
		//PhotonNetwork.JoinOrCreateRoom(roomname.text, ro, TypedLobby.Default);
	}

	void OnJoinedRoom()
	{	
		isJoinedRoom =true;
		connectionText.text = "";
		StartSpawnProcess(0f);
	}

	void StartSpawnProcess(float respawnTime)
	{
		sceneCamera.enabled = true;
		StartCoroutine("SpawnPlayer", respawnTime);
	}

	IEnumerator SpawnPlayer(float respawnTime)
	{
		yield return new WaitForSeconds(respawnTime);

		int index = Random.Range(0, spawnPoints.Length);
		player = (GameObject)Instantiate(FPSPlayer,
			spawnPoints[index].position,
			spawnPoints[index].rotation);
		player.transform.name = username .text;

		player.GetComponent<PlayerNetworkMover>().RespawnMe += StartSpawnProcess;
		player.GetComponent<PlayerNetworkMover>().SendNetworkedMessage += AddMessage;

		sceneCamera.enabled = false;
		//AddMessage("Spawned Player : " + PhotonNetwork.player.name);
		sm.SetScore (PhotonNetwork.player.name, "Kills", 0);
		sm.SetScore (PhotonNetwork.player.name, "Deaths", 0);
		sm.SetScore (PhotonNetwork.player.name, "Assists", 0);
	}

	void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
	}

	void AddMessage(string message)
	{
		//photonView.RPC ("AddMessage_RPC", PhotonTargets.All, message);
		messages.Enqueue (message);
		if (messages.Count > messageCount)
			messages.Dequeue ();
		messageWindow.text = "";
		foreach (string m in messages) {
			messageWindow.text += m + '\n';
		}

	}
	[PunRPC]
	void AddMessage_RPC(string message){
		messages.Enqueue (message);
		if (messages.Count > messageCount)
			messages.Dequeue ();
		messageWindow.text = "";
		foreach (string m in messages) {
			messageWindow.text += m + '\n';
		}
	}

	void AddScore(){
		string[] names = sm.GetPlayerNames ("Kills");
		usernameScore.text = "";
		Kills.text = "";
		Assists.text = "";
		Deaths.text = "";
		foreach (string name in names) {
			usernameScore.text += name + '\n';
			Kills.text += sm.GetScore(name,"Kills").ToString() + '\n';
			Assists.text += sm.GetScore(name,"Assists").ToString() + '\n';
			Deaths.text += sm.GetScore(name,"Deaths").ToString() + '\n';
		}
	}

	[PunRPC]
	void AddScore_RPC(){
		string[] names = sm.GetPlayerNames ("Kills");
		usernameScore.text = "";
		Kills.text = "";
		Assists.text = "";
		Deaths.text = "";
		foreach (string name in names) {
			usernameScore.text += name + '\n';
			Kills.text += sm.GetScore(name,"Kills").ToString() + '\n';
			Assists.text += sm.GetScore(name,"Assists").ToString() + '\n';
			Deaths.text += sm.GetScore(name,"Deaths").ToString() + '\n';
		}
	}
}