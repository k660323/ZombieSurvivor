using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class SoundManager : MonoBehaviour
{
    [Header("사운드 믹서")]
    public AudioMixer mixer;

    [Header("배경음 리스트")]
    public AudioClip[] bglist;

    [Header("배경 사운드 플레이어")]
    public AudioSource bgSound;

    public static SoundManager instance;

    // Start is called before the first frame update
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(instance);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
    }

    void OnSceneLoaded(Scene arg0,LoadSceneMode arg1)
    {
        for(int i=0; i< bglist.Length;i++)
        {
            if (arg0.name == bglist[i].name)
                BgSoundPlay(bglist[i]);
        }
    }

    #region 볼륨 조절
    public void BGSoundVolume(float val)
    {
        if (val == -30f)
            mixer.SetFloat("BGVolume", -80);
        else
            mixer.SetFloat("BGVolume", val);
    }

    public void SFXVolume(float val)
    {
        if (val == -30f)
            mixer.SetFloat("SFXVolume", -80);
        else
            mixer.SetFloat("SFXVolume", val);
    }
    #endregion

    #region 배경
    public void BgSoundPlay(AudioClip clip)
    {
        bgSound.clip = clip;
        bgSound.loop = true;
        bgSound.Play();
    }
    #endregion

    #region 효과음

    public void SFXPlay(AudioClip clip)
    {
        GameObject go = new GameObject("UISound");
        AudioSource audioSource = go.AddComponent<AudioSource>();
        audioSource.outputAudioMixerGroup = mixer.FindMatchingGroups("SFXVolume")[0];
        audioSource.clip = clip;
        audioSource.Play();

        Destroy(go, clip.length);
    }

    public void SFXPlay(AudioClip clip,string sfxName)
    {
        GameObject go = new GameObject(sfxName + "Sound");
        AudioSource audioSource = go.AddComponent<AudioSource>();
        audioSource.outputAudioMixerGroup = mixer.FindMatchingGroups("SFXVolume")[0];
        audioSource.clip = clip;
        audioSource.Play();

        Destroy(go, clip.length);
    }

    public void SFXPlay(AudioClip clip, string sfxName,float speed)
    {
        GameObject go = new GameObject(sfxName + "Sound");
        AudioSource audioSource = go.AddComponent<AudioSource>();
        audioSource.outputAudioMixerGroup = mixer.FindMatchingGroups("SFXVolume")[0];
        audioSource.pitch = speed;
        audioSource.clip = clip;
        audioSource.Play();

        Destroy(go, clip.length);
    }


    public void SFXPlay3D(AudioClip clip, string sfxName,Transform pos)
    {
        GameObject go = new GameObject(sfxName + "Sound");
        go.transform.parent.position = pos.position;
        AudioSource audioSource = go.AddComponent<AudioSource>();
        audioSource.clip = clip;
        audioSource.Play();

        Destroy(go, clip.length);
    }

    #endregion
}
