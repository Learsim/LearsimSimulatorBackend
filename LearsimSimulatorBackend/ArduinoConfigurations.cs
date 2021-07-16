using System;

namespace LearsimSimulatorBackend
{
    /// <summary>
    /// Configuration for a Arduino
    /// </summary>
    public class ArduinoConfiguration
    {
        /// <summary>
        /// The ID of the Arduino board
        /// </summary>
        public Guid ClientID;
        /// <summary>
        /// The bindings of a Arduino client
        /// </summary>
        public ArduinoBinding[] ArduinoBindings;
    }
    /// <summary>
    /// Arduino binging, for saving and editing a Arduinos function
    /// </summary>
    public class ArduinoBinding
    {
    }
}