﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using Cynthia.Card;
using Autofac;
using Assets.Script.Localization;
using UnityEngine.SceneManagement;

////////////////////////////
//Script added by Charli
///////////////////////////
[System.Serializable]
public class Sound
{
    public string name;
    private AudioClip clip;

    [Range(0f, 1f)]
    public float volume = 0.7f;
    [Range(0.5f, 1.5f)]
    public float pitch = 1f;
    [Range(0f, 0.5f)]
    public float randomVolum = 0f;
    [Range(0f, 0.5f)]
    public float randomPitch = 0f;
    public bool loop = false;
    public bool isBGM = false;  //will stop when other hintSound is played

    [HideInInspector]
    public AudioSource source;
    private static string Witcher3BgmDirectory = "Music/Witcher3Bgm/";
    private static string OldGwentBgmDirectory = "Music/OldGwentBgm/";

    public void SetSource(AudioSource _source)
    {
        source = _source;
        source.clip = clip;
        source.loop = loop;
        source.outputAudioMixerGroup = _source.outputAudioMixerGroup;
    }
    public void Play()
    {
        if (clip == null) // check Witcher3BgmDirectory
        {
            clip = Resources.Load<AudioClip>(Witcher3BgmDirectory + name);
        }
        if (clip == null) // check OldGwentBgmDirectory
        {
            clip = Resources.Load<AudioClip>(OldGwentBgmDirectory + name);
        }
        source.clip = clip;
        source.volume = volume * (1 + Random.Range(-randomVolum / 2f, randomVolum / 2f));
        source.pitch = pitch * (1 + Random.Range(-randomPitch / 2f, randomPitch / 2f));
        source.Play();
    }
    public void Stop()
    {
        source.Stop();
    }
}


public class BGMManager : MonoBehaviour
{
    [SerializeField]
    public Sound[] soundList;

    public static BGMManager instance;
    public AudioMixer audioMixer;
    public AudioMixerGroup groupMusic;
    public float bgmChangeSpeed = 0.01f;


    private float countChangeSpeed = 0;
    private int lastCardselectBgm = -1;
    //private GameObject tempObject = null;
    private LeaderCard leaderCard = null;
    //private CardShowInfo card = null;

    //private CardStatus cardStat = null;

    private GameObject editorUI = null;
    private GameObject matchButton = null;
    private GameObject enenmyLeader = null;
    private GameObject smallRoundUI = null;
    private GameObject resultUI = null;
    private GameObject resultPage = null;

    private bool inCardselect = false;
    private bool inGameplay = false;
    //private bool inSmallround = false;
    private bool inLine = false;
    private bool inResult = false;

    private int faction = -1;

    private string tempString;
    private Text tempText;
    private Sound lerpSound = null;
    private Sound playingSound = null;
    private static float defaultVolum = 0.0f;
    private static float tempVolum = 0.0f;
    private bool inGame = false;
    private LocalizationService translator;
    private Scene scene;
    /*
        All = 0, Cardselect
        Neutral = 1, Witcher3
        Monsters = 2,
        Nilfgaard = 3,
        NorthernRealms = 4,
        ScoiaTael = 5,
        Skellige = 6,
        Vectory = 7,
        Defeat = 8,
        Middle = 9
     */


    private string[,] BGMs =
    {
        {
        "Cardselect01",
        "Cardselect02",
        "Cardselect03",
        "Cardselect04",
        " ",
        " ",
        " ",
        ""
        },{
        "Another Round For Everyone",
        "A Story You Won't Believe",
        "Back On the Path",
        "Drink Up, There’s More!",
        "Searching for Cecilia Bellant",
        "The Mandragora",
        "The Nightingale",
        "The Musty Scent of Fresh Pâté"
        },{
        "Monsters01",
        "Monsters02",
        "Monsters03",
        "Monsters04",
        " ",
        " ",
        " ",
        ""
        },{
        "Nilfgaardian01",
        "Nilfgaardian02",
        "Nilfgaardian03",
        "Nilfgaardian04",
        " ",
        " ",
        " ",
        ""
        },{
        "Northern Realms01",
        "Northern Realms02",
        "Northern Realms03",
        " ",
        " ",
        " ",
        " ",
        ""
        },{
        "Scoiatael01",
        "Scoiatael02",
        "Scoiatael03",
        " ",
        " ",
        " ",
        " ",
        ""
        },{
        "Skellige01",
        "Skellige02",
        "Skellige03",
        " ",
        " ",
        " ",
        " ",
        ""
        },{
        "Vectory01",
        "Vectory02",
        "Vectory03",
        "Vectory04",
        "Vectory05",
        " ",
        " ",
        ""
        },{
        "Defeat01",
        "Defeat02",
        "Defeat03",
        "Defeat04",
        "Defeat05",
        " ",
        " ",
        ""
        },{
        "Middle01",
        "Middle02",
        "Middle03",
        " ",
        " ",
        " ",
        " ",
        ""
        }
    };


