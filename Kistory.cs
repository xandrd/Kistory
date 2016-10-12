using System;
using System.IO;
using System.Collections;
using UnityEngine;

namespace Kistory
{
    [KSPAddon(KSPAddon.Startup.MainMenu, true)]  
    public class Kistory : MonoBehaviour
    {
        // Main class it is a wrapper for plugin
        
        private static ReportManager report; // Singlton that works with reports, missions, messages
        private WindowManager _windows;      // Draw window and button for toolbar
        

        /////  not in use at the moment...
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
        //screeshot message, ass a second so it not interfier with the game
        private DateTime eventTime;
        private String str_eventTime()
        {
            eventTime = DateTime.Now;
            return eventTime.Year.ToString() + eventTime.Month.ToString() + eventTime.Day.ToString() + eventTime.Hour.ToString() + eventTime.Minute.ToString() + eventTime.Second.ToString();
        }
        private IEnumerator delayed_message(String message)
        {
            yield return new WaitForSeconds(1f);
            ScreenMessages.PostScreenMessage(message, 1f, ScreenMessageStyle.LOWER_CENTER);
        }

        // Run once at the mode loading
        public void Awake()            
        {
            // it might help
            //ReportManager report = gameObject.AddComponent<ReportManager>();

            report = ReportManager.Instance(); // Call the instance
            report.Kistory = this; // We need this for corutines

            _windows = new WindowManager(); // we need this to draw interface
            
            this.eventTime = DateTime.Now; // ?

            KDebug.Log("Awake", KDebug.Type.MONO);
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
            KDebug.Log("OnDestroy", KDebug.Type.MONO);

            //if(this._windows != null)
            //   this._windows.Destroy();

            KDebug.Log("OnDestroy events", KDebug.Type.MONO);
            if (report != null)
                report.clear();
        }

        public void ShowWindow() 
        {
            KDebug.Log("ShowWindow", KDebug.Type.MONO);
            _windows.Show();
        }

        public void CloseWindow() 
        {
            KDebug.Log("CloseWindow", KDebug.Type.MONO);
            _windows.Close();
        }

        // Corutine add message
        private IEnumerator add_delayed_message(EntryCorutine data)
        {
            KDebug.Log("pre add_delayed_message", KDebug.Type.CORUTINE);

            float waitTime = 3;
            //this._situationRunning = true;
            yield return new WaitForSeconds(waitTime);
            KDebug.Log("post add_delayed_message", KDebug.Type.CORUTINE);
            report.add_situation_message(data.situation, data.ves, data.message, data.vessel_situation);
        }

        // Corutine add message and photo Very ugly function at that moment
        // This function is not finished yet!!!! 
        private IEnumerator add_message_and_delayed_photo(EntryCorutine data)
        {
            KDebug.Log("pre add_message_and_delayed_photo", KDebug.Type.CORUTINE);
            Mission M = report.add_message_return_mission(data.situation, data.ves, data.message);
            int iE = M.get_last_entry_index();           

            float waitTime = 1;
            if(data.situation == Entry.Situations.EXPLODE)
                waitTime = 0.1f; // Faster screenshot if we have an explosion

            //this._situationRunning = true;
            yield return new WaitForSeconds(waitTime);
            KDebug.Log("post add_message_and_delayed_photo", KDebug.Type.CORUTINE);


            // We are serching for right filepath!
            // The name of the screenshot  should be constructed!
            String dirName = KSPUtil.ApplicationRootPath + "saves/" + HighLogic.SaveFolder;
            var dirInfo = new System.IO.DirectoryInfo(dirName);            
            String fileName = dirInfo.FullName + "/" + str_eventTime() + data.situation.ToString();

            int cntr = 0;
            FileInfo info = new FileInfo(fileName + cntr.ToString() + ".png");            
            while (info != null & info.Exists != false)
            {
                cntr++;
                info = new FileInfo(fileName + cntr.ToString() + ".png");                
            }
            fileName = fileName + cntr.ToString() + ".png";

            KDebug.Log("filename " + fileName, KDebug.Type.CORUTINE);

            // Screeshot shold be captured only on Launch and Explosion that is not reacent
            Boolean addShot = true;
            if (data.situation == Entry.Situations.EXPLODE & !M.ready_for_shot())
                addShot = false;

            if(addShot)
            {
                KDebug.Log("Capturing screeshot for " + iE.ToString() + " in " + data.situation.ToString() +  " " + fileName, KDebug.Type.CORUTINE);
                Application.CaptureScreenshot(fileName);
                while (!File.Exists(fileName))
                {
                    yield return null; // apparently it should make the screeshot first and then continue
                }
                KDebug.Log("Trasfering screeshot for " + iE.ToString() + " in " + data.situation.ToString() + " " + fileName, KDebug.Type.CORUTINE);
                M.add_screenshot(fileName, iE);
                StartCoroutine("delayed_message", "Screenshot captured!");
            }
        }



    }


}
