using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Kistory
{
    // Class Mission correspond to single flying vessel (I hope)
    // Vessel should have a probe or crew capsule
    class Mission
    {        
        public Guid missionId;                  // Unique ID for the mission. Maybe we should take it from the 
        public Boolean missionApproved = false; // false if mission was not created. We should check this flag after we create an instance of the mission and we expect that mission is ok
        
        private String missionName;
        private List<Entry> entries = new List<Entry>(); // Container of Entries

        private double missionTime;

        private int lastDestoryed = -1;
        private int lastExployed  = -1;

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

        // We can create mission if we know the Vessel
        public Mission(Vessel ves)
        {
            
            if (ves != null & this.is_vessel_fits_to_mission(ves))
            {
                KDebug.Log("Mission is creating by Vessel: " + ves.ToString(), KDebug.Type.CREATE);

                this.missionId = ves.id;
                this.missionApproved = true;
                this.missionName = ves.GetName();
                this.missionSituation = ves.situation;
                this.missionTime = HighLogic.CurrentGame.flightState.universalTime;

                if ( String.IsNullOrEmpty(this.missionName) ) this.missionName = ves.name; // Let's see the unknown created vessels
            }
            else this.missionApproved = false;
        }
 
        // Create mission from the Vessel
        public Boolean is_vessel_fits_to_mission(Vessel ves)
        {
            if (ves == null) return false;

            VesselType type = ves.vesselType;            
            //return type == VesselType.Ship | type == VesselType.Probe;
            if (type != VesselType.SpaceObject & type != VesselType.EVA & type != VesselType.Flag & type != VesselType.Debris)
            {
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

            // (ves.missionTime < 0.1); Condition for the mission time. It should be checked only if we create a new flight.

            // This conditions should be ok
            Boolean is_fit = ( (ves.isCommandable | ves.IsControllable) & (type == VesselType.Ship | type == VesselType.Probe) & ves.loaded );  // Apparently the last parameter (ves.loaded) needs to be check
            KDebug.Log("Is vessel conditions are ok for aproove for " + VesselType.Ship.ToString() + " " + ves.name + "? " + is_fit.ToString(), KDebug.Type.CREATE);
            return is_fit;
        }

        // Change the name if nessesary
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
        public void add_entry(Entry.Situations S,  Vessel ves, String message)
        {
            KDebug.Log("Add message from the Mission class: " + message, KDebug.Type.CHANGE);

            // If we have CRASH situation we need to add the part to the previous Entry
            if (S == Entry.Situations.DESTROYED & ves != null & entries.Count > 0 & lastDestoryed > 0) // Should be applyed only to the active Vessel and with 
            {
                Entry destroyed = entries[lastDestoryed];
                double timeMargin = 1; // 1 minimum time before next crash
                if(Math.Abs(ves.missionTime - destroyed.get_save_time()) < timeMargin )
                {
                    destroyed.add_to_message(", " + message);
                    return;
                }else
                {
                    lastDestoryed = -1;
                }
            }

            // If we have CRASH situation we need to add the part to the previous Entry
            if (S == Entry.Situations.EXPLODE & ves != null & entries.Count > 0 & lastExployed > 0) // Should be applyed only to the active Vessel and with 
            {
                Entry exploded = entries[lastExployed];
                double timeMargin = 1; // 1 minimum time before next exposion
                if (Math.Abs(ves.missionTime - exploded.get_save_time()) < timeMargin)
                {                    
                    return;
                }
            }

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
                e.set_time(last.get_save_time() + 0.1);
            }
            this.entries.Add(e);

            // Set last destroyed adn exploided index
            if (S == Entry.Situations.DESTROYED)
                lastDestoryed = entries.Count - 1;
            if (S == Entry.Situations.EXPLODE)
                lastExployed = entries.Count - 1;
        }

        // Function for loader
        public void load_entry(Entry.Situations S, String message, double time)
        {
            //KDebug.Log("Add message from the Mission class: " + message); // No more double load spam
            Entry e = new Entry();
            e.load(S, message, time);
            this.entries.Add(e);
        }

        // Add Entry that we created. Can be used in any cases
        public void add_entry(Entry e)
        {
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

        #region Get Entry

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

        public void set_situation(Vessel.Situations situation)
        {
            this.missionSituation = situation;
        }

        public bool is_detached_mission()
        {
            if (entries.Count > 0)
                if (entries[0].get_save_situation() == Entry.Situations.DETACHED)
                    return true;

            return false;
        }

        public int get_last_entry_index()
        {
            return entries.Count-1;
        }

        public bool is_ready_for_exposion()
        {
            if (find_last_exposion() > 0)
            {
                if(FlightGlobals.ActiveVessel != null)
                {
                    double timeMargin = 1;
                    if (Math.Abs(FlightGlobals.ActiveVessel.missionTime - entries[lastExployed].get_save_time()) < timeMargin)
                        return false;
                }
                    
            }
            return true;
        }

        private int find_last_exposion()
        {
            if(lastExployed < 0)
            {
                foreach (Entry E in entries)
                {
                    if(E.get_save_situation() == Entry.Situations.EXPLODE)
                    {
                        lastExployed = entries.IndexOf(E);
                        return lastExployed;
                    }
                }
            }
            return lastExployed;

        }

        public bool ready_for_shot()
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
        #endregion
    }
}
