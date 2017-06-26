using System;
using UnityEngine;

public class AprilFirst : Block, IDetonatable, IAlive, IBlock
{
	private bool _detonated;

	private static ulong _frameDetonated;

	public override void Secondary(Transform T)
	{
		this.Detonate();
	}

	public void Detonate()
	{
		if (!this._detonated)
		{
			this._detonated = true;
			HUD.InfoStore.Add(new InfoSnippet("Boom!", 3f));
			ExplosionCreator.Explosion(this.MainConstruct, new ExplosionDamageDescription(this.MainConstruct.GunnerReward, 20000f, 50f, this.GameWorldPosition));
			if (GameTimer.Instance.FrameCounter != AprilFirst._frameDetonated)
			{
				UnityEngine.Object.Instantiate(Resources.Load("Detonator-MushroomCloud") as GameObject, this.GameWorldPosition, Quaternion.identity);
			}
			AprilFirst._frameDetonated = GameTimer.Instance.FrameCounter;
		}
	}

	public override InteractionReturn Secondary()
	{
		InteractionReturn interactionReturn = new InteractionReturn();
		interactionReturn.SpecialNameField = "Tactical nuke";
		interactionReturn.SpecialBasicDescriptionField = "Manually detonated explosive with a high yield";
		interactionReturn.AddExtraLine("Press <<Q>> to detonate the tactical nuke");
		return interactionReturn;
	}

	public override void StateChanged(IBlockStateChange change)
	{
		base.StateChanged(change);
		if (change.IsAvailableToConstruct)
		{
			this.MainConstruct.iBlockTypeStorage.DetonatableStore.Add(this);
		}
		else if (change.IsLostToConstructOrConstructLost)
		{
			this.MainConstruct.iBlockTypeStorage.DetonatableStore.Remove(this);
		}
		if (change.Killed)
		{
			this.Detonate();
		}
		else if (change.Type == BlockStateChangeType.Separated)
		{
			this.Detonate();
		}
	}

	public override BlockTechInfo GetTechInfo()
	{
		return new BlockTechInfo().AddStatement("Automatic Control Block 'Fire all weapons' will set this off").AddStatement("Destruction of this block will set it off").AddStatement("Vehicle warranty and insurance invalidated by placement of this thermonuclear device.");
	}
}
