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

        // To create mission we need to udentity the ID
        // This needs to load missions
        public Mission(Guid id, String name, double time)
        {
            Debug.Log("[Kistory] Mission is creating by Id: " + id.ToString());
            this.missionId = id;
            this.missionName = name;
            this.missionApproved = true;
            this.missionTime = time;
         
        }

        // We can create mission if we know the Vessel
        public Mission(Vessel ves)
        {
            
            if (ves != null & this.is_vessel_fits_to_mission(ves))
            {
                Debug.Log("[Kistory] Mission is creating by Vessel: " + ves.ToString());

                this.missionId = ves.id;
                this.missionApproved = true;
                this.missionName = ves.GetName();
                this.missionTime = HighLogic.CurrentGame.flightState.universalTime;
            }
            else this.missionApproved = false;
        }

        // We can try to create the mission if the vessel if we have an Active Vessel
        public Mission()
        {
            
            if (FlightGlobals.ActiveVessel != null & this.is_vessel_fits_to_mission(FlightGlobals.ActiveVessel))
            {
                Debug.Log("[Kistory] Mission is creating by Active Vessel");

                this.missionId = FlightGlobals.ActiveVessel.id;
                this.missionApproved = true;
                this.missionName = FlightGlobals.ActiveVessel.GetName();
                this.missionTime = HighLogic.CurrentGame.flightState.universalTime;
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
            // Debug information
            Debug.Log("[Kistory] Vessel debug information: ");
            Debug.Log("[Kistory] ves.name: " + ves.name);
            Debug.Log("[Kistory] ves.vesselName: " + ves.vesselName);
            Debug.Log("[Kistory] ves.GetInstanceID: " + ves.GetInstanceID().ToString());
            //Debug.Log("[Kistory] ves.GetVessel: " + ves.GetVessel().ToString());
            //Debug.Log("[Kistory] ves.vesselType: " + ves.vesselType.ToString());
            //Debug.Log("[Kistory] ves.vesselRanges: " + ves.vesselRanges.ToString());
            //Debug.Log("[Kistory] ves.vesselTransform: " + ves.vesselTransform.ToString());
            //Debug.Log("[Kistory] ves.VesselValues: " + ves.VesselValues.ToString());
            //Debug.Log("[Kistory] ves.GetType: " + ves.GetType().ToString());
            Debug.Log("[Kistory] ves.GetTotalMass: " + ves.GetTotalMass().ToString());
            //Debug.Log("[Kistory] ves.GetTransform: " + ves.GetTransform().ToString());
            //Debug.Log("[Kistory] ves.tag: " + ves.tag.ToString());            
            Debug.Log("[Kistory] ves.loaded: " + ves.loaded.ToString());            
            Debug.Log("[Kistory] ves.isActiveAndEnabled: " + ves.isActiveAndEnabled.ToString());
            Debug.Log("[Kistory] ves.isActiveVessel: " + ves.isActiveVessel.ToString());
            Debug.Log("[Kistory] ves.isCommandable: " + ves.isCommandable.ToString());
            Debug.Log("[Kistory] ves.IsControllable: " + ves.IsControllable.ToString());                     
            Debug.Log("[Kistory] ves.launchTime: " + ves.launchTime.ToString());
            Debug.Log("[Kistory] ves.missionTime: " + ves.missionTime.ToString());
            Debug.Log("[Kistory] ves.packed: " + ves.packed.ToString());
            Debug.Log("[Kistory] ves.situation: " + ves.situation.ToString());
            Debug.Log("[Kistory] ves.state: " + ves.state.ToString());
            //Debug.Log("[Kistory] ves.ctrlState: " + ves.ctrlState.ToString());
            Debug.Log("[Kistory] ves.currentStage: " + ves.currentStage.ToString());
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
            return (ves.isCommandable | ves.IsControllable) & (type == VesselType.Ship | type == VesselType.Probe); 
        }

        // general function to add message
        public void add_entry(String message)
        {
            Debug.Log("[Kistory] Add message from the Mission class: " + message);
            Entry e = new Entry(message);
            if (FlightGlobals.ActiveVessel != null)
            {
                e.set_time(FlightGlobals.ActiveVessel.missionTime);
            }
            this.entries.Add(e);
        }

        // Add antry not from Acrive vessel
        public void add_entry(Vessel ves, String message)
        {
            Debug.Log("[Kistory] Add message from the Mission class: " + message);
            Entry e = new Entry(message);
            if (ves != null)
            {
                e.set_time(ves.missionTime);
            }
            this.entries.Add(e);
        }

        // Function for loader
        public void add_entry(String message, double time)
        {
            Debug.Log("[Kistory] Add message from the Mission class: " + message);
            Entry e = new Entry(message, time);
            this.entries.Add(e);
        }

        // Most usefull case
        public void add_entry(Entry e)
        {
            this.entries.Add(e);
        }


        public List<Entry> get_entries()
        {
            return this.entries;
        }

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
            t.AddSeconds(this.missionTime);
            return t.ToString("dd-MM-yy HH:mm:ss");
            //return this.missionTime.ToString();
        }
    }
}
