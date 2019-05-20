using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public static MusicManager init;

    //music player
    public AudioClip[] music_clips;
    private AudioSource music_source;

    //sounds
    private Transform sounds;

    private float music_value;
    [SerializeField]
    private float sound_value;

    private void Awake()
    {
        if (init is null)
        {
            init = this;
            DontDestroyOnLoad(gameObject);
        }
        else
            Destroy(gameObject);

        music_source = GetComponent<AudioSource>();
        music_source.volume = 0.5f;
        music_source.clip = music_clips[0];//main menu
        music_source.Play();//*/

        sounds = transform.GetChild(0);
        sound_value = 0.5f;
        for (int i = 0; i < sounds.childCount; ++i)
            sounds.GetChild(i).GetComponent<AudioSource>().volume = sound_value;
    }
    

    private void OnLevelWasLoaded(int level)
    {

        //0 - main menu
        //1 - all level, no boss
        //2 - medusa - boss

        if (level == 3)
            music_source.clip = music_clips[2];
        else
            music_source.clip = music_clips[1];
        if (level == 0)
            music_source.clip = music_clips[0];
        music_source.Play();
        Screen.orientation = ScreenOrientation.Landscape;
    }

    public void SetMusicValue()
    {
        music_value = PlayerPrefs.GetFloat("Music");
        music_source.volume = music_value;

    }

    public void SetSoundValue()
    {
        sound_value = PlayerPrefs.GetFloat("Sound");
        for (int i = 0; i < sounds.childCount; ++i)
            sounds.GetChild(i).GetComponent<AudioSource>().volume = sound_value;
        PlaySoundClip(0);//test current change
    }

    public void PlaySoundClip(byte index)
    {
        //0 - fire
        //1 - reload
        //2 - flesh
        //3 - bonus
        //4 - player hit
        //5 - use trigger
        sounds.GetChild(index).GetComponent<AudioSource>().Play();
    }
    


}
