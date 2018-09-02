using System;
using BrilliantSkies.Core;
using BrilliantSkies.Core.Modding;
using BrilliantSkies.Core.Timing;
using BrilliantSkies.Ftd.Planets;
using BrilliantSkies.Ftd.Planets.Factions;
using BrilliantSkies.Ftd.Planets.Instances;
using BrilliantSkies.Ftd.Planets.Instances.Headers;
using UnityEngine;

namespace w0otness
{
	public class TournamentPlugin : GamePlugin
	{
		static Tournament _t;
		public void OnLoad()
		{
			SafeLogging.Log("Loading Tournament Stuff");
			_t = new Tournament();
			GameEvents.UniverseChange += OnPlanetChange;
			GameEvents.StartEvent += OnInstanceChange;
		}

		public void OnSave() { }
		
		public string name {
			get { return "Tournament"; }
		}
		
		public static string Name {
			get { return "Tournament"; }
		}
		
		public Version version {
			get { return new Version("2.2.14"); }
		}
		
		public static void OnInstanceChange()
		{
			GameEvents.Twice_Second -= _t._me.SlowUpdate;
			GameEvents.FixedUpdateEvent -= _t._me.FixedUpdate;
			GameEvents.PreLateUpdate -= _t._me.LateUpdate;
			GameEvents.OnGui -= _t._me.OnGUI;
			if (@is.Header.Name == InstanceSpecification.i.Header.Name) {
				_t._me._GUI.ActivateGui(_t._me);
				SafeLogging.Log("Avatar is "+InstanceSpecification.i.Header.CommonSettings.AvatarAvailability.ToString());
			}
		}
		
		static InstanceSpecification @is;
		
		public static void OnPlanetChange()
		{
			@is = new InstanceSpecification();
			@is.Header.Name = "Tournament Creator";
			@is.Header.Summary = "Create custom tournament style matches.";
			@is.Header.Type = InstanceType.None;
			@is.Header.CommonSettings.AvatarAvailability = AvatarAvailability.None;
			@is.Header.CommonSettings.AvatarDamage = AvatarDamage.Off;
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
			@is.Header.CommonSettings.WarpAbility = WarpAbility.Off;
			FactionSpecifications.i.AddNew(new FactionSpecificationFaction {
				Name = "King",
				AbreviatedName = "K",
				FleetColors = new Color[] {
					new Color(1f, 0.84f, 0f, 0.75f),//gold
					new Color(0.85f, 0.65f, 0.13f, 0.75f),//goldenrod
					new Color(1f, 0.65f, 0f, 0.75f),//orange
					new Color(0.85f, 0.55f, 0f, 0.75f)//brown-orange?
				}
			});
			FactionSpecifications.i.AddNew(new FactionSpecificationFaction {
				Name = "Challenger",
				AbreviatedName = "C",
				FleetColors = new Color[] {
					new Color(1f, 0f, 0f, 0.75f),//red
					new Color(0.55f, 0f, 0f, 0.75f),//dark red
					new Color(0.7f, 0.13f, 0.13f, 0.75f),//fire brick
					new Color(1f, 0.39f, 0.23f, 0.75f)//tomato
				}
			});
			Planet.i.Designers.AddInstance(@is);
		}
	}
}