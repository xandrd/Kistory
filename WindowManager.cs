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

        private IButton toolButton; // toolbar button object

        private bool _windowMainIsOpen, _windowSecondIsOpen; // flag to check if the window is open
        
        private readonly int _windowMainId = UnityEngine.Random.Range(8000000, 9000000);   // unique ID for the window
        private readonly int _windowSecondId = UnityEngine.Random.Range(8000000, 9000000); // unique ID for the window

        private Rect _windowMainRect = new Rect(100, 100, 600, 400);   // position and size
        private Rect _windowSecondRect = new Rect(400, 100, 800, 400); // position and size

        private Vector2 _scrollMainPosition = new Vector2();   // Scroll inside the window
        private Vector2 _scrollSecondPosition = new Vector2(); // Scroll inside the window

        private int _selectedMissionIndex = 0; // Which mission to show

        private String stringEntryToAdd;

        public WindowManager()
        {
            // Close windows by default
            _windowMainIsOpen = false;
            _windowSecondIsOpen = false;

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
                    _windowSecondIsOpen = false;
                    _windowMainIsOpen = true;
                    RenderingManager.AddToPostDrawQueue(_windowMainId, WindowMainOnDraw);
                }
                else // if open close main and second windows
                {
                    _windowMainIsOpen = false;
                    _windowSecondIsOpen = false;
                    RenderingManager.RemoveFromPostDrawQueue(_windowMainId, WindowMainOnDraw);                    
                }
            };
        }

        public void Destroy()
        {
            toolButton.Destroy();

            if (_windowMainIsOpen)
                RenderingManager.RemoveFromPostDrawQueue(_windowMainId, WindowMainOnDraw);

            if (_windowSecondIsOpen)
                RenderingManager.RemoveFromPostDrawQueue(_windowSecondId, WindowSecondOnDraw);
        }

        private void WindowMainOnDraw() // create the window
        {
            _windowMainRect = GUI.Window(_windowMainId, _windowMainRect, WindowsMainOnGUI, "Missions");
        }
        private void WindowSecondOnDraw() // create the window
        {
            _windowSecondRect = GUI.Window(_windowSecondId, _windowSecondRect, WindowsSecondOnGUI, "Entries");
        }

        // Show all mission
        private void MissionsContent()
        {
            _scrollMainPosition = GUILayout.BeginScrollView(_scrollMainPosition);
            GUILayout.BeginVertical(GUILayout.ExpandHeight(true));

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();            
            CloseButton();
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
            CloseButton();
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
                    Debug.Log("[Kistory] Delete button clicked");
                    int entryIndex;
                    entryIndex = report.get_mission_by_index(_selectedMissionIndex).get_entries().IndexOf(E);
                    report.get_mission_by_index(_selectedMissionIndex).detele_entry_by_index(entryIndex);
                    break;
                    //PopupDialog.SpawnPopupDialog()
                }
                GUILayout.EndHorizontal();
            }

            GUILayout.EndVertical();
            GUILayout.EndScrollView();

            GUI.DragWindow();
        }
        private void CloseButton()
        {
            if (GUILayout.Button("Close", GUILayout.ExpandWidth(false)))
            {
                RenderingManager.RemoveFromPostDrawQueue(_windowMainId, WindowMainOnDraw);
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

        private void WindowsSecondOnGUI(int id) // draw secondary window
        {
            EntriesContent();
        }

    }
}
