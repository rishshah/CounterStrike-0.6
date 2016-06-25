using UnityEngine;
using System.Collections;
using UnityStandardAssets.Characters.FirstPerson;
using UnityEngine.UI;
public class PlayerNetworkMover : Photon.MonoBehaviour
{

	public delegate void Respawn(float time);
	public event Respawn RespawnMe;
	public delegate void SendMessage(string message);
	public event SendMessage SendNetworkedMessage;

	Vector3 position;
	Quaternion rotation;
	float smoothing = 10f;
	float health = 100f;


	void Start()
	{

		if (photonView.isMine)
		{
			GetComponent<Rigidbody>().useGravity = true;
			GetComponent<FirstPersonController>().enabled = true;
			GetComponent <AudioListener>().enabled = true;
			//GetComponent<FirstPersonHeadBob>().enabled = true;
			// GetComponent<SimpleMouseRotator>().enabled = true;
			GetComponentInChildren<PlayerShooting>().enabled = true;
			// foreach (SimpleMouseRotator rot in GetComponentsInChildren<SimpleMouseRotator>())
			//    rot.enabled = true;
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
			transform.rotation = Quaternion.Lerp(transform.rotation, rotation, Time.deltaTime * smoothing);
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

	[PunRPC]
	public void GetShot(float damage,string shootingPerson)
	{
		health -= damage;
		//consoleText.text +=
		if (health <= 0 && photonView.isMine)
		{
			if (SendNetworkedMessage != null)
				SendNetworkedMessage (PhotonNetwork.player.name + " was killed by " + shootingPerson);
			if (RespawnMe != null)
				RespawnMe(3f);

			PhotonNetwork.Destroy(gameObject);
		}
	}

}