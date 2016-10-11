using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


namespace Kistory
{
    using KSP.UI.Screens;

    // Singlton that manage the report in the game
    // Containg the missions
    class ReportManager
    {

        public Kistory Kistory; 

        private List<Mission> missions = new List<Mission>();

        private bool _situationRunning = false;
        public EntryCorutine objCorutine = new EntryCorutine();

        private static ApplicationLauncherButton button;

        // Singlton
        private static volatile ReportManager instance;        
        public static ReportManager Instance()
        {
            if (instance == null)
            {
                instance = new ReportManager();
                //instance = new GameObject("ReportManager").AddComponent<ReportManager>();
            }
            return instance;
        }

        #region Event Subscribe

        private ReportManager()
        {
            KDebug.Log("New ReportManager Created");
            // == Add and remove button == //
            KDebug.Log("Add and remove button");
            GameEvents.onGUIApplicationLauncherReady.Add(this.add_button); // add
            GameEvents.onGUIApplicationLauncherUnreadifying.Add(this.remove_button); // remove

            KDebug.Log("Events No Active Vessel");
            // === Event No Active Vessel (?) === //
            GameEvents.onGameStateLoad.Add(this.on_game_load); // Load Kistory from the save file
            GameEvents.onGameStateSave.Add(this.on_game_save); // Save Kistory to the save file

            GameEvents.onVesselCreate.Add(this.on_create); // When Vessel is created and there is no existing mission - create mission
            GameEvents.onNewVesselCreated.Add(this.on_new_create);
            GameEvents.onVesselRecovered.Add(this.on_recovered);

            GameEvents.onCrewBoardVessel.Add(this.on_board); // Kerbal board to the mission Vessel (active vessel is Kerbal)
            GameEvents.onCrewKilled.Add(this.on_killed);     // Kerbal died. Mission will not be found if Kerbal died outside the Vessel

            // === Events with Active Vessel === //
            KDebug.Log("Events with Active Vessel");
            GameEvents.onLaunch.Add(this.on_launch); // Squad calls this Liftof! 

            GameEvents.onCrewOnEva.Add(this.on_EVA); // EVA
            
            GameEvents.onPartDie.Add(this.on_destroyed); // Scructural damage, part destroyed.
            GameEvents.onPartExplode.Add(this.on_explode); // When exposion happens we may take the awesomness. We not going to use this feature now.

            GameEvents.onVesselSituationChange.Add(this.on_situation);

            GameEvents.onVesselSOIChanged.Add(this.on_soi);

            GameEvents.Contract.onFinished.Add(this.on_contract);

            GameEvents.onStageActivate.Add(this.on_stage);
            //EventData<Contracts.Contract> GameEvents.Contract.onFinished
        }

        #endregion

        #region Evet Processing

