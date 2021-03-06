﻿using System;

namespace Ambiance
{
    
    class AmbianceGenerator
    {
        /*
        randomly selects ambiance from a given list
        */
        static string[] DfltAmbiance = new string[] {
            "The sound of a humming computer permiates the air, then dies away",
            "Your the floor gives an echoy clack  beneath your step",
            "You cast the beam of you flashlight around in the oily darkness.",
            "The air is cold, think and stagnanant.",
            "A metalic and decrepted odor lingers on the stagnant air."
        };
        static Random rand = new Random();
        string[] ambiance;
        int frequency; //inversed

        public AmbianceGenerator( int frequency = 5, string[] ambiance = null ) {
            this.ambiance = (ambiance == null ? DfltAmbiance : ambiance);
            this.frequency = frequency;

        }
        public string genAmbiance()
        {
            if (rand.Next(frequency) == 0)
            {
                return "";
            }
            return ambiance[rand.Next(ambiance.Length)];
        }
    }
}
