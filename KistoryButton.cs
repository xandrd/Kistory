/*
 * using System;
using System.Collections;
using UnityEngine;

namespace Kistory
{
    using KSP.UI.Screens;
    
    static class KistoryButton
    {

        private static ApplicationLauncherButton button;
        private static Texture buttonTexture;

        // Event should call this to create the button after ApplicationLauncher is ready
        public static void Init()
        {
            KDebug.Log("Creating the button");
            if (ApplicationLauncher.Instance != null && button == null)
            {
                KDebug.Log("Load texture");
                buttonTexture = GameDatabase.Instance.GetTexture("Kistory/Kistory", false);
                KDebug.Log("Add button");
                button = ApplicationLauncher.Instance.AddModApplication(OnToggleTrue, OnToggleFalse, null, null, null, null, ApplicationLauncher.AppScenes.ALWAYS, buttonTexture);
            }
                
        }

        // Event should call this to delete the button
        public static void Destroy(GameScenes scene)
        {
            if (ApplicationLauncher.Instance != null && button != null)
            {
                ApplicationLauncher.Instance.RemoveModApplication(button);
            }
        }

        private static void OnToggleTrue()
        {
            // Code here
            //PilotAssistantFlightCore.bDisplayAssistant = true;
            KDebug.Log("New button Toggle on ");
        }

        private static void OnToggleFalse()
        {
            // Code here
            //PilotAssistantFlightCore.bDisplayAssistant = false;
            KDebug.Log("New button Toggle off ");
        }

        public static void setBtnState(bool state, bool click = false)
        {
            if (state)
                button.SetTrue(click);
            else
                button.SetFalse(click);
        }

    }
}
*/