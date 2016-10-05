using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Toolbar;
using UnityEngine;

namespace Kistory
{

    // Class for display window and button in the toolbar
    class WindowManager
    {

        private static ReportManager report = ReportManager.Instance(); // Manager 

       // private IButton toolButton; // toolbar button object

        private bool _windowMainIsOpen, _windowSecondIsOpen, _windowConfirmIsOpen; // flag to check if the window is open
        
        private readonly int _windowMainId = UnityEngine.Random.Range(8000000, 9000000);   // unique ID for the window
        private readonly int _windowSecondId = UnityEngine.Random.Range(8000000, 9000000); // unique ID for the window
        private readonly int _windowConfirmId = UnityEngine.Random.Range(8000000, 9000000); // unique ID for the window

        private Rect _windowMainRect = new Rect(100, 100, 600, 400);   // position and size
        private Rect _windowSecondRect = new Rect(400, 100, 800, 400); // position and size
        private Rect _windowConfirmRect = new Rect(Screen.width * 0.5f - 300, Screen.height * 0.5f - 110, 300, 110); // position and size

        private Vector2 _scrollMainPosition = new Vector2();   // Scroll inside the window
        private Vector2 _scrollSecondPosition = new Vector2(); // Scroll inside the window
        

        private int _selectedMissionIndex = 0; // Which mission to show
        private int _selectedMissionToDeleteEntry = -1; // IF we select Enter to delete, which mission?
        private int _selectedEntryToDelete = -1; // Selected entry to delete

        private String stringEntryToAdd;

        public WindowManager()
        {
            // Close windows by default
            _windowMainIsOpen = false;
            _windowSecondIsOpen = false;
            _windowConfirmIsOpen = false;

            /*
            // regular button from Toolbar
            toolButton = ToolbarManager.Instance.add("Kistory", "Kistory_button");
            toolButton.TexturePath = "Kistory/Kistory"; // temporary icon
            toolButton.ToolTip = "Click to view the list of Missions";
            
            // Action on click
            toolButton.OnClick += (e) =>
            {
                Debug.Log("[Kistory] Kistory_button clicked");
                if (!_windowMainIsOpen && !_windowSecondIsOpen) // if close open main window
                {
                    RenderingManager.AddToPostDrawQueue(_windowMainId, new Callback( WindowMainOnDraw));
                    if (_windowConfirmIsOpen)
                        RenderingManager.RemoveFromPostDrawQueue(_windowConfirmId, new Callback(WindowConfirmOnDraw));

                    _windowMainIsOpen = true;
                    _windowSecondIsOpen = false;
                    _windowConfirmIsOpen = false;
                }
                else // if open close main and second windows
                {
                    if(_windowMainIsOpen || _windowSecondIsOpen)
                        RenderingManager.RemoveFromPostDrawQueue(_windowMainId, new Callback(WindowMainOnDraw));
                    if (_windowConfirmIsOpen)
                        RenderingManager.RemoveFromPostDrawQueue(_windowConfirmId, new Callback(WindowConfirmOnDraw));

                    _windowMainIsOpen = false;
                    _windowSecondIsOpen = false;
                    _windowConfirmIsOpen = false;
                                  
                }
            };*/
            
        }

        /*public void Destroy()
        {
           //  toolButton.Destroy();

            if (_windowMainIsOpen)
                RenderingManager.RemoveFromPostDrawQueue(_windowMainId, new Callback( WindowMainOnDraw ));

            if (_windowConfirmIsOpen)
                RenderingManager.RemoveFromPostDrawQueue(_windowConfirmId, new Callback( WindowConfirmOnDraw ));
                  
        }*/

        public void Show()
        {            
            _windowMainIsOpen = true;
            _windowSecondIsOpen = false;
            _windowConfirmIsOpen = false;
            Debug.Log("[Kistory] Show " + _windowMainIsOpen.ToString() + " " + _windowSecondIsOpen.ToString() + " " + _windowConfirmIsOpen.ToString());
        }

        public void Close()
        {
            Debug.Log("[Kistory] Close");
            _windowMainIsOpen = false;
            _windowSecondIsOpen = false;
            _windowConfirmIsOpen = false;
        }