        // Save Load
        private void on_game_load(ConfigNode nodeGame)
        {
            if (nodeGame != null)
            {
                KDebug.Log("Loading game...");
                this.clear_missions();

                // REPORT
                String nodeName = "Kistory";
                ConfigNode nodeReport = new ConfigNode();
                if (nodeGame.HasNode(nodeName)) // reserved for future if we will add variables to the report node
                {
                    nodeReport = nodeGame.GetNode(nodeName);
                }
                else
                {
                    nodeReport = nodeGame;
                }
                // MISSION
                nodeName = "Mission";
                if (nodeReport.HasNode(nodeName))
                {
                    ConfigNode[] nodeMissions = nodeReport.GetNodes(nodeName);
                    foreach (ConfigNode nodeMission in nodeMissions)
                    {
                        if (nodeMission.HasValue("missionId") & nodeMission.HasValue("missionName") & nodeMission.HasValue("missionTime") & nodeMission.HasValue("missionSituation"))
                        {
                            String missionId = nodeMission.GetValue("missionId");
                            String missionName = nodeMission.GetValue("missionName");
                            double missionTime = Convert.ToDouble(nodeMission.GetValue("missionTime"));
                            Vessel.Situations missionSituation = (Vessel.Situations) Enum.Parse(typeof(Vessel.Situations), nodeMission.GetValue("missionSituation")); // I hope that will work
                            // create mission
                            Mission M = new Mission(new Guid(missionId), missionName, missionTime, missionSituation);

                            nodeName = "Entry";
                            if (nodeMission.HasNode("Entry"))
                            {
                                // ENTRY
                                ConfigNode[] nodeEnties = nodeMission.GetNodes("Entry");
                                foreach (ConfigNode nodeEntie in nodeEnties)
                                {                                    
                                    if (nodeEntie.HasValue("situation") & nodeEntie.HasValue("message") & nodeEntie.HasValue("time"))
                                    {                                        
                                        // Create variables, to exclude some unknown exeptions
                                        String nodem = nodeEntie.GetValue("message");
                                        String nodet = nodeEntie.GetValue("time");
                                        String nodes = nodeEntie.GetValue("situation");                                        
                                        double t = Convert.ToDouble(nodeEntie.GetValue("time"));
                                        Entry.Situations S = (Entry.Situations)  Enum.Parse(typeof(Entry.Situations), nodes);
                                        M.load_entry(S, nodem, t);
                                    }
                                    else { KDebug.Log("on load one of the Entry field is missing"); }
                                }
                            }

                            this.add_mission(M);
                        } else { KDebug.Log("on load one of the Mission field is missing"); }
                    }
                }
            }
        }
        private void on_game_save(ConfigNode node)
        {
            KDebug.Log("on_game_save node");

            // Mod node
            var nodeKistory = node.AddNode("Kistory");

            // Walk thought all missions                        
            foreach (Mission M in this.missions)
            {
                // this is only Mission parameters
                var nodeMission = nodeKistory.AddNode("Mission");
                nodeMission.AddValue("missionId", M.missionId);
                nodeMission.AddValue("missionName", M.get_name());
                nodeMission.AddValue("missionTime", M.get_time());
                nodeMission.AddValue("missionSituation", M.get_situation());

                // Walk thought all messages of the mission
                foreach (Entry E in M.get_entries())
                {
                    var nodeEntry = nodeMission.AddNode("Entry");
                    nodeEntry.AddValue("situation", E.get_save_situation());
                    nodeEntry.AddValue("message", E.get_save_message()); 
                    nodeEntry.AddValue("time", E.get_save_time()); 
                }
            }

        }

        // Events
        private void on_create(Vessel ves) // Triggered by creating a new vessel. Apparantelly we create a new mission. However, decoupling also create the new mission. Unfortunatelly, we cannot find the name here.
        {
            // This event should be called only if we have a new mission
            if (ves !=null)
            {
                KDebug.Log("on_create");

                Mission M = new Mission(ves); // Possible new mission
                if (M.missionApproved) // Mission was created
                {                    
                    KDebug.Log("on_create approved");
                    Entry E = new Entry();       
                    if(ves.situation == Vessel.Situations.PRELAUNCH)
                    {
                        E.add(Entry.Situations.CREATE, M.get_name(), (double)0);
                    }
                    else
                    {                                  
                        E.add(Entry.Situations.DETACHED, M.get_name(), (double)0);
                    }
                    M.add_entry( E );
                    this.add_mission(M);
                }
            }
        }

        // Cannot find this event
        private void on_new_create(Vessel ves)
        {
            KDebug.Log("on_new_create " + ves.vesselType.ToString() + " " + ves.vesselName + " " + ves.name);
        }

        private void on_recovered(ProtoVessel ves, bool flag)
        {
            KDebug.Log("on_recovered " + ves.vesselType.ToString() + " " + ves.vesselName);

            this.add_message(Entry.Situations.RECOVERED, ves);
        }

