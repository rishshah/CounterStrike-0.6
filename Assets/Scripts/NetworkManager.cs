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
	Camera sceneCamera;

	//#segregation
	[SerializeField]
	Transform[] CTspawnPoints;
	[SerializeField]
	Transform[] TspawnPoints;

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
	public Text CTusernameScore;
	public Text CTKills;
	public Text CTAssists;
	public Text CTDeaths;
	public Text TusernameScore;
	public Text TKills;
	public Text TAssists;
	public Text TDeaths;

	//General state check
	public bool isJoinedRoom=false;
	public bool isJoinedLobby=false;

	//#toggle# multiplayer & singleplayer
	public SceneChanger sc;

	//toggle and segregation
	public GameObject FPSPlayer;
	GameObject player;

    //CT and T segregation
    public GameObject CT;
    public GameObject T;
	Color blue= new Color(17,255,255);
	Color red= new Color(203,70,70);

	//bots spawning
	public InputField numCT;
	public InputField numT;

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

        //AddScore();
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
    
    void StartSpawnProcess(float respawnTime)
	{
		sceneCamera.enabled = true;
		StartCoroutine(SpawnPlayer(respawnTime,false,sc.isPlayerCT,username.text));

		int valCT, valT;
		Debug.Log ("T: " + numT.text + " CT: " + numCT.text);
		if (int.TryParse (numT.text, out valT)) {
			for (int i = 0; i < valT; i++) {
				string s = "BOT CT: " + i.ToString ();	
				StartCoroutine (SpawnPlayer( respawnTime, true, false,s));
			}
		}
		if (int.TryParse (numCT.text, out valCT)) {
			for (int i = 0; i < valCT; i++) {
				string s = "BOT T: " + i.ToString ();
				StartCoroutine (SpawnPlayer(respawnTime, true, true,s));
			}
		}
	}

	IEnumerator SpawnPlayer(float respawnTime , bool  isBot, bool  isPlayerCT,string name)
    {	
		sceneCamera.enabled = false;
        yield return new WaitForSeconds(respawnTime);
        
		//#toggle & #segregation spawning
        int CTindex = Random.Range(0, CTspawnPoints.Length);
		int Tindex = Random.Range(0, TspawnPoints.Length);

		if (isPlayerCT) {
			player = PhotonNetwork.Instantiate ("FPSPlayer", CTspawnPoints [CTindex].position, CTspawnPoints [CTindex].rotation, 0);
			player.transform.parent = CT.transform;
			player.transform.Find("Body").GetComponent<MeshRenderer> ().material.color = blue;
			player.transform.Find ("Head/Cap").GetComponent<MeshRenderer> ().material.color = blue;
		} 
		else if (!isPlayerCT) {
			player = PhotonNetwork.Instantiate ("FPSPlayer", TspawnPoints [Tindex].position, TspawnPoints [Tindex].rotation, 0);
			player.transform.parent = T.transform;
			player.transform.Find("Body").GetComponent<MeshRenderer> ().material.color = red;
			player.transform.Find ("Head/Cap").GetComponent<MeshRenderer> ().material.color = red;
		}
		player.transform.name = name;

		if (!isBot) 
			player.GetComponent<PlayerNetworkMover> ().isBot = false;
		else 
			player.GetComponent<PlayerNetworkMover> ().isBot = true;
		
		//DELEGATE NOTING
		player.GetComponent<PlayerNetworkMover>().RespawnMe += StartSpawnProcess;
		player.GetComponent<PlayerNetworkMover>().SendNetworkedMessage += AddMessage;
		sm.SetScoreRPC += SetScore;

		//console message for spawn
        AddMessage("Spawned Player : " + name);

        //score init
        sm.Init();
        if (!sm.playerScores.ContainsKey(name)) { 
			SetScore(name, "Kills", 0);
            SetScore(name, "Assists", 0);
            SetScore(name, "Deaths", 0);
            if (isPlayerCT) SetScore(name, "Team", 1);
			else 	SetScore(name, "Team", 0);
        }
    }

    //DISPLAY SCORE  ###### NOT AN RPC
    void AddScore()
    {
        string[] CTnames = sm.GetPlayerNamesCT("Kills");
        CTusernameScore.text = "";
        CTKills.text = "";
        CTAssists.text = "";
        CTDeaths.text = "";
        foreach (string name in CTnames)
        {
            CTusernameScore.text += name + '\n';
            CTKills.text += sm.GetScore(name, "Kills").ToString() + '\n';
            CTAssists.text += sm.GetScore(name, "Assists").ToString() + '\n';
            CTDeaths.text += sm.GetScore(name, "Deaths").ToString() + '\n';
        }

        string[] Tnames = sm.GetPlayerNamesT("Kills");
        TusernameScore.text = "";
        TKills.text = "";
        TAssists.text = "";
        TDeaths.text = "";
        foreach (string name in Tnames)
        {
            TusernameScore.text += name + '\n';
            TKills.text += sm.GetScore(name, "Kills").ToString() + '\n';
            TAssists.text += sm.GetScore(name, "Assists").ToString() + '\n';
            TDeaths.text += sm.GetScore(name, "Deaths").ToString() + '\n';
        }
    }
   	
	void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
	}
    //################################################ RPCS AND thEIR HELPER FN ###################################################

	//SETSCORE RPC
	void SetScore(string username, string scoreType, int value)
    {
        photonView.RPC("SetScore_RPC", PhotonTargets.AllBuffered, username, scoreType, value);
    }
    [PunRPC]
    public void SetScore_RPC(string username, string scoreType, int value)
    {
        sm.Init();
        sm.counter++;
        if (sm.playerScores.ContainsKey(username) == false){
			sm.playerScores[username] = new Dictionary<string, int>();
        }
		sm.playerScores[username][scoreType] = value;
    }

	//CONSOLE MESSAGE RPC
	void ShortAddMessage(string message){
		messages.Enqueue (message);
		if (messages.Count > messageCount)
			messages.Dequeue ();
		messageWindow.text = "";
		foreach (string m in messages) {
			messageWindow.text += m + '\n';
		}
	}
	void AddMessage(string message)
	{	
		//toggle
		if (sc.singlePlayer) {
			ShortAddMessage (message);
		}
		else photonView.RPC ("AddMessage_RPC", PhotonTargets.All, message);
	}
	[PunRPC]
	void AddMessage_RPC(string message){
		ShortAddMessage (message);
	}	
}