using SLZ.Marrow.Warehouse;

using System;

using UltEvents;

using UnityEditor;
using UnityEngine;

namespace LabFusion.Marrow.Integration
{
    [CustomEditor(typeof(GamemodeMarker))]
    public class GamemodeMarkerEditor : Editor
    {
        public const string AddTeamMethodName = nameof(GamemodeMarker.AddTeam);

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            // Make sure the AssetWarehouse is initialized
            if (!AssetWarehouse.ready)
            {
                return;
            }

            var gamemodeMarker = target as GamemodeMarker;

            var lifeCycleEvent = gamemodeMarker.GetComponent<LifeCycleEvents>();

            if (lifeCycleEvent != null)
            {
                OverrideLifeCycleEvent(gamemodeMarker, lifeCycleEvent);

                EditorGUILayout.HelpBox("The LifeCycleEvents on this GameObject is used to inject variables for this marker." +
                    " Make sure nothing else is using the LifeCycleEvents on this same GameObject.", MessageType.Info);
            }
            else
            {
                EditorGUILayout.HelpBox("If you want to set a specific Team for this Gamemode Marker, please add" +
                    " a LifeCycleEvents to this GameObject!", MessageType.Warning);

                if (GUILayout.Button("Add LifeCycleEvents"))
                {
                    Undo.AddComponent<LifeCycleEvents>(gamemodeMarker.gameObject);
                }
            }
        }

        private void OverrideLifeCycleEvent(GamemodeMarker gamemodeMarker, LifeCycleEvents lifeCycleEvent)
        {
            // Make sure the awake event is properly set up
            if (lifeCycleEvent.AwakeEvent == null)
            {
                lifeCycleEvent.AwakeEvent = new UltEvent();

                EditorUtility.SetDirty(lifeCycleEvent);
            }

            var awakeEvent = lifeCycleEvent.AwakeEvent;

            Action<string> addTeamAction = gamemodeMarker.AddTeam;

            // Add a persistent call
            if (awakeEvent.PersistentCallsList == null || awakeEvent.PersistentCallsList.Count != 1)
            {
                awakeEvent.Clear();

                awakeEvent.AddPersistentCall(addTeamAction);

                EditorUtility.SetDirty(lifeCycleEvent);
            }

            var firstCall = awakeEvent.PersistentCallsList[0];

            // First call isn't AddTeam, change it
            if (firstCall.MethodName != AddTeamMethodName)
            {
                firstCall.SetMethod(addTeamAction);

                EditorUtility.SetDirty(lifeCycleEvent);
            }

            var barcode = firstCall.PersistentArguments[0].String;

            AssetWarehouse.Instance.TryGetDataCard<BoneTag>(new Barcode(barcode), out var teamTag);

            EditorGUI.BeginChangeCheck();

            teamTag = EditorGUILayout.ObjectField("Team Tag", teamTag, typeof(BoneTag), false) as BoneTag;

            // Override the life cycle event value
            if (EditorGUI.EndChangeCheck())
            {
                barcode = teamTag ? teamTag.Barcode.ToString() : string.Empty;

                firstCall.SetArguments(barcode);

                EditorUtility.SetDirty(lifeCycleEvent);
            }
        }
    }
}