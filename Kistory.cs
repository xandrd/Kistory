﻿using System;
using System.Collections;
using UnityEngine;
using Toolbar;

namespace Kistory
{
    [KSPAddon(KSPAddon.Startup.MainMenu, true)]  
    public class Kistory : MonoBehaviour
    {
        // Main class it is a wrapper for plugin
        
        private static ReportManager report; // Singlton that works with reports, missions, messages
        private WindowManager _windows;      // Draw window and button for toolbar
        

        /////  not in use at the moment...
        private DateTime eventTime;
        private String str_eventTime()
        {
            return eventTime.Year.ToString() + eventTime.Month.ToString() + eventTime.Day.ToString() + eventTime.Hour.ToString() + eventTime.Minute.ToString() + eventTime.Second.ToString();
        }
        private void make_screenshot(String type)
        {
            String letter;
            String fileName;

            this.eventTime = DateTime.Now;

            switch(type){
                 case "Launch":
                    letter = "l";
                    break;
                case "EVA":
                    letter = "e";
                    break;
              default:            
                    letter = "O";
                    break;
            }

            fileName = this.str_eventTime() + letter + ".png";

            Debug.Log("[Kistory]" + type + ": " + fileName);
            
            StartCoroutine( delay_screenshot(2, fileName) ); 
            
        }
        IEnumerator delay_screenshot(float waitTime, String fileName)
        {
            yield return new WaitForSeconds(waitTime);
            Application.CaptureScreenshot(fileName);
        }
        ////

        // Run once at the mode loading
        public void Awake()            
        {
            // it might help
            //ReportManager report = gameObject.AddComponent<ReportManager>();

            report = ReportManager.Instance(); // Call the instance
            report.Kistory = this;

            _windows = new WindowManager();
            
            this.eventTime = DateTime.Now; // ?

            Debug.Log("[Kistory] Awake");
        }

        // Load before the first frame
        public void Start()
        {
            DontDestroyOnLoad(this); // We will try to prevent the mode restart
        }

 
        // Calls if this.enabled = true;
        public void OnEnable()
        {
            
        }

        // Calls id this.enabled = false;
        public void OnDisable()
        { 
        
        }

        // Calls on each step
        void Update()
        {

        }

        // Calls on each draw step, more often than Update()
        void OnGUI()
        {            
            _windows.OnDraw();
        }

        // .. dispose
        void OnDestroy()
        {
            Debug.Log("[Kistory] OnDestroy");

            //if(this._windows != null)
            //   this._windows.Destroy();

            Debug.Log("[Kistory] OnDestroy events");
            if (report != null)
                report.clear();
        }

        public void ShowWindow() 
        {
            Debug.Log("[Kistory] ShowWindow");
            _windows.Show();
        }

        public void CloseWindow() 
        {
            Debug.Log("[Kistory] CloseWindow");
            _windows.Close();
        }

        // Corutine add message
        private IEnumerator add_delayed_message(EntryCorutine data)
        {
            Debug.Log("[Kistory] pre add_delayed_message");

            float waitTime = 3;
            //this._situationRunning = true;
            yield return new WaitForSeconds(waitTime);
            Debug.Log("[Kistory] post add_delayed_message");
            report.add_situation_message(data.ves, data.message, data.situation);
        }

        // Corutine add message and photo
        // This function is not finished yet!!!! 
        private IEnumerator add_delayed_message_and_photo(EntryCorutine data)
        {
            Debug.Log("[Kistory] pre add_delayed_message_and_photo");

            String fileName;
            float waitTime = 3;
            //this._situationRunning = true;
            yield return new WaitForSeconds(waitTime);
            Debug.Log("[Kistory] post add_delayed_message_and_photo");
            report.add_situation_message(data.ves, data.message, data.situation);

            // We are serching for right filepath!
            // The name of the screenshot  should be constructed!
            fileName = Application.dataPath + HighLogic.SaveFolder + "/screen.png";
            Debug.Log("[Kistory] filename");
            Application.CaptureScreenshot(fileName);
        }



    }


}
