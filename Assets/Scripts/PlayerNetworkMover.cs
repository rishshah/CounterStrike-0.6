using UnityEngine;
using System.Collections;
using UnityStandardAssets.Characters.FirstPerson;
using UnityEngine.UI;
using System.Collections.Generic;
public class PlayerNetworkMover : Photon.MonoBehaviour
{

	public delegate void Respawn(float time);
	public event Respawn RespawnMe;
    public delegate void SendMessage(string message);
	public event SendMessage SendNetworkedMessage;

	Vector3 position;
	Quaternion rotation;
	float smoothing = 10f;
	public float health = 100f;

	//Assist 
	Dictionary<string, float> damageRecord;
    public ScoreManager sm;
    //
    public Canvas canvas;
	void Start()
	{

		if (photonView.isMine)
		{
			GetComponent<Rigidbody>().useGravity = true;
			GetComponent<FirstPersonController>().enabled = true;
			GetComponent<UIManager>().enabled = true;
            canvas.enabled = true;
			GetComponent <AudioListener>().enabled = true;
			GetComponentInChildren<PlayerShooting>().enabled = true;
			foreach (Camera cam in GetComponentsInChildren<Camera>())
				cam.enabled = true;
			for(int i=0; i<4; i++)
			{
				transform.Find("FirstPersonCharacter/Camera/M4A1/Mesh_0011").GetChild(i).gameObject.layer = 11;
			}
			transform.Find("FirstPersonCharacter/Camera/M4A1/mag/Mesh_0012").gameObject.layer = 11;
			transform.Find("FirstPersonCharacter/Camera/M4A1/mag").gameObject.layer = 11;
			transform.Find("FirstPersonCharacter/Camera/M4A1/Mesh_0011").gameObject.layer = 11;
		}
		else{
			StartCoroutine("UpdateData");
		}
	}


	IEnumerator UpdateData()
	{
		while(true)
		{
			transform.position = Vector3.Lerp(transform.position, position, Time.deltaTime * smoothing);
			if(rotation.w!=0	 || rotation.x!=0 || rotation.y!=0 || rotation.z!=0) transform.rotation = Quaternion.Lerp(transform.rotation, rotation, Time.deltaTime * smoothing);
			yield return null;
		}
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

	void AddDamage(string playerShooting , float damage)
	{	
		if (damageRecord == null) {
			damageRecord = new Dictionary<string, float>();
		}	
		if (damageRecord.ContainsKey (playerShooting) ) {
			damageRecord [playerShooting] += damage;

		} else {
			Debug.Log (playerShooting);
			damageRecord.Add (playerShooting, damage);
		}
	}
	void search(string playerKilling)
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
		health -= damage;
		AddDamage(shootingPerson, damage);
		if (health <= 0)
		{
            search(shootingPerson);
            sm.ChangeScore(shootingPerson, "Kills", 1);
            if (photonView.isMine)
            {
                sm.ChangeScore(PhotonNetwork.player.name, "Deaths", 1);
                if (SendNetworkedMessage != null)
                    SendNetworkedMessage(PhotonNetwork.player.name + " was killed by " + shootingPerson);
                if (RespawnMe != null)
                    RespawnMe(3f);
                PhotonNetwork.Destroy(gameObject);
            }
        }
	}
}