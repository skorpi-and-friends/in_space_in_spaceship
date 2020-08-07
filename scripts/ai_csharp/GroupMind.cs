using System.Collections.Generic;
using System.Linq;
using Godot;
using GreenBehaviors;
using GreenBehaviors.Composite;
using GreenBehaviors.Decorator;
using GreenBehaviors.LeafLambda;

namespace ISIS {

	public partial class GroupMind : CraftMind {

		protected MasterMind _masterMind;
		protected List<ScanPresence> _hostileContacts = new List<ScanPresence>();

		protected Dictionary<string, RigidBody> _members = new Dictionary<string, RigidBody>();
		protected Dictionary<string, SteeringRoutine> _memberSteeringRoutines = new Dictionary<string, SteeringRoutine>();
		protected Dictionary<string, ScanPresence> _memberTargets = new Dictionary<string, ScanPresence>();

		protected GreenBehaviors.Node _mainBehaviorTree;

		public override void _Ready() {
			base._Ready();
			CollectMembers();
			_masterMind = (MasterMind) GetTree().GetNodesInGroup("MasterMind") [0];
			_masterMind.Connect(nameof(MasterMind.ContactMade), this, nameof(ContactMade));
			_masterMind.Connect(nameof(MasterMind.ContactLost), this, nameof(ContactLost));
			// AnalyzeAllContacts();
			foreach (var contact in _masterMind.MasterContactList) {
				ContactMade(contact);
			}
			SetupBehavior();
		}

		public override void _Process(float delta) {
			base._Process(delta);
			RunSteeringRoutines();
			Think();
		}

		protected virtual void CollectMembers() {
			foreach (var item in GetChildren()) {
				var(isCraftMaster, craftMaster) = IsCraftMaster(item);
				if (!isCraftMaster)
					continue;
				AddMember(craftMaster);
			}
		}

		protected virtual void SetupBehavior() {
			_mainBehaviorTree = DestroyAllHostiles();
		}

		protected virtual bool IsHostileContact(ScanPresence contact) {
			return contact is Boid && !this.IsAParentOf(contact);
		}

		protected virtual bool IsFreindlyContact(ScanPresence contact) {
			return contact is Boid boid &&
				_members.ContainsKey(GenerateCraftId((RigidBody) boid.GetBody()));
		}

		public virtual Relationship AssessRelationship(ScanPresence contact) {
			if (IsHostileContact(contact))
				return Relationship.Hostile;
			if (IsFreindlyContact(contact))
				return Relationship.Freindly;
			return Relationship.Neutral;
		}

		protected virtual void ContactMade(ScanPresence contact) {
			if (IsHostileContact(contact))
				_hostileContacts.Add(contact);
		}

		protected virtual void ContactLost(ScanPresence contact) {
			_hostileContacts.Remove(contact);
		}

		protected virtual void RunSteeringRoutines() {
			foreach (var memberId in _members.Keys) {
				var member = _members[memberId];
				if (_memberSteeringRoutines.TryGetValue(memberId, out var routine)) {
					var state = GetCraftState(member);
					var input = routine.Invoke(member.GlobalTransform, state);
					SetCraftInput(state, input);
				}
			}
		}
		protected virtual void Think() {
			_mainBehaviorTree.FullTick();
		}

		protected virtual void AddMember(RigidBody craft) {
			var id = GenerateCraftId(craft);
			if (_members.ContainsKey(id))
				return;
			_members[id] = craft;
			CancelAllBehaviors();
		}

		protected virtual void RemoveMember(string id) {
			_members.Remove(id);
			_memberSteeringRoutines.Remove(id);
			_memberTargets.Remove(id);
			CancelAllBehaviors();
		}

		protected virtual void RemoveAllMembers() {
			_members.Clear();
			_memberSteeringRoutines.Clear();
			_memberTargets.Clear();
			CancelAllBehaviors();
		}
		protected static string GenerateCraftId(RigidBody craft) => craft.Name;

		private void CancelAllBehaviors() {
			_mainBehaviorTree?.Cancel();
		}
	}

	public partial class GroupMind {

