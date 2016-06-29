using UnityEngine;
using System.Collections;
using UnityStandardAssets.Characters.FirstPerson;
using UnityEngine.UI;
using System.Collections.Generic;
public class PlayerNetworkMover : Photon.MonoBehaviour
{

	public delegate void Respawn(float respawnTime, bool isBot, bool isPlayerCT, string name);
	public event Respawn RespawnMe;
    public delegate void SendMessage(string message,string color);
	public event SendMessage SendNetworkedMessage;

	Vector3 position;
	Quaternion rotation;
	float smoothing = 10f;
	public float health = 100f;

	//Assist 
	Dictionary<string, float> damageRecord;
    public ScoreManager sm;
   
	//AI
	public bool isCT;
	public bool isBot;

	public void SetGunLayer(int layer){

		transform.Find("FirstPersonCharacter/Camera/M4A1/Mesh_0011").gameObject.layer = layer;
		for(int i=0; i<4; i++)
		{
			transform.Find("FirstPersonCharacter/Camera/M4A1/Mesh_0011").GetChild(i).gameObject.layer = layer;
		}

		transform.Find("FirstPersonCharacter/Camera/M4A1/mag/Mesh_0012").gameObject.layer = layer;
		transform.Find("FirstPersonCharacter/Camera/M4A1/mag").gameObject.layer = layer;
	}
		
	void Start()
	{

		if (photonView.isMine && !isBot)
		{
            health = 100f;
			GetComponent<FirstPersonController>().enabled = true;
			GetComponent <AudioListener>().enabled = true;
			GetComponentInChildren<PlayerShooting>().enabled = true;
            GetComponent<PowerUp>().enabled = true;
			foreach (Camera cam in GetComponentsInChildren<Camera>())
				cam.enabled = true;
			SetGunLayer (11);
			Debug.Log (transform.name + " :: " + transform.position.y.ToString ());

		}
		else if(!photonView.isMine && !isBot){
			StartCoroutine("UpdateData");
			Debug.Log (transform.name + " :: " + transform.position.y.ToString ());
		}
	}


	IEnumerator UpdateData()
	{	
		Debug.Log (transform.name + " ::1 " + transform.position.y.ToString ());

		while(true)
		{
			
			transform.position = Vector3.Lerp(transform.position, position, Time.deltaTime * smoothing);
			if(rotation.w!=0	 || rotation.x!=0 || rotation.y!=0 || rotation.z!=0) transform.rotation = Quaternion.Lerp(transform.rotation, rotation, Time.deltaTime * smoothing);
			yield return null;
		}
		Debug.Log (transform.name + " ::2 " + transform.position.y.ToString ());

	}
	void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		if (stream.isWriting)
		{
			stream.SendNext(transform.position);
			stream.SendNext(transform.rotation);
			stream.SendNext(health);
		}
		else
		{
			position = (Vector3)stream.ReceiveNext();
			rotation = (Quaternion)stream.ReceiveNext();
			health = (float)stream.ReceiveNext();
		}
	}

	public void AddDamage(string playerShooting , float damage)
	{	
		if (damageRecord == null) {
			damageRecord = new Dictionary<string, float>();
		}	
		if (damageRecord.ContainsKey (playerShooting) ) {
			damageRecord [playerShooting] += damage;
		}
        else {
			damageRecord.Add (playerShooting, damage);
		}
	}
	public void search(string playerKilling)
	{
		Debug.Log (playerKilling + "insarch");
		string assistPlayer="";
		foreach(KeyValuePair<string, float> entry in damageRecord)
		{
			if (entry.Value > health / 2 && entry.Key != playerKilling)
			{
				assistPlayer = entry.Key;
				break;
			}
		}
		if(assistPlayer!="")
			sm.ChangeScore(assistPlayer, "Assists", 1);
	}
	[PunRPC]
	public void GetShot(float damage,string shootingPerson)
	{
		Debug.Log ("INRPC");
		health -= damage;
		AddDamage(shootingPerson, damage);
		if (health <= 0)
		{
            search(shootingPerson);
            sm.ChangeScore(shootingPerson, "Kills", 1);
            if (photonView.isMine)
            {
                sm.ChangeScore(PhotonNetwork.player.name, "Deaths", 1);
				if (SendNetworkedMessage != null){	
					string color = "Red";
					if (sm.playerScores [shootingPerson] ["Team"] == 1)
						color = "Blue";
					SendNetworkedMessage( shootingPerson + " => " + PhotonNetwork.player.name,color);
				}
				if (RespawnMe != null)
					RespawnMe(3f,false,isCT,PhotonNetwork.player.name);
                PhotonNetwork.Destroy(gameObject);
            }
        }
	}
}