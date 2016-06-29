using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class SceneChanger : MonoBehaviour {

	public GameObject canvasBg;
	public GameObject menu1;
	public GameObject menu2;
	public GameObject menu3;
	public GameObject tabWindow; 
	public GameObject consoleWindow; 
	public GameObject error;
	public GameObject normalWindow;
	public GameObject mainTab;
	public GameObject botsTab;
	public GameObject mapTab;
	public Image playarea;

	//error validation
	public InputField username;
	public InputField roomname;
	public InputField tBots;
	public InputField ctBots;
	public Text errorMessage;
	public bool isError=false;


	//Network related
	public NetworkManager nm;
	public Button joinRoom;

	//CT and T segregation
	public bool isPlayerCT =false; 

	public InputField numOfCTBots;
	public InputField numOfTBots;
	int valCT;
	int valT;

	public string map="";

	public bool singlePlayer = false;

	void Start(){
		joinRoom.enabled = false;
		canvasBg.SetActive (true);
		menu1.SetActive (true);
	}
		
	void Update(){
		tabWindow.SetActive (Input.GetKey (KeyCode.Tab));
		//consoleWindow.SetActive (!Input.GetKey (KeyCode.Tab));

		if (nm.isJoinedLobby == true) joinRoom.enabled = true;
	}
	public void EnableSinglePlayer(){
		singlePlayer = true;
        //toggle
        PhotonNetwork.offlineMode = true;
        joinRoom.enabled = true;
		joinRoom.GetComponentInChildren<Text>().text="Create Room";
	}

    public void EnableMultiPlayer()
    {
        singlePlayer = false;
        //toggle
        PhotonNetwork.offlineMode = false;
        PhotonNetwork.ConnectUsingSettings("0.4");
        joinRoom.GetComponentInChildren<Text>().text = "Join Or Create Room";
    }

    public void Menu1ToMenu2(){
		menu1.SetActive (false);
		menu2.SetActive(true);
		consoleWindow.SetActive (true);
	}
	public void Menu2ToMenu1(){
		menu2.SetActive (false);
		menu1.SetActive(true);
		consoleWindow.SetActive (false);
	}

	public void Menu2ToMenu3(){
		if (username.text == "" || roomname.text == "") {
			error.SetActive (true);
			isError = true;
			return;
		}
		menu2.SetActive (false);
		menu3.SetActive(true);
		mainTab.SetActive (true);
	}
	public void Menu3ToMenu2(){
		mainTab.SetActive (false);
		botsTab.SetActive (false);
		mapTab.SetActive (false);
		menu3.SetActive (false);
		menu2.SetActive(true);
	}

	public void CloseError(){
		errorMessage.text = "Invalid Input";
		isError = false;
		error.SetActive(false);
	}

	public void Menu3ToPlaySite(){
		if(!int.TryParse(ctBots.text,out valCT)|| !int.TryParse(tBots.text,out valT)){
			error.SetActive(true);
			isError = true;
			errorMessage.text = "Number of bots is ??";
			return;
		}
		else if(valT > 5 || valT > 5){
			error.SetActive(true);
			isError = true;
			errorMessage.text = "Max Bots 5";
			return;
		}
		if (map == "") {
			Debug.Log("MAP is NONE");
			error.SetActive(true);
			isError = true;
			errorMessage.text = "No map selected";
			return;
		}
		Debug.Log(map);
		mainTab.SetActive (false);
		botsTab.SetActive (false);
		mapTab.SetActive (false);
		menu3.SetActive (false);
		canvasBg.SetActive (false);
		normalWindow.SetActive (true);
	}


	public void CTInit( bool isActive){
		isPlayerCT = isActive;
		playarea.color = nm.BLUE;
	}
	public void TInit( bool isActive){
		isPlayerCT = !isActive;
		playarea.color = nm.RED;
	}
	public void Map1000Init(bool isActive){
		Debug.Log ("1000 "+ isActive.ToString());
		if (isActive)
			map = "1000";
		else
			map = "";
	}
	public void MapAwpInit( bool isActive){
		Debug.Log ("awp "+ isActive.ToString());
		if(isActive) map = "awpbase";
		else
			map = "";
		
	}
	public void MapSineInit( bool isActive){
		if(isActive) map = "sine";
		else
			map = "";
	}


	public void BotInitSpawn(){
		for (int i = 0; i < valCT; i++) {
			string s = "BOT CT: " + (i+1).ToString ();	
			nm.StartSpawnProcess(0f, true, true, s);
		}
		for (int i = 0; i < valT; i++) {
			string s = "BOT T: " + (i+1).ToString ();
			nm.StartSpawnProcess(0f, true, false, s);
		}
	}

	/////////////// TABS

	public void Main(){
		mainTab.SetActive (true);
		botsTab.SetActive (false);
		mapTab.SetActive (false);
	}
	public void Bots(){
		mainTab.SetActive (false);
		botsTab.SetActive (true);
		mapTab.SetActive (false);
	}
	public void Maps(){
		mainTab.SetActive (false);
		botsTab.SetActive (false);
		mapTab.SetActive (true);
	}
}


