using Assets.Scripts.Gui;
using Assets.Scripts.Persistence;
using BrilliantSkies.FromTheDepths.Game;
using BrilliantSkies.FromTheDepths.Game.UserInterfaces;
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
            this._Style = LazyLoader.LoadVehicle.Get();
        }

        public override void SetGuiSettings()
        {
            this.GuiSettings.PausesPlay = false;
            this.GuiSettings.PausesMultiplayerPlay = false;
        }

        public override void OnActivateGui()
        {
            this._Style = LazyLoader.LoadVehicle.Get();
            BlueprintFolder combinedBlueprintFolder = GameFolders.GetCombinedBlueprintFolder();
            this._treeSelector = FtdGuiUtils.GetFileBrowserFor(combinedBlueprintFolder);
            this._treeSelector.Refresh();
            this._focus.ResetCam();
        }

        public override void OnGui()
        {
            GUILayout.BeginArea(new Rect(0f, 0f, 340f, 580f), "Select Contestants", GUI.skin.window);
            this._treeSelector.OnGui(new Rect(30f, 35f, 280f, 520f), LazyLoader.TreeView.Get(), new Action<BlueprintFile>(this.UpdateFileData));
            GUILayout.EndArea();

            GUILayout.BeginArea(new Rect(340f, 0f, 600f, 580f), "Tournament Settings", GUI.skin.window);
            optpos = GUILayout.BeginScrollView(optpos);
            GUISliders.TotalWidthOfWindow = 580;
            GUISliders.TextWidth = 240;
            GUISliders.DecimalPlaces = 0;
            GUISliders.UpperMargin = 0;
            this._focus.spawndis = GUISliders.DisplaySlider(0, "Spawn Distance", this._focus.spawndis, 100f, 5000f, enumMinMax.none, new ToolTip("Spawn distance between teams"));
            this._focus.spawngap = GUISliders.DisplaySlider(1, "Spawn Gap", this._focus.spawngap, 10f, 500f, enumMinMax.none, new ToolTip("Spawn distance between team members"));
            this._focus.minalt = GUISliders.DisplaySlider(2, "Min Alt", this._focus.minalt, -500f, this._focus.maxalt, enumMinMax.none, new ToolTip("Add to penalty time when below this"));
            this._focus.maxalt = GUISliders.DisplaySlider(3, "Max Alt", this._focus.maxalt, this._focus.minalt, 2000f, enumMinMax.none, new ToolTip("Add to penalty time when above this"));
            this._focus.maxdis = GUISliders.DisplaySlider(4, "Max Dis", this._focus.maxdis, 0f, 10000f, enumMinMax.none, new ToolTip("Max distance from nearest enemy before penalty time added"));
            this._focus.maxoob = GUISliders.DisplaySlider(5, "Penalty Time", this._focus.maxoob, 0f, 10000f, enumMinMax.none, new ToolTip("Max penalty time (seconds)"));
            this._focus.maxtime = GUISliders.DisplaySlider(6, "Match Time", this._focus.maxtime, 0f, 10000f, enumMinMax.none, new ToolTip("Max match time (seconds), Currently doesn't effect anything"));
            this._focus.maxcost = GUISliders.DisplaySlider(7, "Max Design Cost", this._focus.maxcost, 0f, 10000000f, enumMinMax.none, new ToolTip("Max design cost, Currently doesn't effect anything"));
            this._focus.maxmat = GUISliders.DisplaySlider(8, "Starting Material", this._focus.maxmat, 0f, 100000f, enumMinMax.none, new ToolTip("Amount of material per team (centralised)"));
            this._focus.matconv = GUISliders.DisplaySlider(9, "Dmg to Mat %", this._focus.matconv, -1f, 100f, enumMinMax.none, new ToolTip("Damage to material conversion, -1 disables self/team damage material return"));
            //this._focus.detection = GUISliders.DisplaySlider(10, "Detection", this._focus.detection, 0f, 100f, enumMinMax.none, new ToolTip("Automatic Detection Level"));

            this._focus.srules = Convert.ToBoolean(GUISliders.DisplaySlider(10, ((Tournament.OPTIONS.STANDARDRULES)Convert.ToInt32(this._focus.srules)).ToString(), Convert.ToInt32(this._focus.srules), 0f, 1f, enumMinMax.none, new ToolTip("Standard despawn rules, or customise")));
            if (!this._focus.srules)
            {
                this._focus.penaltynoai = Convert.ToBoolean(GUISliders.DisplaySlider(11, ((Tournament.OPTIONS.AIPENALTY)Convert.ToInt32(this._focus.penaltynoai)).ToString(), Convert.ToInt32(this._focus.penaltynoai), 0f, 1f, enumMinMax.none, new ToolTip("Does having no AI left add to penalty time?")));
                this._focus.standardhp = Convert.ToBoolean(GUISliders.DisplaySlider(12, ((Tournament.OPTIONS.HPMODE)Convert.ToInt32(this._focus.standardhp)).ToString(), Convert.ToInt32(this._focus.standardhp), 0f, 1f, enumMinMax.none, new ToolTip("Calculate HP by % of alive blocks or % of alive block costs")));
                this._focus.penaltyhp = GUISliders.DisplaySlider(13, "HP Penalty %", this._focus.penaltyhp, 0f, 100f, enumMinMax.none, new ToolTip("Adds to penalty time when below hp %, 0 disables"));
            }
            GUILayout.EndScrollView();
            GUILayout.EndArea();

            GUILayout.BeginArea(new Rect(0f, 580f, 940f, 200f), "Spawn Settings", GUI.skin.window);
            GUISliders.TotalWidthOfWindow = 600;
            GUISliders.TextWidth = 240;
            GUISliders.DecimalPlaces = 0;
            GUISliders.UpperMargin = 40;
            this.Dir = (Tournament.SPAWN.DIR)GUISliders.DisplaySlider(0, ((Tournament.SPAWN.DIR)Dir).ToString(), (int)this.Dir, 0, 3, enumMinMax.none, new ToolTip("Direction"));
            this.Loc = (Tournament.SPAWN.LOC)GUISliders.DisplaySlider(1, ((Tournament.SPAWN.LOC)Loc).ToString(), (int)this.Loc, 0, 3, enumMinMax.none, new ToolTip("Location"));
            if (this._treeSelector.CurrentData != null)
            {
                if (GUI.Button(new Rect(600f, 25f, 280f, 50f), "Add to Team 1"))
                {
                    GUISoundManager.GetSingleton().PlayBeep();
                    TournamentEntry tmp = new TournamentEntry();
                    tmp.IsKing = true;
                    tmp.spawn_direction = (Tournament.SPAWN.DIR)Dir;
                    tmp.spawn_location = (Tournament.SPAWN.LOC)Loc;
                    tmp.bpf = this._treeSelector.CurrentData;
                    this._focus.entry_t1.Add(tmp);

                    //this._focus.entry_king = new TournamentEntry();
                    //this._focus.entry_king.IsKing = true;
                    //this._focus.entry_king.spawn_direction = (Tournament.SPAWN.DIR)Dir;
                    //this._focus.entry_king.spawn_location = (Tournament.SPAWN.LOC)Loc;
                    //this._focus.entry_king.bpf = this._treeSelector.CurrentData;
                }
                if (GUI.Button(new Rect(600f, 100f, 280f, 50f), "Add to Team 2"))
                {
                    GUISoundManager.GetSingleton().PlayBeep();
                    TournamentEntry tmp = new TournamentEntry();
                    tmp.IsKing = false;
                    tmp.spawn_direction = (Tournament.SPAWN.DIR)Dir;
                    tmp.spawn_location = (Tournament.SPAWN.LOC)Loc;
                    tmp.bpf = this._treeSelector.CurrentData;
                    this._focus.entry_t2.Add(tmp);

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
            if (this._focus.entry_t1.Count != 0)
            {
                foreach (TournamentEntry tp in this._focus.entry_t1)
                {
                    var tmpk = "";
                    foreach (var s in tp.labelCost)
                    {
                        tmpk += "\n" + s;
                    }
                    GUILayout.Box(String.Format("<color=#ffa500ff>{3} {2}\n{0} {1}\n~-------SPAWNS-------~</color>{4}\n<color=#ffa500ff>~--------------------~</color>", tp.bpf.Name, tp.bp.CalculateResourceCost(false, true).Material, tp.spawn_location, tp.spawn_direction, tmpk));
                    if (GUILayout.Button("^ Remove ^"))
                    {
                        this._focus.entry_t1.Remove(tp);
                    }
                }
            }
            
			GUILayout.Box("<color=#ff0000ff>~---------T2---------~</color>");
            if (this._focus.entry_t2.Count != 0)
            {
                foreach (TournamentEntry tp in this._focus.entry_t2)
                {
                    var tmpk = "";
                    foreach (var s in tp.labelCost)
                    {
                        tmpk += "\n" + s;
                    }
                    GUILayout.Box(String.Format("<color=#ff0000ff>{3} {2}\n{0} {1}\n~-------SPAWNS-------~</color>{4}\n<color=#ffa500ff>~--------------------~</color>", tp.bpf.Name, tp.bp.CalculateResourceCost(false, true).Material, tp.spawn_location, tp.spawn_direction, tmpk));
                    if (GUILayout.Button("^ Remove ^"))
                    {
                        this._focus.entry_t2.Remove(tp);
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
            if (GUI.Button(new Rect(970f, 660f, 280f, 50f), "Start") && (this._focus.entry_t1.Count > 0 && this._focus.entry_t2.Count > 0))
            {
                //GameObject gameObject = new GameObject();
                //gameObject.transform.position = CameraManager.GetSingleton().Position;
                //AudioSource audioSource = gameObject.AddComponent<AudioSource>();
                //audioSource.clip = this._focus.audioClip;
                //audioSource.Play();
                //TimedObjectDestructor timedObjectDestructor = gameObject.AddComponent<TimedObjectDestructor>();
                //timedObjectDestructor.timeOut = this._focus.audioClip.length + 2f;
                this.DeactivateGui();
                this._focus.StartMatch();
            }
            //		if (GUI.Button(new Rect(970f, 700f, 280f, 50f),"Boom!"))
            //		{
            //			
            //		}
        }

        private void UpdateFileData(BlueprintFile file)
        {
            //preview window?
            //		if (file == null)
            //		{
            //			//this._resourceCosts = null;
            //		}
            //		else
            //		{
            //			//Logger.Log("load bp start");
            //			//Blueprint blueprint = file.Load(true);
            //			//this._resourceCosts = blueprint.CalculateResourceCost(false, true);
            //			//DesignViewer component = (UnityEngine.Object.Instantiate(StaticInstantiables.designViewer.Get()) as GameObject).GetComponent<DesignViewer>();
            //			//component.View(blueprint);
            //			//BlockViewer.GetSingleton().BeginDisplay(component.transform, new Rect(0f, 0f, 640f, 800f), () => this.MenuActive);
            //			//Logger.Log("load bp end");
            //		}
        }
    }
}