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

	//error validation
	public InputField username;
	public InputField roomname;
	public InputField tBots;
	public InputField ctBots;

	//Network related
	public NetworkManager nm;
	public Button joinRoom;

	//CT and T segregation
	public bool isPlayerCT=false; 

	public InputField numOfCTBots;
	public InputField numOfTBots;
	int valCT;
	int valT;


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
			return;
		}
		menu2.SetActive (false);
		menu3.SetActive(true);
	}
	public void Menu3ToMenu2(){
		menu3.SetActive (false);
		menu2.SetActive(true);
	}

	public void CloseError(){
		error.SetActive(false);
	}

	public void Menu3ToPlaySite(){
		if(!int.TryParse(ctBots.text,out valCT)|| !int.TryParse(tBots.text,out valT)){
			error.SetActive(true);
			return;
		}
		menu3.SetActive (false);
		canvasBg.SetActive (false);
		normalWindow.SetActive (true);
	}

	public void CTInit(){
		isPlayerCT = true;

	}
	public void BotInitSpawn(){
		for (int i = 0; i < valCT; i++) {
			string s = "BOT CT: " + (i+1).ToString ();	
			nm.StartSpawnProcess(3f, true, true,s);
		}
		for (int i = 0; i < valT; i++) {
			string s = "BOT T: " + (i+1).ToString ();
			nm.StartSpawnProcess(3f, true, false,s);
		}
	}
}
