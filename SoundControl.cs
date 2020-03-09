using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundControl : SingletonMonobehavior<SoundControl> {

	public float sfxVolume = 1.0f;
	public bool isSfxMute = false;
	
	
	public void Volume(float volume)
	{
		sfxVolume = volume;
	}


	 public void PlayerSfx(Vector3 pos, AudioClip sfx, float volbalance =1.0f)
	{
		if (isSfxMute) return;
		GameObject soundObj = new GameObject("Sfx");
		//동적할당 동적 생성
		soundObj.transform.position = pos;
		AudioSource audioSource = soundObj.AddComponent<AudioSource>();

		audioSource.clip = sfx;
		audioSource.minDistance = 10f;
		audioSource.maxDistance = 30f;
		audioSource.volume = sfxVolume * volbalance;
		audioSource.Play();
		Destroy(soundObj, sfx.length);
	}

}
