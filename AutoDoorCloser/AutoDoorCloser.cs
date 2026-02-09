using System;
using System.Collections.Generic;
using System.Linq;
using Fougerite;
using Fougerite.Events;
using UnityEngine;

namespace AutoDoorCloser
{
    public class AutoDoorCloser : Fougerite.Module
    {
        //private MethodInfo togglestateserver;
        public static int doorLayer;

        public override string Name
        {
            get { return "AutoDoorCloser"; }
        }

        public override string Author
        {
            get { return "DreTaX"; }
        }

        public override string Description
        {
            get { return "AutoDoorCloser"; }
        }

        public override Version Version
        {
            get { return new Version("1.2"); }
        }

        public override void Initialize()
        {
            Hooks.OnCommand += OnCommand;
            Hooks.OnDoorUse += OnDoorUse;
            doorLayer = LayerMask.GetMask(new string[] { "Mechanical" });
            /*foreach (MethodInfo methodinfo in typeof(BasicDoor).GetMethods((BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.NonPublic)))
            {
                if (methodinfo.Name == "ToggleStateServer")
                {
                    if (methodinfo.GetParameters().Length == 3)
                    {
                        togglestateserver = methodinfo;
                        break;
                    }
                }
            }*/
        }

        public override void DeInitialize()
        {
            Hooks.OnCommand -= OnCommand;
            Hooks.OnDoorUse -= OnDoorUse;
        }

        public void OnDoorUse(Fougerite.Player p, DoorEvent de)
        {
            BasicDoor door = (from collider in Physics.OverlapSphere(p.Location, 3f, doorLayer) where collider.GetComponent<BasicDoor>() select collider.GetComponent<BasicDoor>()).FirstOrDefault();
            if (DataStore.GetInstance().Get("AutoCloser", p.UID) != null && door != null)
            {
                if (door.state.ToString() != "Closed") return;
                int i = (int) DataStore.GetInstance().Get("AutoCloser", p.UID);
                var dict = new Dictionary<string, object>();
                dict["Loc"] = p.Location;
                dict["Door"] = door;
                var delay = 1000 * i;
                CreateParallelTimer("AutoDoorCloser_" + p.UID, delay, dict, Callback).Start();
            }
        }

        public void OnCommand(Fougerite.Player player, string cmd, string[] args)
        {
            if (cmd == "doorcloser")
            {
                if (args.Length == 0)
                {
                    player.Message("Usage: /doorcloser number (1-10)");
                    return;
                }
                int i;
                string s = string.Join("", args);
                bool b = int.TryParse(s, out i);
                if (!b || i > 10 || i < 1)
                {
                    player.Message("Usage: /doorcloser number (1-10)");
                    return;
                }
                DataStore.GetInstance().Add("AutoCloser", player.UID, i);
                player.Message("Closing door after " + i + " seconds");
            }
        }

        private void Callback(TimedEvent e)
        {
            e.Kill();
            var data = e.Args;
            var door = (BasicDoor) data["Door"];
            var loc = (Vector3) data["Loc"];
            if (door.state.ToString() != "Closing" && door.state.ToString() != "Closed")
            {
                door.ToggleStateServer(loc, NetCull.timeInMillis, null);
            }

            //togglestateserver.Invoke(door, new object[] { loc, NetCull.timeInMillis, null });;
        }
    }
}
