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

        // Simple creation of the Entry
        public Entry( String m)
        {
            this.message = m;
            Debug.Log("[Kistory] Entry: " + m);
        }

        public String get_message()
        {
            return this.message;
        }
    }
}
