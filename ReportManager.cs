using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;


namespace Kistory
{


    // Singlton that manage the report in the game
    // Containg the missions
    class ReportManager : MonoBehaviour
    {

        // public Kistory kistory; 

        private static List<Mission> missions = new List<Mission>();

        private bool _situationRunning = false;
        public EntryCorutine objCorutine = new EntryCorutine();

        //screeshot message, as a second so it not interfier with the game
        private DateTime eventTime;

        private bool isLoaded = false; // Indicates if the game was loaded. We don't save the game before load

        // Singlton
        //private static volatile ReportManager instance;        
        /*private static volatile ReportManager instance;        
        public static ReportManager Instance()
        {
            if (instance == null)
            {
                KDebug.Log("ReportManager Instance()", KDebug.Type.MONO);
                this.instance = new ReportManager();
                //instance = new GameObject("ReportManager").AddComponent<ReportManager>();
            }
            return instance;
        }*/

        #region Event Subscribe
        public void Awake()
        {
            KDebug.Log("New ReportManager Created", KDebug.Type.EVENT);
            // == Add and remove button == //


            KDebug.Log("Events No Active Vessel", KDebug.Type.EVENT);
            // === Event No Active Vessel (?) === //
            GameEvents.onGameStateLoad.Add(this.on_game_load); // Load Kistory from the save file
            GameEvents.onGameStateSave.Add(this.on_game_save); // Save Kistory to the save file

            GameEvents.onVesselCreate.Add(on_create); // When Vessel is created and there is no existing mission - create mission
            GameEvents.onVesselLoaded.Add(on_vessel_loaded);

            //GameEvents.onNewVesselCreated.Add(this.on_new_create);
            GameEvents.onVesselRecovered.Add(this.on_recovered);
            // Consider GameEvents.VesselSituation.onReturnFromOrbit // Fired when a vessel returns from orbiting on a celestial body (?)
            // Consider GameEvents.VesselSituation.onReturnFromSurface // Fired when a vessel returns from landing on a celestial body (?)

            //GameEvents.onVesselRename.Add(this.on_detached_rename);
            //GameEvents.onVesselChange.Add(this.on_vessel_changed);

            // CREW
            // TBC - To Be Changed
            GameEvents.onCrewBoardVessel.Add(on_board); // Kerbal board to the mission Vessel (active vessel is Kerbal)
            // TBC - To Be Changed
            GameEvents.onCrewKilled.Add(on_killed);     // Kerbal died. Mission will not be found if Kerbal died outside the Vessel
            // TBC - To Be Changed
            GameEvents.onCrewOnEva.Add(on_EVA); // EVA

            GameEvents.onVesselCrewWasModified.Add(on_crew_modified);

            // === Events with Active Vessel === //
            KDebug.Log("Events with Active Vessel", KDebug.Type.EVENT);
            // TBC - To Be Changed
            //GameEvents.onLaunch.Add(this.on_launch); // Squad calls this Liftof! 
            // Alternative  
            GameEvents.VesselSituation.onLaunch.Add(on_launch);

            // GameEvents.onVesselDocking.Add(on_dock); // TBD in KSP 1.4


            GameEvents.onPartDie.Add(on_destroyed); // Scructural damage, part destroyed.
            GameEvents.onPartExplode.Add(on_explode); // When exposion happens we may take the awesomness. We not going to use this feature now.

            // GameEvents.onVesselDestroy.Add(on_crash); // Vessel is destroyed. Is calls evey time, even when unloaded. 3 times when actually crashed.

            GameEvents.onVesselSituationChange.Add(on_situation);
            // Consider GameEvents.VesselSituation.onLand // Fired when a vessel lands on a celestial body
            // Consider GameEvents.VesselSituation.onOrbit // Fired when a vessel achieves orbit around a celestial body
            // Consider GameEvents.VesselSituation.onReachSpace // 	Fired when a vessel reaches space
            // Consider GameEvents.VesselSituation.onEscape  //Fired when a vessel escapes from a celestial body
            // Consider GameEvents.VesselSituation.onFlyBy  // Fired when a vessel starts a flyby past a celestial body


            GameEvents.onVesselSOIChanged.Add(on_soi);

            GameEvents.Contract.onFinished.Add(on_contract);

            GameEvents.onStageActivate.Add(on_stage);
            //EventData<Contracts.Contract> GameEvents.Contract.onFinished
        }

