using System;
using System.Collections.Generic;
using BrilliantSkies.Core.Timing;
using BrilliantSkies.FromTheDepths.Game;
using BrilliantSkies.Ftd.Avatar;
using BrilliantSkies.Ftd.Planets.Factions;
using BrilliantSkies.Ftd.Planets.Instances;
using BrilliantSkies.Ftd.Planets.Instances.Headers;
using BrilliantSkies.Ui.Displayer;
using UnityEngine;
namespace w0otness
{
	public class Tournament
	{

		public static class SPAWN
		{
			
			public enum DIR
			{
				Facing,
				Away,
				Left,
				Right
			}
			
			public enum LOC
			{
				Air,
				Sea,
				Sub,
				Land
			}
		}

		public static class OPTIONS
		{

			public enum STANDARDRULES
			{
				CustomRules,
				StandardRules
			}

			public enum AIPENALTY
			{
				NoAIPenaltyOff,
				NoAIPenaltyOn
			}

			public enum HPMODE
			{
				ResourceHP,
				StandardHP
			}
		}

		public Tournament _me;

		public TournamentGUI _GUI;
		public Font _Font;
		public GUIStyle _Top;
		public GUIStyle _Left;
		public GUIStyle _Right;

		private float timerTotal = 0;
		private float timerTotal2 = 0;
		private bool overtime = false;

		public float minalt = -50;
		public float maxalt = 500;
		public float maxdis = 1500;
		public float maxoob = 120;
		public float maxtime = 900;
		public float maxcost = 150000;
		public float maxmat = 10000;
		public float matconv = -1;
		public float spawndis = 1000;
		public float spawngap = 100;
		public float penaltyhp = 50;

		/// <summary>
		/// If true, the standard rules for despawning will be used.
		/// </summary>
		public bool srules = true;
		public bool penaltynoai = true;
		public bool standardhp = true;

		private SortedDictionary<int,SortedDictionary<int,TournamentParticipant>> HUDLog = new SortedDictionary<int,SortedDictionary<int,TournamentParticipant>>();

		//public TournamentEntry entry_king;
		//public TournamentEntry entry_chal;

		public float t1_res;
		public float t2_res;

		public List<TournamentEntry> entry_t1 = new List<TournamentEntry>{ };
		public List<TournamentEntry> entry_t2 = new List<TournamentEntry>{ };

		//public AudioClip audioClip;

		public Tournament()
		{
			_me = this;
			//var gui = new InstantGui();
			_GUI = new TournamentGUI();


			//(Font)Resources.Load("LiberationMono-Regular.ttf");
			//Debug.developerConsoleVisible = true;
			//			var f = new WWW("file:///"+StaticPaths.GetAssetFileNameForMod("Tournament", "LiberationMono-Regular.ttf"));
			//			while (!f.isDone) {}
			//			_Font = f;
			//_Font = Resources.Load("Assets/LiberationMono-Regular") as Font;
			//Debug.Log(_Font.name);
			_Top = new GUIStyle(LazyLoader.HUD.Get().interactionStyle);
			_Top.alignment = TextAnchor.MiddleCenter;
			_Top.richText = true;
			_Top.fontSize = 12;

			_Left = new GUIStyle(LazyLoader.HUD.Get().interactionStyle);
			_Left.alignment = TextAnchor.UpperLeft;
			//_Left.font =_Font;
			_Left.richText = true;
			_Left.fontSize = 12;
			_Left.wordWrap = false;
			_Left.clipping = TextClipping.Clip;

			_Right = new GUIStyle(LazyLoader.HUD.Get().interactionStyle);
			_Right.alignment = TextAnchor.UpperRight;
			//_Right.font = _Font;
			_Right.richText = true;
			_Right.fontSize = 12;
			_Right.wordWrap = false;
			_Right.clipping = TextClipping.Clip;

			//audioClip = (AudioClip)new WWW("file:///"+StaticPaths.GetAssetFileNameForMod("Tournament", "boom.ogg")).assetBundle.mainAsset;
			//WWW wWW = new WWW("file:///"+StaticPaths.GetAssetFileNameForMod("Tournament", "boom.ogg"));
			//Debug.Log("file:///"+StaticPaths.GetAssetFileNameForMod("Tournament", "boom.ogg"));
			//while (!wWW.isDone) {
			//}
			//audioClip = wWW.GetAudioClip(true);
		}

