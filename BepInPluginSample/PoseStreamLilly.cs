using BepInEx;
using BepInEx.Configuration;
using COM3D2.LillyUtill;
using COM3D2API;
using HarmonyLib;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace COM3D2.PoseStreamLilly.Plugin
{
    class MyAttribute
    {
        public const string PLAGIN_NAME = "PoseStreamLilly";
        public const string PLAGIN_VERSION = "21.7.27";
        public const string PLAGIN_FULL_NAME = "COM3D2.PoseStreamLilly.Plugin";
    }

    [BepInPlugin(MyAttribute.PLAGIN_FULL_NAME, MyAttribute.PLAGIN_FULL_NAME, MyAttribute.PLAGIN_VERSION)]// 버전 규칙 잇음. 반드시 2~4개의 숫자구성으로 해야함. 미준수시 못읽어들임
    //[BepInPlugin("COM3D2.Sample.Plugin", "COM3D2.Sample.Plugin", "21.6.6")]// 버전 규칙 잇음. 반드시 2~4개의 숫자구성으로 해야함. 미준수시 못읽어들임
    [BepInProcess("COM3D2x64.exe")]
    public class PoseStreamLilly : BaseUnityPlugin
    {
        // 단축키 설정파일로 연동
        private ConfigEntry<BepInEx.Configuration.KeyboardShortcut> ShowCounter;

        public static MyLog myLog;

        public static PoseStreamLilly sample;

        public PoseStreamLilly()
        {
            sample = this;
        }

        /// <summary>
        ///  게임 실행시 한번만 실행됨
        /// </summary>
        public void Awake()
        {
            myLog = new MyLog(MyAttribute.PLAGIN_NAME);
            myLog.LogMessage("Awake");

            // 단축키 기본값 설정
            ShowCounter = Config.Bind("KeyboardShortcut", "KeyboardShortcut0", new BepInEx.Configuration.KeyboardShortcut(KeyCode.Alpha9, KeyCode.LeftControl));

            

            // 기어 메뉴 추가. 이 플러그인 기능 자체를 멈추려면 enabled 를 꺽어야함. 그러면 OnEnable(), OnDisable() 이 작동함
        }



        public void OnEnable()
        {
            myLog.LogMessage("OnEnable");

            SceneManager.sceneLoaded += this.OnSceneLoaded;



        }

        /// <summary>
        /// 게임 실행시 한번만 실행됨
        /// </summary>
        public void Start()
        {
            myLog.LogMessage("Start");

            PoseStreamLillyGUI.Install(gameObject, Config);

            //SystemShortcutAPI.AddButton(MyAttribute.PLAGIN_FULL_NAME, new Action(delegate () { enabled = !enabled; }), MyAttribute.PLAGIN_NAME, MyUtill.ExtractResource(COM3D2.PoseStreamLilly.Plugin.Properties.Resources.icon));
            SystemShortcutAPI.AddButton(MyAttribute.PLAGIN_FULL_NAME, new Action(delegate () {
                myLog.LogMessage("SystemShortcutAPI.AddButton", MyAttribute.PLAGIN_FULL_NAME,PoseStreamLillyGUI.isGUIOn);
                PoseStreamLillyGUI.isGUIOn = !PoseStreamLillyGUI.isGUIOn; 
            }), MyAttribute.PLAGIN_NAME + " : " + ShowCounter.Value.ToString(), MyUtill.ExtractResource(COM3D2.PoseStreamLilly.Plugin.Properties.Resources.icon));
        }

        public string scene_name = string.Empty;

        public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            myLog.LogMessage("OnSceneLoaded", scene.name, scene.buildIndex);
            //  scene.buildIndex 는 쓰지 말자 제발
            scene_name = scene.name;
        }

        public void FixedUpdate()
        {

        }

        public void Update()
        {
            if (ShowCounter.Value.IsDown())
            {
                myLog.LogMessage("IsDown", ShowCounter.Value.Modifiers, ShowCounter.Value.MainKey);
            }
            if (ShowCounter.Value.IsPressed())
            {
                myLog.LogMessage("IsPressed", ShowCounter.Value.Modifiers, ShowCounter.Value.MainKey);
            }
            if (ShowCounter.Value.IsUp())
            {
                myLog.LogMessage("IsUp", ShowCounter.Value.Modifiers, ShowCounter.Value.MainKey);
            }
        }

        public void LateUpdate()
        {

        }

        

        public void OnGUI()
        {
          
        }



        public void OnDisable()
        {
            myLog.LogMessage("OnDisable");

            SceneManager.sceneLoaded -= this.OnSceneLoaded;

        }

        public void Pause()
        {
            myLog.LogMessage("Pause");
        }

        public void Resume()
        {
            myLog.LogMessage("Resume");
        }





    }
}
