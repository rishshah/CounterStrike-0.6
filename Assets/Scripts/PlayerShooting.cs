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
	bool reloading = false;

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

		if (Input.GetKey(KeyCode.Mouse0) && !Input.GetKey(KeyCode.LeftShift) && timeSinceShot>=minTimeBWShots && bulletsInMagzin>0)
		{
			reloading = false;
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
			muzzleFlash.Emit ();
			shooting = true;
		}

		if (Input.GetKeyDown(KeyCode.R) && bulletsInMagzin<magzinSize && bulletsOutMagzin>0 && !isAiming && !Input.GetKey(KeyCode.LeftShift))
		{
			reloading = true;
			anim.SetBool("Reloading", true);
			timeSinceShot =- reloadTime;
			if (bulletsOutMagzin >= magzinSize-bulletsInMagzin)
			{
				bulletsOutMagzin -= (magzinSize - bulletsInMagzin);
				bulletsInMagzin = magzinSize;
			}
			else
			{
				bulletsInMagzin += bulletsOutMagzin;
				bulletsOutMagzin = 0;
			}
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
				if (hit.collider.tag == "Body")
				{
					impacts[currentImpact].transform.position = hit.point;
					impacts[currentImpact].GetComponent<ParticleSystem>().Play();
					if (++currentImpact >= maxImpacts)
						currentImpact = 0;
					hit.transform.GetComponentInParent<PhotonView>().RPC("GetShot", PhotonTargets.All, damageBody,PhotonNetwork.player.name);
				}
				else if (hit.collider.tag == "Head")
				{
					impacts[currentImpact].transform.position = hit.point;
					impacts[currentImpact].GetComponent<ParticleSystem>().Play();
					if (++currentImpact >= maxImpacts)
						currentImpact = 0;
					hit.transform.GetComponentInParent<PhotonView>().RPC("GetShot", PhotonTargets.All, damageHead,PhotonNetwork.player.name);
				}
			}
		}
	}

	private void PlayGunAudio()
	{
		audioSource.clip = gunSounds[gunSoundsnum];
		audioSource.PlayOneShot(audioSource.clip);
		gunSoundsnum = (gunSoundsnum + 1) % gunSounds.Length;
	}
}