		public void StartMatch()
		{
			ClearArea();
			HUDLog.Clear();
			InstanceSpecification.i.Header.CommonSettings.EnemyBlockDestroyedResourceDrop = matconv / 100;

			//1/entry_king.Spawn(spawndis);
			//1/entry_king.team_id.FactionInst.ResourceStore.SetResources(maxmat);
			//1/entry_king.res = maxmat;
			//1/entry_chal.Spawn(spawndis);
			//1/entry_chal.team_id.FactionInst.ResourceStore.SetResources(maxmat);
			//1/entry_chal.res = maxmat;

			t1_res = maxmat;
			foreach (TournamentEntry tp in entry_t1) {
				tp.Spawn(spawndis, spawngap, entry_t1.Count, entry_t1.IndexOf(tp));
				tp.team_id.FactionInst().ResourceStore.SetResources(maxmat);
			}

			t2_res = maxmat;
			foreach (TournamentEntry tp in entry_t2) {
				tp.Spawn(spawndis, spawngap, entry_t2.Count, entry_t2.IndexOf(tp));
				tp.team_id.FactionInst().ResourceStore.SetResources(maxmat);
			}

			timerTotal = 0;
			timerTotal2 = Time.timeSinceLevelLoad;

			standardhp |= srules;
			penaltynoai |= !srules;

			if (srules) {
				InstanceSpecification.i.Header.CommonSettings.ConstructableCleanUp = ConstructableCleanUp.Ai;
			} else {
				InstanceSpecification.i.Header.CommonSettings.ConstructableCleanUp = ConstructableCleanUp.Off;
			}

			foreach (MainConstruct current in StaticConstructablesManager.constructables) {
				if (!HUDLog.ContainsKey(current.GetTeam().Id)) {
					HUDLog.Add(current.GetTeam().Id, new SortedDictionary<int, TournamentParticipant>());
				}
				//if standard hp and has ai (not spawn stick)
				if (standardhp && !HUDLog[current.GetTeam().Id].ContainsKey(current.UniqueId)) {
					HUDLog[current.GetTeam().Id].Add(current.UniqueId, new TournamentParticipant {
						TeamId = current.GetTeam(),
						TeamName = current.GetTeam().FactionSpec().AbreviatedName,
						UniqueId = current.UniqueId,
						BlueprintName = current.GetBlueprintName(),
						AICount = current.BlockTypeStorage.MainframeStore.Blocks.Count,
						HP = current.BlockTypeStorage.MainframeStore.Blocks.Count > 0 ? current.iMainStatus.GetFractionAliveBlocksIncludingSubConstructables() * 100 : current.iMainStatus.GetFractionAliveBlocks() * 100,
						HPCUR = current.BlockTypeStorage.MainframeStore.Blocks.Count > 0 ? current.iMainStatus.GetNumberAliveBlocksIncludingSubConstructables() : current.iMainStatus.GetNumberAliveBlocks(),
						HPMAX = current.BlockTypeStorage.MainframeStore.Blocks.Count > 0 ? current.iMainStatus.GetNumberBlocksIncludingSubConstructables() : current.iMainStatus.GetNumberBlocks()
					});
				}
				//if resource hp and not spawn stick
				else if (!standardhp && !HUDLog[current.GetTeam().Id].ContainsKey(current.UniqueId)) {
					HUDLog[current.GetTeam().Id].Add(current.UniqueId, new TournamentParticipant {
						TeamId = current.GetTeam(),
						TeamName = current.GetTeam().FactionSpec().AbreviatedName,
						UniqueId = current.UniqueId,
						BlueprintName = current.GetBlueprintName(),
						AICount = current.BlockTypeStorage.MainframeStore.Blocks.Count,
						HP = 100,
						HPCUR = current.iResourceCosts.GetResourceCost().Material,
						HPMAX =	current.iResourceCosts.GetResourceCost().Material
					});
				}
			}
			GameEvents.Twice_Second += SlowUpdate;
			GameEvents.FixedUpdateEvent += FixedUpdate;
			GameEvents.OnGui += OnGUI;
			Time.timeScale = 0f;
			ResetCam();
		}

		public void ClearArea()
		{
			ForceManager.Instance.forces.ForEach(t => ForceManager.Instance.DeleteForce(t));
		}

		public void ResetCam()
		{
			GuiDisplayer.displayGUIs = false;
			I_All_cInterface @interface = ClientInterface.GetInterface();
			if (@interface != null) {
				@interface.Get_I_world_cCameraControl().DetachCameraCompletely();
			}
			CameraManager.GetSingleton().TransformToMove.rotation = Quaternion.LookRotation(Vector3.right);
			CameraManager.GetSingleton().TransformToMove.position = new Vector3(-500, 50, 0);
		}

		public void OnGUI()
		{
			GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(1f * (float)Screen.width / 1280f, 1f * (float)Screen.height / 800f, 1f));
			GUI.backgroundColor = new Color(0f, 0f, 0f, .6f);
			GUI.Label(new Rect(590f, 0f, 100f, 30f), String.Format("{0}m {1}s", Math.Floor(timerTotal / 60), (Math.Floor(timerTotal)) % 60), _Top);

