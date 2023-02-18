using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using LSPD_First_Response.Mod.Callouts;
using Rage;
using LSPD_First_Response.Mod.API;

namespace GPTCallouts.Callouts
{

    [CalloutInfo("[GPTCalls] Fighting Pedestrians", CalloutProbability.Medium)]
    public class Fight : Callout
    {
        // Input: generate a lspdfr callout in c# where two pedastrians are fighting each other and you have to stop the fight
        /*
            This callout generates two pedestrians who are fighting each other, and the player must stop the fight and arrest the combatants.
            The callout is initiated when the player is within 100 meters of the spawn point, and the player is given a message to respond to the scene of the fight. 
            Once the player arrives, they must break up the fight and arrest the combatants. 
            The callout ends when the player has either arrested the combatants or they have fled the scene.
        */

        private Ped _ped1, _ped2;
        private Blip _ped1Blip, _ped2Blip;
        private Vector3 _spawnPoint;

        public override bool OnBeforeCalloutDisplayed()
        {
            _spawnPoint = World.GetNextPositionOnStreet(Game.LocalPlayer.Character.Position.Around(100f, 200f));

            ShowCalloutAreaBlipBeforeAccepting(_spawnPoint, 30f);
            CalloutMessage = "Fighting Pedestrians";
            CalloutPosition = _spawnPoint;

            return base.OnBeforeCalloutDisplayed();
        }

        public override bool OnCalloutAccepted()
        {
            _ped1 = new Ped("s_m_y_construct_01", _spawnPoint, 0f);
            _ped2 = new Ped("s_m_y_construct_02", _spawnPoint + new Vector3(0, 5, 0), 0f);

            _ped1.Inventory.GiveNewWeapon("WEAPON_KNIFE", 1, true);
            _ped2.Inventory.GiveNewWeapon("WEAPON_KNIFE", 1, true);

            _ped1.Tasks.FightAgainst(_ped2);
            _ped2.Tasks.FightAgainst(_ped1);
            _ped1.KeepTasks = true;
            _ped2.KeepTasks = true;
            _ped1.MaxHealth = 1200;
            _ped2.MaxHealth = 1200;
            _ped1.Health = 1200;
            _ped2.Health = 1200;
            _ped1Blip = new Blip(_ped1)
            {
                Color = Color.Red,
                IsRouteEnabled = true,
                Scale = 0.8f,
                Name = "Suspect"
            };
            _ped2Blip = new Blip(_ped2)
            {
                Color = Color.Red,
                IsRouteEnabled = true,
                Scale = 0.8f,
                Name = "Suspect"
            };

            return base.OnCalloutAccepted();
        }

        public override void Process()
        {
            if (!_ped1 || !_ped2)
            {
                End();
            }
            else if (_ped1.IsFleeing || _ped2.IsFleeing)
            {
                Game.DisplaySubtitle("A pedestrian has fled the scene. Find and arrest them before leaving the area.");
            }
            else if (_ped1.IsInCombat || _ped2.IsInCombat)
            {
                Game.DisplaySubtitle("The fight is ~r~still ongoing.~w~ Break up the fight and ~o~arrest the combatants.", 10);
            }
            else
            {
                Game.DisplayHelp("~g~Investigation complete. ~w~You can ~y~clear the scene now.");
                End();
            }

            if (Functions.IsPedInPursuit(_ped1)) if (_ped1Blip) _ped1Blip.Delete();
            if (Functions.IsPedInPursuit(_ped2)) if (_ped2Blip) _ped2Blip.Delete();
            base.Process();
        }

        public override void End()
        {
            if (_ped1) _ped1.Dismiss();
            if (_ped2) _ped2.Dismiss();

            if (_ped1Blip) _ped1Blip.Delete();
            if (_ped2Blip) _ped2Blip.Delete();
            base.End();
        }
    }
}
