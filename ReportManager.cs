using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Kistory
{
    // Singlton that manage the report in the game
    // Containg the missions
    class ReportManager
    {

        public Kistory Kistory; 

        private List<Mission> missions = new List<Mission>();

        public enum Situations
        {
            CREATE,
            LAUNCH,
            EVA,
            CRASH
        }

        private struct MissionStrings
        {
            public static String CREATE = "The new mission";
            public static String LAUNCH = "Lauched!";
            public static String EVA = "Kerbal does to Extra-Vehicular Activity";
            public static String KILLED = "Kerbal killed";
            public static String BOARD = "Kerbal board";
            public static String CRASH = "Chash!";
            public static String DESTROYED = "Part destroyed";
            public static String EXPLODE = "Part exploded";
            public static String SITUATION = "Situation changed";
                public static String ORBIT = "Orbit closed";
                public static String ESCAPE = "On escape trajectory";
            public static String SOI = "Main body changed";
            public static String CONTRACT = "Contract finished";
            public static String STAGE = "Stage activated";
        }

        private bool _situationRunning = false;
        public EntryCorutine objCorutine = new EntryCorutine();

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
            Debug.Log("[Kistory] New ReportManager Created");
            // === Event No Active Vessel (?) === //
            GameEvents.onGameStateLoad.Add(this.on_game_load); // Load Kistory from the save file
            GameEvents.onGameStateSave.Add(this.on_game_save); // Save Kistory to the save file

            GameEvents.onVesselCreate.Add(this.on_create); // When Vessel is created and there is no existing mission - create mission
            
            GameEvents.onCrewBoardVessel.Add(this.on_board); // Kerbal board to the mission Vessel (active vessel is Kerbal)
            GameEvents.onCrewKilled.Add(this.on_killed);     // Kerbal died. Mission will not be found if Kerbal died outside the Vessel

            // === Events with Active Vessel === //
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

        private void on_game_load(ConfigNode nodeGame)
        {
            if (nodeGame != null)
            {
                Debug.Log("[Kistory] Loading game...");
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
                        if (nodeMission.HasValue("missionId"))
                        {
                            String missionId = nodeMission.GetValue("missionId");
                            String missionName = nodeMission.GetValue("missionName");
                            double missionTime = Convert.ToDouble(nodeMission.GetValue("missionTime"));
                            // create mission
                            Mission M = new Mission(new Guid(missionId), missionName, missionTime);

                            nodeName = "Entry";
                            if (nodeMission.HasNode("Entry"))
                            {
                                // ENTRY
                                ConfigNode[] nodeEnties = nodeMission.GetNodes("Entry");
                                foreach (ConfigNode nodeEntie in nodeEnties)
                                {
                                    if (nodeEntie.HasValue("message"))
                                    {
                                        M.load_entry(nodeEntie.GetValue("message"), Convert.ToDouble(nodeEntie.GetValue("time")));
                                    }
                                }
                            }

                            this.add_mission(M);
                        }
                    }
                }
            }
        }
        private void on_game_save(ConfigNode node)

        {
            Debug.Log("[Kistory] on_game_save node");

            // Walk thought all missions
            
            var nodeKistory = node.AddNode("Kistory");

            foreach (Mission M in this.missions)
            {
                var nodeMission = nodeKistory.AddNode("Mission");
                nodeMission.AddValue("missionId", M.missionId);
                nodeMission.AddValue("missionName", M.get_name());
                nodeMission.AddValue("missionTime", M.get_time());

                // Walk thought all messages of the mission
                foreach (Entry E in M.get_entries())
                {
                    var nodeEntry = nodeMission.AddNode("Entry");
                    nodeEntry.AddValue("message", E.get_message()); 
                    nodeEntry.AddValue("time", E.get_time()); 
                }
            }

        }

        private void on_create(Vessel ves) // Triggered by creating a new vessel. Apparantelly we create a new mission.
        {
            // This event should be called only if we have a new mission
            if (ves !=null)
            {
                Debug.Log("[Kistory] on_create");

                Mission M = new Mission(ves); // Possible new mission
                if (M.missionApproved) // Mission was created
                {
                    Debug.Log("[Kistory] on_create approved");
                    Entry E = new Entry();
                    E.add(MissionStrings.CREATE, (double) 0);
                    M.add_entry( E );
                    this.add_mission(M);
                }
            }
        }

        private void on_launch(EventReport data) // Triggered on launch
        {
            Debug.Log("[Kistory] on_launch");
            this.add_message(MissionStrings.LAUNCH);
            //this.make_screenshot("Launch");
        }

        private void on_EVA(GameEvents.FromToAction<Part, Part> data)
        {
            Debug.Log("[Kistory] on_EVA " + data.to.vessel.vesselName);
            this.add_message(MissionStrings.EVA + " : " + data.to.vessel.vesselName);
            //this.make_screenshot("EVA");
        }

        private void on_killed(EventReport report)
        {
            // If Kerbin died outside the vessel we would not be able to find that.
            Debug.Log("[Kistory] on_killed " + report.sender);
            this.add_message(MissionStrings.KILLED + " " + report.sender);
        }

        private void on_board(GameEvents.FromToAction<Part, Part> data)
        {
            Debug.Log("[Kistory] on_board " + data.from.vessel.vesselName);
            this.add_message(data.to.vessel, MissionStrings.BOARD + " : " + data.from.vessel.vesselName);
        }

        private void on_explode(GameEvents.ExplosionReaction r)
        {
            Debug.Log("[Kistory] on_explode: " + r.magnitude.ToString());
            //this.add_message(MissionStrings.EXPLODE + " :" + r.magnitude.ToString());
        }

        private void on_destroyed(Part part)
        {

            Debug.Log("[Kistory] on_destroyed: " + part.partInfo.title);
            
           // foreach (PartModule module in part.Modules)
           //     Debug.Log("[Kistory] on_destroyed: [" + module.moduleName + "] " + module.GetInfo() + " | " + module.guiText + " | " + module.name); //[0].GetInfo()
            this.add_message(MissionStrings.DESTROYED + " :" + part.partInfo.title);
        }

        private void on_crash(EventReport data)
        {
            // Happens even before Launch

            if(data.origin != null)
            { 
                Debug.Log("[Kistory] explosionPotential  " + data.origin.explosionPotential.ToString());
                Debug.Log("[Kistory] flightID  " + data.origin.flightID);
                Debug.Log("[Kistory] launchID  " + data.origin.launchID);
                Debug.Log("[Kistory] missionID  " + data.origin.missionID);
            }

            Debug.Log("[Kistory] on_crash");            
            this.add_message(MissionStrings.CRASH);
        }

        private void on_situation( GameEvents.HostedFromToAction< Vessel, Vessel.Situations > data)
        {
            if(data.host != null)
                if (data.host.isActiveVessel)
                {
                    Debug.Log("[Kistory] on_situation: " +  data.host.vesselName +" " + data.from.ToString() + " -> " + data.to.ToString());
                    if (data.to != Vessel.Situations.PRELAUNCH) // bug or feature?
                        manage_corutine(data.host, MissionStrings.SITUATION + ": from " + data.from.ToString() + " to:" + data.to.ToString());
                    
                }
        }

        private void on_soi(GameEvents.HostedFromToAction< Vessel, CelestialBody > data)
        {
            Debug.Log("[Kistory] on_soi");
            add_message(data.host, MissionStrings.SOI + " from: " + data.from.bodyName + " to: " + data.to.bodyName);
        }

        private void on_contract(Contracts.Contract contract)
        {
            Debug.Log("[Kistory] on_contract");
            add_message(MissionStrings.CONTRACT + " : [" + contract.ContractState.ToString() + "] " + contract.Title );
        }

        private void on_stage(int stage)
        {
            Debug.Log("[Kistory] on_stage");
            add_message(MissionStrings.STAGE + " #" + stage.ToString());
        } 

        #endregion

        #region Get Mission

        // Return list of the mission. Used to display mission
        public List<Mission> get_missions()
        {
            return this.missions;
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
            foreach (Mission mission in this.missions)
                if (mission.missionId == id)
                {
                    Debug.Log("[Kistory] Mission found: " + id.ToString());

                    return mission;
                }
            Debug.Log("[Kistory] MISSION NOT FOUND FROM find_the_mission");
            return null;
        }

        // Get the current mission from ActiveVessel.
        private Mission get_current_mission()
        {
            if (FlightGlobals.ActiveVessel != null)
            {
                return find_the_mission(FlightGlobals.ActiveVessel);
            }


            Debug.Log("[Kistory] MISSION NOT FOUND ");
            return null; // ??
        }


        #endregion

        #region Report Manager Properties


        #endregion


        #region Add Mission
        // Create a new mission by Mission object. Used to crate and load mission.
        private void add_mission(Mission M)
        {
            Debug.Log("[Kistory] Add mission from Report by Mission: " + M.get_name());
            if(!this.is_missionId_exists(M))
                this.missions.Add(M);
            else
            {
                Debug.Log("[Kistory] Mission already exists: " + M.get_name());
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

        #region Add Entry

        // Add message to current mission. Add message for current vessel
        public void add_message(String message){

            Debug.Log("[Kistory] add_message: " + message);

            Mission M = this.get_current_mission();
            if (M != null)
                M.add_entry(message);
        }

        // Add message to mission related to other vessel
        public void add_message(Vessel ves, String message)
        {

            Debug.Log("[Kistory] add_message: " + message);

            Mission M = this.find_the_mission(ves);
            if (M != null)
                M.add_entry(ves, message);
        }



        private void manage_corutine(Vessel ves, String message)
        {
            Debug.Log("[Kistory] manage_corutine");

            
            this.objCorutine.ves = ves;
            this.objCorutine.message = message;

            if(this._situationRunning)
            {
                Debug.Log("[Kistory] stop running corutine");

                Kistory.StopCoroutine("add_delyed_message");
            }

            Debug.Log("[Kistory] Start corutine");
            this._situationRunning = true;
            Kistory.StartCoroutine("add_delyed_message", this.objCorutine);
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
            Debug.Log("[Kistory] Clear missions");   
            this.missions.Clear();
        }

        // Calld in the descructor of the Kistory plugin
        public void clear()
        {
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