			foreach (KeyValuePair<int,SortedDictionary<int,TournamentParticipant>> team in HUDLog) { //foreach team
				int y = 0;
				bool king = entry_t1[0].team_id.Id == team.Key;
				var temp = "";
				int cnt = 1;
				int cntmax = HUDLog[team.Key].Values.Count;
				float teamhp = 0;
				float teamhpmax = 0;
				foreach (KeyValuePair<int,TournamentParticipant> member in team.Value) { //foreach member in team
					//GUI.color = new Color(1f, 0f, 0f, 1f);
					if (!member.Value.Disqual && !member.Value.Scrapping && (penaltynoai && member.Value.AICount != 0)) {
						teamhp += member.Value.HPCUR;
						teamhpmax += member.Value.HPMAX;
						if (king) {
							temp += String.Format("\n{0,-6} {1,-4} {2}", Math.Floor(member.Value.OoBTime / 60) + "m" + Math.Floor(member.Value.OoBTime) % 60 + "s", Math.Round(member.Value.HP, 1) + "%", member.Value.BlueprintName);
						} else {
							temp += String.Format("\n{2} {1,4} {0,6}", Math.Floor(member.Value.OoBTime / 60) + "m" + Math.Floor(member.Value.OoBTime) % 60 + "s", Math.Round(member.Value.HP, 1) + "%", member.Value.BlueprintName);
						}
					} else {
						teamhpmax += member.Value.HPMAX;
						if (king) {
							temp += String.Format("\n{0,-16}{1}", "DQ", member.Value.BlueprintName);
						} else {
							temp += String.Format("\n{1}{0,16}", "DQ", member.Value.BlueprintName);
						}
					}
					if (cnt == cntmax) {
						if (king) {
							GUI.Label(new Rect(0f, 0f, 200f, 38f + 16f * cntmax), String.Format("{0,-6} <color=#ffa500ff>{1,-4}</color> <color=cyan>{2}M</color>\n{3}",
								"Team 1",
								Math.Round(teamhp / teamhpmax * 100, 1) + "%",
								FactionSpecifications.i.Factions.Find(f => f.Id.Id == team.Key).InstanceOfFaction.ResourceStore.Material,
								temp
							), _Left);
						} else {
							GUI.Label(new Rect(1080f, 0f, 200f, 38f + 16f * cntmax), String.Format("<color=cyan>{2}M</color> <color=#ffa500ff>{1,4}</color> {0,6}\n{3}",
								"Team 2",
								Math.Round(teamhp / teamhpmax * 100, 1) + "%",
								FactionSpecifications.i.Factions.Find(f => f.Id.Id == team.Key).InstanceOfFaction.ResourceStore.Material,
								temp
							), _Right);
						}
					}
					cnt += 1;
					y += 1;
				}
			}
		}

		public void FixedUpdate(ITimeStep dt)
		{
			if (Time.timeScale != 0) { //dont calc when paused....
				if (matconv == -1) { //no material fix
					if (t1_res < entry_t1[0].team_id.FactionInst().ResourceStore.Material.Quantity) {
						entry_t1[0].team_id.FactionInst().ResourceStore.SetResources(t1_res);
					} else {
						t1_res = (float)entry_t1[0].team_id.FactionInst().ResourceStore.Material.Quantity;
					}
					if (t2_res < entry_t2[0].team_id.FactionInst().ResourceStore.Material.Quantity) {
						entry_t2[0].team_id.FactionInst().ResourceStore.SetResources(t2_res);
					} else {
						t2_res = (float)entry_t2[0].team_id.FactionInst().ResourceStore.Material.Quantity;
					}
				}

				foreach (MainConstruct current in StaticConstructablesManager.constructables.ToArray()) { //alive loop
					if (!HUDLog[current.GetTeam().Id][current.UniqueId].Disqual || !HUDLog[current.GetTeam().Id][current.UniqueId].Scrapping) {
						if (penaltyhp > 0 && HUDLog[current.GetTeam().Id][current.UniqueId].HP < penaltyhp && !srules) { //dont check aicount or location oob if under hp%
							HUDLog[current.GetTeam().Id][current.UniqueId].OoBTime += Time.timeSinceLevelLoad - timerTotal - timerTotal2;
						} else {
							HUDLog[current.GetTeam().Id][current.UniqueId].AICount = current.BlockTypeStorage.MainframeStore.Blocks.Count;
							if (penaltynoai && HUDLog[current.GetTeam().Id][current.UniqueId].AICount == 0) { //dont check location oob if aipenalty on and no ai
								HUDLog[current.GetTeam().Id][current.UniqueId].OoBTime += Time.timeSinceLevelLoad - timerTotal - timerTotal2;
							} else {
								if (current.CentreOfMass.y < minalt || current.CentreOfMass.y > maxalt) { //check Out of Bounds vertically
									HUDLog[current.GetTeam().Id][current.UniqueId].OoBTime += Time.timeSinceLevelLoad - timerTotal - timerTotal2;
								} else { //check Out of Bounds horizontally
									float nearest = -1;
									float nearestvelocity = -1;
									foreach (MainConstruct current2 in StaticConstructablesManager.constructables.ToArray()) {
										if (current != current2 && current.GetTeam() != current2.GetTeam()) { //find nearest enemy
											float distance = Vector3.Distance(current.CentreOfMass, current2.CentreOfMass);
											if (nearest < 0) {
												nearest = distance;
												nearestvelocity = Vector3.Distance(current.CentreOfMass + current.iPartPhysics.iVelocities.VelocityVector, current2.CentreOfMass);
											} else if (Vector3.Distance(current.CentreOfMass, current2.CentreOfMass) < nearest) {
												nearest = distance;
												nearestvelocity = Vector3.Distance(current.CentreOfMass + current.iPartPhysics.iVelocities.VelocityVector, current2.CentreOfMass);
											}
										}
									}
									if (nearest > maxdis && nearest < nearestvelocity) { //if further than nearest and moving away
										HUDLog[current.GetTeam().Id][current.UniqueId].OoBTime += Time.timeSinceLevelLoad - timerTotal - timerTotal2;
									}
								}
							}
						}
						HUDLog[current.GetTeam().Id][current.UniqueId].Disqual = HUDLog[current.GetTeam().Id][current.UniqueId].OoBTime > maxoob;
					}
				}
				timerTotal += Time.timeSinceLevelLoad - timerTotal - timerTotal2;
			}
		}