		protected virtual GreenBehaviors.Node DestroyAllHostiles() {
			// setup basicFlightBehaviorTree
			/* 

				 --MainTree       blackboard = [members, hostiles]
					 |
			  M(HostilesAround)
					 |
				  Sequence
				  |        \
				Assign      Parallel Selector Resume
				Targets       \    |    /
				ToMembers     AttackPersue
							 (members, Target)        
				 */
			GreenBehaviors.Node memberDrivers = LambdaLeafNode.EmptyFailureNode;
			return new Monitored("GroupMind Main Tree")
				// conditional
				.SetGuard(
					"Are Hostiles Nearby",
					(_) => _hostileContacts.Count != 0
				)
				.SetChild(
					new Sequence(
						// name
						"Eliminate Hostiles")
					// action
					.AddChild(
						// name
						"Assign Targets To Members",
						// at tick callback
						(_) => {
							var hostileCount = _hostileContacts.Count;
							var ii = 0;
							foreach (var member in _members.Keys) {
								_memberTargets[member] = _hostileContacts[ii % hostileCount];
								ii++;
							}
							return NodeState.Success;
						}
					)
					.AddChild(
						new Action(
							// name
							"Set Members To Attack Pursue",
							// at tick callback
							(_) => memberDrivers.FullTick(),
							// at start callback
							(_) => {
								memberDrivers = EliminateTargets(_members.Keys.ToArray());
								memberDrivers.Start();
							}
						)
					)
				);
		}

		protected virtual GreenBehaviors.Node InterceptTargets(params string[] members) {
			foreach (var memberId in members) {
				var member = _members[memberId];
				_memberSteeringRoutines[memberId] = InterceptRoutine(_memberTargets[memberId]);
			}
			return LambdaLeafNode.EmptyRunningNode;
		}

		protected virtual GreenBehaviors.Node EliminateTargets(params string[] members) {

			var childrenRoutines = new PrioritizedSelector(
				"");

			foreach (var memberId in members) {
				var member = _members[memberId];
				DecoratorNode craftInputRoutineWrapper = new SimpleInclude($"{memberId} AttackPursue");
				_memberSteeringRoutines[memberId] = AttackPersueRoutineClosure(ref craftInputRoutineWrapper, member, _memberTargets[memberId]);
				childrenRoutines.AddChild(craftInputRoutineWrapper);
			}
			return childrenRoutines;
		}

		protected static SteeringRoutine AttackPersueRoutineClosure(
			ref DecoratorNode wrappingNode,
			RigidBody craft,
			ScanPresence quarry
		) {

			/*
						 |
					AttackPersue(quarry)       blackboard = [quarry, attackingRange]
						 |
				Prioritized Selector
				   /           |
				  /            |
		   IsTargetDead      Prioritized Selector
									 |            \
								m(IsFar)           \
									 |              \  
								Intecept          Attack
								(quarry)         (quarry)
				*/
			var quarryRigidbody = (RigidBody) ((quarry as Boid)?.GetBody());

			var attackingRange = 1000;

			// AbstractDecoratorNode interceptSubtree = new Checker("Intercept Subtree Wrap");
			DecoratorNode attackSubtree = new SimpleInclude("Attack Subtree Wrap");

			var attackSubroutine = AttackRoutine(ref attackSubtree, craft, quarry);
			var interceptSubroutine = InterceptRoutine(quarry);

			// initially assume we're too far to attack 
			var activeSubroutine = interceptSubroutine;

			{
				// setupTree
				var attackPersueTree = new PrioritizedSelector(
					$"AttackPersue : Attack Persue Subtree"
				).AddChild(
					new Conditional(
						$"AttackPersue : Is Quarry Dead",
						_ => false
					)
				).AddChild(
					new PrioritizedSelector(
						"AttackPersue : Core")
					.AddChild(
						new Monitored(
							// name
							"AttackPersue : Intercept Target",
							// guard node 
							"AttackPersue : Is Target Beyond Fire Range",
							(_) => {
								var distanceToTargetSquared = (quarry.GlobalTransform.origin - craft.GlobalTransform.origin).LengthSquared();
								return distanceToTargetSquared > (attackingRange * attackingRange);
							},
							// chargeNode
							new Action(
								// name
								"AttackPersue : Intercept Wrapper",
								// action
								_ => NodeState.Running,
								// start
								_ => {
									activeSubroutine = interceptSubroutine;
								}
							)
						)
					).AddChild(
						new Action(
							// name
							"AttackPersue : Attack Wrapper",
							// action
							_ => attackSubtree.FullTick(),
							// start
							_ => {
								// attackSubtree.Start();
								activeSubroutine = attackSubroutine;
							}
						)
					)
				);

				wrappingNode.SetChild(attackPersueTree);
			}

			return (Transform currentTransform, Godot.Object currentState) => {
				return activeSubroutine(currentTransform, currentState);
			};
		}

		protected static SteeringRoutine GetAttackPersueRoutineBlackboard(
			ref DecoratorNode wrappingNode,
			RigidBody craft,
			ScanPresence quarry
		) {
			throw new System.NotImplementedException();
		}

