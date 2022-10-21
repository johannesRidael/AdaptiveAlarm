using System;
using System.Collections.Generic;

namespace DataMonitorLib
{
    /// <summary>
    /// Outlines the basic functions that will be used by the Adaptive Alarm app to collect and save data as well as determine wake time.
    /// This will form the abstraction that will cover each type of data monitor including subtypes for Fitbit, AppleWatch, AndroidWear, 
    /// and No Device.
    /// </summary>
    public abstract class DataMonitor
    {
        /// <summary>
        /// Used to save the approximation's current state to local storage such that it can be recovered later, built upon, and used to make approximations.
        /// </summary>
        public abstract void SaveState();

        /// <summary>
        /// Used to retrieve the approximation's last saved state from local storage. Will be used immediately upon creation of a new DataMonitor to populate
        /// it with the previous app session's state.
        /// </summary>
        public abstract void LoadState();

        /// <summary>
        /// Clears the data associated with this data monitor.
        /// </summary>
        public abstract void ClearState();

        /// <summary>
        /// Estimates and returns the optimal wakeup time given the DataMonitor's current state.
        /// </summary>
        public abstract DateTime EstimateWakeupTime();

        /// <summary>
        /// Collects a datapoint and updates its internal state accordingly.
        /// </summary>
        public abstract void CollectDataPoint();

    }
}
