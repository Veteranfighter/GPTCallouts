using LSPD_First_Response.Engine.Scripting.Entities;
using LSPD_First_Response.Mod.API;
using LSPD_First_Response.Mod.Callouts;
using Rage;
using System.Drawing;

namespace GPTCallouts.Callouts
{
    [CalloutInfo("[GPTCalls] Suspicious Person", CalloutProbability.Medium)]
    internal class SuspiciousPerson : Callout
    {
        // Input: write a lspdfr callout in c#

        /*
        This callout creates a suspicious person at a random location around the player, and tasks the player with investigating
        the area and questioning any suspicious persons. The suspect will flee when the player gets close,
        and the callout will end when the suspect is either arrested or killed.
        */

        private Ped suspect;
        private Vector3 spawnPoint;
        private Blip suspectBlip;
        private LHandle pursuit;

        public override bool OnBeforeCalloutDisplayed()
        {
            // Set the callout message and location
            CalloutMessage = "Reports of suspicious activity";
            spawnPoint = World.GetNextPositionOnStreet(Game.LocalPlayer.Character.Position.Around(200f));

            // Display the callout message and location
            AddMinimumDistanceCheck(100f, spawnPoint);

            CalloutPosition = spawnPoint;
            ShowCalloutAreaBlipBeforeAccepting(CalloutPosition, 50f);
            return base.OnBeforeCalloutDisplayed();
        }

        public override bool OnCalloutAccepted()
        {
            // Display additional information to the player
            Game.DisplayHelp("Investigate the area and question any suspicious persons.");

            // Create the suspect
            suspect = new Ped(spawnPoint);
            suspect.IsPersistent = true;
            suspect.BlockPermanentEvents = true;
            suspect.Tasks.Wander();
            Persona persona = Functions.GetPersonaForPed(suspect);
            persona.Wanted = true;
            Functions.SetPersonaForPed(suspect, persona);

            // Create a blip for the suspect
            suspectBlip = suspect.AttachBlip();
            suspectBlip.Color = Color.Yellow;
            suspectBlip.IsRouteEnabled = true;

            return base.OnCalloutAccepted();
        }

        public override void Process()
        {

            // Check if the player has arrived at the scene
            if (Game.LocalPlayer.Character.DistanceTo(spawnPoint) < 20f && pursuit == null)
            {
                // Set the suspect to flee when the player gets close
                suspect.Tasks.Flee(Game.LocalPlayer.Character, 9999f, -1);
                suspect.KeepTasks = true;
                pursuit = Functions.CreatePursuit();
                Functions.AddPedToPursuit(pursuit, suspect);
                Functions.SetPursuitIsActiveForPlayer(pursuit, true);
                if (suspectBlip) suspectBlip.Delete();
            }

            if(pursuit != null)
            {
                if(Functions.IsPursuitStillRunning(pursuit))
                {
                    Game.DisplaySubtitle("Catch the ~r~wanted suspect.");
                } else if(!IsEnding)
                {
                    End();
                }
            }

            // Check if the suspect has escaped or been arrested
            if (suspect.IsDead || suspect.IsCuffed)
            {
                End();
            }

            base.Process();
        }

        public override void End()
        {
            // Clean up the callout
            if (suspect) suspect.Dismiss();
            if (suspectBlip) suspectBlip.Delete();

            base.End();
        }

    }
}
