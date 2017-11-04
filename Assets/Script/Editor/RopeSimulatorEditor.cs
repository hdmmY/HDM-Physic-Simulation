using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


namespace SpringSimulation
{
    [CustomEditor(typeof(RopeSimulator))]
    public class RopeSimulatorEditor : Editor
    {
        private void OnSceneGUI()
        {
            RopeSimulator script = target as RopeSimulator;

            script.m_startPoint = Handles.PositionHandle(script.m_startPoint, script.transform.rotation);
            script.m_endPoint = Handles.PositionHandle(script.m_endPoint, script.transform.rotation);
        }   
    }

}