        /*private void SceneManager_activeSceneChanged(Scene arg0, Scene arg1)
        {
            KDebug.Log("Scene change:" + arg0.name + " -> " + arg1.name, KDebug.Type.MONO);
            throw new NotImplementedException();
        }*/
        #endregion

        #region Evet Processing

        // Save Load
        private void on_game_load(ConfigNode nodeGame)
        {
            if (nodeGame != null)
            {
                KDebug.Log("Loading game...", KDebug.Type.LOAD);
                this.isLoaded = true;
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
                            Vessel.Situations missionSituation = (Vessel.Situations)Enum.Parse(typeof(Vessel.Situations), nodeMission.GetValue("missionSituation")); // I hope that will work
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
                                        Entry.Situations S = (Entry.Situations)Enum.Parse(typeof(Entry.Situations), nodes);
                                        M.load_entry(S, nodem, t);

                                        // Screeshot
                                        if (nodeEntie.HasValue("screenshot"))
                                            M.add_screenshot(nodeEntie.GetValue("screenshot"));
                                    }
                                    else { KDebug.Log("on load one of the Entry field is missing", KDebug.Type.LOAD); }
                                }
                            }

                            this.add_mission(M);
                        } else { KDebug.Log("on load one of the Mission field is missing", KDebug.Type.LOAD); }
                    }
                }
            }
        }
        private void on_game_save(ConfigNode node)
        {
            if (isLoaded) // Save note only if the game was loaded before
            {
                KDebug.Log("on_game_save node", KDebug.Type.LOAD);

                // Mod node
                var nodeKistory = node.AddNode("Kistory");

                // Walk thought all missions                        
                foreach (Mission M in missions)
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
                        nodeEntry.AddValue("situation", E.get_situation());
                        nodeEntry.AddValue("message", E.get_message());
                        nodeEntry.AddValue("time", E.get_time());
                        if (E.has_screeshot)
                            nodeEntry.AddValue("screenshot", E.get_screenshot());
                    }
                }
            }
        }
        // Load the save file on the start (if it has not been done)
        /*private void on_start_load()
        {
            ConfigNode node = ConfigNode.Load(KSPUtil.ApplicationRootPath + "saves/" + HighLogic.fetch.GameSaveFolder + "/persistent.sfs");
            if(!isLoaded)
                on_game_load(node);
        }
        // Save the data on change scence due to destroy. 
        private void on_end_save()
        {
            ConfigNode node = ConfigNode.Load(KSPUtil.ApplicationRootPath + "saves/" + HighLogic.fetch.GameSaveFolder + "/persistent.sfs");
            on_game_save(node);
        }*/

        // Events
        private void on_create(Vessel ves) // Triggered by creating a new vessel. Apparantelly we create a new mission. However, decoupling also create the new mission. Unfortunatelly, we cannot find the name here.
        {
            // This event should be called only if we have a new mission
            KDebug.Log("on_create", KDebug.Type.CREATE);

            Mission M = new Mission(ves); // Possible new mission (ves is check for null)
            if (M.missionApproved) // Mission was created
            {
                KDebug.Log("on_create approved", KDebug.Type.CREATE);
                int CM = ves.FindPartModulesImplementing<ModuleCommand>().Count();

                KDebug.Log("Command Modules: " + CM.ToString(), KDebug.Type.CREATE); // should wotk!

                //Entry E = new Entry();       
                if (ves.situation == Vessel.Situations.PRELAUNCH)
                {
                    KDebug.Log("create " + M.get_name(), KDebug.Type.CREATE);

                    this.add_mission(M);
                    add_event(Entry.Situations.CREATE, ves, M.get_name());
                    //E.add(Entry.Situations.CREATE, M.get_name(), (double)0);
                }
                else if (CM > 0) // Additional check to exclude debrees
                {
                    KDebug.Log("detached " + M.get_name(), KDebug.Type.CREATE);

                    // TODO
                    // Find the command module and name the mission by command module (if we cannot find how to name it wihtout commant module)
                    // Let's find the command module                    

                    Part P = ves.FindPartModulesImplementing<ModuleCommand>().Last().part;

                    this.add_mission(M);
                    M.rename(P.partInfo.title);
                    //add_event(Entry.Situations.DETACHED, ves, M.get_name());

                    add_event(Entry.Situations.DETACHED, ves, P.partInfo.title);

                    //E.add(Entry.Situations.DETACHED, M.get_name(), (double)0);
                    // To collect the name of the mission we probably need to listed Rename event
                }
                else
                {
                    return;
                }
                // M.add_entry( E ); // I'm not sue I need this

            }
        }

        #region Temporary or not in use functions
        /// TEMPORARY FUNCTION
        private void on_vessel_loaded(Vessel ves)
        {

            KDebug.Log("on loaded: ", KDebug.Type.EVENT);
            if (ves == null) return;

            VesselType type = ves.vesselType;

            if (type != VesselType.SpaceObject & type != VesselType.EVA & type != VesselType.Flag & type != VesselType.Debris)
            {
                // === Debug information ===
                KDebug.Log("Vessel debug information: ", KDebug.Type.CREATE);
                KDebug.Log("ves.name: " + ves.name, KDebug.Type.CREATE);
                KDebug.Log("ves.vesselName: " + ves.vesselName, KDebug.Type.CREATE);
                KDebug.Log("ves.GetInstanceID: " + ves.GetInstanceID().ToString(), KDebug.Type.CREATE);
                //KDebug.Log("ves.GetVessel: " + ves.GetVessel().ToString());
                KDebug.Log("ves.vesselType: " + ves.vesselType.ToString(), KDebug.Type.CREATE);
                //KDebug.Log("ves.vesselRanges: " + ves.vesselRanges.ToString());
                //KDebug.Log("ves.vesselTransform: " + ves.vesselTransform.ToString());
                //KDebug.Log("ves.VesselValues: " + ves.VesselValues.ToString());
                //KDebug.Log("ves.GetType: " + ves.GetType().ToString());
                KDebug.Log("ves.GetTotalMass: " + ves.GetTotalMass().ToString(), KDebug.Type.CREATE);
                //KDebug.Log("ves.GetTransform: " + ves.GetTransform().ToString());
                //KDebug.Log("ves.tag: " + ves.tag.ToString());            
                KDebug.Log("ves.loaded: " + ves.loaded.ToString(), KDebug.Type.CREATE);
                KDebug.Log("ves.isActiveAndEnabled: " + ves.isActiveAndEnabled.ToString(), KDebug.Type.CREATE);
                KDebug.Log("ves.isActiveVessel: " + ves.isActiveVessel.ToString(), KDebug.Type.CREATE);
                KDebug.Log("ves.isCommandable: " + ves.isCommandable.ToString(), KDebug.Type.CREATE);
                KDebug.Log("ves.IsControllable: " + ves.IsControllable.ToString(), KDebug.Type.CREATE);
                KDebug.Log("ves.launchTime: " + ves.launchTime.ToString(), KDebug.Type.CREATE);
                KDebug.Log("ves.missionTime: " + ves.missionTime.ToString(), KDebug.Type.CREATE);
                KDebug.Log("ves.packed: " + ves.packed.ToString(), KDebug.Type.CREATE);
                KDebug.Log("ves.situation: " + ves.situation.ToString(), KDebug.Type.CREATE);
                KDebug.Log("ves.state: " + ves.state.ToString(), KDebug.Type.CREATE);
                //KDebug.Log("ves.ctrlState: " + ves.ctrlState.ToString());
                KDebug.Log("ves.currentStage: " + ves.currentStage.ToString(), KDebug.Type.CREATE);
            }
        }

        // Cannot find this event
        /*
        private void on_new_create(Vessel ves)
        {
            KDebug.Log("on_new_create " + ves.vesselType.ToString() + " " + ves.vesselName + " " + ves.name, KDebug.Type.EVENT);
        }
        */

        private void on_recovered(ProtoVessel ves, bool flag)
        {
            KDebug.Log("on_recovered " + ves.vesselType.ToString() + " " + ves.vesselName, KDebug.Type.EVENT);

            this.add_event(Entry.Situations.RECOVERED, ves);
        }

        // cannot find this event
        /*
        private void on_detached_rename(GameEvents.HostedFromToAction<Vessel, String> data)
        {
            KDebug.Log("on_rename", KDebug.Type.CREATE);
            Mission M = this.find_the_mission(data.host);
            if (M !=null)
            {                
                if(M.is_detached_mission())
                {
                    KDebug.Log("rename to " + data.to, KDebug.Type.CREATE);
                    M.rename(data.to);
                }

            }
        }
        */
        // All possible changes
        /*
        private void on_vessel_changed(Vessel ves)
        {
            KDebug.Log("on_vessel_changed " + ves.name + " " + ves.vesselName, KDebug.Type.EVENT);
        }
        */
        #endregion

        public void on_launch(EventReport data) // Triggered on launch (old version)
        {
            KDebug.Log("on_launch", KDebug.Type.CREATE);
            this.manage_photo_corutine(Entry.Situations.LAUNCH, FlightGlobals.ActiveVessel, "");
            //this.make_screenshot("Launch");
        }

        public void on_launch(Vessel ves) // Triggered on launch
        {
            KDebug.Log("on_launch", KDebug.Type.CREATE);
            this.manage_photo_corutine(Entry.Situations.LAUNCH, FlightGlobals.ActiveVessel, ""); // It seems like we need to use ActiveVessel to create corutine(!)
            //this.make_screenshot("Launch");
        }

        private void on_dock(EventData<uint, uint> data)
        {
            KDebug.Log("on_dock + Event name" + data.EventName);
        }

        private void on_EVA(GameEvents.FromToAction<Part, Part> data)
        {
            // data.to.vessel is Kerbal and it has unique id (I think there should be an instanse of KerbalEVA somewhere after that)
            KDebug.Log("on_EVA " + data.to.vessel.vesselName + " " + data.to.vessel.rootPart.vessel.vesselName, KDebug.Type.EVENT);
            // On EVA we create a new mission
            Mission M = new Mission(data.to.vessel);
            this.add_mission(M);
            // this.add_event(Entry.Situations.CREATEEVA, data.to.vessel, M.get_name()); // from current
            this.manage_photo_corutine(Entry.Situations.CREATEEVA, data.to.vessel, M.get_name()); // from current

            this.add_event(Entry.Situations.EVA, data.to.vessel.vesselName); // from active 
        }

        private void on_killed(EventReport report)
        {
            // If Kerbin died outside the vessel we would not be able to find that.
            // The Kebal EVA vessel has different id at this moment
            KDebug.Log("on_killed " + report.sender, KDebug.Type.EVENT);
            this.add_event(Entry.Situations.KILLED, report.sender);
        }

        private void on_board(GameEvents.FromToAction<Part, Part> data)
        {
            // Kerbal has the same id as before eva
            KDebug.Log("on_board " + data.from.vessel.vesselName, KDebug.Type.EVENT);

            this.add_event(Entry.Situations.ENDEVA, data.from.vessel, data.from.vessel.vesselName); // from current
            this.add_event(Entry.Situations.BOARD, data.to.vessel, data.from.vessel.vesselName);
        }

        private void on_crew_modified(Vessel ves)
        {
            // Event called after a vessel had its crew modified in some way (added, removed, or killed).
            // KDebug.Log("on_crew_modified ", KDebug.Type.EVENT);
        }

        // Unfortunatelly, we dont know what exploded
        private void on_explode(GameEvents.ExplosionReaction r)
        {
            KDebug.Log("on_explode: " + r.magnitude.ToString(), KDebug.Type.EVENT);
            if (r.magnitude >= 0.5 & r.distance < 100) // Take into account only object that were relatevely close
            {
                // We need to check if we are ready for the next courutine
                Mission M = this.get_current_mission();
                Vessel V = FlightGlobals.ActiveVessel;
                if (M != null)
                {
                    double marginTime = 1;
                    Entry E = M.get_last_exploded();
                    if (!M.is_too_soon(E, V, marginTime)) // call the courutine only if we ready for the next exposion of the current Vessel
                        this.manage_photo_corutine(Entry.Situations.EXPLODE, FlightGlobals.ActiveVessel, " (" + Math.Round(r.distance).ToString() + "m away)");
                }

            }

        }

        private void on_destroyed(Part part)
        {

            KDebug.Log("on_destroyed: " + part.partInfo.title + " " + part.vessel.id, KDebug.Type.EVENT);
            // If destroy happend less that some time ago we modify the string
            // Othervise we add entry
            Vessel V = part.vessel;
            String message = part.partInfo.title;
            Mission M = find_the_mission(part.vessel);
            if (M != null)
            {
                double marginTime = 1;
                Entry E = M.get_last_destroyed();
                if (M.is_too_soon(E, V, marginTime))
                    E.add_to_message(", " + message); // maybe we need count for parts... Let say more that 10 parts = photo
                else
                    this.add_event(Entry.Situations.DESTROYED, V, message);
            }
        }

        // We will use this even if the Vessal is destroyed
        private void on_crash(Vessel ves) // TBD
        {
            if (ves != null) // Many additional conditions are needed to be check (maybe at least ves.isloaded??)
            {
                if (ves.loaded) // This should help!
                {
                    KDebug.Log("Crashed (destroyed) flightID  " + ves.id + " Name " + ves.name, KDebug.Type.EVENT);
                    this.add_event(Entry.Situations.CRASH, ves, ""); // It will find the mission first
                }


            }
            KDebug.Log("on_crash", KDebug.Type.EVENT);

        }

        private void on_situation(GameEvents.HostedFromToAction<Vessel, Vessel.Situations> data)
        {
            if (data.host != null)
                if (data.host.isActiveVessel)
                {
                    KDebug.Log("on_situation: " + data.host.vesselName + " " + data.from.ToString() + " -> " + data.to.ToString(), KDebug.Type.EVENT);
                    if (data.to != Vessel.Situations.PRELAUNCH) // bug or feature?
                        manage_corutine(Entry.Situations.SITUATION, data.host, "from " + data.from.ToString() + " to:" + data.to.ToString(), data.to);
                    // else
                    // KDebug.Log("on_launch from situation", KDebug.Type.CREATE); // photo corutine may be good here!

                }
        }

        private void on_soi(GameEvents.HostedFromToAction<Vessel, CelestialBody> data)
        {
            KDebug.Log("on_soi", KDebug.Type.EVENT);
            add_event(Entry.Situations.SOI, data.host, " from: " + data.from.bodyName + " to: " + data.to.bodyName);
        }

        private void on_contract(Contracts.Contract contract)
        {
            KDebug.Log("on_contract", KDebug.Type.EVENT);
            add_event(Entry.Situations.CONTRACT, " [" + contract.ContractState.ToString() + "] " + contract.Title);
        }

        private void on_stage(int stage)
        {
            KDebug.Log("on_stage", KDebug.Type.EVENT);
            add_event(Entry.Situations.STAGE, " #" + stage.ToString());
        }
        #endregion

        #region Corutines

        // Corutine add message
        private IEnumerator add_delayed_event(EntryCorutine data)
        {
            KDebug.Log("pre add_delayed_event", KDebug.Type.CORUTINE);

            float waitTime = 3;
            //this._situationRunning = true;
            yield return new WaitForSeconds(waitTime);
            KDebug.Log("post add_delayed_event", KDebug.Type.CORUTINE);
            data.report.add_situation_event(data.situation, data.ves, data.message, data.vessel_situation);
            KDebug.Log("after add_delayed_event", KDebug.Type.CORUTINE);
        }

        // This corutine add event imideatelly and add photo later        
        private IEnumerator add_event_and_delayed_photo(EntryCorutine data)
        {
            KDebug.Log("pre add_event_and_delayed_photo", KDebug.Type.CORUTINE);
            Mission M = add_event_get_mission(data.situation, data.ves, data.message);
            if (M == null) yield break;

            int iE = M.get_last_entry_index(); // we should return index that we just added

            float waitTime = 1; // default wait time
            if (data.situation == Entry.Situations.EXPLODE)
                waitTime = 0.1f; // Faster watitime for explosion (capture cool photo)
            else if (data.situation == Entry.Situations.USERPHOTO)
                waitTime = 0.5f;
            yield return new WaitForSeconds(waitTime); // Now we wait
            KDebug.Log("post add_event_and_delayed_photo", KDebug.Type.CORUTINE);


            // We are serching for right filepath!
            // The name of the screenshot  should be constructed!            
            String dirName = KSPUtil.ApplicationRootPath + "saves/" + HighLogic.SaveFolder;
            var dirInfo = new System.IO.DirectoryInfo(dirName);
            // TODO: when the game is created
            //  if (!Directory.Exists(dirName + "/Kistory/Photo/"))
            //{
            //Directory.CreateDirectory(filePath + "/Kistory/Photo/");
            //}
            //String fileName = dirInfo.FullName + "/Kistory/Photo/" + str_eventTime() + data.situation.ToString();
            String fileName = dirInfo.FullName + "/" + str_eventTime() + data.situation.ToString();

            // Let's make sure that we don't have the file with this name
            int cntr = 0;
            FileInfo info = new FileInfo(fileName + cntr.ToString() + ".png");
            while (info != null & info.Exists != false)
            {
                cntr++;
                info = new FileInfo(fileName + cntr.ToString() + ".png");
            }
            fileName = fileName + cntr.ToString() + ".png";
            KDebug.Log("filename " + fileName, KDebug.Type.CORUTINE);

            KDebug.Log("Capturing screeshot for " + iE.ToString() + " in " + data.situation.ToString() + " " + fileName, KDebug.Type.CORUTINE);
            // ScreenCapture.CaptureScreenshot("SomeLevel");
            Application.CaptureScreenshot(fileName); // SCREEN SHOT
            while (!File.Exists(fileName))
            {
                yield return null; // apparently it should make the screeshot first and then continue
            }
            KDebug.Log("Trasfering screeshot for " + iE.ToString() + " in " + data.situation.ToString() + " " + fileName, KDebug.Type.CORUTINE);
            M.add_screenshot(fileName, iE);
            StartCoroutine("delayed_message", "Screenshot captured!"); // add message that we added the screenshot with 1s delay
        }

        private IEnumerator delayed_message(String message)
        {
            yield return new WaitForSeconds(1f);
            ScreenMessages.PostScreenMessage(message, 1f, ScreenMessageStyle.LOWER_CENTER);
        }

        // We use this time for Screeshots
        private String str_eventTime()
        {
            eventTime = DateTime.Now;
            return eventTime.Year.ToString() + eventTime.Month.ToString() + eventTime.Day.ToString() + eventTime.Hour.ToString() + eventTime.Minute.ToString() + eventTime.Second.ToString();
        }
        #endregion

        #region Get Mission

        // Return list of the mission. Used to display mission
        public List<Mission> get_missions()
        {
            return missions;
        }

        public List<Mission> get_reverse_missions()
        {
            List<Mission> reverse = missions;
            reverse.Reverse();
            return reverse;
        }

        // Get mission from the list by ID. Used to display mission
        public Mission get_mission_by_index(int index)
        {
            // check the mission index...
            // I'm not sure that this is the right way to do that
            if (missions[index] != null)
                return missions[index];

            return null;
        }

        // Get the mission from vessel. Used during creating of Entry
        private Mission find_the_mission(Vessel ves)
        {
            //Guid id = ves.id; // this is not very good for undocing. New GUID will be created in that case...
            Guid id = Mission.mission_id(ves);
            return find_the_mission(id);
        }

        private Mission find_the_mission(ProtoVessel ves)
        {
            return find_the_mission(Mission.mission_id(ves));
        }

        //
        // Main return mission function
        //       
        private Mission find_the_mission(Guid id)
        {
            foreach (Mission mission in missions)
                if (mission.missionId == id)
                {
                    KDebug.Log("Mission found: " + id.ToString(), KDebug.Type.EVENT);

                    return mission;
                }
            KDebug.Log("MISSION NOT FOUND FROM find_the_mission", KDebug.Type.EVENT);
            return null;
        }

        // Get the current mission from ActiveVessel.
        private Mission get_current_mission()
        {
            if (FlightGlobals.ActiveVessel != null)
            {
                return find_the_mission(FlightGlobals.ActiveVessel);
            }

            KDebug.Log("MISSION NOT FOUND ", KDebug.Type.EVENT);
            return null;
        }


        #endregion

        #region Report Manager Properties


        #endregion

        #region Add Mission
        // Create a new mission by Mission object. Used to crate and load mission.
        private void add_mission(Mission M)
        {
            KDebug.Log("Add mission from Report by Mission: " + M.get_name(), KDebug.Type.CREATE);
            if (!this.is_missionId_exists(M))
                missions.Add(M);
            else
            {
                KDebug.Log("Mission already exists: " + M.get_name(), KDebug.Type.CREATE);
            }

        }
        // Used to create mission
        private Boolean is_missionId_exists(Mission M)
        {
            return is_missionId_exists(M.missionId);
        }
        private Boolean is_missionId_exists(Guid id)
        {
            foreach (Mission M in missions)
            {
                if (M.missionId == id) return true;
            }
            return false;
        }

        #endregion

        #region Delete Mission
        public void detele_mission_by_index(int index)
        {
            KDebug.Log("Delete Mission " + index.ToString(), KDebug.Type.EVENT);
            // check if the entry exist then delete            
            if (missions[index] != null)
            {
                KDebug.Log("Mission deleted", KDebug.Type.EVENT);
                missions.RemoveAt(index);
            }
        }
        #endregion

        #region Add Entry

        // Add message to current mission. Add message for current vessel
        public void add_event(Entry.Situations S, String message) {

            KDebug.Log("add_message: " + message, KDebug.Type.EVENT);

            Mission M = this.get_current_mission();
            if (M != null)
                M.add_entry(S, message);
        }

        // Add message to mission related to other vessel
        public void add_event(Entry.Situations S, Vessel ves, String message)
        {

            KDebug.Log("add_message: " + message, KDebug.Type.EVENT);

            Mission M = this.find_the_mission(ves);
            if (M != null)
                M.add_entry(S, ves, message);
            //  We will use only explicit missions in this call
            // else
            //  add_event(S, message); // Here we will try to find active vessel

        }

        // Specific add method for ProtoVessel
        public void add_event(Entry.Situations S, ProtoVessel ves)
        {
            KDebug.Log("add_message with ProtoVessel", KDebug.Type.EVENT);
            Mission M = this.find_the_mission(ves);
            if (M != null)
                M.add_entry(S, "");
        }

        // Add message from coroutine. We check the situation and change the situation
        public void add_situation_event(Entry.Situations S, Vessel ves, String message, Vessel.Situations situation)
        {

            KDebug.Log("add_situation_event: " + message, KDebug.Type.CORUTINE);

            Mission M = this.find_the_mission(ves);
            if (M != null)
            {
                if (M.get_situation() != situation)
                {
                    KDebug.Log("situation change from : " + M.get_situation().ToString() + " to: " + situation.ToString(), KDebug.Type.EVENT);
                    M.set_situation(situation);
                    if (ves.Landed)
                        message = message + " at: " + FlightGlobals.currentMainBody.name + " (" + FlightGlobals.currentMainBody.BiomeMap.GetAtt(ves.latitude * Mathf.Deg2Rad, ves.longitude * Mathf.Deg2Rad).name + ")";
                    M.add_entry(S, ves, message);
                }
            }
        }

        // Add message from coroutine with photo
        public Mission add_event_get_mission(Entry.Situations S, Vessel ves, String message)
        {
            KDebug.Log("add_situation_event with Photo : " + message, KDebug.Type.CORUTINE);
            Mission M = this.find_the_mission(ves);
            if (M != null)
            {
                M.add_entry(S, ves, message);
                return M;
            }
            return null;

        }

        // Add user entry with photo
        public void add_user_photoentry(String msg)
        {
            manage_photo_corutine(Entry.Situations.USERPHOTO, FlightGlobals.ActiveVessel, msg);
        }
        
        // Function start and stom the corutine. We need to create a delay for our methods.
        private void manage_corutine(Entry.Situations S, Vessel ves, String message, Vessel.Situations situation)
        {
            KDebug.Log("manage_corutine", KDebug.Type.CORUTINE);

            
            this.objCorutine.ves = ves;
            this.objCorutine.message = message;
            this.objCorutine.vessel_situation = situation;
            this.objCorutine.situation = S;
            this.objCorutine.report = this;

            if (this._situationRunning)
            {
                KDebug.Log("stop running corutine", KDebug.Type.CORUTINE);

                StopCoroutine("add_delayed_event");
            }

            KDebug.Log("Start corutine", KDebug.Type.CORUTINE);
            this._situationRunning = true;
            StartCoroutine("add_delayed_event", this.objCorutine);
        }

        private void manage_photo_corutine(Entry.Situations S, Vessel ves, String message)
        {
            // CameraManager.Instance.currentCameraMode == Flight // CameraManager.CameraMode.Flight;
            // CameraManager.Instance.currentCameraMode == Map // CameraManager.CameraMode.Map;
            KDebug.Log("manage_photo_corutine", KDebug.Type.CORUTINE);
            this.objCorutine.ves = ves;
            this.objCorutine.message = message;
            this.objCorutine.situation = S;
            this.objCorutine.report = this;
            StartCoroutine("add_event_and_delayed_photo", this.objCorutine); // Get NullRef Exception. I'm not sure why
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
            KDebug.Log("Clear missions", KDebug.Type.EVENT);   
            missions.Clear();
        }

        // Calld in the descructor of the Kistory plugin
        public void clear()
        {
            // save changes before die
            // on_end_save();

            GameEvents.onGameStateLoad.Remove(on_game_load);
            GameEvents.onGameStateSave.Remove(on_game_save);

            GameEvents.onVesselCreate.Remove(on_create);
            GameEvents.onVesselLoaded.Remove(on_vessel_loaded);

            GameEvents.onVesselRecovered.Remove(on_recovered);


            GameEvents.onCrewBoardVessel.Remove(on_board);
            GameEvents.onCrewKilled.Remove(on_killed);

            //GameEvents.onLaunch.Remove(this.on_launch);
            GameEvents.VesselSituation.onLaunch.Remove(on_launch);

            GameEvents.onCrewOnEva.Remove(on_EVA);


            GameEvents.onPartDie.Remove(on_destroyed); 
            GameEvents.onPartExplode.Remove(on_explode);

            GameEvents.onVesselSituationChange.Remove(on_situation);

            GameEvents.onVesselSOIChanged.Remove(on_soi);

            GameEvents.Contract.onFinished.Remove(on_contract);

            GameEvents.onStageActivate.Remove(on_stage);
        }

        #endregion

        public void OnDestroy()
        {
            clear();
        }

    }
}
