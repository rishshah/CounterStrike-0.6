using UnityEngine;
using System.Collections;
using UnityEngine.UI;
public class PlayerShooting : MonoBehaviour
{
	//public GameObject bloodParticles;
	//public ParticleSystem muzzleFlash;
	Animator anim;
	public AudioClip[] gunSounds;
	int gunSoundsnum = 0;
	float reloadTime = 2f;
	AudioSource audioSource;
	int magzinSize = 30;
	public int bulletsOutMagzin = 90;
	public int bulletsInMagzin = 30;
	float timeSinceShot = 0;
	float minTimeBWShots = 0.1f;
	Transform gun;
	public GameObject impactPrefab;

	GameObject[] impacts;
	int currentImpact = 0;
	int maxImpacts = 5;
	float damageBody = 25f;
	float damageHead = 80f;
	public ParticleEmitter muzzleFlash;
	bool shooting = false;
	bool isAiming = false;
	public bool reloading = false;

	public delegate void Respawn(float respawnTime, bool isBot, bool isPlayerCT, string name);
	public event Respawn RespawnMe;
	public delegate void SendMessage(string message);
	public event SendMessage SendNetworkedMessage;
	// Use this for initialization
	void Start()
	{

		impacts = new GameObject[maxImpacts];
		for (int i = 0; i < maxImpacts; i++)
			impacts[i] = (GameObject)Instantiate(impactPrefab);
		muzzleFlash.emit=false;
		anim = GetComponent<Animator>();
		Debug.Log(anim.GetBool("isAiming"));
		audioSource = GetComponent<AudioSource>();
		gun = GetComponentInChildren<Transform>();
	}

	// Update is called once per frame
	void Update()
	{
		timeSinceShot += Time.deltaTime;
		if (!reloading) isAiming = Input.GetKey(KeyCode.Mouse1);
		anim.SetBool("isAiming", isAiming);

		if (timeSinceShot>=minTimeBWShots){
			if (reloading){
			    if (bulletsOutMagzin >= magzinSize - bulletsInMagzin){
                bulletsOutMagzin -= (magzinSize - bulletsInMagzin);
                bulletsInMagzin = magzinSize;
	            }
	            else{
	                bulletsInMagzin += bulletsOutMagzin;
	                bulletsOutMagzin = 0;
	            }
	        }
			reloading = false;
            anim.SetBool("Reloading", false);
            if (Input.GetKey(KeyCode.Mouse0) && !Input.GetKey(KeyCode.LeftShift) && bulletsInMagzin > 0)
            {
                anim.SetBool("Reloading", false);
                bulletsInMagzin -= 1;
                timeSinceShot = 0f;
                //muzzleFlash.Play();
                if (isAiming)
                {
                    anim.SetTrigger("aimedFire");
                    PlayGunAudio();
                }
                else
                {
                    anim.SetTrigger("blindFire");
                    PlayGunAudio();
                }
                muzzleFlash.Emit();
                shooting = true;
            }
		}

		if (Input.GetKeyDown(KeyCode.R) && bulletsInMagzin<magzinSize && bulletsOutMagzin>0 && !isAiming && !Input.GetKey(KeyCode.LeftShift))
		{
			reloading = true;
			anim.SetBool("Reloading", true);
			timeSinceShot =- reloadTime;
		}

	}

	void FixedUpdate()
	{
		if (shooting)
		{
			shooting = false;

			RaycastHit hit;

			if (Physics.Raycast(gun.position, gun.forward, out hit, 50f))
			{
				//bloodParticles.transform.position = hit.point;
				//bloodParticles.GetComponent<ParticleSystem>().Play();
				if (hit.collider.tag == "Body") {
					impacts [currentImpact].transform.position = hit.point;
					impacts [currentImpact].GetComponent<ParticleSystem> ().Play ();
					if (++currentImpact >= maxImpacts)
						currentImpact = 0;
					
					if (hit.transform.gameObject.GetComponent<PlayerNetworkMover> ().isBot) {
						RPCBots (hit,damageBody);
					}
					else hit.transform.GetComponentInParent<PhotonView>().RPC("GetShot", PhotonTargets.All, damageBody,PhotonNetwork.player.name);
				}
				else if (hit.collider.tag == "Head")
				{
					impacts[currentImpact].transform.position = hit.point;
					impacts[currentImpact].GetComponent<ParticleSystem>().Play();
					if (++currentImpact >= maxImpacts)
						currentImpact = 0;
					if (hit.transform.gameObject.GetComponent<PlayerNetworkMover> ().isBot) {
						RPCBots (hit,damageHead);
					}
					else hit.transform.GetComponentInParent<PhotonView>().RPC("GetShot", PhotonTargets.All, damageHead,PhotonNetwork.player.name);
				}
			}
		}
	}

	void RPCBots(RaycastHit hit,float damage){
		PlayerNetworkMover localPNM = hit.transform.gameObject.GetComponent<PlayerNetworkMover> ();
		string shootingPlayer = PhotonNetwork.player.name;
		string dyingPlayer = hit.transform.gameObject.name;

		localPNM.health -= damage;
		localPNM.AddDamage (shootingPlayer, damage);
		if (localPNM.health <= 0) {
			localPNM.search (shootingPlayer);
			localPNM.sm.ChangeScore (shootingPlayer, "Kills", 1);
			localPNM.sm.ChangeScore (dyingPlayer, "Deaths", 1);
			if (SendNetworkedMessage != null)
				SendNetworkedMessage (dyingPlayer + " was killed by " + shootingPlayer);
			if (RespawnMe != null){
				RespawnMe (3f, true, localPNM.isCT, dyingPlayer);
			}
			PhotonNetwork.Destroy (hit.transform.gameObject.GetComponent<PhotonView> ());
		}
	}

	private void PlayGunAudio()
	{
		audioSource.clip = gunSounds[gunSoundsnum];
		audioSource.PlayOneShot(audioSource.clip);
		gunSoundsnum = (gunSoundsnum + 1) % gunSounds.Length;
	}
}