using LSPD_First_Response.Mod.API;
using LSPD_First_Response.Mod.Callouts;
using Rage;
using Rage.Native;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPTCallouts.Callouts
{

    [CalloutInfo("[GPTCalls] DeadPerson", CalloutProbability.Low)]
    public class DeadPerson : Callout
    {
        // Input: write a dead person callout for lspdfr in c#
        /*
            This callout spawns a dead pedestrian at a random location, and the player is prompted to investigate the body. 
            When the player gets close enough to the body, the callout ends.
        */

        private Ped deadPed;
        private Blip deadBlip;

        public override bool OnBeforeCalloutDisplayed()
        {
            // Choose a random location for the callout
            Vector3 spawnPoint = World.GetNextPositionOnStreet(Game.LocalPlayer.Character.Position.Around(300f, 500f));

            // Spawn the dead pedestrian
            deadPed = new Ped(spawnPoint);
            deadPed.IsPersistent = true;
            deadPed.Kill();

            NativeFunction.Natives.APPLY_PED_DAMAGE_PACK(deadPed, "BigHitByVehicle", 1f, 1f);

            // Set the callout message and location
            CalloutMessage = "Dead Person";
            CalloutPosition = spawnPoint;
            
            // Play the dispatch audio
            Functions.PlayScannerAudioUsingPosition("CITIZENS_REPORT_01 CRIME_HIT_AND_RUN_03 IN_OR_ON_POSITION", CalloutPosition);
            ShowCalloutAreaBlipBeforeAccepting(spawnPoint, 50);

            return base.OnBeforeCalloutDisplayed();
        }

        public override bool OnCalloutAccepted()
        {
            // Show the dead pedestrian blip on the map
            deadBlip = new Blip(deadPed)
            {
                Color = Color.Red,
                IsRouteEnabled = true,
                Scale = 0.8f,
                Name = "Dead Person",
            };

            return base.OnCalloutAccepted();
        }

        public override void Process()
        {
            if (deadPed.DistanceTo(Game.LocalPlayer.Character) < 2f)
            {
                // If the player investigates the body, end the callout
                End();
                Game.DisplayHelp("The person needs ~r~medical assistance.");
            }
            base.Process();
        }

        public override void End()
        {
            // Remove the dead pedestrian blip and delete the ped
            if (deadPed) deadPed.Dismiss();
            if (deadBlip) deadBlip.Delete();

            base.End();
        }
    }
}