        private void on_launch(EventReport data) // Triggered on launch
        {
            KDebug.Log("on_launch");
            this.add_message(Entry.Situations.LAUNCH,"");
            //this.make_screenshot("Launch");
        }

        private void on_EVA(GameEvents.FromToAction<Part, Part> data)
        {
            KDebug.Log("on_EVA " + data.to.vessel.vesselName + " " + data.to.vessel.rootPart.vessel.vesselName);
            this.add_message(Entry.Situations.EVA,  data.to.vessel.vesselName);
            //this.make_screenshot("EVA");
        }

        private void on_killed(EventReport report)
        {
            // If Kerbin died outside the vessel we would not be able to find that.
            KDebug.Log("on_killed " + report.sender);
            this.add_message(Entry.Situations.KILLED, report.sender);
        }

        private void on_board(GameEvents.FromToAction<Part, Part> data)
        {
            KDebug.Log("on_board " + data.from.vessel.vesselName);
            this.add_message(Entry.Situations.BOARD, data.to.vessel,  data.from.vessel.vesselName);
        }

        // It is unknown when this event happens
        // TODO: Find the application for this event.
        private void on_explode(GameEvents.ExplosionReaction r)
        {
            KDebug.Log("on_explode: " + r.magnitude.ToString());
            //this.add_message(MissionStrings.EXPLODE + " :" + r.magnitude.ToString());
        }

        private void on_destroyed(Part part)
        {

            KDebug.Log("on_destroyed: " + part.partInfo.title + " " + part.vessel.id );            
            // Specific event!
            // foreach (PartModule module in part.Modules)
            //     KDebug.Log("on_destroyed: [" + module.moduleName + "] " + module.GetInfo() + " | " + module.guiText + " | " + module.name); //[0].GetInfo()
            this.add_message(Entry.Situations.DESTROYED, part.vessel, part.partInfo.title);
        }

        private void on_crash(EventReport data)
        {
            // Happens even before Launch

            if(data.origin != null)
            { 
                KDebug.Log("explosionPotential  " + data.origin.explosionPotential.ToString());
                KDebug.Log("flightID  " + data.origin.flightID);
                KDebug.Log("launchID  " + data.origin.launchID);
                KDebug.Log("missionID  " + data.origin.missionID);
            }

            KDebug.Log("on_crash");            
            this.add_message(Entry.Situations.CRASH,"");
        }

        private void on_situation( GameEvents.HostedFromToAction< Vessel, Vessel.Situations > data)
        {
            if(data.host != null)
                if (data.host.isActiveVessel)
                {
                    KDebug.Log("on_situation: " +  data.host.vesselName +" " + data.from.ToString() + " -> " + data.to.ToString());
                    if (data.to != Vessel.Situations.PRELAUNCH) // bug or feature?
                        manage_corutine(Entry.Situations.SITUATION, data.host, "from " + data.from.ToString() + " to:" + data.to.ToString(), data.to);
                    
                }
        }

        private void on_soi(GameEvents.HostedFromToAction< Vessel, CelestialBody > data)
        {
            KDebug.Log("on_soi");
            add_message(Entry.Situations.SOI, data.host, " from: " + data.from.bodyName + " to: " + data.to.bodyName);
        }

        private void on_contract(Contracts.Contract contract)
        {
            KDebug.Log("on_contract");
            add_message(Entry.Situations.CONTRACT, " [" + contract.ContractState.ToString() + "] " + contract.Title );
        }

        private void on_stage(int stage)
        {
            KDebug.Log("on_stage");
            add_message(Entry.Situations.STAGE, " #" + stage.ToString());
        }

