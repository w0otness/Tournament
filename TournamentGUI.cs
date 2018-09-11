using Assets.Scripts.Gui;
using Assets.Scripts.Persistence;
using BrilliantSkies.Core.Timing;
using BrilliantSkies.Core.UiSounds;
using BrilliantSkies.FromTheDepths.Game;
using BrilliantSkies.Ftd.Planets.World;
using BrilliantSkies.ScriptableObjects;
using BrilliantSkies.Ui.Displayer.Types;
using BrilliantSkies.Ui.Layouts;
using BrilliantSkies.Ui.Tips;
using BrilliantSkies.Ui.TreeSelection;
using System;
using UnityEngine;
namespace w0otness
{
	public class TournamentGUI : ThrowAwayObjectGui<Tournament>
	{
		public Vector2 listpos;
		public Vector2 optpos;
		private TreeSelectorGuiElement<BlueprintFile, BlueprintFolder> _treeSelector;

		public SO_LoadVehicleGUI _Style;

		private Tournament.SPAWN.DIR Dir = Tournament.SPAWN.DIR.Facing;
		private Tournament.SPAWN.LOC Loc = Tournament.SPAWN.LOC.Sea;

		private int sectionsNorthSouth, sectionsEastWest;

		public TournamentGUI()
		{
			_Style = LazyLoader.LoadVehicle.Get();
		}

		public override void SetGuiSettings()
		{
			GuiSettings.PausesPlay = false;
			GuiSettings.PausesMultiplayerPlay = false;
		}

		public override void OnActivateGui()
		{
			_Style = LazyLoader.LoadVehicle.Get();
			BlueprintFolder combinedBlueprintFolder = GameFolders.GetCombinedBlueprintFolder();
			sectionsNorthSouth = WorldSpecification.i.BoardLayout.NorthSouthBoardSectionCount - 1;
			sectionsEastWest = WorldSpecification.i.BoardLayout.EastWestBoardSectionCount - 1;
			_treeSelector = FtdGuiUtils.GetFileBrowserFor(combinedBlueprintFolder);
			_treeSelector.Refresh();
			_focus.ResetCam();
			GameEvents.UpdateEvent += _focus.UpdateBoardSectionPreview;
		}

