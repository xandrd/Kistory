using System;
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
        private List<Mission> missions = new List<Mission>();

        public enum Situations
        {
            CREATE,
            LAUNCH,
            EVA,
            CRASH
        }

        public struct MissionStrings
        {
            public static String CREATE = "The new mission";
            public static String LAUNCH = "Lauched!";
            public static String EVA = "Extra-Vehicular Activity";
            public static String CRASH = "Chash!";
            public static String DESTROYED = "Part destroyed";
            public static String EXPLODE = "Part exploded";
        }

        // Singlton
        private static volatile ReportManager instance;        
        public static ReportManager Instance()
        {
            if (instance == null)
            {
                instance = new ReportManager();         
            }
            return instance;
        }
        private ReportManager()
        {
            Debug.Log("[Kistory] New ReportManager Created");
            GameEvents.onGameStateCreated.Add(this.on_game_created); // game created on loaded
            GameEvents.onGameStateSaved.Add(this.on_game_save);      // game saved. 
            //GameEvents.onGameStateLoad.Add(on_load_game); // should be exculded


            GameEvents.onVesselCreate.Add(this.on_create); // Here we propaply need to create a new mission
            GameEvents.onLaunch.Add(this.on_launch);
            GameEvents.onCrewOnEva.Add(this.on_EVA);
            //GameEvents.onCrash.Add(this.on_crash); // We don't need this event
            GameEvents.onPartDie.Add(this.on_destroyed); // That is good event to capture things
            GameEvents.onPartExplode.Add(this.on_explode); // Maybe explode?
        }

        public void on_game_created(Game newgame)
        {
            // We eather create a new game of load the existing game
            Debug.Log("[Kistory] on_game_created");
            

            String filePath = this.get_root_path() + "/saves/" + HighLogic.SaveFolder;
            var nodeGame = ConfigNode.Load(filePath + "/Kistory.sav"); // Generate warning. It is better the check the file existance
            
            if (nodeGame != null) 
            {
                Debug.Log("[Kistory] Load Game");
                this.clear_missions();
                
                // REPORT
                String nodeName = "Report";
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
                                // create mission
                                Mission M = new Mission(new Guid(missionId), missionName);

                                nodeName = "Entry";
                                if (nodeMission.HasNode("Entry"))
                                {
                                    // ENTRY
                                    ConfigNode[] nodeEnties = nodeMission.GetNodes("Entry");
                                    foreach (ConfigNode nodeEntie in nodeEnties)
                                    {
                                        if (nodeEntie.HasValue("message"))
                                        {
                                            M.add_entry(nodeEntie.GetValue("message"));
                                        }
                                    }
                                }

                                this.add_mission(M);
                            }
                        }
                    }
            }
            else
            {
                Debug.Log("[Kistory] Save file no found. New Game.");
                this.clear_missions();
            }
        }

        public void on_game_save(Game thisGame)
        {            
            Debug.Log("[Kistory] on_game_save");

            // Basic container. 
            var nodeReport = new ConfigNode("Report");

            // Walk thought all missions
            foreach (Mission M in this.missions)
            {
                var nodeMission = nodeReport.AddNode("Mission");
                nodeMission.AddValue("missionId", M.missionId);
                nodeMission.AddValue("missionName", M.get_name());
                
                // Walk thought all messages of the mission
                foreach (Entry E in M.get_entries())
                {
                    var nodeEntry = nodeMission.AddNode("Entry");                    
                        nodeEntry.AddValue("message", E.get_message() ); // one message per Entry at the moment
                }
            }
            String filePath = this.get_root_path() + "/saves/" + HighLogic.SaveFolder;
            nodeReport.Save(filePath + "/Kistory.sav");
        }

        // (C) FinalFrontier mod
        private String get_root_path()
        {
            String path = KSPUtil.ApplicationRootPath;
            path = path.Replace("\\", "/");
            if (path.EndsWith("/")) path = path.Substring(0, path.Length - 1);
            //
            return path;
        }

        public void on_create(Vessel ves) // Triggered by creating a new vessel.
        {
            // This event should be called only if we have a new mission
            if (ves !=null)
            {
                Debug.Log("[Kistory] on_create");

                Mission M = new Mission(ves); // Possible new mission
                if (M.missionApproved) // Mission was created
                {
                    Debug.Log("[Kistory] on_create approved");                    
                    M.add_entry( MissionStrings.CREATE );
                    this.add_mission(M);
                    
                }
            }
        }

        public void on_launch(EventReport data) // Triffered on launch
        {
            Debug.Log("[Kistory] on_launch");
            this.add_message(MissionStrings.LAUNCH);
            //this.make_screenshot("Launch");
        }

        public void on_EVA(GameEvents.FromToAction<Part, Part> data)
        {
            Debug.Log("[Kistory] on_EVA");
            this.add_message(MissionStrings.EVA);
            //this.make_screenshot("EVA");
        }

        public void on_explode(GameEvents.ExplosionReaction r)
        {
            Debug.Log("[Kistory] on_explode: " + r.magnitude.ToString());
            this.add_message(MissionStrings.EXPLODE + " :" + r.magnitude.ToString());
        }


        public void on_destroyed(Part part)
        {
            
            Debug.Log("[Kistory] on_destroyed: " + part.name);
            foreach (PartModule module in part.Modules)
                Debug.Log("[Kistory] on_destroyed: [" + module.GUIName + "] " + module.GetInfo()   ); //[0].GetInfo()

            
               
            this.add_message(MissionStrings.DESTROYED + " :" + part.name);
        }

        public void on_crash(EventReport data)
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

        public List<Mission> get_missions()
        {
            return this.missions;
        }

        public Mission get_mission(int index)
        {
            // check the mission index...
            // I'm not sure that this is the right way to do that
            if (this.missions[index] != null)
                return this.missions[index];

            return null;
        }

        public void clear_missions()
        {
            Debug.Log("[Kistory] Clear missions");   
            this.missions.Clear();
        }

        /*
        // Create a new mission by vessel
        private void add_mission(Vessel ves){            
            Debug.Log("[Kistory] Add mission from Report by Vessel: " + ves.vesselType.ToString());
            if(ves.vesselType == VesselType.Ship || ves.vesselType == VesselType.Probe )
                this.missions.Add(new Mission(ves));
        }

        // Create a new mission by id
        private void add_mission(Guid id, String name)
        {
            Debug.Log("[Kistory] Add mission from Report by id: " + id.ToString());
            this.missions.Add(new Mission(id, name));
        }*/

        // Create a new mission by Mission object
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
        
        // Add message to current mission        
        private void add_message(String message){

            Debug.Log("[Kistory] add_message: " + message);

            Mission M = this.get_current_mission();
            if (M != null)
                M.add_entry(message);
        }

        private Mission get_current_mission()
        {
            if(FlightGlobals.ActiveVessel != null)
            {
                Guid id = FlightGlobals.ActiveVessel.id; // this is not very good for undocing. New GUID will be created in that case...
                foreach (Mission mission in this.missions)
                    if (mission.missionId == id)
                    {
                        Debug.Log("[Kistory] Mission found: " + id.ToString());
                        return mission;
                    }
            }

            Debug.Log("[Kistory] MISSION NOT FOUND ");
            return null; // ??
        }

        public void clear()
        {
            GameEvents.onVesselCreate.Remove(this.on_create);
            GameEvents.onLaunch.Remove(this.on_launch);
            GameEvents.onCrewOnEva.Remove(this.on_EVA);
            GameEvents.onCrash.Remove(this.on_crash);

        }
    }
}
