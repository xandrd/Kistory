using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Kistory
{
    static class KDebug
    {
        static public void Log(String str)
        {
            Debug.Log("[Kistory] " + str);
        }            
    }
}