      /*  public void ToggleWindow()
        {
            Debug.Log("[Kistory] Kistory_button toggled");
            if (!_windowMainIsOpen && !_windowSecondIsOpen) // if close open main window
            {
                Debug.Log("[Kistory] Kistory_button draw window " + _windowMainId.ToString());
                RenderingManager.AddToPostDrawQueue(_windowMainId, new Callback( WindowMainOnDraw ));                
                Debug.Log("[Kistory] Kistory_button draw window ready " + _windowConfirmIsOpen.ToString());
                if (_windowConfirmIsOpen)
                    RenderingManager.RemoveFromPostDrawQueue(_windowConfirmId, new Callback( WindowConfirmOnDraw ));

                _windowMainIsOpen = true;
                _windowSecondIsOpen = false;
                _windowConfirmIsOpen = false;
            }
            else // if open close main and second windows
            {
                if (_windowMainIsOpen || _windowSecondIsOpen)
                    RenderingManager.RemoveFromPostDrawQueue(_windowMainId, new Callback( WindowMainOnDraw ));
                if (_windowConfirmIsOpen)
                    RenderingManager.RemoveFromPostDrawQueue(_windowConfirmId, new Callback( WindowConfirmOnDraw ));

                _windowMainIsOpen = false;
                _windowSecondIsOpen = false;
                _windowConfirmIsOpen = false;

            }
        } */

        public void OnDraw() // onGui
        {

            if (_windowMainIsOpen || _windowSecondIsOpen)
                WindowMainOnDraw();
            if (_windowConfirmIsOpen)
                WindowConfirmOnDraw();

        }

        private void WindowMainOnDraw() // create the window
        {
                _windowMainRect = GUI.Window(_windowMainId, _windowMainRect, this.WindowsMainOnGUI, "Missions");
        }
        //private void WindowSecondOnDraw() // create the window
        //{
        //    _windowSecondRect = GUI.Window(_windowSecondId, _windowSecondRect, WindowsSecondOnGUI, "Entries");
        //}
        private void WindowConfirmOnDraw()
        {
            _windowConfirmRect = GUI.Window(_windowConfirmId, _windowConfirmRect, this.WindowsConfirmOnGUI, "Confirm");
        }

