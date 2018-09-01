using Assets.Scripts.Gui;
using Assets.Scripts.Persistence;
using BrilliantSkies.Core.UiSounds;
using BrilliantSkies.FromTheDepths.Game;
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
			_treeSelector = FtdGuiUtils.GetFileBrowserFor(combinedBlueprintFolder);
			_treeSelector.Refresh();
			_focus.ResetCam();
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
			_focus.spawndis = GUISliders.DisplaySlider(0, "Spawn Distance", _focus.spawndis, 100f, 5000f, enumMinMax.none, new ToolTip("Spawn distance between teams"));
			_focus.spawngap = GUISliders.DisplaySlider(1, "Spawn Gap", _focus.spawngap, 10f, 500f, enumMinMax.none, new ToolTip("Spawn distance between team members"));
			_focus.minalt = GUISliders.DisplaySlider(2, "Min Alt", _focus.minalt, -500f, _focus.maxalt, enumMinMax.none, new ToolTip("Add to penalty time when below this"));
			_focus.maxalt = GUISliders.DisplaySlider(3, "Max Alt", _focus.maxalt, _focus.minalt, 2000f, enumMinMax.none, new ToolTip("Add to penalty time when above this"));
			_focus.maxdis = GUISliders.DisplaySlider(4, "Max Dis", _focus.maxdis, 0f, 10000f, enumMinMax.none, new ToolTip("Max distance from nearest enemy before penalty time added"));
			_focus.maxoob = GUISliders.DisplaySlider(5, "Penalty Time", _focus.maxoob, 0f, 10000f, enumMinMax.none, new ToolTip("Max penalty time (seconds)"));
			_focus.maxtime = GUISliders.DisplaySlider(6, "Match Time", _focus.maxtime, 0f, 10000f, enumMinMax.none, new ToolTip("Max match time (seconds), Currently doesn't effect anything"));
			_focus.maxcost = GUISliders.DisplaySlider(7, "Max Design Cost", _focus.maxcost, 0f, 10000000f, enumMinMax.none, new ToolTip("Max design cost, Currently doesn't effect anything"));
			_focus.maxmat = GUISliders.DisplaySlider(8, "Starting Material", _focus.maxmat, 0f, 100000f, enumMinMax.none, new ToolTip("Amount of material per team (centralised)"));
			_focus.matconv = GUISliders.DisplaySlider(9, "Dmg to Mat %", _focus.matconv, -1f, 100f, enumMinMax.none, new ToolTip("Damage to material conversion, -1 disables self/team damage material return"));
			_focus.srules = Convert.ToBoolean(GUISliders.DisplaySlider(10, ((Tournament.OPTIONS.STANDARDRULES)Convert.ToInt32(_focus.srules)).ToString(), Convert.ToInt32(_focus.srules), 0f, 1f, enumMinMax.none, new ToolTip("Standard despawn rules, or customise")));
			if (!_focus.srules) {
				_focus.penaltynoai = Convert.ToBoolean(GUISliders.DisplaySlider(11, ((Tournament.OPTIONS.AIPENALTY)Convert.ToInt32(_focus.penaltynoai)).ToString(), Convert.ToInt32(_focus.penaltynoai), 0f, 1f, enumMinMax.none, new ToolTip("Does having no AI left add to penalty time?")));
				_focus.standardhp = Convert.ToBoolean(GUISliders.DisplaySlider(12, ((Tournament.OPTIONS.HPMODE)Convert.ToInt32(_focus.standardhp)).ToString(), Convert.ToInt32(_focus.standardhp), 0f, 1f, enumMinMax.none, new ToolTip("Calculate HP by % of alive blocks or % of alive block costs")));
				_focus.penaltyhp = GUISliders.DisplaySlider(13, "HP Penalty %", _focus.penaltyhp, 0f, 100f, enumMinMax.none, new ToolTip("Adds to penalty time when below hp %, 0 disables"));
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
						bpf = _treeSelector.CurrentData
					};
					_focus.entry_t1.Add(tmp);

					//this._focus.entry_king = new TournamentEntry();
					//this._focus.entry_king.IsKing = true;
					//this._focus.entry_king.spawn_direction = (Tournament.SPAWN.DIR)Dir;
					//this._focus.entry_king.spawn_location = (Tournament.SPAWN.LOC)Loc;
					//this._focus.entry_king.bpf = this._treeSelector.CurrentData;
				}
				if (GUI.Button(new Rect(600f, 100f, 280f, 50f), "Add to Team 2")) {
					GUISoundManager.GetSingleton().PlayBeep();
					TournamentEntry tmp = new TournamentEntry {
						IsKing = false,
						spawn_direction = Dir,
						spawn_location = Loc,
						bpf = _treeSelector.CurrentData
					};
					_focus.entry_t2.Add(tmp);

					//this._focus.entry_chal = new TournamentEntry();
					//this._focus.entry_chal.IsKing = false;
					//this._focus.entry_chal.spawn_direction = (Tournament.SPAWN.DIR)Dir;
					//this._focus.entry_chal.spawn_location = (Tournament.SPAWN.LOC)Loc;
					//this._focus.entry_chal.bpf = this._treeSelector.CurrentData;
				}
			}
			GUILayout.EndArea();
			GUILayout.BeginArea(new Rect(940f, 0f, 340f, 580f), "Selected", GUI.skin.window);
			listpos = GUILayout.BeginScrollView(listpos);
			//var tmpk = "";
			//if (this._focus.entry_king != null)
			//{
			//    foreach (var s in this._focus.entry_king.labelCost)
			//    {
			//        tmpk += "\n" + s;
			//    }
			//}

			GUILayout.Box("<color=#ffa500ff>~---------T1---------~</color>");
			if (_focus.entry_t1.Count != 0) {
				foreach (TournamentEntry tp in _focus.entry_t1) {
					var tmpk = "";
					foreach (var s in tp.labelCost) {
						tmpk += "\n" + s;
					}
					GUILayout.Box(string.Format("<color=#ffa500ff>{3} {2}\n{0} {1}\n~-------SPAWNS-------~</color>{4}\n<color=#ffa500ff>~--------------------~</color>", tp.bpf.Name, tp.bp.CalculateResourceCost(false, true).Material, tp.spawn_location, tp.spawn_direction, tmpk));
					if (GUILayout.Button("^ Remove ^")) {
						_focus.entry_t1.Remove(tp);
					}
				}
			}
			
			GUILayout.Box("<color=#ff0000ff>~---------T2---------~</color>");
			if (_focus.entry_t2.Count != 0) {
				foreach (TournamentEntry tp in _focus.entry_t2) {
					var tmpk = "";
					foreach (var s in tp.labelCost) {
						tmpk += "\n" + s;
					}
					GUILayout.Box(string.Format("<color=#ff0000ff>{3} {2}\n{0} {1}\n~-------SPAWNS-------~</color>{4}\n<color=#ffa500ff>~--------------------~</color>", tp.bpf.Name, tp.bp.CalculateResourceCost(false, true).Material, tp.spawn_location, tp.spawn_direction, tmpk));
					if (GUILayout.Button("^ Remove ^")) {
						_focus.entry_t2.Remove(tp);
					}
				}
			}
			
			//GUILayout.Box(this._focus.entry_king != null ? String.Format("<color=#ffa500ff>~--------KING--------~\n{3} {2}\n{0} {1}\n~-------SPAWNS-------~</color>{4}\n<color=#ffa500ff>~--------------------~</color>",this._focus.entry_king.bpf.Name,this._focus.entry_king.bp.CalculateResourceCost(false,true).Material,this._focus.entry_king.spawn_location,this._focus.entry_king.spawn_direction,tmpk) : "<color=#ffa500ff>~--------KING--------~</color>");

			//var tmpc = "";
			//if (this._focus.entry_chal != null)
			//{
			//    foreach (var s in this._focus.entry_chal.labelCost)
			//    {
			//        tmpc += "\n" + s;
			//    }
			//}
			//GUILayout.Box(this._focus.entry_chal != null ? String.Format("<color=#ff0000ff>~---------T2---------~\n{3} {2}\n{0} {1}\n~-------SPAWNS-------~</color>{4}\n<color=#ff0000ff>~--------------------~</color>", this._focus.entry_chal.bpf.Name, this._focus.entry_chal.bp.CalculateResourceCost(false, true).Material, this._focus.entry_chal.spawn_location, this._focus.entry_chal.spawn_direction, tmpc) : "<color=#ff0000ff>~---------T2---------~</color>");
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