using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Kistory
{
    static class KDebug
    {
        public enum Type
        {
            GUI, // All from GUI
            EVENT, // All events related except CREATE
            CORUTINE, // All about  delayed debug
            CREATE, // All about create mission
            MONO, // MonoBehaviour
            CHANGE, // Modification with arrays
            LOAD // Save and load
        }

        static private Boolean SHOWTYPE = true;
        static private Boolean DEBUG    = true;

        static public void Log(String str)
        {
            if(DEBUG)
                Debug.Log("[Kistory] " + str);
        }            

        static public void Log(String str, Type t)
        {
            if (t != Type.GUI & t != Type.MONO & t != Type.LOAD)
            {
                if(SHOWTYPE)
                    KDebug.Log("|" + t.ToString() + "| " + str);
                else
                    KDebug.Log(str);
            }

        }
    }
}
