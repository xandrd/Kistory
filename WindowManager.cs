using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using KSP.UI.Screens;


namespace Kistory
{

    // Class for display window and button in the toolbar
    class WindowManager : MonoBehaviour
    {

        //private static ReportManager report = ReportManager.Instance(); // Manager 
        // private IButton toolButton; // toolbar button object

        private ReportManager report;
        private static ApplicationLauncherButton button;


        private bool _windowMainIsOpen, _windowSecondIsOpen, _windowConfirmIsOpen; // flag to check if the window is open
        
        private int _windowMainId;   // unique ID for the window        
        private int _windowConfirmId; // unique ID for the window
        //private readonly int _windowSecondId = UnityEngine.Random.Range(8000000, 9000000); // unique ID for the window

        private Rect _windowMainRect = new Rect(100, 100, Screen.width * 0.5f, Screen.height * 0.7f);   // position and size        
        private Rect _windowConfirmRect = new Rect(Screen.width * 0.5f - 300, Screen.height * 0.5f - 110, 300, 110); // position and size
        //private Rect _windowSecondRect = new Rect(400, 100, 800, 400); // position and size

        private Vector2 _scrollMainPosition = new Vector2();   // Scroll inside the window
        private Vector2 _scrollSecondPosition = new Vector2(); // Scroll inside the window
        
        private int  _selectedMissionIndex = 0; // Which mission to show
        private int  _selectedMissionToDelete = -1; // IF we select Mission to delete, which mission?
        private int  _selectedMissionToDeleteEntry = -1; // IF we select Enter to delete, which mission?
        private int  _selectedEntryToDelete = -1; // Selected entry to delete
        private bool _selectedPhotoEntrly = false; // Selected entry to delete

        private int _selectedScrollImage = -1; // This is the indication of the selected image that changes to scroll view
        private Vector2 _imageScrollPosition;

        private String stringEntryToAdd;

        public void Awake()
        {
             _windowMainId = UnityEngine.Random.Range(8000000, 9000000);   // unique ID for the window            
            _windowConfirmId = UnityEngine.Random.Range(8000000, 9000000); // unique ID for the window
        }

