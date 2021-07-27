using BepInEx;
using BepInEx.Configuration;
using COM3D2.LillyUtill;
using COM3D2.PoseStreamLilly.Plugin;
using COM3D2API;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace COM3D2.PoseStreamLilly.Plugin
{
    class PoseStreamLillyGUI : MonoBehaviour
    {
        public static PoseStreamLillyGUI instance;

        private static ConfigFile config;

        private static ConfigEntry<BepInEx.Configuration.KeyboardShortcut> ShowCounter;

        private static int windowId = new System.Random().Next();

        private static Vector2 scrollPosition;

        // 위치 저장용 테스트 json
        public static MyWindowRect myWindowRect;

        public static bool IsOpen
        {
            get => myWindowRect.IsOpen;
            set => myWindowRect.IsOpen = value;            
        }

        // GUI ON OFF 설정파일로 저장
        private static ConfigEntry<bool> IsGUIOn;

        public static bool isGUIOn
        {
            get => IsGUIOn.Value;
            set => IsGUIOn.Value = value;
        }

        internal static PoseStreamLillyGUI Install(GameObject parent,ConfigFile config)
        {
            PoseStreamLillyGUI. config = config;
            instance = parent.GetComponent<PoseStreamLillyGUI>();
            if (instance == null)
            {
                instance = parent.AddComponent<PoseStreamLillyGUI>();
                PoseStreamLilly.myLog.LogMessage("GameObjectMgr.Install", instance.name);                
            }
            return instance;
        }

        public void Awake()
        {
            myWindowRect = new MyWindowRect(config, MyAttribute.PLAGIN_FULL_NAME, MyAttribute.PLAGIN_NAME, "PS", ho:204);
            IsGUIOn = config.Bind("GUI", "isGUIOn", false);
            ShowCounter = config.Bind("GUI", "isGUIOnKey", new BepInEx.Configuration.KeyboardShortcut(KeyCode.Alpha9, KeyCode.LeftControl));           
            
        }

        public void OnEnable()
        {
            PoseStreamLillyGUI.myWindowRect.load();
            SceneManager.sceneLoaded += this.OnSceneLoaded;
        }

        /*
        public void Start()
        {
            MyLog.LogMessage("Start");            
        }
        */

        public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {            
            PoseStreamLillyGUI.myWindowRect.save();
        }

        private void Update()
        {
            //if (ShowCounter.Value.IsDown())
            //{
            //    MyLog.LogMessage("IsDown", ShowCounter.Value.Modifiers, ShowCounter.Value.MainKey);
            //}
            //if (ShowCounter.Value.IsPressed())
            //{
            //    MyLog.LogMessage("IsPressed", ShowCounter.Value.Modifiers, ShowCounter.Value.MainKey);
            //}
            if (ShowCounter.Value.IsUp())
            {
                isGUIOn = !isGUIOn;
                PoseStreamLilly.myLog.LogMessage("IsUp", ShowCounter.Value.Modifiers, ShowCounter.Value.MainKey);
            }
        }

        public void OnGUI()
        {
            if (!isGUIOn)
                return;

            //GUI.skin.window = ;

            //myWindowRect.WindowRect = GUILayout.Window(windowId, myWindowRect.WindowRect, WindowFunction, MyAttribute.PLAGIN_NAME + " " + ShowCounter.Value.ToString(), GUI.skin.box);
            myWindowRect.WindowRect = GUILayout.Window(windowId, myWindowRect.WindowRect, WindowFunction, "", GUI.skin.box);
        }

        
        private String resultMessage = "";

        public void WindowFunction(int id)
        {
            GUI.enabled = true;

            GUILayout.BeginHorizontal();
            GUILayout.Label(myWindowRect.windowName, GUILayout.Height(20));
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("-", GUILayout.Width(20), GUILayout.Height(20))) { IsOpen = !IsOpen; }
            if (GUILayout.Button("x", GUILayout.Width(20), GUILayout.Height(20))) { isGUIOn = false; }
            GUILayout.EndHorizontal();

            if (!IsOpen)
            {

            }
            else
            {
                scrollPosition = GUILayout.BeginScrollView(scrollPosition);

                PoseStreamLillyUtill.anmName = GUILayout.TextField(PoseStreamLillyUtill.anmName);
                GUILayout.Label("이름(*_00000000의*부분)을 입력하십시오. \n 이미 생성 된 파일이 있으면 덮어 씁니다.test_00000000파일인 경우 test입력");

                GUILayout.BeginHorizontal();
                if (GUILayout.Button("anime anm 생성"))
                {
                    // anmMake 호출
                    resultMessage = PoseStreamLillyUtill.anmMake(false);
                }
                // 생성 버튼 클릭시
                if (GUILayout.Button("mid anm 생성"))
                {
                    // anmMake 호출
                    resultMessage = PoseStreamLillyUtill.anmMake(true);
                }
                GUILayout.EndHorizontal();

                GUILayout.Label(resultMessage);

                GUILayout.EndScrollView();

                if (GUI.changed)
                {
                    Debug.Log("changed");
                }

            }
            GUI.enabled = true;
            GUI.DragWindow(); // 창 드레그 가능하게 해줌. 마지막에만 넣어야함
        }

        public void OnDisable()
        {

            PoseStreamLillyGUI.myWindowRect.save();
            SceneManager.sceneLoaded -= this.OnSceneLoaded;
        }


    }
}
