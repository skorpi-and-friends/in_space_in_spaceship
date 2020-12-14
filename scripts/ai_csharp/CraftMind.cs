using Godot;
using GreenBehaviors;
using GreenBehaviors.Composite;
using GreenBehaviors.Decorator;
using GreenBehaviors.LeafLambda;
using ISIS.SteeringBehaviors;
using static ISIS.Static;
// using SteeringRoutineResult = System.ValueTuple<Godot.Vector3, Godot.Vector3, string>;

namespace ISIS.Minds {
	// KEEP THIS CLASS SIMPLE BOYO
	public partial class CraftMind : Godot.Node {
		public RigidBody Craft => GetParent<RigidBody>();
		public Boid Presence => GetNode<Boid>("../Boid");
#if DEBUG
		[Export] public string ActiveRoutineDesc = "NO ACTIVE ROUTINE";
#endif
		[Export] public bool EnableAutoPilot { get; set; } = true;
		public ScanPresence Target { get; set; }
		public SteeringRoutineClosure ActiveRoutine { get; set; }
		// public SteeringRoutineClosure SurvivalRoutine { get; set; }

		public async override void _Ready() {
			base._Ready();
			await ToSignal(GetTree(), "idle_frame");
			// SurvivalRoutine = SteeringRoutines.AvoidObstacleSebLagueRay(Craft, GetCraftExtents(Craft));
			// SurvivalRoutine = SteeringRoutines.AvoidObstacle(Craft);
		}

		public override void _Process(float delta) {
			base._Process(delta);
			if (EnableAutoPilot) {
				var state = GetCraftState(Craft);
				// var craftInput = (Vector3.Zero, Vector3.Zero);
				var linearInput = Vector3.Zero;
				var angularInput = Vector3.Zero;
				if (ActiveRoutine != null) {
					(linearInput, angularInput) = ActiveRoutine.Invoke(Craft.GlobalTransform, state);
				}
				linearInput *= state.LinearVLimit;
				state.SetCraftInput(linearInput, angularInput);
			}
		}

		public virtual void RemoveActiveRoutine() {
#if DEBUG
			ActiveRoutineDesc = "NO ACTIVE ROUTINE";
#endif
			ActiveRoutine = null;
			// ActiveRoutine = SteeringRoutines.GetSurvivalRoutine(
			// 	Craft,
			// 	GetCraftExtents(Craft)
			// );
		}

		public virtual void InterceptSetTarget() {
			if (Target == null) {
				return;
			}
#if DEBUG
			ActiveRoutineDesc = $"Intercepting Target {Target.Name}";
#endif
			ActiveRoutine = SteeringRoutines.LookWhereYouGoRoutineComposer(
				SteeringRoutines.SurvivalRoutineComposer(
					SteeringRoutines.InterceptRoutine(Target),
					Craft,
					GetCraftExtents(Craft)
				)
			);
		}

		public virtual void EliminateSetTarget(in DecoratorNode craftInputRoutineWrapper) {
			if (Target == null) {
				return;
			}
#if DEBUG
			ActiveRoutineDesc = $"Eliminating Target {Target.Name}";
#endif
			ActiveRoutine = SteeringRoutines.SurvivalRoutineComposer(
				SteeringRoutines.AttackPersueRoutineClosure(in craftInputRoutineWrapper,
					Craft,
					Target
				),
				Craft,
				GetCraftExtents(Craft)
			);
		}

		public virtual void FollowPath(Path path) {
#if DEBUG
			ActiveRoutineDesc = $"Following Path {path}";
#endif
			ActiveRoutine = SteeringRoutines.LookWhereYouGoRoutineComposer(
				SteeringRoutines.SurvivalRoutineComposer(
					SteeringRoutines.FollowPathRoutine(path),
					Craft,
					GetCraftExtents(Craft)
				)
			);
		}
	}

	#region UTILITIES
	public partial class CraftMind {
		public static(bool, CraftMind) IsMindfulCraft(object instance) {
			var(isCraftMaster, craft) = IsCraftMaster(instance);
			if (!isCraftMaster) {
				return (false, null);
			}
			return IsMindful(craft);
		}

		public static(bool, CraftMind) IsMindful(RigidBody craft) {
			var mind = craft.GetNodeOrNull<CraftMind>("Mind");
			if (mind == null) {
				return (false, null);
			}
			return (true, mind);
		}

		public static(bool, RigidBody) IsCraftMaster(object instance) {
			if (!(instance is RigidBody godotObject))
				return (false, null);

			var craftMasterScript = GD.Load<GDScript>("res://scripts/crafts/craft_master.gd");
			if (!IsInstanceOfGDScript(godotObject, craftMasterScript))
				return (false, null);

			return (true, godotObject);
		}

		/// <summary>
		/// Assumes passed craft is already verified to be a craft
		/// </summary>
		public static CraftStateWrapper GetCraftState(Godot.Object craft) {
			var engine = (Object) craft.Get("engine");
			return new CraftStateWrapper((Godot.Object) engine.Get("state"));
		}

		public static Vector3 GetCraftExtents(Godot.Object craft) {
			var engine = (Godot.Object) craft.Get("engine");
			return (Vector3) engine.Get("craft_extents");
		}

		public static void FirePrimaryWeapons(Godot.Object craft) {
			((Godot.Object) craft.Get("arms")).Call("activate_primary");
		}
	}
	#endregion
}