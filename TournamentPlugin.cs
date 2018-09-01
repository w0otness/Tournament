using System;
using BrilliantSkies.Core.Modding;
using BrilliantSkies.Core.Timing;
using BrilliantSkies.Ftd.Planets;
using BrilliantSkies.Ftd.Planets.Factions;
using BrilliantSkies.Ftd.Planets.Instances;
using BrilliantSkies.Ftd.Planets.Instances.Headers;
using UnityEngine;

namespace w0otness
{
	public class TournamentPlugin:GamePlugin
	{
		static Tournament _t;
		public void OnLoad()
		{
			//Debug.Log("Loading Tournament Stuff");
			_t = new Tournament();
			GameEvents.UniverseChange += OnPlanetChange;
			GameEvents.StartEvent += OnInstanceChange;
		}
		
		public void OnSave()
		{
			
		}
		
		public string name {
			get { return "Tournament"; }
		}
		
		public static string Name {
			get { return "Tournament"; }
		}
		
		public Version version {
			get { return new Version("2.2.12"); }
		}
		
		public static void OnInstanceChange()
		{
			//Debug.Log("TEST1 "+InstanceSpecification.i.Header.Name);
			GameEvents.Twice_Second -= _t._me.SlowUpdate;
			GameEvents.FixedUpdateEvent -= _t._me.FixedUpdate;
			GameEvents.OnGui -= _t._me.OnGUI;
			if (@is.Header.Name == InstanceSpecification.i.Header.Name) {
				_t._me._GUI.ActivateGui(_t._me);
			}
		}
		
		static InstanceSpecification @is;
		
		public static void OnPlanetChange()
		{
			@is = new InstanceSpecification();
			@is.Header.Name = "Tournament Creator";
			@is.Header.Summary = "Create custom tournament style matches.";
			@is.Header.Type = InstanceType.Designer;
			@is.Header.CommonSettings.ConstructableCleanUp = ConstructableCleanUp.Off;
			@is.Header.CommonSettings.HeartStoneRequirement = HeartStoneRequirement.None;
			@is.Header.CommonSettings.BuildModeRules = BuildModeRules.Disabled;
			@is.Header.CommonSettings.SavingOptions = SavingOptions.None;
			@is.Header.CommonSettings.BlueprintSpawningOptions = BlueprintSpawningOptions.NoNewVehicles;
			@is.Header.CommonSettings.EnemyBlockDestroyedResourceDrop = 0f;
			@is.Header.CommonSettings.LocalisedResourceMode = LocalisedResourceMode.UseCentralStore;
			@is.Header.CommonSettings.FogOfWarType = FogOfWarType.None;
			@is.Header.CommonSettings.DesignerOptions = DesignerOptions.Off;
			@is.Header.CommonSettings.LuckyMechanic = LuckyMechanic.Off;
			@is.GenerateBlankInstance();
			Planet.i.Designers.AddInstance(@is);
			var kid = FactionSpecifications.i.AddNew(new FactionSpecificationFaction {
				Name = "King",
				AbreviatedName = "K",
				FleetColors = new Color[] {
					new Color(1f, 0.84f, 0f, 0.75f),//gold
					new Color(0.85f, 0.65f, 0.13f, 0.75f),//goldenrod
					new Color(1f, 0.65f, 0f, 0.75f),//orange
					new Color(0.85f, 0.55f, 0f, 0.75f)//brown-orange?
				}
			}).Id;
			var cid = FactionSpecifications.i.AddNew(new FactionSpecificationFaction {
				Name = "Challenger",
				AbreviatedName = "C",
				FleetColors = new Color[] {
					new Color(1f, 0f, 0f, 0.75f),//red
					new Color(0.55f, 0f, 0f, 0.75f),//dark red
					new Color(0.7f, 0.13f, 0.13f, 0.75f),//fire brick
					new Color(1f, 0.39f, 0.23f, 0.75f)//tomato
				}
			}).Id;
			//Planet.i.Designers.GetInstance(@is.Header.Id).Factions.GetFaction(kid).eController = FactionController.None;
			//Planet.i.Designers.GetInstance(@is.Header.Id).Factions.GetFaction(cid).eController = FactionController.AI_General;
		}
	}
}