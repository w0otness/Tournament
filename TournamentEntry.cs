using System;
using Assets.Scripts;
using Assets.Scripts.Persistence;
using BrilliantSkies.Core.Id;
using BrilliantSkies.Ftd.Planets.Instances;
using BrilliantSkies.Core.Types;
using UnityEngine;
namespace w0otness
{
	public class TournamentEntry
	{
		public bool IsKing { get; set; }
		public Tournament.SPAWN.DIR spawn_direction { get; set; }
		public Tournament.SPAWN.LOC spawn_location { get; set; }
		public ObjectId Team_id { get; set; }
		public float Res { get; set; }
		private BlueprintFile _bpf;
		public BlueprintFile bpf {
			get {
				return _bpf;
			}
			set {
				_bpf = value;
				bp = bpf.Load();
			}
		}
		public Blueprint bp;
		public string[] labelCost {
			get {
				if (bp != null) {
					var scs = bp.SCs.FindAll(x => !x.IsSubConstructable());
					var num = scs.Count;
					if (num > 0) {
						var s = new string[num + 1];
						float smax = 0;
						for (int i = 0; i < num; i++) {
							float max = scs[i].CalculateResourceCost(false, true).Material;
							s[i + 1] = String.Format("{0} <color=cyan>{1}</color>", scs[i].blueprintName, max);
							smax += max;
						}
						s[0] = string.Format("{0} <color=cyan>{1}</color>", bp.blueprintName, bp.CalculateResourceCost(false, true).Material - smax);
						return s;
					} else {
						var s = new string[1];
						s[0] = string.Format("{0} <color=cyan>{1}</color>", bp.blueprintName, bp.CalculateResourceCost(false, true).Material);
						return s;
					}
				}
				return null;
			}
		}
		public string[] label {
			get {
				if (bp != null) {
					var scs = bp.SCs.FindAll(x => !x.IsSubConstructable());
					var num = scs.Count;
					if (num > 0) {
						var s = new string[num + 1];
						float smax = 0;
						float scur = 0;
						for (int i = 0; i < num; i++) {
							float max = scs[i].CalculateResourceCost(false, true).Material;
							float cur = scs[i].CalculateResourceCost(true, true).Material;
							s[i + 1] = string.Format("{0} {1}", scs[i].blueprintName, Math.Round(cur / max * 100, 1));
							smax += max;
							scur += cur;
						}
						s[0] = string.Format("{0} {1}", bp.blueprintName, Math.Round((bp.CalculateResourceCost(true, true).Material - scur) / (bp.CalculateResourceCost(false, true).Material - smax) * 100, 1));
						return s;
					} else {
						var s = new string[1];
						s[0] = string.Format("{0} {1}", bp.blueprintName, Math.Round(bp.CalculateResourceCost(true, true).Material / bp.CalculateResourceCost(false, true).Material * 100, 1));
						return s;
					}
				}
				return null;
			}
		}

		public void Spawn(float dis, float gap, int count, int pos)
		{
			MainConstruct mainConstruct = BlueprintConverter.Convert(bp);
			Team_id = IsKing ? InstanceSpecification.i.Factions.Factions.Find(f => f.FactionSpec.AbreviatedName == "K").Id : InstanceSpecification.i.Factions.Factions.Find(f => f.FactionSpec.AbreviatedName == "C").Id;
			BlueprintConverter.Initiate(mainConstruct, new Vector3d(VLoc(gap, count, pos, dis)), VDir(), Team_id, null, SpawnPositioning.OriginOrCentre);
		}
		
		public Vector3 VLoc(float gap, int count, int pos, float dis)
		{
			switch (spawn_location) {
				case Tournament.SPAWN.LOC.Sea:
					return new Vector3((count - 1 * gap) / 2 - (pos * gap), 0, IsKing ? dis / 2 : dis / 2 - dis);
				case Tournament.SPAWN.LOC.Air:
					return new Vector3((count - 1 * gap) / 2 - (pos * gap), 50, IsKing ? dis / 2 : dis / 2 - dis);
				case Tournament.SPAWN.LOC.Sub:
					return new Vector3((count - 1 * gap) / 2 - (pos * gap), -10, IsKing ? dis / 2 : dis / 2 - dis);
				case Tournament.SPAWN.LOC.Land:
					return new Vector3((count - 1 * gap) / 2 - (pos * gap), 51, IsKing ? dis / 2 : dis / 2 - dis);
			}
			return new Vector3((count - 1 * gap) / 2 - (pos * gap), 0, IsKing ? 500 : -500);
		}
		
		public Quaternion VDir()
		{
			switch (spawn_direction) {
				case Tournament.SPAWN.DIR.Facing:
					return Quaternion.LookRotation(new Vector3(0, 0, IsKing ? -1 : 1));
				case Tournament.SPAWN.DIR.Away:
					return Quaternion.LookRotation(new Vector3(0, 0, IsKing ? 1 : -1));
				case Tournament.SPAWN.DIR.Left:
					return Quaternion.LookRotation(new Vector3(IsKing ? 1 : -1, 0, 0));
				case Tournament.SPAWN.DIR.Right:
					return Quaternion.LookRotation(new Vector3(IsKing ? -1 : 1, 0, 0));
			}
			return Quaternion.LookRotation(new Vector3(0, 0, IsKing ? -1 : 1));
		}
		
	}
}