		public override void OnGui()
		{
			GUILayout.BeginArea(new Rect(0f, 0f, 340f, 580f), "Select Contestants", GUI.skin.window);
			_treeSelector.OnGui(new Rect(30f, 35f, 280f, 520f), new Action<BlueprintFile>(UpdateFileData));
			GUILayout.EndArea();

			GUILayout.BeginArea(new Rect(340f, 0f, 600f, 580f), "Tournament Settings", GUI.skin.window);
			optpos = GUILayout.BeginScrollView(optpos,false,true);
			GUISliders.TotalWidthOfWindow = 580;
			GUISliders.TextWidth = 240;
			GUISliders.DecimalPlaces = 0;
			GUISliders.UpperMargin = 0;
			_focus.spawndis = GUISliders.LayoutDisplaySlider("Spawn Distance", _focus.spawndis, 100f, 5000f, enumMinMax.none, new ToolTip("Spawn distance between teams"));
			_focus.spawngap = GUISliders.LayoutDisplaySlider("Spawn Gap", _focus.spawngap, 10f, 500f, enumMinMax.none, new ToolTip("Spawn distance between team members"));
			_focus.minalt = GUISliders.LayoutDisplaySlider("Min Alt", _focus.minalt, -500f, _focus.maxalt, enumMinMax.none, new ToolTip("Add to penalty time when below this"));
			_focus.maxalt = GUISliders.LayoutDisplaySlider("Max Alt", _focus.maxalt, _focus.minalt, 2000f, enumMinMax.none, new ToolTip("Add to penalty time when above this"));
			_focus.maxdis = GUISliders.LayoutDisplaySlider("Max Dis", _focus.maxdis, 0f, 10000f, enumMinMax.none, new ToolTip("Max distance from nearest enemy before penalty time added"));
			_focus.maxoob = GUISliders.LayoutDisplaySlider("Penalty Time", _focus.maxoob, 0f, 10000f, enumMinMax.none, new ToolTip("Max penalty time (seconds)"));
			_focus.maxtime = GUISliders.LayoutDisplaySlider("Match Time", _focus.maxtime, 0f, 10000f, enumMinMax.none, new ToolTip("Max match time (seconds), Currently doesn't effect anything"));
			_focus.maxcost = GUISliders.LayoutDisplaySlider("Max Design Cost", _focus.maxcost, 0f, 10000000f, enumMinMax.none, new ToolTip("Max design cost, Currently doesn't effect anything"));
			_focus.maxmat = GUISliders.LayoutDisplaySlider("Starting Material", _focus.maxmat, 0f, 100000f, enumMinMax.none, new ToolTip("Amount of material per team (centralised)"));
			_focus.matconv = GUISliders.LayoutDisplaySlider("Dmg to Mat %", _focus.matconv, -1f, 100f, enumMinMax.none, new ToolTip("Damage to material conversion, -1 disables self/team damage material return"));
			_focus.eastWestBoard = (int)GUISliders.LayoutDisplaySlider("E-W Board", _focus.eastWestBoard, 0, sectionsEastWest, enumMinMax.none, new ToolTip("The east-west boardindex, it is the first number on the map. 0 is the left side"));
			_focus.northSouthBoard = (int)GUISliders.LayoutDisplaySlider("N-S Board", _focus.northSouthBoard, 0, sectionsNorthSouth, enumMinMax.none, new ToolTip("The north-south boardindex, it is the second number on the map. 0 is the bottom side."));
			_focus.MoveCam();
			_focus.srules = Convert.ToBoolean(GUISliders.LayoutDisplaySlider(((Tournament.OPTIONS.STANDARDRULES)Convert.ToInt32(_focus.srules)).ToString(), Convert.ToInt32(_focus.srules), 0f, 1f, enumMinMax.none, new ToolTip("Standard despawn rules, or customise")));
			if (!_focus.srules) {
				_focus.penaltynoai = Convert.ToBoolean(GUISliders.LayoutDisplaySlider(((Tournament.OPTIONS.AIPENALTY)Convert.ToInt32(_focus.penaltynoai)).ToString(), Convert.ToInt32(_focus.penaltynoai), 0f, 1f, enumMinMax.none, new ToolTip("Does having no AI left add to penalty time?")));
				_focus.standardhp = Convert.ToBoolean(GUISliders.LayoutDisplaySlider(((Tournament.OPTIONS.HPMODE)Convert.ToInt32(_focus.standardhp)).ToString(), Convert.ToInt32(_focus.standardhp), 0f, 1f, enumMinMax.none, new ToolTip("Calculate HP by % of alive blocks or % of alive block costs")));
				_focus.penaltyhp = GUISliders.LayoutDisplaySlider("HP Penalty %", _focus.penaltyhp, 0f, 100f, enumMinMax.none, new ToolTip("Adds to penalty time when below hp %, 0 disables"));
			}
			GUILayout.EndScrollView();
			GUILayout.EndArea();

			GUILayout.BeginArea(new Rect(0f, 580f, 940f, 200f), "Spawn Settings", GUI.skin.window);
			GUISliders.TotalWidthOfWindow = 600;
			GUISliders.TextWidth = 240;
			GUISliders.DecimalPlaces = 0;
			GUISliders.UpperMargin = 40;
			Dir = (Tournament.SPAWN.DIR)GUISliders.DisplaySlider(0, Dir.ToString(), (int)Dir, 0, 3, enumMinMax.none, new ToolTip("Direction"));
			Loc = (Tournament.SPAWN.LOC)GUISliders.DisplaySlider(1, Loc.ToString(), (int)Loc, 0, 3, enumMinMax.none, new ToolTip("Location"));
			if (_treeSelector.CurrentData != null) {
				if (GUI.Button(new Rect(600f, 25f, 280f, 50f), "Add to Team 1")) {
					GUISoundManager.GetSingleton().PlayBeep();
					TournamentEntry tmp = new TournamentEntry {
						IsKing = true,
						spawn_direction = Dir,
						spawn_location = Loc,
						Bpf = _treeSelector.CurrentData
					};
					_focus.entry_t1.Add(tmp);
				}
				if (GUI.Button(new Rect(600f, 100f, 280f, 50f), "Add to Team 2")) {
					GUISoundManager.GetSingleton().PlayBeep();
					TournamentEntry tmp = new TournamentEntry {
						IsKing = false,
						spawn_direction = Dir,
						spawn_location = Loc,
						Bpf = _treeSelector.CurrentData
					};
					_focus.entry_t2.Add(tmp);
				}
			}
			GUILayout.EndArea();
			GUILayout.BeginArea(new Rect(940f, 0f, 340f, 580f), "Selected", GUI.skin.window);
			listpos = GUILayout.BeginScrollView(listpos);

			GUILayout.Box("<color=#ffa500ff>~---------T1---------~</color>");
			if (_focus.entry_t1.Count != 0) {
				foreach (TournamentEntry tp in _focus.entry_t1) {
					var tmpk = "";
					foreach (var s in tp.LabelCost) {
						tmpk += "\n" + s;
					}
					GUILayout.Box(string.Format("<color=#ffa500ff>{3} {2}\n{0} {1}\n~-------SPAWNS-------~</color>{4}\n<color=#ffa500ff>~--------------------~</color>", tp.Bpf.Name, tp.bp.CalculateResourceCost(false, true).Material, tp.spawn_location, tp.spawn_direction, tmpk));
					if (GUILayout.Button("^ Remove ^")) {
						_focus.entry_t1.Remove(tp);
					}
				}
			}
			
			GUILayout.Box("<color=#ff0000ff>~---------T2---------~</color>");
			if (_focus.entry_t2.Count != 0) {
				foreach (TournamentEntry tp in _focus.entry_t2) {
					var tmpk = "";
					foreach (var s in tp.LabelCost) {
						tmpk += "\n" + s;
					}
					GUILayout.Box(string.Format("<color=#ff0000ff>{3} {2}\n{0} {1}\n~-------SPAWNS-------~</color>{4}\n<color=#ffa500ff>~--------------------~</color>", tp.Bpf.Name, tp.bp.CalculateResourceCost(false, true).Material, tp.spawn_location, tp.spawn_direction, tmpk));
					if (GUILayout.Button("^ Remove ^")) {
						_focus.entry_t2.Remove(tp);
					}
				}
			}
			
			GUILayout.EndScrollView();
			GUILayout.EndArea();
			if (GUI.Button(new Rect(970f, 660f, 280f, 50f), "Start") && _focus.entry_t1.Count > 0 && _focus.entry_t2.Count > 0) {
				DeactivateGui();
				_focus.StartMatch();
			}
		}
		private void UpdateFileData(BlueprintFile file) {}
	}
}