		protected static SteeringRoutine AttackRoutine(
			ref DecoratorNode wrappingNode,
			RigidBody craft,
			ScanPresence quarry
		) {
			/*
			  --Attack(quarry)       blackboard = Static[quarry]{targetDirecton}
					 |
				   ProritizedSelect
				   /        |       \
				  /         |        \
			  M(isAhead)  M(isAside)  +
				 |          |         |
			Line Up       Loop    Brake With Flair 
			 Shot
		*/

			var lineUpShotRoutine = LineUpShotRoutine(quarry);
			var interceptSubroutine = InterceptRoutine(quarry);
			var loopSubroutine = interceptSubroutine;
			var brakeWithFlairSubroutine = interceptSubroutine;

			var activeSubroutine = interceptSubroutine;

			// var lastTargetDirection = GeneralRelativeDirection.Ahead;
			var targetDirection = Spatial.GeneralRelativeDirection.Ahead;

			var attackRoutineTree = new PrioritizedSelector(
				"Attack : Core"
			).AddChild(
				new PrioritizedSelector(
					$"Attack : Choose Craft Input Routine FSM",
					new Monitored(
						// name
						$"Attack : Target Ahead Gurad",
						// guard name
						$"Attack : Is Target Ahead",
						// guard callback
						(_) => {
							targetDirection = Spatial.GetGeneralRelativeDirectionOfTransforms(craft.GlobalTransform, quarry.GlobalTransform);
							return targetDirection == Spatial.GeneralRelativeDirection.Ahead;
						},
						// charge
						new Action(
							"Attack : Taget Ahead - Line Up Shot",
							(_) => {
								if (IsInCroshair(craft.GlobalTransform, quarry))
									FirePrimaryWeapons(craft);
								return NodeState.Running;
							},
							// start callback
							(_) => {
								GD.Print("Target Is Ahead: Lining Up Shot");
								activeSubroutine = lineUpShotRoutine;
							}
						)
					),
					new Monitored(
						$"Attack : Target Aside Gurad",
						$"Attack : Is Target Aside",
						(_) => targetDirection == Spatial.GeneralRelativeDirection.Aside, // targetDirection was caclulated earlier
						new Action(
							"Attack : Target Aside",
							// tick callback
							(_) => NodeState.Running,
							// start callback
							(_) => {
								GD.Print("Target Is Aside");
								activeSubroutine = loopSubroutine;
							}
						)
					),
					new Action(
						"Attack : Target Behind",
						(_) => {
							return NodeState.Running;
						},
						(_) => {
							GD.Print("Target Is Behind");
							activeSubroutine = loopSubroutine;
						}
					)
				)
			);
			wrappingNode.SetChild(attackRoutineTree);

			return (Transform currentTransform, Godot.Object currentState) => {
				return activeSubroutine(currentTransform, currentState);
			};
		}

		protected static SteeringRoutine LineUpShotRoutine(
			ScanPresence quarry
		) {

			var quarryRigidbody = (RigidBody) ((quarry as Boid)?.GetBody());
			var averageWeaponVelocity = 500; //_craft.ArmsManager.AverageFixMountedProjectileVelocity;

			return (Transform currentTransform, Godot.Object currentState) => {
				var quarryVelocity = quarryRigidbody != null ? quarryRigidbody.LinearVelocity : Vector3.Zero;
				var targetPosition = SteeringBehaviors.FindInterceptionPosition(
					currentTransform.origin,
					averageWeaponVelocity,
					quarry.GlobalTransform.origin,
					quarryVelocity
				);

				var linearVLimit = (Vector3) currentState.Get("linear_v_limit");
				return (
					SteeringBehaviors.SeekPosition(currentTransform.origin, targetPosition) * linearVLimit,
					FacePositionAngularInput(targetPosition, currentTransform)
				);
			};
		}

		protected static bool IsInCroshair(
			Transform currentTransform,
			ScanPresence quarry
		) {

			var quarryRigidbody = (RigidBody) ((quarry as Boid)?.GetBody());

			float targetAngularProximity;

			// if target is moving
			if (quarryRigidbody != null) {

				// find position where weapons can hit mark
				var intereptPosition = SteeringBehaviors.FindInterceptionPosition(
					currentTransform.origin,
					500, // use weapon average velocity 
					quarry.GlobalTransform.origin,
					quarryRigidbody.LinearVelocity
				);

				// cacluate angular proximity
				targetAngularProximity = currentTransform.basis.z.AngleTo(
					intereptPosition - currentTransform.origin
				);
			} else {
				// for immotile targets
				targetAngularProximity = currentTransform.basis.z.AngleTo(
					quarry.GlobalTransform.origin - currentTransform.origin
				);
			}
			return targetAngularProximity < 0.001 * Static.DegreeToRadian;
		}

	}
}