        // Interface
        private void add_button()
        {
            KDebug.Log("Creating the button");
            if (ApplicationLauncher.Instance != null && button == null)
            {
                                
                KDebug.Log("Add button");
                ApplicationLauncher.AppScenes VisibleInScenes = ApplicationLauncher.AppScenes.FLIGHT | ApplicationLauncher.AppScenes.SPACECENTER | ApplicationLauncher.AppScenes.MAPVIEW; //, VAB, SPH, ALWAYS
                button = ApplicationLauncher.Instance.AddModApplication(this.button_OnToggleTrue, this.button_OnToggleFalse, null, null, null, null, VisibleInScenes, GameDatabase.Instance.GetTexture("Kistory/Kistory", false));
            }
        }

        private void remove_button(GameScenes scene)
        {
            if (ApplicationLauncher.Instance != null && button != null)
            {
                ApplicationLauncher.Instance.RemoveModApplication(button);
            }
        }

        #endregion

        #region Get Mission

        // Return list of the mission. Used to display mission
        public List<Mission> get_missions()
        {
            return this.missions;
        }

        public List<Mission> get_reverse_missions()
        {
            List<Mission> reverse = this.missions;
            reverse.Reverse();
            return reverse;
        }

        // Get mission from the list by ID. Used to display mission
        public Mission get_mission_by_index(int index)
        {
            // check the mission index...
            // I'm not sure that this is the right way to do that
            if (this.missions[index] != null)
                return this.missions[index];

            return null;
        }

        // Get the mission from vessel. Used during creating of Entry
        private Mission find_the_mission(Vessel ves)
        {
            Guid id = ves.id; // this is not very good for undocing. New GUID will be created in that case...
            return find_the_mission(id);
        }

        private Mission find_the_mission(ProtoVessel ves)
        {            
            return find_the_mission(ves.vesselID);
        }

        private Mission find_the_mission(Guid id)
        {
            foreach (Mission mission in this.missions)
                if (mission.missionId == id)
                {
                    KDebug.Log("Mission found: " + id.ToString());

                    return mission;
                }
            KDebug.Log("MISSION NOT FOUND FROM find_the_mission");
            return null;
        }

        // Get the current mission from ActiveVessel.
        private Mission get_current_mission()
        {
            if (FlightGlobals.ActiveVessel != null)
            {
                return find_the_mission(FlightGlobals.ActiveVessel);
            }

            KDebug.Log("MISSION NOT FOUND ");
            return null; // ??
        }


        #endregion

        #region Report Manager Properties


        #endregion

        #region Add Mission
        // Create a new mission by Mission object. Used to crate and load mission.
        private void add_mission(Mission M)
        {
            KDebug.Log("Add mission from Report by Mission: " + M.get_name());
            if(!this.is_missionId_exists(M))
                this.missions.Add(M);
            else
            {
                KDebug.Log("Mission already exists: " + M.get_name());
            }

        }
        // Used to create mission
        private Boolean is_missionId_exists(Mission M)
        {
            return is_missionId_exists(M.missionId);
        }
        private Boolean is_missionId_exists(Guid id)
        {
            foreach(Mission M in this.missions)
            {
                if (M.missionId == id) return true;
            }
            return false;
        }

        #endregion

        #region Delete Mission
        public void detele_mission_by_index(int index)
        {
            KDebug.Log("Delete Mission " + index.ToString());
            // check if the entry exist then delete            
            if (missions[index] != null)
            {
                KDebug.Log("Mission deleted");
                missions.RemoveAt(index);
            }
        }
        #endregion

        #region Add Entry

        // Add message to current mission. Add message for current vessel
        public void add_message(Entry.Situations S, String message){

            KDebug.Log("add_message: " + message);

            Mission M = this.get_current_mission();
            if (M != null)
                M.add_entry(S, message);
        }

        // Add message to mission related to other vessel
        public void add_message(Entry.Situations S, Vessel ves, String message)
        {

            KDebug.Log("add_message: " + message);

            Mission M = this.find_the_mission(ves);
            if (M != null)
                M.add_entry(S, ves, message);
            else
                add_message(S, message); // Here we will try to find active vessel
        }

