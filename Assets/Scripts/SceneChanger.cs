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
	public GameObject normal; 

	public NetworkManager nm;
	public Button joinRoom;

	void Start(){
		canvasBg.SetActive (true);
		menu1.SetActive (true);
	}

	void Update(){
		tabWindow.SetActive (Input.GetKey (KeyCode.Tab));
		consoleWindow.SetActive (!Input.GetKey (KeyCode.Tab));

		if (nm.isJoinedLobby == true) joinRoom.enabled = true;

	}

	public void Menu1ToMenu2(){
		joinRoom.enabled = false;
		menu1.SetActive (false);
		menu2.SetActive(true);
	}
	public void Menu2ToMenu1(){
		menu2.SetActive (false);
		menu1.SetActive(true);
	}
	public void Menu2ToMenu3(){
		menu2.SetActive (false);
		menu3.SetActive(true);
	}
	public void Menu3ToMenu2(){
		menu3.SetActive (false);
		menu2.SetActive(true);
	}
	public void Menu3ToPlaySite(){
		menu3.SetActive (false);
		canvasBg.SetActive(false);
		normal.SetActive (true);
	}
}
