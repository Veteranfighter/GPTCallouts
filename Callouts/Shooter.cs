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

    [CalloutInfo("[GPTCalls] Shooter", CalloutProbability.Low)]
    public class Shooter : Callout
    {
        // Input: generate a shooter callout
        /*
            In this example, a shooter is randomly spawned on the map, and the player must locate and neutralize them. 
            The callout displays a message and plays a police radio audio to alert the player. 
            Once the player accepts the callout, they are given objectives to get close to the shooter and neutralize them. 
            The callout will end if the shooter is killed or if the player gets too far away from them.
         */

        private Ped shooter;
        private Vector3 spawnPoint;
        private Blip shooterBlip;

        public override bool OnBeforeCalloutDisplayed()
        {
            // Choose a random spawn point
            spawnPoint = World.GetNextPositionOnStreet(Game.LocalPlayer.Character.Position.Around(300f, 1000f));

            // Create the shooter
            shooter = new Ped(spawnPoint);
            shooter.BlockPermanentEvents = true;
            shooter.IsPersistent = true;
            shooter.Inventory.GiveNewWeapon("WEAPON_COMBATPISTOL", 100, true);
            shooter.Tasks.Wander();
            shooter.KeepTasks = true;

            // Set the callout message
            CalloutMessage = "Shooter on the loose";
            CalloutPosition = spawnPoint;

            // Play the police radio audio
            Functions.PlayScannerAudioUsingPosition("UNITS_RESPOND_CODE_99_01 IN_OR_ON_POSITION ", spawnPoint);

            ShowCalloutAreaBlipBeforeAccepting(spawnPoint, 50);

            // Return true to display the callout
            return base.OnBeforeCalloutDisplayed();
        }

        public override bool OnCalloutAccepted()
        {
            shooterBlip = new Blip(shooter)
            {
                Scale = 0.8f,
                IsFriendly = false,
                Color = Color.Red,
                Name = "Shooter"
            };
            return base.OnCalloutAccepted();
        }

        public override void Process()
        {

            // If the shooter is dead, end the callout
            if(shooter)
            {
                if (shooter.DistanceTo(Game.LocalPlayer.Character) < 80f)
                {
                    shooter.Tasks.FightAgainst(Game.LocalPlayer.Character, -1);
                    shooter.KeepTasks = true;
                }

                // If the player kills the shooter, end the callout
                if (shooter.IsDead || Functions.IsPedArrested(shooter))
                {
                    End();
                }
            } else
            {
                End();
            }
            base.Process();
        }

        public override void End()
        {
            // Clean up and end the callout
            if (shooter) shooter.Dismiss();
            if (shooterBlip) shooterBlip.Delete();
            base.End();
        }
    }
}
