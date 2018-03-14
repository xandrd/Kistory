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

         //screeshot message, as a second so it not interfier with the game
        private DateTime eventTime;

        // Run once at the mode loading
        public void Awake()            
        {
            // it might help
            //ReportManager report = gameObject.AddComponent<ReportManager>();

            this.report = new ReportManager(); // Create the instance
            this.report.kistory = this; // We need this for corutines

            this.windows = new WindowManager(report); // we need this to draw interface
            
            this.eventTime = DateTime.Now; // ?

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
            this.windows.OnDraw();
        }

        // .. dispose
        void OnDestroy()
        {
            KDebug.Log("OnDestroy", KDebug.Type.MONO);

            if(this.windows != null)
               this.windows.Close();

            KDebug.Log("OnDestroy events", KDebug.Type.MONO);
            if (this.report != null)
                this.report.clear();

            //this.report.destroy();

            //Destroy(gameObject); // Cleanup. This is taken from AssemblyReloader manual
        }

        public void ShowWindow() 
        {
            KDebug.Log("ShowWindow", KDebug.Type.MONO);
            windows.Show();
        }

        public void CloseWindow() 
        {
            KDebug.Log("CloseWindow", KDebug.Type.MONO);
            windows.Close();
        }

        // Corutine add message
        private IEnumerator add_delayed_event(EntryCorutine data)
        {
            KDebug.Log("pre add_delayed_event", KDebug.Type.CORUTINE);

            float waitTime = 3;
            //this._situationRunning = true;
            yield return new WaitForSeconds(waitTime);
            KDebug.Log("post add_delayed_event", KDebug.Type.CORUTINE);
            data.report.add_situation_event(data.situation, data.ves, data.message, data.vessel_situation);
            KDebug.Log("after add_delayed_event", KDebug.Type.CORUTINE);
        }

        // This corutine add event imideatelly and add photo later        
        private IEnumerator add_event_and_delayed_photo(EntryCorutine data)
        {
            KDebug.Log("pre add_event_and_delayed_photo", KDebug.Type.CORUTINE);
            Mission M = report.add_event_get_mission(data.situation, data.ves, data.message);
            if (M == null) yield break;

            int iE = M.get_last_entry_index(); // we should return index that we just added

            float waitTime = 1; // default wait time
            if(data.situation == Entry.Situations.EXPLODE)
                waitTime = 0.1f; // Faster watitime for explosion (capture cool photo)
            yield return new WaitForSeconds(waitTime); // Now we wait
            KDebug.Log("post add_event_and_delayed_photo", KDebug.Type.CORUTINE);


            // We are serching for right filepath!
            // The name of the screenshot  should be constructed!            
            String dirName = KSPUtil.ApplicationRootPath + "saves/" + HighLogic.SaveFolder;
            var dirInfo = new System.IO.DirectoryInfo(dirName);
            // TODO: when the game is created
            //  if (!Directory.Exists(dirName + "/Kistory/Photo/"))
            //{
            //Directory.CreateDirectory(filePath + "/Kistory/Photo/");
            //}
            //String fileName = dirInfo.FullName + "/Kistory/Photo/" + str_eventTime() + data.situation.ToString();
            String fileName = dirInfo.FullName + "/" + str_eventTime() + data.situation.ToString();

            // Let's make sure that we don't have the file with this name
            int cntr = 0;
            FileInfo info = new FileInfo(fileName + cntr.ToString() + ".png");            
            while (info != null & info.Exists != false)
            {
                cntr++;
                info = new FileInfo(fileName + cntr.ToString() + ".png");                
            }
            fileName = fileName + cntr.ToString() + ".png";
            KDebug.Log("filename " + fileName, KDebug.Type.CORUTINE);

            KDebug.Log("Capturing screeshot for " + iE.ToString() + " in " + data.situation.ToString() +  " " + fileName, KDebug.Type.CORUTINE);
            // ScreenCapture.CaptureScreenshot("SomeLevel");
            Application.CaptureScreenshot(fileName); // SCREEN SHOT
            while (!File.Exists(fileName))
            {
                yield return null; // apparently it should make the screeshot first and then continue
            }
            KDebug.Log("Trasfering screeshot for " + iE.ToString() + " in " + data.situation.ToString() + " " + fileName, KDebug.Type.CORUTINE);
            M.add_screenshot(fileName, iE); 
            StartCoroutine("delayed_message", "Screenshot captured!"); // add message that we added the screenshot with 1s delay
        }

        private IEnumerator delayed_message(String message)
        {
            yield return new WaitForSeconds(1f);
            ScreenMessages.PostScreenMessage(message, 1f, ScreenMessageStyle.LOWER_CENTER);
        }

        // We use this time for Screeshots
        private String str_eventTime()
        {
            eventTime = DateTime.Now;
            return eventTime.Year.ToString() + eventTime.Month.ToString() + eventTime.Day.ToString() + eventTime.Hour.ToString() + eventTime.Minute.ToString() + eventTime.Second.ToString();
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
