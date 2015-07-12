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

        // Simple creation of the Entry
        public Entry(String m)
        {
            this.message = m;
            this.time = 0;
            this.screenShot = null;
            Debug.Log("[Kistory] Entry: " + m);
        }

        public Entry(String m, double t)
        {
            this.message = m;
            this.time = t;
            this.screenShot = null;
            Debug.Log("[Kistory] Entry: " + t + " | " + m);
        }

        public String get_time_str()
        {
            DateTime t = new DateTime();
            t = t.AddSeconds(this.time);

            return t.ToString("dd-MM-yy HH:mm:ss");
            //return this.time.ToString();
        }

        public String get_message()
        {
            return  this.message;
        }

        public void set_time(double t)
        {
            this.time = t;
        }

        public double get_time()
        {
            return this.time;
        }

        public String get_string()
        {
            return "[" + this.get_time_str() + "]" + this.message;
        }
    }
}
