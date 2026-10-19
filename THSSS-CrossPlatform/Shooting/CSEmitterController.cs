using System.Drawing;

namespace Shooting;

public class CSEmitterController : BaseBulleEmitter
{
	private CS_Data CSData;

	public bool BossBinding { get; set; }

	public bool OnRoad { get; set; }

	public PointF BaseOriginalPosition { get; set; }

	public CSEmitterController(StageDataPackage StageData, CS_Data CSData)
		: base(StageData)
	{
		this.CSData = CSData;
		BaseOriginalPosition = new PointF(base.BoundRect.Width / 2, 130f);
	}

	public override void Shoot()
	{
		if (base.Time >= CSData.TimeTotal)
		{
			base.Time -= CSData.TimeTotal;
		}
		if (OnRoad && base.Boss != null)
		{
			return;
		}
		CSData.EmitterList.ForEach(delegate(BaseEmitter_CS em)
		{
			if (em.BindingID == -1 && em.StartTime == base.Time)
			{
				BaseEmitter_CS baseEmitter_CS = (BaseEmitter_CS)em.Clone();
				if (BossBinding && base.Boss != null)
				{
					baseEmitter_CS.OriginalPosition = new PointF(baseEmitter_CS.OriginalPosition.X + base.Boss.OriginalPosition.X - BaseOriginalPosition.X, baseEmitter_CS.OriginalPosition.Y + base.Boss.OriginalPosition.Y - BaseOriginalPosition.Y);
					baseEmitter_CS.EmitPoint = new PointF(-99998f, -99998f);
					baseEmitter_CS.SetBinding(base.Boss);
				}
				baseEmitter_CS.Time = base.Time;
				base.StageData.EnemyPlaneList.Add(baseEmitter_CS);
			}
		});
	}
}