        public void Start()
        {
            // Close windows by default
            _windowMainIsOpen = false;
            _windowSecondIsOpen = false;
            _windowConfirmIsOpen = false;
            report = gameObject.GetComponent<ReportManager>();

            KDebug.Log("Add and remove button", KDebug.Type.EVENT);
            GameEvents.onGUIApplicationLauncherReady.Add(this.Add_GUI_button); // add
            GameEvents.onGUIApplicationLauncherUnreadifying.Add(this.Remove_GUI_button); // remove

            /*
            // regular button from Toolbar
            toolButton = ToolbarManager.Instance.add("Kistory", "Kistory_button");
            toolButton.TexturePath = "Kistory/Kistory"; // temporary icon
            toolButton.ToolTip = "Click to view the list of Missions";
            
            // Action on click
            toolButton.OnClick += (e) =>
            {
                KDebug.Log("Kistory_button clicked");
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

        // Interface
        private void Add_GUI_button()
        {
            KDebug.Log("Creating the button", KDebug.Type.GUI);
            if (ApplicationLauncher.Instance != null && button == null)
            {

                KDebug.Log("Add button", KDebug.Type.GUI);
                ApplicationLauncher.AppScenes VisibleInScenes = ApplicationLauncher.AppScenes.FLIGHT | ApplicationLauncher.AppScenes.SPACECENTER | ApplicationLauncher.AppScenes.MAPVIEW; //, VAB, SPH, ALWAYS
                button = ApplicationLauncher.Instance.AddModApplication(this.Show, this.Close, null, null, null, null, VisibleInScenes, GameDatabase.Instance.GetTexture("Kistory/Kistory", false));
            }
        }

        private void Remove_GUI_button(GameScenes scene)
        {
            if (ApplicationLauncher.Instance != null && button != null)
            {
                ApplicationLauncher.Instance.RemoveModApplication(button);
            }
        }

        #region Button events
        /*private void button_OnToggleTrue()
        {
            // Code here            
            KDebug.Log("New button Toggle on ", KDebug.Type.GUI);

            Show();
        }

        private void button_OnToggleFalse()
        {
            // Code here            
            KDebug.Log("New button Toggle off ", KDebug.Type.GUI);
            kistory.CloseWindow();
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
        }*/
        #endregion

        public void Show()
        {            
            _windowMainIsOpen = true;
            _windowSecondIsOpen = false;
            _windowConfirmIsOpen = false;
            KDebug.Log("Show " + _windowMainIsOpen.ToString() + " " + _windowSecondIsOpen.ToString() + " " + _windowConfirmIsOpen.ToString(), KDebug.Type.GUI);
        }

        public void Close()
        {
            KDebug.Log("Close", KDebug.Type.GUI);
            _windowMainIsOpen = false;
            _windowSecondIsOpen = false;
            _windowConfirmIsOpen = false;
            _selectedPhotoEntrly = false;
        }

      /*  public void ToggleWindow()
        {
            KDebug.Log("Kistory_button toggled");
            if (!_windowMainIsOpen && !_windowSecondIsOpen) // if close open main window
            {
                KDebug.Log("Kistory_button draw window " + _windowMainId.ToString());
                RenderingManager.AddToPostDrawQueue(_windowMainId, new Callback( WindowMainOnDraw ));                
                KDebug.Log("Kistory_button draw window ready " + _windowConfirmIsOpen.ToString());
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

        public void OnGUI() // OnDraw() // onGui
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
            Color defaultColor = GUI.color;

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
                // Highlight the name is thi is current mission
                if(FlightGlobals.ActiveVessel != null)                
                    if( Mission.mission_id(FlightGlobals.ActiveVessel) == M.missionId)
                        GUI.color = Color.green;                                                     
                GUILayout.Label(M.get_mission_string()); // + " " + M.missionId.ToString());
                        GUI.color = defaultColor;

                GUILayout.FlexibleSpace();
                // Button
                ShowMissionButton(M);
                DeleteMissionButton(M);

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

            
            if (report.get_mission_by_index(_selectedMissionIndex).missionId == Mission.mission_id(FlightGlobals.ActiveVessel))
                PhotoButton();

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
                DeleteEntryButton(E);
                GUILayout.EndHorizontal();
                if (E.has_screeshot)
                {
                    float W = _windowMainRect.width - 50;
                    float H = W * Screen.height / Screen.width;
                    // Minimum Width adn Height. Make the pitcure nice
                    GUILayoutOption GW = GUILayout.MinWidth(W); 
                    GUILayoutOption GH = GUILayout.MinHeight(H);
                    Texture2D img = E.get_texture();
                    int Eidx = report.get_mission_by_index(_selectedMissionIndex).get_entries().IndexOf(E); // current index

                    GUILayout.BeginVertical(GW, GH);
                    if (_selectedScrollImage != Eidx) // If has not been clicked show the "zoom" button
                    { 
                        if (GUILayout.Button(new GUIContent(img, "Click to zoom in"), GUIStyle.none, GW, GH))  //GUIStyle.none
                        {
                            if (_selectedScrollImage == Eidx)
                                _selectedScrollImage = -1;
                            else
                                _selectedScrollImage = Eidx; 
                            KDebug.Log("Screenshot zoom button pressed", KDebug.Type.GUI);
                        }
                    }
                    else // This is zoom case
                    {
                        _imageScrollPosition = GUILayout.BeginScrollView(_imageScrollPosition, GH); // Begin the ScrollView, (!) only GH is applyed. Othervise it is a mess
                        if (GUILayout.Button(new GUIContent(img, "Click to zoom out"), GUIStyle.none, GUILayout.Width(img.width), GUILayout.Height(img.height))) //
                        {
                            if (_selectedScrollImage == Eidx)
                                _selectedScrollImage = -1;
                            else
                                _selectedScrollImage = Eidx;
                            KDebug.Log("Screenshot unzoom button pressed", KDebug.Type.GUI);
                        }
                        GUILayout.EndScrollView(); // End the ScrollView
                    }
                    GUILayout.EndVertical();
                }

            }

            GUILayout.EndVertical();
            GUILayout.EndScrollView();

            GUI.DragWindow();
        }

        
        private void ConfirmDeleteMessage()
        {

            String titleToDelete = "";                        
            Boolean isOkEntry = _selectedMissionToDeleteEntry != -1 & _selectedEntryToDelete != -1;
            Boolean isOkMission = _selectedMissionToDelete != -1;

            if (isOkEntry)
            {
                
                Mission M = report.get_mission_by_index(_selectedMissionToDeleteEntry);
                titleToDelete = M.get_name();
                titleToDelete = titleToDelete + " " + M.get_entries().ElementAt(_selectedEntryToDelete).get_short_string();
            }
            else if(isOkMission)
            {
                Mission M = report.get_mission_by_index(_selectedMissionToDelete);
                titleToDelete = "Mission:"  + M.get_name();                
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
                KDebug.Log("Confirm delete button clicked [" + _selectedMissionToDeleteEntry.ToString() + "] (" + _selectedEntryToDelete.ToString()+")", KDebug.Type.GUI);
                if(isOkEntry)
                { 
                    report.get_mission_by_index(_selectedMissionToDeleteEntry).detele_entry_by_index(_selectedEntryToDelete);
                    // index reset just in case
                    _selectedMissionToDeleteEntry = -1;
                    _selectedEntryToDelete = -1;
                    _selectedMissionToDelete = -1;
                    _windowConfirmIsOpen = false;
                }
                else if (isOkMission)
                {
                    report.detele_mission_by_index(_selectedMissionToDelete);
                    // index reset just in case
                    // index reset just in case
                    _selectedMissionToDeleteEntry = -1;
                    _selectedEntryToDelete = -1;
                    _selectedMissionToDelete = -1;
                    _windowConfirmIsOpen = false;
                }

                if (_windowConfirmIsOpen) // this check is not neccecary... 
                    _windowConfirmIsOpen = false;
            }

            if (GUILayout.Button("Cancel"))
            {
                KDebug.Log("Confirm cancel button clicked", KDebug.Type.GUI);
                if (_windowConfirmIsOpen) // this check is not neccecary... 
                    _windowConfirmIsOpen = false;
            }
            //GUILayout.EndArea();

            GUILayout.EndHorizontal();
            GUILayout.EndArea();

            GUILayout.EndVertical();
            GUI.DragWindow();
        }

        private void ConfirmPhotoEntrly()
        {
            GUILayout.BeginVertical(GUILayout.ExpandHeight(true));
            GUILayout.BeginArea(new Rect(5, 20, 290, 90));

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label("Would you like to add with photo with a message: " + stringEntryToAdd + "? The main window will close and photo will be captured in 0.5 second."); // + " " + M.missionId.ToString());
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();

            //GUILayout.BeginArea(new Rect(50, 60, 200, 20));
            if (GUILayout.Button("Photo"))
            {
                Close(); // close all windows
                AddPhoto();
                stringEntryToAdd = "";
            }

            if (GUILayout.Button("Cancel"))
            {
                _selectedPhotoEntrly = false;
                if (_windowConfirmIsOpen) // this check is not neccecary... 
                    _windowConfirmIsOpen = false;                
            }
            //GUILayout.EndArea();

            GUILayout.EndHorizontal();
            GUILayout.EndArea();

            GUILayout.EndVertical();
            GUI.DragWindow();
        }

        private void ShowMissionButton(Mission M)
        {
            if (GUILayout.Button("Show"))
            {
                KDebug.Log("Show button clicked", KDebug.Type.GUI);
                if (!_windowSecondIsOpen)
                {
                    KDebug.Log("close main window open second", KDebug.Type.GUI);
                    stringEntryToAdd = ""; // Clear the add string
                    _selectedMissionIndex = report.get_missions().IndexOf(M);
                    _windowSecondIsOpen = true;
                    _windowMainIsOpen = false;
                }
            }
        }
        private void DeleteMissionButton(Mission M)
        {
            if (GUILayout.Button("X")) // Delete the entire Mission
            {

                _selectedMissionToDelete = report.get_missions().IndexOf(M); // Selected mission to delete                    
                KDebug.Log("Mission Delete button clicked [" + _selectedMissionToDelete.ToString() + "] ", KDebug.Type.GUI);
                _windowConfirmIsOpen = true;
                _windowSecondIsOpen = false;
                _windowMainIsOpen = true;
            }
        }
        private void DeleteEntryButton(Entry E)
        {
            if (GUILayout.Button("X")) // Delete the entry
            {

                _selectedMissionToDeleteEntry = _selectedMissionIndex; // IF we select Enter to delete, which mission?
                _selectedEntryToDelete = report.get_mission_by_index(_selectedMissionIndex).get_entries().IndexOf(E); // Selected entry to delete                    
                KDebug.Log("Delete button clicked [" + _selectedMissionToDeleteEntry.ToString() + "] (" + _selectedEntryToDelete.ToString() + ")", KDebug.Type.GUI);
                _windowConfirmIsOpen = true;
                //RenderingManager.AddToPostDrawQueue(_windowConfirmId, new Callback( WindowConfirmOnDraw ));
                //break;
                //PopupDialog.SpawnPopupDialog()
            }
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
                _selectedScrollImage = -1; // Reset the screenshot scroll if we have one
            }
        }
        private void AddButton()
        {
            if (GUILayout.Button("Add", GUILayout.ExpandWidth(false)))
            {
                KDebug.Log("Add entry button: " + stringEntryToAdd, KDebug.Type.GUI);
                report.get_mission_by_index(_selectedMissionIndex).add_user_entry(stringEntryToAdd);
                stringEntryToAdd = "";
            }
        }
        private void PhotoButton()
        {
            if (GUILayout.Button("Photo", GUILayout.ExpandWidth(false)))
            {
                KDebug.Log("Add photo button: ", KDebug.Type.GUI);
                _windowConfirmIsOpen = true;
                _selectedPhotoEntrly = true;
            }
        }

        private void AddPhoto()
        {
            report.add_user_photoentry(stringEntryToAdd);
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
            if (_selectedPhotoEntrly)
                ConfirmPhotoEntrly();
            else
                ConfirmDeleteMessage();
        }

        public void OnDestroy()
        {
            Close();
            GameEvents.onGUIApplicationLauncherReady.Remove(this.Add_GUI_button); // add
            GameEvents.onGUIApplicationLauncherUnreadifying.Remove(this.Remove_GUI_button); // remove
        }

    }
}
