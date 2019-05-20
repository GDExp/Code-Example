using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Analytics;


public class GameAnalytics : MonoBehaviour
{
    public static GameAnalytics init;

    private string user_name;
    private string user_mobile_model;

    private int all_dead;
    private float time_lvl_start;


    private void Awake()
    {
        if (init is null)
        {
            init = this;
            DontDestroyOnLoad(gameObject);
        }
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        user_name = "user_" + SystemInfo.deviceUniqueIdentifier;
        user_mobile_model = "mobile_" + SystemInfo.deviceModel;
        CMDUnityAnalytics(0);
    }
    
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            ExitApplication();

        if (Application.platform == RuntimePlatform.Android && Input.GetKeyUp(KeyCode.Escape))
        {
            AndroidJavaObject activity = new AndroidJavaClass("com.unity3d.player.UnityPlayer").GetStatic<AndroidJavaObject>("currentActivity");
            activity.Call<bool>("moveTaskToBack", true);
            ExitApplication();
        }
    }

    private void ExitApplication()
    {
        Application.Quit();
    }

    #region UNITY ANALYTICS

    public void CMDUnityAnalytics(int id)
    {
        switch (id)
        {
            case (1)://start
                EventStartLevel();
                break;
            case (2)://end
                EventEndLevel();
                break;
            case (3)://restart
                RestartLvl();
                break;

            case (777)://kill boss
                KillBoss();
                break;
                
            default://link user
                LinkUserAnalytics();
                break;
        }
    }

    public void EventStartLvlInMainMenu(int index)
    {
        time_lvl_start = Time.time;
        all_dead = 0;
        Analytics.CustomEvent(user_name, new Dictionary<string, object>
        {
            {"Level - ", index }
        });
    }

    private void LinkUserAnalytics()
    {
        Analytics.CustomEvent("new_user", new Dictionary<string, object>
        {
            {"ID - ",user_name },
            {"Model - ",user_mobile_model }
        });
    }

    private void RestartLvl()
    {
        all_dead++;
    }

    private void EventStartLevel()
    {
        time_lvl_start = Time.time;
        all_dead = 0;
        int lvl = 1;
        if (GameController.init != null & GameController.init.current_lvl_id != 0)
            lvl = GameController.init.current_lvl_id + 1;
        Analytics.CustomEvent(user_name, new Dictionary<string, object>
        {
            {"Level - ", lvl }
        });
    }

    private void EventEndLevel()
    {
        float time_to_end = Time.time - time_lvl_start;

        Analytics.CustomEvent(user_name, new Dictionary<string, object>
        {
            {"Level - ", GameController.init.current_lvl_id },
            {"Dead - ", all_dead },
            {"Time - ", time_to_end }
        });

        all_dead = 0;
    }

    private void KillBoss()
    {
        Analytics.CustomEvent(user_name, new Dictionary<string, object>
        {
            {"Medusa was dead",true },
            {"Dead - ",all_dead }
        });
    }

    #endregion
}
