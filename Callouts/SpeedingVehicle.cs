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
    [CalloutInfo("[GPTCalls] SpeedingVehicle", CalloutProbability.Medium)]
    public class SpeedingVehicle : Callout
    {
        // Input: create a speeding vehicle callout for lspdfr gta 5 c#
        /*
        In this example, a speeding vehicle is randomly spawned on the map, and the player must locate and stop the vehicle. 
        The callout displays a message and plays a police radio audio to alert the player. Once the player accepts the callout, they are given objectives to get close to the vehicle and stop it. 
        The callout will end if the vehicle is stopped, destroyed, or if the player gets too far away from it.
        */

        private Vehicle vehicle;
        private Vector3 spawnPoint;
        private Blip driverBlip;
        private Ped driver;
        private LHandle pursuit;

        public override bool OnBeforeCalloutDisplayed()
        {
            // Choose a random spawn point
            spawnPoint = World.GetNextPositionOnStreet(Game.LocalPlayer.Character.Position.Around(300f, 1000f));

            // Create the vehicle
            vehicle = new Vehicle("carbonizzare", spawnPoint);
            vehicle.IsPersistent = true;

            driver = vehicle.CreateRandomDriver();
            driver.BlockPermanentEvents = true;
            driver.IsPersistent = true;
            driver.Tasks.CruiseWithVehicle(vehicle.TopSpeed, VehicleDrivingFlags.Emergency);

            ShowCalloutAreaBlipBeforeAccepting(spawnPoint, 50);

            // Set the callout message
            CalloutMessage = "Speeding vehicle reported";
            CalloutPosition = spawnPoint;

            // Play the police radio audio
            Functions.PlayScannerAudioUsingPosition("ATTENTION_ALL_UNITS_01 WE_HAVE_01 CRIME_SUSPECT_ON_THE_RUN_02 IN_OR_ON_POSITION", spawnPoint);

            // Return true to display the callout
            return base.OnBeforeCalloutDisplayed();
        }

        public override bool OnCalloutAccepted()
        {
            // Set the objectives
            driverBlip = new Blip(driver)
            {
                IsFriendly = false,
                Color = Color.Orange,
                IsRouteEnabled = true,
            };

            // Return true to start the callout
            return base.OnCalloutAccepted();
        }

        public override void Process()
        {
            base.Process();

            // If the vehicle is destroyed, end the callout
            if (!vehicle || !driver)
            {
                End();
            }

            if (Game.LocalPlayer.Character.DistanceTo(vehicle) < 30f && pursuit == null)
            {
                pursuit = Functions.CreatePursuit();
                Functions.AddPedToPursuit(pursuit, driver);
                Functions.SetPursuitIsActiveForPlayer(pursuit, true);
                if (driverBlip) driverBlip.Delete();
            }

            if (pursuit != null)
            {
                if (Functions.IsPursuitStillRunning(pursuit))
                {
                    Game.DisplaySubtitle("Catch the ~r~driver.");
                }
                else if (!IsEnding)
                {
                    End();
                }
            }
        }

        public override void End()
        {
            // Clean up and end the callout
            if (vehicle) vehicle.Dismiss();
            if (driver) driver.Dismiss();
            if (driverBlip) driverBlip.Delete();
            base.End();
        }
    }
}