        // Specific add method for ProtoVessel
        public void add_message(Entry.Situations S, ProtoVessel ves)
        {
            KDebug.Log("add_message with ProtoVessel");

            Mission M = this.find_the_mission(ves);
            if (M != null)
                M.add_entry(S, ves, "");
            else
                add_message(S, ""); // Here we will try to find active vessel
        }

        // Add message from coroutine. We check the situation and change the situation
        public void add_situation_message(Entry.Situations S, Vessel ves, String message, Vessel.Situations situation)
        {

            KDebug.Log("add_situation_message: " + message);

            Mission M = this.find_the_mission(ves);
            if (M != null & M.get_situation() != situation)
            {
                KDebug.Log("situation change from : " + M.get_situation().ToString() + " to: " + situation.ToString());
                M.set_situation(situation);
                if (ves.Landed)
                    message = message + " at: " + FlightGlobals.currentMainBody.name + " (" + FlightGlobals.currentMainBody.BiomeMap.GetAtt(ves.latitude * Mathf.Deg2Rad, ves.longitude * Mathf.Deg2Rad).name + ")";

                M.add_entry(S, ves, message);
            }

        }

        // Function start and stom the corutine. We need to create a delay for our methods.
        private void manage_corutine(Entry.Situations S, Vessel ves, String message, Vessel.Situations situation)
        {
            KDebug.Log("manage_corutine");

            
            this.objCorutine.ves = ves;
            this.objCorutine.message = message;
            this.objCorutine.vessel_situation = situation;
            this.objCorutine.situation = S;

            if (this._situationRunning)
            {
                KDebug.Log("stop running corutine");

                Kistory.StopCoroutine("add_delayed_message");
            }

            KDebug.Log("Start corutine");
            this._situationRunning = true;
            Kistory.StartCoroutine("add_delayed_message", this.objCorutine);
        }

        #endregion

        #region Button events
        private void button_OnToggleTrue()
        {
            // Code here
            //PilotAssistantFlightCore.bDisplayAssistant = true;
            KDebug.Log("New button Toggle on ");

            Kistory.ShowWindow();
        }

        private void button_OnToggleFalse()
        {
            // Code here
            //PilotAssistantFlightCore.bDisplayAssistant = false;
            KDebug.Log("New button Toggle off ");
            Kistory.CloseWindow();
        }
        #endregion

        // (C) FinalFrontier mod
        private String get_root_path()
        {
            String path = KSPUtil.ApplicationRootPath;
            path = path.Replace("\\", "/");
            if (path.EndsWith("/")) path = path.Substring(0, path.Length - 1);
            //
            return path;
        }

        #region Clear
        // Clear list of the mission. Clear memory.
        public void clear_missions()
        {
            KDebug.Log("Clear missions");   
            this.missions.Clear();
        }

        // Calld in the descructor of the Kistory plugin
        public void clear()
        {
            GameEvents.onGUIApplicationLauncherReady.Remove(this.add_button); // add
            GameEvents.onGUIApplicationLauncherUnreadifying.Remove(this.remove_button); // remove

            GameEvents.onGameStateLoad.Remove(this.on_game_load);
            GameEvents.onGameStateSave.Remove(this.on_game_save);

            GameEvents.onVesselCreate.Remove(this.on_create);

            GameEvents.onLaunch.Remove(this.on_launch);

            GameEvents.onCrewOnEva.Remove(this.on_EVA);
            GameEvents.onCrewBoardVessel.Remove(this.on_board);
            GameEvents.onCrewKilled.Remove(this.on_killed);

            GameEvents.onPartDie.Remove(this.on_destroyed); 
            GameEvents.onPartExplode.Remove(this.on_explode);

            GameEvents.onVesselSituationChange.Remove(this.on_situation);

            GameEvents.onVesselSOIChanged.Remove(this.on_soi);

            GameEvents.Contract.onFinished.Remove(this.on_contract);

            GameEvents.onStageActivate.Remove(this.on_stage);

        }

        #endregion
    }
}
