using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityStandardAssets.Characters.FirstPerson;

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
	const int messageCount = 20;
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

	//#toggle# multiplayer & singleplayer
	public SceneChanger sc;

	//toggle
	public GameObject FPSPlayer;
	GameObject player;

    //CT and T segregation
    public GameObject CT;
    public GameObject T;


    void Start()
	{
		//general networking init
		photonView = GetComponent<PhotonView> ();
		PhotonNetwork.logLevel = PhotonLogLevel.Full;

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
		
		//toggle
		if (sc.singlePlayer)
			PhotonNetwork.JoinRoom (roomname.text);
		else {
			RoomOptions ro = new RoomOptions() { isVisible = true, maxPlayers = 10 };
			PhotonNetwork.JoinOrCreateRoom(roomname.text, ro, TypedLobby.Default);
		}
	}

	void OnJoinedRoom()
	{	
		isJoinedRoom =true;
		connectionText.text = "";
		StartSpawnProcess(0f);
	}
    void EnableComponents()
    {
        //#pnm
        player.GetComponent<Rigidbody>().useGravity = true;
        player.GetComponent<FirstPersonController>().enabled = true;
        player.GetComponent<UIManager>().enabled = true;
        player.GetComponentInChildren<Canvas>().enabled = true;
        player.GetComponent<AudioListener>().enabled = true;
        player.GetComponentInChildren<PlayerShooting>().enabled = true;
        foreach (Camera cam in player.GetComponentsInChildren<Camera>())
            cam.enabled = true;
        for (int i = 0; i < 4; i++)
        {
            player.transform.Find("FirstPersonCharacter/Camera/M4A1/Mesh_0011").GetChild(i).gameObject.layer = 11;
        }
        player.transform.Find("FirstPersonCharacter/Camera/M4A1/mag/Mesh_0012").gameObject.layer = 11;
        player.transform.Find("FirstPersonCharacter/Camera/M4A1/mag").gameObject.layer = 11;
        player.transform.Find("FirstPersonCharacter/Camera/M4A1/Mesh_0011").gameObject.layer = 11;
        //#pnm
    }
    void StartSpawnProcess(float respawnTime)
	{
		sceneCamera.enabled = true;
		StartCoroutine("SpawnPlayer", respawnTime);
	}

    IEnumerator SpawnPlayer(float respawnTime)
    {
        yield return new WaitForSeconds(respawnTime);
        sceneCamera.enabled = false;

        int index = Random.Range(0, spawnPoints.Length);
        if (!sc.singlePlayer)
        {
            player = PhotonNetwork.Instantiate("FPSPlayer", spawnPoints[index].position, spawnPoints[index].rotation, 0);
        }
        else
        {
            player = (GameObject)Instantiate(FPSPlayer, spawnPoints[index].position, spawnPoints[index].rotation);
            //#pnm
            EnableComponents();
        }
        player.transform.parent = CT.transform;
        player.transform.name = username.text;

        player.GetComponent<PlayerNetworkMover>().RespawnMe += StartSpawnProcess;
        player.GetComponent<PlayerNetworkMover>().SendNetworkedMessage += AddMessage;
        sm.SetScoreRPC += SetScore;

        //console message for spawn
        AddMessage("Spawned Player : " + PhotonNetwork.player.name);

        //score init
        sm.Init();
        if (!sm.playerScores.ContainsKey(PhotonNetwork.player.name)) { 
            SetScore(PhotonNetwork.player.name, "Kills", 0);
        }
    }

    void SetScore(string username, string scoreType, int value)
    {
        photonView.RPC("SetScore_RPC", PhotonTargets.All, username, scoreType, value);
    }
    [PunRPC]
    public void SetScore_RPC(string username, string scoreType, int value)
    {
        sm.Init();
        sm.counter++;
        if (sm.playerScores.ContainsKey(username) == false)
        {
            sm.playerScores[username] = new Dictionary<string, int>();
        }
        sm.playerScores[username][scoreType] = value;
    }
   
	void AddMessage(string message)
	{	
		//toggle
		if (sc.singlePlayer) {
			messages.Enqueue (message);
			if (messages.Count > messageCount)
				messages.Dequeue ();
			messageWindow.text = "";
			foreach (string m in messages) {
				messageWindow.text += m + '\n';
			}
		}
		else photonView.RPC ("AddMessage_RPC", PhotonTargets.All, message);
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
		if (sc.singlePlayer) {
			string[] names = sm.GetPlayerNames ("Kills");
			usernameScore.text = "";
			Kills.text = "";
			Assists.text = "";
			Deaths.text = "";
			foreach (string name in names) {
				usernameScore.text += name + '\n';
				Kills.text += sm.GetScore (name, "Kills").ToString () + '\n';
				Assists.text += sm.GetScore (name, "Assists").ToString () + '\n';
				Deaths.text += sm.GetScore (name, "Deaths").ToString () + '\n';
			}
		}
		else 	photonView.RPC("AddScore_RPC", PhotonTargets.All);
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

    void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        /* if (stream.isWriting)
         {
             stream.SendNext(sm.playerScores);
         }
         else
         {
             sm.playerScores = (Dictionary<string, Dictionary<string, int>>)stream.ReceiveNext();
         }*/
    }

}