		public void SlowUpdate(ITimeStep dt)
		{
			foreach (MainConstruct current in StaticConstructablesManager.constructables.ToArray()) {
				if (standardhp && (!HUDLog[current.GetTeam().Id][current.UniqueId].Disqual || !HUDLog[current.GetTeam().Id][current.UniqueId].Scrapping)) {
					HUDLog[current.GetTeam().Id][current.UniqueId].HPCUR = current.iMainStatus.GetNumberAliveBlocksIncludingSubConstructables();
					HUDLog[current.GetTeam().Id][current.UniqueId].HP =	current.iMainStatus.GetFractionAliveBlocksIncludingSubConstructables() * 100;
				} else if (!standardhp && (!HUDLog[current.GetTeam().Id][current.UniqueId].Disqual || !HUDLog[current.GetTeam().Id][current.UniqueId].Scrapping)) {
					HUDLog[current.GetTeam().Id][current.UniqueId].HPCUR = current.iResourceCosts.CalculateResourceCostOfAliveBlocksIncludingSubConstructs_ForCashBack(true).Material;
					HUDLog[current.GetTeam().Id][current.UniqueId].HP =	HUDLog[current.GetTeam().Id][current.UniqueId].HPCUR / HUDLog[current.GetTeam().Id][current.UniqueId].HPMAX * 100;
				} else {
					HUDLog[current.GetTeam().Id][current.UniqueId].HPCUR = 0;
					HUDLog[current.GetTeam().Id][current.UniqueId].HP =	0;
				}
			}

			foreach (KeyValuePair<int,SortedDictionary<int,TournamentParticipant>> team in HUDLog) { //foreach team
				foreach (KeyValuePair<int,TournamentParticipant> member in HUDLog[team.Key]) { //foreach member in team

					if ((!StaticConstructablesManager.constructables.Contains(StaticConstructablesManager.constructables.Find(c => c.GetTeam() == member.Value.TeamId && c.UniqueId == member.Key))
					    || HUDLog[team.Key][member.Key].Disqual)
					    && !HUDLog[team.Key][member.Key].Scrapping) {
						HUDLog[team.Key][member.Key].HPCUR = 0;
						HUDLog[team.Key][member.Key].Scrapping = true;
						var loc = StaticConstructablesManager.constructables.Find(c => c.GetTeam() == member.Value.TeamId && c.UniqueId == member.Key).iMain.CentreOfMass;
						UnityEngine.Object.Instantiate(Resources.Load("Detonator-MushroomCloud") as GameObject, loc, Quaternion.identity);
						StaticConstructablesManager.constructables.Find(c => c.GetTeam() == member.Value.TeamId && c.UniqueId == member.Key).DestroyCompletely();
//						GameObject gameObject = new GameObject();
//						gameObject.transform.position = loc;
//						AudioSource audioSource = gameObject.AddComponent<AudioSource>();
//						audioSource.clip = audioClip;
//						audioSource.Play();
//						TimedObjectDestructor timedObjectDestructor = gameObject.AddComponent<TimedObjectDestructor>();
//						timedObjectDestructor.timeOut = audioClip.length + 2f;
					}
				}
			}

			if (timerTotal > maxtime && overtime == false) {
				Time.timeScale = 0f;
				overtime = true;
			}
		}
	}
}
