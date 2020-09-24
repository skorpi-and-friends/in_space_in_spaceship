using System;
using Godot;

#if GODOT_Real_IS_DOUBLE
using Real = System.Double;
#else
using Real = System.Single;
#endif

namespace ISIS {
	public class Obstacle : RigidBody {
		private const int MetricTonMultiplier = 1000;
		[Export] public int MassMetricTon { get; set; } = 1;

		public override void _EnterTree() {
			base._EnterTree();
			System.Diagnostics.Debug.Assert(MassMetricTon > 0);
			Mass = MassMetricTon * MetricTonMultiplier;
		}
	}
}
