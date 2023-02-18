using LSPD_First_Response.Mod.API;
using LSPD_First_Response.Mod.Callouts;
using Rage;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPTCallouts.Callouts
{
    [CalloutInfo("[GPTCalls] CarOnFire", CalloutProbability.Medium)]
    public class CarOnFire : Callout
    {
        // Input: generate a lspdfr callout in c# where a car on fire is standing on the street
        /*
            This callout spawns a burning vehicle at a random location, and the player is prompted to put out the fire. 
            When the player puts out the fire, the callout ends.
         */


        private Vehicle burningVehicle;
        private Blip vehicleBlip;
        private Ped driver;

        public override bool OnBeforeCalloutDisplayed()
        {
            // Choose a random location for the callout
            Vector3 spawnPoint = World.GetNextPositionOnStreet(Game.LocalPlayer.Character.Position.Around(300f, 500f));

            // Spawn a burning vehicle at the location
            burningVehicle = new Vehicle("bodhi2", spawnPoint);
            burningVehicle.IsPersistent = true;
            burningVehicle.EngineHealth = -1000f;

            driver = burningVehicle.CreateRandomDriver();
            driver.IsPersistent = true;
            driver.Kill();

            // Set the callout message and location
            CalloutMessage = "Car on Fire";
            CalloutPosition = spawnPoint;

            // Play the dispatch audio
            Functions.PlayScannerAudioUsingPosition("ATTENTION_ALL_UNITS_01 WE_HAVE_01 CITIZENS_REPORT_02 CRIME_DISTURBING_THE_PEACE_01 IN_OR_ON_POSITION", CalloutPosition);

            return base.OnBeforeCalloutDisplayed();
        }

        public override bool OnCalloutAccepted()
        {
            // Show the burning vehicle blip on the map
            vehicleBlip = new Blip(burningVehicle)
            {
                Color = Color.Red,
                Scale = 0.8f,
                IsRouteEnabled = true,
                Name = "Car on Fire",
            };

            return base.OnCalloutAccepted();
        }

        public override void Process()
        {
            if(!burningVehicle)
            {
                End();
                return;
            }

            if(burningVehicle.EngineHealth >= 0 || !burningVehicle.IsOnFire)
            {
                End();
                return;
            }

            if(Game.LocalPlayer.Character.DistanceTo(burningVehicle) < 30f && !IsEnding && burningVehicle.EngineHealth > 0)
            {
                burningVehicle.EngineHealth = -1000f;
                Game.DisplayHelp("Put out ~r~the fire.");
            }

            base.Process();
        }

        public override void End()
        {
            // Remove the burning vehicle blip, delete the vehicle and the driver
            if (burningVehicle) burningVehicle.Dismiss();
            if (vehicleBlip) vehicleBlip.Delete();
            if (driver) driver.Dismiss();

            base.End();
        }
    }
}

