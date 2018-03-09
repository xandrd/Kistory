using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Kistory
{
    // Class Mission correspond to single flying vessel (I hope)
    // Vessel should have a probe or crew capsule (as I belive it is commpandable module)
    // Mission class only operates with arrays of entries and contains information about the mission
    // It does not operate with event logic
    class Mission
    {        
        public Guid missionId;                  // Unique ID for the mission. Maybe we should take it from the 
        public Boolean missionApproved = false; // false if mission was not created. We should check this flag after we create an instance of the mission and we expect that mission is ok
        
        private String missionName;
        private List<Entry> entries = new List<Entry>(); // Container of Entries

        private double missionTime;

        private int lastDestoryed = -1;
        private int lastExployed = -1;

        private Vessel.Situations missionSituation; 

        #region Mission create

        // To create mission we need to udentity the ID
        // This needs to load missions
        public Mission(Guid id, String name, double time, Vessel.Situations situation)
        {
            KDebug.Log("Mission is creating by Id: " + id.ToString(), KDebug.Type.LOAD);
            this.missionId = id;
            this.missionName = name;
            this.missionApproved = true;
            this.missionTime = time;
            this.missionSituation = situation;
        }

        // Mission is created only if the have a Vessel
        // Check the vessel, if ok - create the mission
        public Mission(Vessel ves)
        {
            
            if (this.is_vessel_fits_to_mission(ves))
            {
                KDebug.Log("Mission is creating by Vessel: " + ves.ToString(), KDebug.Type.CREATE);

                this.missionId = Mission.mission_id(ves);
                this.missionApproved = true;
                this.missionName = ves.GetName(); // Name of the mission is the name of the Vessel
                this.missionSituation = ves.situation;
                this.missionTime = Mission.mission_time(); // Static call

                if (String.IsNullOrEmpty(this.missionName) ) this.missionName = ves.name; // Let's see the unknown created vessels
            }
            else this.missionApproved = false; // No mission is created, we should not use this mission object
        }
 
        // Create mission from the Vessel
        private Boolean is_vessel_fits_to_mission(Vessel ves)
        {
            if (ves == null) return false;

            VesselType type = ves.vesselType;            
            //return type == VesselType.Ship | type == VesselType.Probe;
            if (type != VesselType.SpaceObject & type != VesselType.EVA & type != VesselType.Flag & type != VesselType.Debris)
            {
                #region Debug information 
                // === Debug information ===
                KDebug.Log("Vessel debug information: ", KDebug.Type.CREATE);
                KDebug.Log("ves.name: " + ves.name, KDebug.Type.CREATE);
                KDebug.Log("ves.vesselName: " + ves.vesselName, KDebug.Type.CREATE);
                KDebug.Log("ves.GetInstanceID: " + ves.GetInstanceID().ToString(), KDebug.Type.CREATE);
                //KDebug.Log("ves.GetVessel: " + ves.GetVessel().ToString());
                //KDebug.Log("ves.vesselType: " + ves.vesselType.ToString());
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
                }else if(type == VesselType.EVA)
                {
                    KDebug.Log("EVA debug information: ", KDebug.Type.CREATE);
                    KDebug.Log("ves.vesselName: " + ves.vesselName, KDebug.Type.CREATE);
                    KDebug.Log("ves.rootPart: " + ves.rootPart.ToString(), KDebug.Type.CREATE);                
                }

            /*
             * ves.situation: PRELAUNCH () <- ?
             * ves.state: INACTIVE
             * ves.currentStage: 1
             * ves.missionTime: 0 (!)   
             * ves.IsControllable: False (.)
             * ves.isCommandable: True (.)
             * ves.isActiveVessel: False
             * ves.isActiveAndEnabled: True (?)
             * ves.loaded: False
             * ves.vesselType: Ship (!) <- ?
             */
            #endregion
            
            // (ves.missionTime < 0.1); Condition for the mission time. It should be checked only if we create a new flight.

            // This conditions should be ok
            Boolean is_ok = ( (ves.isCommandable | ves.IsControllable) & (type == VesselType.Plane | type == VesselType.Rover | type == VesselType.Lander | type == VesselType.Relay | type == VesselType.Station | type == VesselType.Ship | type == VesselType.Probe) & ves.loaded );  // Apparently the last parameter (ves.loaded) needs to be check
            KDebug.Log("Is vessel conditions are ok for aproove for " + VesselType.Ship.ToString() + " " + ves.name + "? " + is_ok.ToString(), KDebug.Type.CREATE);
            return is_ok;
        }

        // Change the name if nessesary
        // NOT IS USE
        public void rename(String newName)
        {
            missionName = newName;
        }

        #endregion

        # region Add Entry
        // general function to add message
        public void add_entry(Entry.Situations S, String message)
        {
            KDebug.Log("Add message from the Mission class: " + message, KDebug.Type.CHANGE);
            add_entry(S, FlightGlobals.ActiveVessel, message);
        }


        // Add antry not from Acrive vessel
        // NEEDS TO BE CHANGE!!!!
        public void add_entry(Entry.Situations S,  Vessel ves, String message)
        {
            KDebug.Log("Add message from the Mission class: " + message, KDebug.Type.CHANGE);
            
            Entry e = new Entry();
            e.add(S, message);

            if (ves != null)
            {
                e.set_time(ves.missionTime);
            } // If not ActiveVessel time is set to 0, we need to take time from the previous entry
            else if (entries.Count > 0)
            {
                Entry last = entries[entries.Count - 1];
                // we add fraction of a second
                e.set_time(last.get_time() + 0.1);
            }
            this.add_entry(e);
        }

        // All entryes should use this functiono
        private void add_entry(Entry e)
        {
            this.entries.Add(e);
            switch (e.get_situation())
            { 
                case Entry.Situations.DESTROYED:
                    lastDestoryed = get_last_entry_index();
                    break;
                case Entry.Situations.EXPLODE:
                    lastExployed = get_last_entry_index();
                    break;
            }
        }

        // Function for loader
        public void load_entry(Entry.Situations S, String message, double time)
        {
            //KDebug.Log("Add message from the Mission class: " + message); // No more double load spam
            Entry e = new Entry();
            e.load(S, message, time);
            this.entries.Add(e);
        }

        // Add entry from GUI
        public void add_user_entry( String message)
        {
            Entry e = new Entry();
            e.add(Entry.Situations.USER, message);
           

            double mission_time = this.get_time();
            double current_time = Planetarium.GetUniversalTime();
                //HighLogic.CurrentGame.flightState.universalTime;            

            KDebug.Log("Add from gui, mission_time:" + mission_time.ToString() + " Planetarium.GetUniversalTime():" + Planetarium.GetUniversalTime().ToString() + " flightState.universalTime:" + HighLogic.CurrentGame.flightState.universalTime.ToString(), KDebug.Type.CHANGE);

            e.set_time( current_time - mission_time ); // For some reason this does not work...
            this.add_entry(e);
        }

        //Add screenshot to the last entry
        public void add_screenshot(String file)
        {
            if (entries.Count > 0)
                entries[entries.Count - 1].set_screenshot(file);
        }

        //Add screeshot to the specific entry
        public void add_screenshot(String file, int iE)
        {
            if (entries.Count > iE)
                entries[iE].set_screenshot(file);
        }

        #endregion

        #region Get Entries

        public List<Entry> get_entries()
        {
            return this.entries;
        }

        #endregion

        #region Delte enrty

        public void detele_entry_by_index(int index)
        {
            KDebug.Log("Delete item " + index.ToString(), KDebug.Type.CHANGE);
            // check if the entry exist then delete            
            if (this.entries[index] != null)
            {
                KDebug.Log("Entry deleted", KDebug.Type.CHANGE);
                this.entries.RemoveAt(index);
            }
         }
            

        #endregion

        #region Get Properties

        public String get_name()
        {
            return this.missionName;
        }
    
        public double get_time()
        {
            return this.missionTime;
        }

        public String get_time_str()
        {
            DateTime t = new DateTime();
            t = t.AddSeconds(this.missionTime);
            return t.ToString("dd-MM-yy HH:mm:ss");
            //return this.missionTime.ToString();
        }

        public String get_mission_string()
        {
            return "[" + this.get_time_str() + "] " + " Mission: " + this.get_name();
        }

        public Vessel.Situations get_situation()
        {
            return this.missionSituation;
        }

        public int get_last_entry_index()
        {
            return entries.Count - 1;
        }

        public Entry get_last_exploded()
        {
            Entry E = get_last_entry_by_situation(lastExployed, Entry.Situations.EXPLODE);
            if( E != null)
                lastExployed = entries.IndexOf(E);
            return E;
        }

        public Entry get_last_destroyed()
        {
            Entry E = get_last_entry_by_situation(lastDestoryed, Entry.Situations.DESTROYED);
            if (E != null)
                lastDestoryed = entries.IndexOf(E);
            return E;
        }

        private Entry get_last_entry_by_situation(int idx, Entry.Situations S)
        {
            // Attempt to find the entry by situaion
            if (idx < 0)
            {
                foreach (Entry E in entries)
                {
                    if (E.get_situation() == S)
                    {                        
                        return E;
                    }
                }
            }
            else
            {
                if (entries[idx].get_situation() == S)
                    return entries[idx];
            }
            return null;
        }

        public bool is_too_soon(Entry previousEntry, Vessel ves, double timeMargin)
        {
            if (previousEntry != null)
            {
                KDebug.Log("Too Soon! ", KDebug.Type.EVENT);
                return Math.Abs(previousEntry.get_time() - Entry.entry_time(ves)) < timeMargin;
            }
                
            else return false;
        }

        public bool is_detached_mission()
        {
            if (entries.Count > 0)
                if (entries[0].get_situation() == Entry.Situations.DETACHED)
                    return true;

            return false;
        }
                
        public bool is_ready_for_shot()
        {
            if(lastExployed > 0)
            {
                return !entries[lastExployed].has_screeshot; // if there is not screenshot, we are ready
            }
            else
            {
                return true;
            }
        }

        static double mission_time()
        {
            return HighLogic.CurrentGame.flightState.universalTime;
        }

        public static Guid mission_id(ProtoVessel ves)
        {
            return Mission.mission_id(ves.vesselRef);
        }

        public static Guid mission_id(Vessel ves)
        {
            // unfotuntally, this code does not really work as I want
            /*
            List < ModuleCommand > L = ves.FindPartModulesImplementing<ModuleCommand>();
            if (L.Count > 0)
                return L.Last().part.flightID;
            else
                return 0;
                */

            //return Convert.ToUInt32(ves.id.ToString()); 
            if (ves)
                return ves.id;
            else
                return new Guid();
        }

        #endregion

        #region Modify properties
        public void set_situation(Vessel.Situations situation)
        {
            this.missionSituation = situation;
        }

        #endregion
    }
}
