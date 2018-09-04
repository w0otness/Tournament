using System;
using System.Collections.Generic;
using BrilliantSkies.Core.Timing;
using BrilliantSkies.FromTheDepths.Game;
using BrilliantSkies.Ftd.Avatar;
using BrilliantSkies.Ftd.Planets.Factions;
using BrilliantSkies.Ftd.Planets.Instances;
using BrilliantSkies.Ftd.Planets.Instances.Headers;
using BrilliantSkies.PlayerProfiles;
using BrilliantSkies.Ui.Displayer;
using UnityEngine;
using BrilliantSkies.Effects.Cameras;
using BrilliantSkies.Core.Returns.PositionAndRotation;
using BrilliantSkies.Core.UniverseRepresentation.Positioning.Frames.Points;
using BrilliantSkies.Core.Types;
using BrilliantSkies.Ftd.Planets.World;
using BrilliantSkies.Ftd.Planets;

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
				StandardHP/*,
				ArrayHP*/
			}
		}

		public static Tournament _me;

		public TournamentGUI _GUI;
		public Font _Font;
		public GUIStyle _Top;
		public GUIStyle _Left;
		public GUIStyle _Right;

		private float timerTotal = 0;
		private float timerTotal2 = 0;
		private bool overtime = false;
		private int orbitIndex = 0;
		private int orbitID = -1;
		private int lastCount = -1;

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
		public int northSouthBoard = 0;
		public int eastWestBoard = 0;

		/// <summary>
		/// If true, the standard rules for despawning will be used.
		/// </summary>
		public bool srules = true;
		public bool penaltynoai = true;
		public bool standardhp = true;

		private SortedDictionary<int,SortedDictionary<int,TournamentParticipant>> HUDLog = new SortedDictionary<int,SortedDictionary<int,TournamentParticipant>>();

		public float t1_res;
		public float t2_res;

		public List<TournamentEntry> entry_t1 = new List<TournamentEntry>();
		public List<TournamentEntry> entry_t2 = new List<TournamentEntry>();

		private GameObject justOrbitCam;
		private MouseLook flycam;
		private MouseOrbit orbitcam;

		public Tournament()
		{
			_me = this;
			_GUI = new TournamentGUI();

			_Top = new GUIStyle(LazyLoader.HUD.Get().interactionStyle) {
				alignment = TextAnchor.MiddleCenter,
				richText = true,
				fontSize = 12
			};

			_Left = new GUIStyle(LazyLoader.HUD.Get().interactionStyle) {
				alignment = TextAnchor.UpperLeft,
				//font =_Font;
				richText = true,
				fontSize = 12,
				wordWrap = false,
				clipping = TextClipping.Clip
			};

			_Right = new GUIStyle(LazyLoader.HUD.Get().interactionStyle) {
				alignment = TextAnchor.UpperRight,
				//font = _Font;
				richText = true,
				fontSize = 12,
				wordWrap = false,
				clipping = TextClipping.Clip
			};
		}

		public Vector3d getBoardOffset() {
			return StaticCoordTransforms.BoardSectionToUniversalPosition(WorldSpecification.i.BoardLayout.BoardSections[eastWestBoard, northSouthBoard].BoardSectionCoords);
		}

		public void StartMatch()
		{
			ClearArea();
			
			InstanceSpecification.i.Header.CommonSettings.EnemyBlockDestroyedResourceDrop = matconv / 100;

			t1_res = maxmat;
			foreach (TournamentEntry tp in entry_t1) {
				tp.Spawn(spawndis, spawngap, entry_t1.Count, entry_t1.IndexOf(tp));
				tp.Team_id.FactionInst().ResourceStore.SetResources(maxmat);
			}

			t2_res = maxmat;
			foreach (TournamentEntry tp in entry_t2) {
				tp.Spawn(spawndis, spawngap, entry_t2.Count, entry_t2.IndexOf(tp));
				tp.Team_id.FactionInst().ResourceStore.SetResources(maxmat);
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
						HP = 100,
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
						HPCUR = current.BlockTypeStorage.MainframeStore.Blocks.Count > 0 ? current.iResourceCosts.CalculateResourceCostOfAliveBlocksIncludingSubConstructs_ForCashBack(true).Material : current.iResourceCosts.GetResourceCostAllNotIncludingSubVehicles().Material,
						HPMAX = current.BlockTypeStorage.MainframeStore.Blocks.Count > 0 ? current.iResourceCosts.CalculateResourceCostOfAliveBlocksIncludingSubConstructs_ForCashBack(true).Material : current.iResourceCosts.GetResourceCostAllNotIncludingSubVehicles().Material
					});
				}
			}
			GameEvents.Twice_Second += SlowUpdate;
			GameEvents.FixedUpdateEvent += FixedUpdate;
			GameEvents.OnGui += OnGUI;
			GameEvents.PreLateUpdate += PreLateUpdate;
			GameEvents.UpdateEvent -= UpdateBoardSectionPreview;
			Time.timeScale = 0f;
			orbitID = StaticConstructablesManager.constructables[0].UniqueId;
			lastCount = StaticConstructablesManager.constructables.Count;
			ResetCam();
		}

		public void ClearArea() {
			ForceManager.Instance.forces.ForEach(t => ForceManager.Instance.DeleteForce(t));
			HUDLog.Clear();
		}

		public void ResetCam()
		{
			GuiDisplayBase.displayGUIs = false;
			foreach (PlayerSetupBase current in Objects.Instance.Players.Objects) {
				UnityEngine.Object.Destroy(current.gameObject);
			}
			justOrbitCam=R_Avatars.JustOrbitCamera.InstantiateACopy().gameObject;
			justOrbitCam.transform.position = new Vector3(0, 50, 0);
			justOrbitCam.transform.rotation = Quaternion.LookRotation(Vector3.right, Vector3.up);
			flycam = justOrbitCam.AddComponent<MouseLook>();
			flycam.enabled = true;
			orbitcam = justOrbitCam.GetComponent<MouseOrbit>();
			orbitcam.enabled = false;
			orbitcam.distance = 100;
			orbitcam.OperateRegardlessOfUiOptions = true;
			orbitcam.UseOrbitTargetRotation = false;
		}

		public void OnGUI()
		{
			GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(1f * Screen.width / 1280f, 1f * Screen.height / 800f, 1f));
			GUI.backgroundColor = new Color(0f, 0f, 0f, .6f);
			GUI.Label(new Rect(590f, 0f, 100f, 30f), string.Format("{0}m {1}s", Math.Floor(timerTotal / 60), (Math.Floor(timerTotal)) % 60), _Top);

			foreach (KeyValuePair<int,SortedDictionary<int,TournamentParticipant>> team in HUDLog) { //foreach team
				int y = 0;
				bool king = entry_t1[0].Team_id.Id == team.Key;
				string temp = "";
				int cnt = 1;
				int cntmax = HUDLog[team.Key].Values.Count;
				float teamhp = 0;
				float teamhpmax = 0;
				foreach (KeyValuePair<int,TournamentParticipant> member in team.Value) { //foreach member in team
					if (!member.Value.Disqual && !member.Value.Scrapping && (penaltynoai && member.Value.AICount != 0)) {
						teamhp += member.Value.HPCUR;
						teamhpmax += member.Value.HPMAX;
						if (king) {
							temp += string.Format("\n{0,-6} {1,-4} {2}", Math.Floor(member.Value.OoBTime / 60) + "m" + Math.Floor(member.Value.OoBTime) % 60 + "s", Math.Round(member.Value.HP, 1) + "%", member.Value.BlueprintName);
						} else {
							temp += string.Format("\n{2} {1,4} {0,6}", Math.Floor(member.Value.OoBTime / 60) + "m" + Math.Floor(member.Value.OoBTime) % 60 + "s", Math.Round(member.Value.HP, 1) + "%", member.Value.BlueprintName);
						}
					} else {
						teamhpmax += member.Value.HPMAX;
						if (king) {
							temp += string.Format("\n{0,-16}{1}", "DQ", member.Value.BlueprintName);
						} else {
							temp += string.Format("\n{1}{0,16}", "DQ", member.Value.BlueprintName);
						}
					}
					if (cnt == cntmax) {
						if (king) {
							GUI.Label(new Rect(0f, 0f, 200f, 38f + 16f * cntmax), string.Format("{0,-6} <color=#ffa500ff>{1,-4}</color> <color=cyan>{2}M</color>\n{3}",
								"Team 1",
								Math.Round(teamhp / teamhpmax * 100, 1) + "%",
								FactionSpecifications.i.Factions.Find(f => f.Id.Id == team.Key).InstanceOfFaction.ResourceStore.Material,
								temp
							), _Left);
						} else {
							GUI.Label(new Rect(1080f, 0f, 200f, 38f + 16f * cntmax), string.Format("<color=cyan>{2}M</color> <color=#ffa500ff>{1,4}</color> {0,6}\n{3}",
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
		#region Updates
		public void FixedUpdate(ITimeStep dt)
		{
			if (Time.timeScale != 0) { //dont calc when paused....
				if (matconv == -1) { //no material fix
					if (t1_res < entry_t1[0].Team_id.FactionInst().ResourceStore.Material.Quantity) {
						entry_t1[0].Team_id.FactionInst().ResourceStore.SetResources(t1_res);
					} else {
						t1_res = (float)entry_t1[0].Team_id.FactionInst().ResourceStore.Material.Quantity;
					}
					if (t2_res < entry_t2[0].Team_id.FactionInst().ResourceStore.Material.Quantity) {
						entry_t2[0].Team_id.FactionInst().ResourceStore.SetResources(t2_res);
					} else {
						t2_res = (float)entry_t2[0].Team_id.FactionInst().ResourceStore.Material.Quantity;
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
					}
				}
			}
			if (timerTotal > maxtime && overtime == false) {
				Time.timeScale = 0f;
				overtime = true;
			}
		}

		public void UpdateBoardSectionPreview(ITimeStep dt) {
			//Vector3d currentBoard = getBoardOffset();
			//justOrbitCam.transform.position = currentBoard.ToSingle() + new Vector3(0, 50, 0);
			justOrbitCam.transform.Rotate(0, (float)(15 * dt.PhysicalDeltaTime.Seconds), 0);//15° pro vergangene Sekunde
		}

		public void PreLateUpdate()
		{
			FtdKeyMap ftdKeyMap = ProfileManager.Instance.GetModule<FtdKeyMap>();
			
			float axis = Input.GetAxis("Mouse ScrollWheel");
			bool shiftPressed = Input.GetKey(KeyCode.LeftShift) | Input.GetKey(KeyCode.RightShift);
			orbitcam.distance -= axis * (shiftPressed ? 200 : 50);
			int count = StaticConstructablesManager.constructables.Count;
			if (lastCount > count) {//Ein Teilnehmer ist von uns gegangen.
				lastCount = count;
				int foundIt = StaticConstructablesManager.constructables.FindIndex((mc) => mc.UniqueId == orbitID);
				if (foundIt == -1) {//Das aktuelle Ziel der Orbitkamera wurde zerstört.
					orbitIndex %= count;
				} else {
					orbitIndex = foundIt;
				}
			}

			orbitcam.xSpeed = (shiftPressed ? 1000 : 250);
			orbitcam.ySpeed = (shiftPressed ? 480 : 120);
			if (Input.GetKeyUp(ftdKeyMap.GetKeyDef(KeyInputsFtd.PauseGame).Key)) {//Default Pause-Key is F11.
				Time.timeScale = (Time.timeScale > 0 ? 0 : 1);
			}
			if (Input.GetKeyUp(ftdKeyMap.GetKeyDef(KeyInputsFtd.InventoryUi).Key)) {//Default Inventory-Key is E.
				orbitIndex = (orbitIndex + 1) % count;
			}
			if (Input.GetKeyUp(ftdKeyMap.GetKeyDef(KeyInputsFtd.Interact).Key)) {//Default Interaction-Key is Q.
				if (orbitIndex == 0) {
					orbitIndex = count;
				}
				orbitIndex--;
			}
			if (Input.GetMouseButtonUp(0)&&count!=0) {//Linke Maustaste und mindestens noch einer da
				flycam.enabled = false;
				orbitcam.enabled = true;
			} else if (Input.GetMouseButtonUp(1)) {//Rechte Maustaste
				flycam.enabled = true;
				orbitcam.enabled = false;
				flycam.transform.rotation = orbitcam.transform.rotation;
			}
			if (flycam.enabled) {
				Vector3 movement = ftdKeyMap.GetMovemementDirection() * (shiftPressed ? 5 : 1);
				flycam.transform.position += flycam.transform.localRotation * movement;
			} else if (orbitcam.enabled) {
				if (count == 0) {//Alle tot!
					flycam.enabled = true;
					orbitcam.enabled = false;
				} else {
					MainConstruct currentConstruct = StaticConstructablesManager.constructables[orbitIndex];
					orbitID = currentConstruct.UniqueId;
					orbitcam.OrbitTarget = new PositionAndRotationReturnUniverseCoord(new UniversalTransform(new Vector3d(currentConstruct.CentreOfMass), currentConstruct.SafeRotation));
				}
			}
		}
		#endregion
	}
}