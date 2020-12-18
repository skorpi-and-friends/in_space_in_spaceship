using Godot;
using GreenBehaviors;
using GreenBehaviors.Composite;
using GreenBehaviors.Decorator;
using GreenBehaviors.LeafLambda;
using ISIS.Minds.SteeringBehaviors;
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
				var currentTransform = Craft.GlobalTransform;

				var(linearInput, angularInput) = ActiveRoutine?.Invoke(currentTransform, state) ?? (Vector3.Zero, Vector3.Zero);

#if DEBUG 
				// GD.Print($"linear input: {linearInput}");
				// this.DebugDraw().Call("draw_line_3d", currentPosition, flockAverageCenter, new Color(0, 1, 0));
				this.DebugDraw().Call("draw_line_3d", Craft.GlobalTranslation(), currentTransform.origin + (linearInput * state.LinearVLimit), new Color(1, 1, 0));
				this.DebugDraw().Call("draw_line_3d", Craft.GlobalTranslation(), currentTransform.TransformPoint(state.LinearVelocty), new Color(0, 1, 1));
#endif
				linearInput = currentTransform.TransformVectorInv(linearInput) * state.LinearVLimit;
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
			ActiveRoutineDesc = $"Following Path {path.Name}";
#endif
			ActiveRoutine = SteeringRoutines.LookWhereYouGoRoutineComposer(
				SteeringRoutines.SurvivalRoutineComposer(
					SteeringRoutines.FollowPathRoutine(path),
					Craft,
					GetCraftExtents(Craft)
				)
			);
		}

		/// <summary>
		/// Does not add itself into flock.
		/// </summary>
		public virtual void FlyWithFlock(SteeringBehaviors.Boids.Flock flock) {
#if DEBUG
			ActiveRoutineDesc = $"Flying With FLock {flock.Name}";
#endif
			ActiveRoutine = SteeringRoutines.LookWhereYouGoRoutineComposer(
				SteeringRoutines.SurvivalRoutineComposer(
					SteeringRoutines.Cohesion(flock),
					Craft,
					GetCraftExtents(Craft)
				)
			);

			/* ActiveRoutine = SteeringRoutines.NoAngularInputComposer(
				SteeringRoutines.Cohesion(flock)
			); */
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
			if (engine == null) {
				var(isCraft, _) = IsCraftMaster(craft);
				GD.Print($"is {(craft as Godot.Node)?.Name} a craftMaster: {isCraft}");
				GD.Print($"does it have sire_craft: {craft.Get("sire_craft") != null}");
				GD.Print($"does it have arms: {craft.Get("arms") != null}");
				GD.Print($"does it have attires: {craft.Get("attires") != null}");
				GD.Print($"does it have mother: {craft.Get("mother") != null}");
				GD.Print($"script: {craft.GetScript()}");
				GD.Print($"craft_master_child script: {GD.Load<GDScript>("res://scripts/crafts/craft_master_child.gd")}");
				(craft as Godot.Node)?.PrintTreePretty();
			}
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