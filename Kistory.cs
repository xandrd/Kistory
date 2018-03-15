using System;
using System.IO;
using System.Collections;
using UnityEngine;

namespace Kistory
{
    
    [KSPAddon(KSPAddon.Startup.AllGameScenes, false)]     
    public class Kistory : MonoBehaviour
    {
        // Main class it is a wrapper for plugin
        
        private ReportManager report;   // Not-Singlton that works with reports, missions, messages
        private WindowManager windows; // Draw window and button for toolbar

        // Run once at the mode loading
        public void Awake()            
        {
            // it might help
            //ReportManager report = gameObject.AddComponent<ReportManager>();

            //this.report = new ReportManager(); // Create the instance
            //this.report.kistory = this; // We need this for corutines
            //this.windows = new WindowManager(report); // we need this to draw interface

            report = gameObject.AddComponent<ReportManager>();
            windows = gameObject.AddComponent<WindowManager>();

            KDebug.Log("Awake", KDebug.Type.MONO);
        }

        // Load before the first frame
        public void Start()
        {
            KDebug.Log("Start", KDebug.Type.MONO);
            //gameObject.AddComponent<Kistory>(); // This is taken from AssemblyReloader manual
            // DontDestroyOnLoad(this); // We will try to prevent the mode restart
        }

        // Calls on each draw step, more often than Update()
        void OnGUI()
        {
            //this.windows.OnDraw();
        }

        // .. dispose
        void OnDestroy()
        {
            KDebug.Log("OnDestroy", KDebug.Type.MONO);

           /* if(windows != null)
               windows.Close();

            KDebug.Log("OnDestroy events", KDebug.Type.MONO);
            if (report != null)
                report.clear();*/

            //Destroy(gameObject); // Cleanup. This is taken from AssemblyReloader manual //?
        }

        #region not_in_use                
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
        /////  not in use at the moment...
        // Calls if this.enabled = true;
        /*
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

            KDebug.Log("" + type + ": " + fileName);
            
            StartCoroutine( delay_screenshot(2, fileName) ); 
            
        }
        private IEnumerator delay_screenshot(float waitTime, String fileName)
        {
            yield return new WaitForSeconds(waitTime);
            Application.CaptureScreenshot(fileName);
        }
       */
        #endregion not_in_use
    }


}
