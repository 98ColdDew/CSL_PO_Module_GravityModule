using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ICities;
using ColossalFramework;
using UnityEngine;
using ProceduralObjects;
using ProceduralObjects.Classes;
using ProceduralObjects.UI;

namespace GravityModule
{
    public class GravityPOMod : LoadingExtensionBase, IUserMod
    {
        public string Name { get { return "Gravity Module"; } }
        public string Description { get { return "Gravity module for Procedural Objects"; } }

        private POModuleType moduleType;

        public override void OnCreated(ILoading loading)
        {
            base.OnCreated(loading);
            if (moduleType == null)
            {
                moduleType = new POModuleType()
                {
                    Name = Name,
                    Author = "SteinsGateSG",
                    TypeID = "GravityModule",
                    ModuleType = typeof(GravityModule),
                    maxModulesOnMap = 1000
                };
            }
            if (!ProceduralObjectsMod.ModuleTypes.Contains(moduleType))
                ProceduralObjectsMod.ModuleTypes.Add(moduleType);
        }

        public override void OnReleased()
        {
            base.OnReleased();
            if (moduleType == null)
                return;
            if (ProceduralObjectsMod.ModuleTypes.Contains(moduleType))
                ProceduralObjectsMod.ModuleTypes.Remove(moduleType);
        }
    }

    public class GravityModule : POModule
    {
        public Vector3 point0, initialSpeed = Vector3.zero;
        public float gravity = 9.807f;
        public int reboundTimes = -1;
        public float reboundFactor = 1f;
        private int t;
        private float vy, timeSpeed = 1f;
        public bool repeat = true, state = false, groupFollows = false;
        public List<Vector3> points = new List<Vector3>();
        public List<float> vys = new List<float>();


