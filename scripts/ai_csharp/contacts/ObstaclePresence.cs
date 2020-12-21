using System;
using Godot;
#if GODOT_Real_IS_DOUBLE
using Real = System.Double;
#else
using Real = System.Single;
#endif

namespace ISIS.Minds {
	public class ObstaclePresence : ScanPresence {
		// public Real Radius { get; set; }
		public ObstacleSilhouette Silhouette { get; private set; }
		public override void _EnterTree() {
			base._EnterTree();
			AddToGroup("Obstacle");
		}
		public override void _Ready() {
			base._Ready();
			Silhouette = GetNode<ObstacleSilhouette>("ObstacleSilhouette");
			System.Diagnostics.Debug.Assert(Silhouette != null);
		}
		public Obstacle GetObstacle() {
			return (Obstacle) GetPresenceOwner();
		}
	}
}