    //public ChoseValue _chosevalue;

    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogError("More than on audiomanager!");
        }
        else
        {
            instance = this;
        }
    }

    private void Start()
    {
        GameObject.DontDestroyOnLoad(gameObject);
        translator = DependencyResolver.Container.Resolve<LocalizationService>();
        for (int s = 0; s < soundList.Length; s++)
        {
            GameObject _go = new GameObject("Sound_" + s + "_" + soundList[s].name);
            _go.transform.SetParent(this.transform);
            _go.AddComponent<AudioSource>();
            _go.GetComponent<AudioSource>().outputAudioMixerGroup = groupMusic;
            soundList[s].SetSource(_go.GetComponent<AudioSource>());
        }
    }

    private void Update()
    {
        if (playingSound != null && playingSound.source.isPlaying == false)//判断音乐停止
        {
            inCardselect = false;
            inGameplay = false;
            //inSmallround = false;
            inLine = false;
            inResult = false;
        }

        //检查是否进入登录界面
        if (inGame == false)
        {
            scene = SceneManager.GetActiveScene();
            if (scene.name == "LoginScene")
            {
                PlaySound("Title");
                inGame = true;
            }
        }

        //检查是否进入卡组编辑器
        if (editorUI != null && editorUI.activeSelf == true)
        {
            if (inCardselect == false)
            {
                //Debug.Log("进入【卡组编辑器】");
                inCardselect = true;
                PlayBGM(0);
            }
        }
        else
        {
            if (inCardselect == true)
            {
                //Debug.Log("退出【卡组编辑器】");
                inCardselect = false;
                PlaySound("Title");
            }
        }

        //检查是否进入匹配队列
        if (matchButton != null && matchButton.activeSelf == true)//确保有button
        {
            tempText = matchButton.GetComponentInChildren<Text>();
            tempString = tempText.text;
            if (inLine == false)
            {
                if (tempString == translator.GetText("MatchmakingMenu_CancelButton"))
                {
                    //Debug.Log("进入【匹配队列】");
                    inLine = true;
                    PlaySound("Inline");
                }
            }
            else
            {
                if (tempString == translator.GetText("MatchmakingMenu_PlayButton"))
                {
                    //Debug.Log("退出【匹配队列】");
                    inLine = false;
                    PlaySound("Title");
                }
            }
        }

        //if (inGameplay == true)
        //    Debug.Log("对局进行");
        //检查是否进入战斗
        if (enenmyLeader != null && enenmyLeader.activeSelf == true)
        {
            if (inGameplay == false)
            {
                leaderCard = enenmyLeader.GetComponent<LeaderCard>();
                if (leaderCard != null && leaderCard.TrueCard != null)
                {
                    if (leaderCard.TrueCard != null)
                    {
                        faction = leaderCard.TrueCard.GetComponent<CardShowInfo>().CurrentCore.CardInfo.Faction.GetHashCode();
                        //Debug.Log("进入【战斗】");
                        inGameplay = true;
                        int isWitcher = PlayerPrefs.GetInt("isWitcher");
                        if (isWitcher == 1)
                        {
                            PlayBGM(1);
                        }
                        else
                        {
                            PlayBGM(faction);
                        }
                    }

                }
            }
        }
        else
        {
            inGameplay = false;
        }
        //检查是否需要连播
        if (faction != -1)
        {
            if (inGameplay == false)
            {
                inGameplay = true;
                int isWitcher = PlayerPrefs.GetInt("isWitcher");
                if (isWitcher == 1)
                {
                    PlayBGM(1);
                }
                else
                {
                    PlayBGM(faction);
                }
            }
        }

        //检查是否触发小局结束
        /*
        if (smallRoundUI != null && smallRoundUI.activeSelf == true)
        {
            if (inSmallround == false)
            {
                Debug.Log("进入【小局】");
                inSmallround = true;
                //PlayBGM(9);
            }
        }
        else
        {
            if (inSmallround == true)
            {
                Debug.Log("退出【小局】");
                inSmallround = false;
                //PlaySound("Title");
                PlayBGM(faction);
            }
        }
        */
        //检查是否触发对局结算
        if (resultPage != null && resultPage.activeSelf == true)
        {
            faction = -1;
            tempText = resultUI.GetComponent<Text>();
            tempString = tempText.text;

            if (inResult == false)
            {
                faction = -1;
                if (tempString == translator.GetText("IngameMenu_VictoryTitle"))
                {
                    //Debug.Log("进入【胜利】");
                    inResult = true;
                    PlayBGM(7);
                }
                else if (tempString == translator.GetText("IngameMenu_DefeatTitle"))
                {
                    //Debug.Log("进入【失败】");
                    inResult = true;
                    PlayBGM(8);
                }
                else if (tempString == translator.GetText("IngameMenu_DrawTitle"))
                {
                    //Debug.Log("进入【平局】");
                    inResult = true;
                    PlayBGM(9);
                }
            }
        }

        else
        {
            if (inResult == true)
            {
                //Debug.Log("退出【结算】");
                inResult = false;
                PlaySound("Title");
            }
        }
    }

    private void FixedUpdate()
    {
        if (lerpSound != null) //给一个短暂的声音淡出效果
        {
            tempVolum = SettingPanel.LinearToDecibel(
                            Mathf.Lerp(SettingPanel.DecibelToLinear(defaultVolum),
                                        SettingPanel.DecibelToLinear(-80),
                                        countChangeSpeed));
            countChangeSpeed += bgmChangeSpeed;
            audioMixer.SetFloat("musicVolum", tempVolum);
            if (tempVolum < -70)
            {
                lerpSound.Stop();
                lerpSound = null;
                audioMixer.SetFloat("musicVolum", defaultVolum);
                countChangeSpeed = 0f;
            }
        }
        else
        {
            audioMixer.GetFloat("musicVolum", out defaultVolum);
        }
    }

    public void SetObject(GameObject temp, int num)
    {
        switch (num)
        {
            //private GameObject editorUI = null;
            //private GameObject matchButton = null;
            //private GameObject enenmyLeader = null;
            //private GameObject smallRoundUI = null;
            //private GameObject bigRound = null;
            //private GameObject resultUI = null;
            //private GameObject resultPage = null;

            case 1:
                editorUI = temp;
                break;
            case 2:
                matchButton = temp;
                break;
            case 3:
                enenmyLeader = temp;
                break;
            case 4:
                smallRoundUI = temp;
                break;
            case 5:
                resultUI = temp;
                break;
            case 6:
                resultPage = temp;
                break;

            default: /* 可选的 */
                break;
        }
    }

    public void PlaySound(string _name)
    {
        bool findFlag = false;

        for (int s = 0; s < soundList.Length; s++)
        {
            if (soundList[s].name == _name)
            {
                if (soundList[s].isBGM == true)  // stop all other BGM
                {
                    if (lerpSound != null) // 避免还有正在淡出的曲子
                    {
                        lerpSound.Stop();
                    }
                    if (soundList[s] != playingSound) // 避免连续播放同一首会杀死自己
                    {
                        lerpSound = playingSound;
                    }
                    else
                    {
                        lerpSound = null;
                    }
                    playingSound = soundList[s];
                }
                soundList[s].Play();
                findFlag = true;
                break;
            }
        }

        if (!findFlag)  //Not found
        {
            Debug.LogError("Sound missing" + _name);
        }
    }
    public void StopSound(string _name)
    {

        for (int s = 0; s < soundList.Length; s++)
        {
            if (soundList[s].name == _name)
            {
                soundList[s].Stop();
                return;
            }
        }
        //Not found
        Debug.LogError("Sound missing" + _name);
    }
    public void PlayBGM(int group)
    {
        int len = -1;

        for (int s = 0; s < 8; s++)
        {
            len++;
            if (BGMs[group, s] == " ")
            {
                break;
            }
        }
        int num = Random.Range(0, len); //0,1,2,3

        if (num == lastCardselectBgm) //不连续重复
        {
            num = (num + 1) % len;
        }
        lastCardselectBgm = num;
        PlaySound(BGMs[group, num]);
    }
}
