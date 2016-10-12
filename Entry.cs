using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using UnityEngine;

namespace Kistory
{
    // Class will contain different field for the entry
    // Now it is just a single message
    class Entry
    {
        public enum Situations
        {
            CREATE,
            CREATEEVA,
            DETACHED,
            RECOVERED,
            LAUNCH,
            EVA,
            KILLED,
            BOARD,
            CRASH,
            DESTROYED,
            EXPLODE,
            SITUATION,
            ORBIT,
            ESCAPE,
            SOI,
            CONTRACT,
            STAGE,
            USER
        }

        private struct MissionStrings
        {
            public static String CREATE   = "The new mission";
            public static String DETACHED = "Detached mission";
            public static String RECOVERED = "Mission complete!";
                public static String CREATEEVA = "EVA mission"; // Reserved for separate EVA missions
            public static String LAUNCH = "Lauched!";
            public static String EVA = "Kerbal let the Vessel starting Extra-Vehicular Activity";
            public static String KILLED = "Kerbal onboard was killed";
            public static String BOARD = "Kerbal board";
            public static String CRASH = "Chash!";
            public static String DESTROYED = "Part destroyed";
            public static String EXPLODE = "Explosion!";
            public static String SITUATION = "Situation changed";
                public static String ORBIT = "Orbit closed";
                public static String ESCAPE = "On escape trajectory";
            public static String SOI = "Main body changed";
            public static String CONTRACT = "Contract finished";
            public static String STAGE = "Stage activated";
        }


        //private List<String> messages = new List<String>();
        private String message;
        private double time;
        private Situations situation;
        private String screenShot;
        private Texture2D shot;
        public Boolean has_screeshot = false;

        #region create Entry
        // Simple creation of the Entry
        public Entry()
        {
            this.message = "";
            this.time = 0;
            this.screenShot = null;
            //KDebug.Log("Entry created");
        }

        public void add(Situations S) // Let Entry deside the content
        {
            this.add(S, "");
        }

        public void add(Situations S, String m)
        {
            this.add(S, m, 0);            
        }

        public void add(Situations S, String m, double t)
        {
            this.situation = S;
            this.message = m;
            this.time = t;
            this.print_message();
            KDebug.Log("add to entry: " + t + " | " + m, KDebug.Type.CHANGE);                        
        }

        public void load(Situations S, String m, double t) 
        {
            this.situation = S;
            this.message = m;
            this.time = t;

            //KDebug.Log("load to entry: " + t + " | " + m);
        }

        // Show on screen
        public void print_message()
        {
            ScreenMessages.PostScreenMessage(this.get_short_string(), 1f, ScreenMessageStyle.UPPER_RIGHT);
        }

        #endregion
        
        #region Set Properties

        // Add string to the message
        public void add_to_message(String m)
        {
            this.message = this.message + m;
        }

        public void set_time(double t)
        {
            this.time = t;
        }

        public void set_screenshot(String file)
        {
            has_screeshot = true;
            screenShot = file;

            // load screeshot
            loadScreenshot();
        }



        #endregion

        #region Get Properties
        // methods to saving
        public String get_save_message()
        {
            return this.message;
        }
        public double get_save_time()
        {
            return this.time;
        }
        public Situations get_save_situation()
        {
            return this.situation;
        }

        public String get_save_screenshot()
        {
            return this.screenShot;
        }

        private void loadScreenshot()
        {

            
            //string url = screenShot;
            //var bytes = File.ReadAllBytes(url);
            //Texture2D texture = new Texture2D(73, 73);
            //texture.LoadImage(bytes);
            //image.texture = texture;
            KDebug.Log("load screenShot " + screenShot, KDebug.Type.EVENT);

            FileInfo info = new FileInfo(screenShot);
            if (info != null & info.Exists != false)
            { 
                byte[] bytes = File.ReadAllBytes(screenShot);            
                //shot = new Texture2D(0, 0, TextureFormat.ATF_RGB_DXT1, false);            
                shot = new Texture2D(0, 0);
                shot.LoadImage(bytes);
                //renderer.material.mainTexture = screenshot;
                KDebug.Log("loaded " + screenShot, KDebug.Type.EVENT);
            }
            else
            {
                KDebug.Log("File not found  " + screenShot, KDebug.Type.EVENT);
                has_screeshot = false;
            }
        }
        public Texture2D get_texture()
        {
            return shot;
        }

        //
        private String get_message()
        {
            String prefix;
            String separator = ":";

            switch (this.situation)
            {
                case Situations.CREATE:
                    prefix = MissionStrings.CREATE;
                    break;
                case Situations.DETACHED:
                    prefix = MissionStrings.DETACHED;
                    break;
                case Situations.RECOVERED:
                    prefix = MissionStrings.RECOVERED;
                    separator = "";
                    break;
                case Situations.EXPLODE:
                    prefix = MissionStrings.EXPLODE;
                    separator = "";
                    break;
                case Situations.LAUNCH:
                    prefix = MissionStrings.LAUNCH;
                    separator = "";
                    break;
                case Situations.EVA:
                    prefix = MissionStrings.EVA;
                    break;
                case Situations.KILLED:
                    prefix = MissionStrings.KILLED;
                    break;
                case Situations.BOARD:
                    prefix = MissionStrings.BOARD;
                    break;
                case Situations.DESTROYED:
                    prefix = MissionStrings.DESTROYED;
                    break;
                case Situations.CRASH:
                    prefix = MissionStrings.CRASH;
                    separator = "";
                    break;
                case Situations.SITUATION:
                    prefix = MissionStrings.SITUATION;
                    break;
                case Situations.STAGE:
                    prefix = MissionStrings.STAGE;
                    break;
                case Situations.ORBIT:
                    prefix = MissionStrings.ORBIT;
                    break;
                case Situations.SOI:
                    prefix = MissionStrings.SOI;
                    break;
                case Situations.CONTRACT:
                    prefix = MissionStrings.CONTRACT;
                    break;
                default:
                    prefix = "";
                    separator = "";
                    break;

            }

            return prefix + " " + separator + " "+ this.message;
        }
        private String get_time_str()
        {
            DateTime t = new DateTime();
            t = t.AddSeconds(this.time);

            return t.ToString("dd-MM-yy HH:mm:ss");
        }
        private String get_short_time_str()
        {
            DateTime t = new DateTime();
            t = t.AddSeconds(this.time);

            return t.ToString("HH:mm:ss");
        }    
        
        // methods to display
        public String get_entry_string()
        {
            return "[" + this.get_time_str() + "] " + this.get_message();
        }
        public String get_short_string()
        {
            return "[" + this.get_short_time_str() + "] " + this.get_message();
        }

        #endregion

     }
}
