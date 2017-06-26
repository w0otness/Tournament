using System;
using BrilliantSkies.FromTheDepths.Game;
using BrilliantSkies.FromTheDepths.Planets;
using UnityEngine;

namespace w0otness
{
	public class TournamentPlugin:FTDPlugin
	{
		static Tournament _t;
	    public void OnLoad()
	    {
	        //Debug.Log("Loading Tournament Stuff");
	        _t = new Tournament();
			GameEvents.PlanetChange += OnPlanetChange;
			GameEvents.StartEvent += OnInstanceChange;
	    }
	    
	    public void OnSave()
	    {
	    	
	    }
	    
	    public string name
	    {
	        get { return "Tournament"; }
	    }
	    
	    public static string Name
	    {
	        get { return "Tournament"; }
	    }
	    
	    public Version version
	    {
	        get { return new Version("0.0.9"); }
	    }
	    
	    public static void OnInstanceChange()
	    {
	    	//Debug.Log("TEST1 "+InstanceSpecification.i.Header.Name);
			GameEvents.Twice_Second -= _t._me.SlowUpdate;
			GameEvents.FixedUpdateEvent -= _t._me.FixedUpdate;
			GameEvents.OnGui -= _t._me.OnGUI;
			if (@is.Header.Name == InstanceSpecification.i.Header.Name)
			{
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
			var kid = FactionSpecifications.i.AddNew(new FactionSpecificationFaction
			{
				Name = "KING",
				AbreviatedName = "KING",
				FleetColors = new Color[]
				{
					new Color(255f/255f,215f/255f,0f/255f,.5f),
					new Color(218f/255f,165f/255f,32f/255f,.5f),
					new Color(255f/255f,165f/255f,0f/255f,.5f),
					new Color(218f/255f,140f/255f,0f/255f,.5f)
				}
			}).Id;
			var cid = FactionSpecifications.i.AddNew(new FactionSpecificationFaction
			{
				Name = "CHAL",
				AbreviatedName = "CHAL",
				FleetColors = new Color[]
				{
					new Color(255f/255f,0f/255f,0f/255f,.5f),
					new Color(139f/255f,0f/255f,0f/255f,.5f),
					new Color(178f/255f,34f/255f,34f/255f,.5f),
					new Color(255f/255f,99f/255f,71f/255f,.5f)
				}
			}).Id;
			//Planet.i.Designers.GetInstance(@is.Header.Id).Factions.GetFaction(kid).eController = FactionController.None;
			//Planet.i.Designers.GetInstance(@is.Header.Id).Factions.GetFaction(cid).eController = FactionController.AI_General;
		}
	}
}