        public override void OnModuleCreated(ProceduralObjectsLogic logic)
        {
            base.OnModuleCreated(logic);
            window = new Rect(0, 0, 315, 250);
        }
        public override void UpdateModule(ProceduralObjectsLogic logic, bool simulationPaused, bool layerVisible)
        {
            if (!state) point0 = new Vector3(parentObject.m_position.x, parentObject.m_position.y, parentObject.m_position.z);
            else if (!simulationPaused && layerVisible)
            {
                float dt = Time.deltaTime;
                dt = timeSpeed * dt;
                MoveChildrenObj(dt);
                vy = vy - gravity * dt;
                parentObject.m_position = new Vector3(
                    parentObject.m_position.x + dt * initialSpeed.x,
                    parentObject.m_position.y + dt * vy,
                    parentObject.m_position.z + dt * initialSpeed.z);
                if (parentObject.m_position.y < ProceduralUtils.NearestGroundPointVertical(parentObject.m_position).y)
                {
                    parentObject.m_position.y = parentObject.m_position.y - dt * vy;
                    vy = -vy * reboundFactor;
                    if (t > 0) t--;
                    else if (t == 0 || vy < 0.1f)
                    {
                        if (!repeat) state = !state;
                        else
                        {
                            parentObject.m_position = new Vector3(point0.x, point0.y, point0.z);
                            t = reboundTimes;
                            vy = initialSpeed.y;
                            MoveChildrenObj(-2f);
                            MoveChildrenObj(-3f);
                        }
                    }
                }
            }
        }
        public override void DrawCustomizationWindow(int id)
        {
            base.DrawCustomizationWindow(id);
            GUI.Label(new Rect(10, 50, 50, 20), "State");
            if (GUI.Button(new Rect(50, 50, 125, 20), "Repeat : " + (repeat ? "ON" : "OFF"))) repeat = !repeat;
            if (GUI.Button(new Rect(180, 50, 125, 20), (state ? "Back" : "Start")))
            {
                state = !state;
                if (state)
                {
                    t = reboundTimes;
                    vy = initialSpeed.y;
                    MoveChildrenObj(-3f);
                }
                else
                {
                    parentObject.m_position = new Vector3(point0.x, point0.y, point0.z);
                    MoveChildrenObj(-2f);
                }
            }
            GUI.Label(new Rect(10, 75, 100, 20), "Time speed " + timeSpeed + "x");
            timeSpeed = Mathf.Floor(GUI.HorizontalSlider(new Rect(110, 80, 195, 20), 10 * timeSpeed, 1f, 100f)) / 10f;
            GUIUtils.DrawSeparator(new Vector2(5, 100), 305);
            GUI.Label(new Rect(10, 105, 300, 20), "Initial speed (m/s)");
            GUI.Label(new Rect(210, 105, 25, 20), "G :");
            GUI.Label(new Rect(10, 130, 25, 20), "X :");
            GUI.Label(new Rect(110, 130, 25, 20), "Y :");
            GUI.Label(new Rect(210, 130, 25, 20), "Z :");
            float newG, newX, newY, newZ;
            if (float.TryParse(GUI.TextField(new Rect(230, 105, 75, 20), gravity.ToString()), out newG))
                gravity = newG;
            if (float.TryParse(GUI.TextField(new Rect(30, 130, 75, 20), initialSpeed.x.ToString()), out newX))
                initialSpeed = new Vector3(newX, initialSpeed.y, initialSpeed.z);
            if (float.TryParse(GUI.TextField(new Rect(130, 130, 75, 20), initialSpeed.y.ToString()), out newY))
                initialSpeed = new Vector3(initialSpeed.x, newY, initialSpeed.z);
            if (float.TryParse(GUI.TextField(new Rect(230, 130, 75, 20), initialSpeed.z.ToString()), out newZ))
                initialSpeed = new Vector3(initialSpeed.x, initialSpeed.y, newZ);
            GUIUtils.DrawSeparator(new Vector2(5, 155), 305);
            GUI.Label(new Rect(10, 160, 300, 20), "Rebound");
            GUI.Label(new Rect(10, 185, 55, 20), "Times :");
            GUI.Label(new Rect(160, 185, 55, 20), "Factor :");
            int times;
            float factor;
            if (int.TryParse(GUI.TextField(new Rect(60, 185, 95, 20), reboundTimes.ToString()), out times))
                reboundTimes = times;
            if (float.TryParse(GUI.TextField(new Rect(210, 185, 95, 20), reboundFactor.ToString()), out factor))
                reboundFactor = factor;

            if (GUI.Button(new Rect(10, 215, 295, 25), "Children in group follow motion : " + (groupFollows ? "ON" : "OFF")))
            {
                groupFollows = !groupFollows;
                if (groupFollows) MoveChildrenObj(-1f);
            }
        }
        public override void GetData(Dictionary<string, string> data, bool forSaveGame)
        {
            data.Add("repeat", repeat.ToString());
            data.Add("state", state.ToString());
            data.Add("timeSpeed", timeSpeed.ToString());
            data.Add("gravity", gravity.ToString());
            data.Add("initialSpeed", initialSpeed.ToStringUnrounded());
            data.Add("reboundTimes", reboundTimes.ToString());
            data.Add("reboundFactor", reboundFactor.ToString());
            data.Add("groupFollows", groupFollows.ToString());
        }
        public override void LoadData(Dictionary<string, string> data, bool fromSaveGame)
        {
            if (data.ContainsKey("repeat"))
                repeat = bool.Parse(data["repeat"]);
            if (data.ContainsKey("state"))
                state = bool.Parse(data["state"]);
            if (data.ContainsKey("timeSpeed"))
                timeSpeed = float.Parse(data["timeSpeed"]);
            if (data.ContainsKey("gravity"))
                gravity = float.Parse(data["gravity"]);
            if (data.ContainsKey("initialSpeed"))
                initialSpeed = VertexUtils.ParseVector3(data["initialSpeed"]);
            if (data.ContainsKey("reboundTimes"))
                reboundTimes = int.Parse(data["reboundTimes"]);
            if (data.ContainsKey("reboundFactor"))
                reboundFactor = float.Parse(data["reboundFactor"]);
            if (data.ContainsKey("groupFollows"))
                groupFollows = bool.Parse(data["groupFollows"]);
        }
        private void MoveChildrenObj(float dt)
        {
            if (!groupFollows || !parentObject.isRootOfGroup) return;
            if (parentObject.group == null) return;
            if (dt == -1f)
            {
                points.Clear();
                vys.Clear();
                foreach (var child in parentObject.group.objects.Where(p => p != parentObject))
                {
                    points.Add(child.m_position);
                    vys.Add(initialSpeed.y);
                }
            }
            else if (dt == -2f)
            {
                int i = 0;
                foreach (var child in parentObject.group.objects.Where(p => p != parentObject))
                {
                    child.m_position = new Vector3(points[i].x, points[i].y, points[i].z);
                    i++;
                }
            }
            else if (dt == -3f)
            {
                int i = 0;
                foreach (var child in parentObject.group.objects.Where(p => p != parentObject))
                {
                    vys[i] = initialSpeed.y;
                    i++;
                }
            }
            else
            {
                int i = 0;
                foreach (var child in parentObject.group.objects.Where(p => p != parentObject))
                {
                    vys[i] = vys[i] - gravity * dt;
                    child.m_position = new Vector3(
                        child.m_position.x + dt * initialSpeed.x,
                        child.m_position.y + dt * vys[i],
                        child.m_position.z + dt * initialSpeed.z);
                    if (child.m_position.y < ProceduralUtils.NearestGroundPointVertical(child.m_position).y)
                    {
                        child.m_position.y = child.m_position.y - dt * vys[i];
                        vys[i] = -vys[i] * reboundFactor;
                    }
                    i++;
                }
            }
        }
    }
}