        // Show all mission
        private void MissionsContent()
        {
            _scrollMainPosition = GUILayout.BeginScrollView(_scrollMainPosition);
            GUILayout.BeginVertical(GUILayout.ExpandHeight(true));

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();            
            //CloseButton();
            GUILayout.EndHorizontal();

            foreach (Mission M in Enumerable.Reverse(report.get_missions()))
            {
                GUILayout.BeginHorizontal();
                // Name
                GUILayout.Label(M.get_mission_string()); // + " " + M.missionId.ToString());

                GUILayout.FlexibleSpace();
                // Button
                if (GUILayout.Button("Show"))
                {
                    Debug.Log("[Kistory] Show button clicked");
                    if (!_windowSecondIsOpen)
                    {
                        Debug.Log("[Kistory] close main window open second");
                        stringEntryToAdd = ""; // Clear the add string
                        _selectedMissionIndex = report.get_missions().IndexOf(M);
                        _windowSecondIsOpen = true;
                        _windowMainIsOpen   = false;                        
                    }                    
                }

                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();
            GUILayout.EndScrollView();

            GUI.DragWindow();
        }

        // Show all entries
        private void EntriesContent()
        {
            _scrollSecondPosition = GUILayout.BeginScrollView(_scrollSecondPosition);
            GUILayout.BeginVertical(GUILayout.ExpandHeight(true));
            
            GUILayout.BeginHorizontal();
            //GUILayout.FlexibleSpace();
            stringEntryToAdd = GUILayout.TextField(stringEntryToAdd, GUILayout.ExpandWidth(true));
            AddButton();
            BackButton();
            //CloseButton();
            GUILayout.EndHorizontal();


            foreach (Entry E in report.get_mission_by_index(_selectedMissionIndex).get_entries())
            {
                GUILayout.BeginHorizontal();
                // Name
                GUILayout.Label(E.get_entry_string());
                GUILayout.FlexibleSpace();
                GUILayout.FlexibleSpace();
                // Button
                if (GUILayout.Button("X")) // Delete the entry
                {
                   
                    _selectedMissionToDeleteEntry = _selectedMissionIndex; // IF we select Enter to delete, which mission?
                    _selectedEntryToDelete = report.get_mission_by_index(_selectedMissionIndex).get_entries().IndexOf(E); // Selected entry to delete                    
                    Debug.Log("[Kistory] Delete button clicked [" + _selectedMissionToDeleteEntry.ToString() + "] (" + _selectedEntryToDelete.ToString() + ")");
                    _windowConfirmIsOpen = true;
                    //RenderingManager.AddToPostDrawQueue(_windowConfirmId, new Callback( WindowConfirmOnDraw ));
                    //break;
                    //PopupDialog.SpawnPopupDialog()
                }
                GUILayout.EndHorizontal();
            }

            GUILayout.EndVertical();
            GUILayout.EndScrollView();

            GUI.DragWindow();
        }

        private void ConfirmDeleteMessage()
        {

            String titleToDelete = "";                        
            Boolean isOk = _selectedMissionToDeleteEntry != -1 & _selectedEntryToDelete != -1;

            if(isOk)
            {
                
                Mission M = report.get_mission_by_index(_selectedMissionToDeleteEntry);
                titleToDelete = M.get_name();
                titleToDelete = titleToDelete + " " + M.get_entries().ElementAt(_selectedEntryToDelete).get_short_string();
            }
              

            
            GUILayout.BeginVertical(GUILayout.ExpandHeight(true));
            GUILayout.BeginArea(new Rect(5, 20, 290, 90));

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label("Delete? " + titleToDelete); // + " " + M.missionId.ToString());
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();

            //GUILayout.BeginArea(new Rect(50, 60, 200, 20));
            if (GUILayout.Button("Delete"))
            {
                Debug.Log("[Kistory] Confirm delete button clicked [" + _selectedMissionToDeleteEntry.ToString() + "] (" + _selectedEntryToDelete.ToString()+")");
                if(isOk)
                { 
                    report.get_mission_by_index(_selectedMissionToDeleteEntry).detele_entry_by_index(_selectedEntryToDelete);
                    // index reset just in case
                    _selectedMissionToDeleteEntry = -1;
                    _selectedEntryToDelete = -1;
                }

                if (_windowConfirmIsOpen) // this check is not neccecary... 
                    //RenderingManager.RemoveFromPostDrawQueue(_windowConfirmId, new Callback( WindowConfirmOnDraw ));
                    _windowConfirmIsOpen = false;
            }

            if (GUILayout.Button("Cancel"))
            {
                Debug.Log("[Kistory] Confirm cancel button clicked");
                if (_windowConfirmIsOpen) // this check is not neccecary... 
                    //RenderingManager.RemoveFromPostDrawQueue(_windowConfirmId, new Callback( WindowConfirmOnDraw ));
                    _windowConfirmIsOpen = false;
            }
            //GUILayout.EndArea();

            GUILayout.EndHorizontal();
            GUILayout.EndArea();

            GUILayout.EndVertical();
            GUI.DragWindow();
        }

        private void CloseButton()
        {
            if (GUILayout.Button("Close", GUILayout.ExpandWidth(false)))
            {
                //RenderingManager.RemoveFromPostDrawQueue(_windowMainId, new Callback( WindowMainOnDraw ));
                _windowSecondIsOpen = false;
                _windowMainIsOpen   = false;
            }         
        }
        private void BackButton()
        {
            if (GUILayout.Button("Back", GUILayout.ExpandWidth(false)))
            {
                _windowMainIsOpen = true;
                _windowSecondIsOpen = false;
            }
        }
        private void AddButton()
        {
            if (GUILayout.Button("Add", GUILayout.ExpandWidth(false)))
            {
                Debug.Log("[Kistory] Add entry button: " + stringEntryToAdd);
                report.get_mission_by_index(_selectedMissionIndex).add_user_entry(stringEntryToAdd);
            }
        }

        private void WindowsMainOnGUI(int id) // draw main window
        {
            if (_windowMainIsOpen)
            {
                MissionsContent();
            }            
            else if(_windowSecondIsOpen)
            {
                EntriesContent();
            }
        }

        //private void WindowsSecondOnGUI(int id) // draw secondary window
        //{
        //    EntriesContent();
        //}

        private void WindowsConfirmOnGUI(int id)
        {
            ConfirmDeleteMessage();
        }
    }
}
