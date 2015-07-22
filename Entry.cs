using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Kistory
{
    // Class will contain different field for the entry
    // Now it is just a single message
    class Entry
    {
        
        //private List<String> messages = new List<String>();
        private String message;
        private double time;
        private String screenShot;

        #region create Entry
        // Simple creation of the Entry
        public Entry()
        {
            this.message = "";
            this.time = 0;
            this.screenShot = null;
            //Debug.Log("[Kistory] Entry created");
        }

        public void add(String m)
        {
            this.message = m;
            this.time = 0;
            this.print_message();
            Debug.Log("[Kistory] add to entry: " + m);
        }

        public void add(String m, double t)
        {
            this.message = m;
            this.time = t;
            this.print_message();
            Debug.Log("[Kistory] add to entry: " + t + " | " + m);
                        
        }

        public void load(String m, double t)
        {
            this.message = m;
            this.time = t;

            //Debug.Log("[Kistory] load to entry: " + t + " | " + m);
        }

        public void print_message()
        {
            ScreenMessages.PostScreenMessage(this.get_short_string(), 1f, ScreenMessageStyle.UPPER_RIGHT);
        }

        #endregion
        
        #region Set Properties
        public void set_time(double t)
        {
            this.time = t;
        }

        #endregion

        #region Get Properties
        public String get_message()
        {
            return  this.message;
        }
        public double get_time()
        {
            return this.time;
        }
        public String get_time_str()
        {
            DateTime t = new DateTime();
            t = t.AddSeconds(this.time);

            return t.ToString("dd-MM-yy HH:mm:ss");
        }

        public String get_short_time_str()
        {
            DateTime t = new DateTime();
            t = t.AddSeconds(this.time);

            return t.ToString("HH:mm:ss");
        }

        // This function format the entry string
        public String get_entry_string()
        {
            return "[" + this.get_time_str() + "] " + this.message;
        }

        public String get_short_string()
        {
            return "[" + this.get_short_time_str() + "] " + this.message;
        }

        #endregion

     }
}
