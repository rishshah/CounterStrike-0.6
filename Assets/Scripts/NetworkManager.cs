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
	public GameObject counterFPSPlayer;
	public GameObject terroFPSPlayer;
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
    
    void StartSpawnProcess(float respawnTime)
	{
		sceneCamera.enabled = true;
		StartCoroutine("SpawnPlayer", respawnTime);
	}

    IEnumerator SpawnPlayer(float respawnTime)
    {
        yield return new WaitForSeconds(respawnTime);
        sceneCamera.enabled = false;

		//DELEGATE NOTING
		player.GetComponent<PlayerNetworkMover>().RespawnMe += StartSpawnProcess;
		player.GetComponent<PlayerNetworkMover>().SendNetworkedMessage += AddMessage;
		sm.SetScoreRPC += SetScore;

		//#toggle & #segregation spawning
        int CTindex = Random.Range(0, CTspawnPoints.Length);
		int Tindex = Random.Range(0, TspawnPoints.Length);

		if (!sc.singlePlayer)  {
			if(sc.isPlayerCT)
            	player = PhotonNetwork.Instantiate("counterFPSPlayer", CTspawnPoints[CTindex].position, CTspawnPoints[CTindex].rotation, 0);
			else 
				player = PhotonNetwork.Instantiate("terroFPSPlayer", TspawnPoints[Tindex].position, TspawnPoints[Tindex].rotation, 0);
		}
        else {
			if(sc.isPlayerCT)
            	player = (GameObject)Instantiate(counterFPSPlayer, CTspawnPoints[CTindex].position, CTspawnPoints[CTindex].rotation);
			else
				player = (GameObject)Instantiate(terroFPSPlayer, TspawnPoints[Tindex].position, TspawnPoints[Tindex].rotation);
			//#pnm
            EnableComponents();
        }
        
		player.transform.name = username.text;

		//segregation
		if(sc.isPlayerCT) player.transform.parent = CT.transform;
		else player.transform.parent = T.transform;


        //console message for spawn
        AddMessage("Spawned Player : " + PhotonNetwork.player.name);

        //score init
        sm.Init();
        if (!sm.playerScores.ContainsKey(PhotonNetwork.player.name)) { 
            SetScore(PhotonNetwork.player.name, "Kills", 0);
        }
    }
	//################################################ HELPER FN NOT NETWORK RELATED ###################################################
	void EnableComponents()
	{
		//#pnm
		player.GetComponent<Rigidbody>().useGravity = true;
		player.GetComponent<FirstPersonController>().enabled = true;
		player.GetComponent<AudioListener>().enabled = true;
		player.GetComponentInChildren<PlayerShooting>().enabled = true;
		foreach (Camera cam in player.GetComponentsInChildren<Camera>())
			cam.enabled = true;
		for (int i = 0; i < 4; i++){
			player.transform.Find("FirstPersonCharacter/Camera/M4A1/Mesh_0011").GetChild(i).gameObject.layer = 11;
		}
		player.transform.Find("FirstPersonCharacter/Camera/M4A1/mag/Mesh_0012").gameObject.layer = 11;
		player.transform.Find("FirstPersonCharacter/Camera/M4A1/mag").gameObject.layer = 11;
		player.transform.Find("FirstPersonCharacter/Camera/M4A1/Mesh_0011").gameObject.layer = 11;
		//#pnm
	}
		
	void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
	}
	//################################################ RPCS AND thEIR HELPER FN ###################################################

	//SETSCORE RPC
	void SetScore(string username, string scoreType, int value)
    {
        photonView.RPC("SetScore_RPC", PhotonTargets.All, username, scoreType, value);
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

	//DISPLAY SCORE RPC
	void ShortAddScore(){
		string[] names = sm.GetPlayerNames ("Kills");
		CTusernameScore.text = "";
		CTKills.text = "";
		CTAssists.text = "";
		CTDeaths.text = "";
		foreach (string name in names) {
			CTusernameScore.text += name + '\n';
			CTKills.text += sm.GetScore (name, "Kills").ToString () + '\n';
			CTAssists.text += sm.GetScore (name, "Assists").ToString () + '\n';
			CTDeaths.text += sm.GetScore (name, "Deaths").ToString () + '\n';
		}
	}
	void AddScore(){
		if (sc.singlePlayer) {
			ShortAddScore ();	
		}
		else 	photonView.RPC("AddScore_RPC", PhotonTargets.All);
	}
	[PunRPC]
	void AddScore_RPC(){
		ShortAddScore ();
	}
}