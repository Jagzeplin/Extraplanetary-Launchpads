using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using KSP.IO;

namespace ExLP {
	public class ExSettings : ScenarioModule
	{
		public static ExSettings current
		{
			get {
				var game = HighLogic.CurrentGame;
				return game.scenarios.Select (s => s.moduleRef).OfType<ExSettings> ().SingleOrDefault ();
				
			}
		}

		public static void CreateSettings (Game game)
		{
			if (!game.scenarios.Any (p => p.moduleName == typeof (ExSettings).Name)) {
				Debug.Log (String.Format ("[EL] Settings create"));
				var proto = game.AddProtoScenarioModule (typeof (ExSettings), GameScenes.SPACECENTER, GameScenes.EDITOR, GameScenes.SPH, GameScenes.TRACKSTATION, GameScenes.FLIGHT);
				proto.Load (ScenarioRunner.fetch);
			}
		}

		public override void OnLoad (ConfigNode config)
		{
			Debug.Log (String.Format ("[EL] Settings load"));
			var settings = config.GetNode ("Settings");
			if (settings == null) {
				settings = new ConfigNode ("Settings");
				if (HighLogic.LoadedScene == GameScenes.SPACECENTER) {
					enabled = !ExLaunchPad.kethane_present;
					//enabled = true;
				}
			}
			if (!settings.HasValue ("ForceResourceUse")) {
				settings.AddValue ("ForceResourceUse", false);
			}

			ExLaunchPad.force_resource_use = false;
			var fru = settings.GetValue ("ForceResourceUse");
			bool.TryParse (fru, out ExLaunchPad.force_resource_use);

			if (settings.HasNode ("ShipInfo")) {
				var node = settings.GetNode ("ShipInfo");
				string val = node.GetValue ("rect");
				if (val != null) {
					Quaternion pos;
					pos = ConfigNode.ParseQuaternion (val);
					ExShipInfo.winpos.x = pos.x;
					ExShipInfo.winpos.y = pos.y;
					ExShipInfo.winpos.width = pos.z;
					ExShipInfo.winpos.height = pos.w;
				}
				val = node.GetValue ("visible");
				if (val != null) {
					bool.TryParse (val, out ExShipInfo.showGUI);
				}
			}
		}

		public override void OnSave(ConfigNode config)
		{
			Debug.Log (String.Format ("[EL] Settings save: {0}", config));
			var settings = new ConfigNode ("Settings");
			bool fru = ExLaunchPad.force_resource_use;
			settings.AddValue ("ForceResourceUse", fru);
			config.AddNode (settings);

			var node = new ConfigNode ("ShipInfo");
			Quaternion pos;
			pos.x = ExShipInfo.winpos.x;
			pos.y = ExShipInfo.winpos.y;
			pos.z = ExShipInfo.winpos.width;
			pos.w = ExShipInfo.winpos.height;
			node.AddValue ("rect", KSPUtil.WriteQuaternion (pos));
			node.AddValue ("visible", ExShipInfo.showGUI);
			settings.AddNode (node);
		}
		
		public override void OnAwake ()
		{
			enabled = false;
		}

		void OnGUI ()
		{
			var rect = new Rect(Screen.width / 2 - 250, Screen.height / 2 - 30,
								500, 100);

			GUI.skin = AssetBase.GetGUISkin("KSP window 2");

			GUILayout.BeginArea(rect, "Extraplanetary Launchpads Settings",
								GUI.skin.window);
			GUILayout.BeginVertical ();

			bool fru = ExLaunchPad.force_resource_use;
			fru = GUILayout.Toggle (fru, "Always use resources");
			ExLaunchPad.force_resource_use = fru;

			if (GUILayout.Button ("OK")) {
				enabled = false;
			}
			GUILayout.EndVertical ();
			GUILayout.EndArea();
		}
	}

	// Fun magic to get a custom scenario into a game automatically.

	public class ExSettingsCreator
	{
		public static ExSettingsCreator me;
		void onGameStateCreated (Game game)
		{
			Debug.Log (String.Format ("[EL] onGameStateCreated"));
			ExSettings.CreateSettings (game);
		}

		public ExSettingsCreator ()
		{
			GameEvents.onGameStateCreated.Add (onGameStateCreated);
		}
	}

	[KSPAddon(KSPAddon.Startup.Instantly, false)]
	public class ExSettingsCreatorSpawn : MonoBehaviour
	{

		void Start ()
		{
			Debug.Log (String.Format ("[EL] ExSettingsCreatorSpawn.Start"));
			ExSettingsCreator.me = new ExSettingsCreator ();
			enabled = false;
		}
	}
}
