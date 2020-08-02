using System.Collections.Generic;
using Godot;
using GreenBehaviors;
using GreenBehaviors.Composite;
using GreenBehaviors.Decorator;
using GreenBehaviors.LeafLambda;

namespace ISIS {

	public class GroupMind : CraftMind {

		private List<ScanPresence> _hostileContacts = new List<ScanPresence>();
		private Dictionary<string, RigidBody> _members = new Dictionary<string, RigidBody>();

		private Dictionary<string, SteeringRoutine> _memberSteeringRoutines = new Dictionary<string, SteeringRoutine>();

		private Dictionary<string, ScanPresence> _memberTargets = new Dictionary<string, ScanPresence>();
		private MasterMind _masterMind;

		private GreenBehaviors.Node _mainBehaviorTree;

		public override void _Ready() {
			base._Ready();
			foreach (var item in GetChildren()) {
				var(isCraftMaster, craftMaster) = IsCraftMaster(item);
				if (!isCraftMaster)
					continue;
				_members[craftMaster.Name] = craftMaster;
			}

			_masterMind = (MasterMind) GetTree().GetNodesInGroup("MasterMind") [0];
			foreach (var contact in _masterMind.MasterContactList) {
				if (IsHostileContact(contact))
					_hostileContacts.Add(contact);
			}
			_masterMind.Connect(nameof(MasterMind.ContactMade), this, nameof(NewContact));
			_masterMind.Connect(nameof(MasterMind.ContactLost), this, nameof(ContactLost));

			SetupBehavior();
		}

		public override void _Process(float delta) {
			base._Process(delta);

			foreach (var memberId in _members.Keys) {
				var member = _members[memberId];
				if (_memberSteeringRoutines.TryGetValue(memberId, out var routine)) {
					var state = GetCraftState(member);
					var input = routine.Invoke(member.GlobalTransform, state);
					SetCraftInput(state, input);
				}
			}
			Think();
			return;
			if (_hostileContacts.Count == 0)
				return;
			var enemy = ((Boid) _hostileContacts[0]).GetBody();
			foreach (var craft in _members.Values) {
				Vector3 hostileDirection = enemy.Translation - craft.Translation;
				var craftState = GetCraftState(craft);
				craftState.Set(
					"angular_input",
					FaceDirectionAngularInput(
						hostileDirection, craft.GlobalTransform));
			}
		}

		protected virtual bool IsHostileContact(ScanPresence contact) {
			return contact is Boid && !this.IsAParentOf(contact);
		}

		protected virtual void NewContact(ScanPresence contact) {
			if (IsHostileContact(contact))
				_hostileContacts.Add(contact);
		}

		protected virtual void ContactLost(ScanPresence contact) {
			_hostileContacts.Remove(contact);
		}

		protected virtual void Think() {
			_mainBehaviorTree.FullTick();
		}

		protected virtual void SetupBehavior() {
			_mainBehaviorTree = DestroyAllHostiles();

		}

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
								memberDrivers = GetEliminateHostilesSubTree();
								memberDrivers.Start();
							}
						)
					)
				);
		}

		private GreenBehaviors.Node GetEliminateHostilesSubTree() {

			foreach (var memberId in _members.Keys) {
				var member = _members[memberId];
				_memberSteeringRoutines[memberId] = InterceptRoutine(_memberTargets[memberId]);
			}
			return LambdaLeafNode.EmptyRunningNode;
		}

		//         protected virtual GreenBehaviors.Node CraftAttackPersue(RigidBody craft, ScanPresence quarry) {

		//             /*
		//                  |
		//             AttackPersue(quarry)       blackboard = [quarry, attackingRange]
		//                  |
		//         Prioritized Selector
		//            /           |
		//           /            |
		//    IsTargetDead      Prioritized Selector
		//                              |            \
		//                         m(IsFar)           \
		//                              |              \  
		//                         Intecept          Attack
		//                         (quarry)         (quarry)
		//         */
		//         }

		//         protected virtual GreenBehaviors.Node CraftIntercept(RigidBody craft, ScanPresence quarry) {
		//             /*
		//                              |
		//                         Intercept(quarry)       blackboard = [quarry]
		//                              |
		//                     Prioritized Selector
		//                        /           |
		//                       /            |
		//                IsTargetDead      Prioritized Selector
		//                                          |            \
		//                                     m(IsFar)           \
		//                                          |              \  
		//                                     Intecept          Attack
		//                                     (quarry)         (quarry)
		//                     */

		//         }

		//         public virtual SteeringRoutine GetAttackPersueRoutineClosure(
		//             ref DecoratorNode wrappingNode,
		//             MonoBehaviour quarry = null
		//         ) {
		//             if (!quarry)
		//                 quarry = _target.GetComponent<MonoBehaviour>();

		//             var attackingRange = 1000;

		//             // AbstractDecoratorNode interceptSubtree = new Checker("Intercept Subtree Wrap");
		//             DecoratorNode attackSubtree = new Checker("Attack Subtree Wrap");

		//             var attackSubroutine = GetAttackRoutine(ref attackSubtree, quarry);
		//             var interceptSubroutine = GetInterceptRoutine(quarry);

		//             // initially assume we're too far to attack 
		//             var activeSubroutine = interceptSubroutine;

		//             {
		//                 // setupTree
		//                 var attackPersueTree = new PrioritizedSelector(
		//                     $"AttackPersue : Attack Persue Subtree"
		//                 ).AddChild(
		//                     new Conditional(
		//                         $"AttackPersue : Is Quarry Dead",
		//                         _ => false
		//                     )
		//                 ).AddChild(
		//                     new PrioritizedSelector(
		//                         "AttackPersue : Core")
		//                     .AddChild(
		//                         new Monitored(
		//                             // name
		//                             "AttackPersue : Intercept Target",
		//                             // guard node 
		//                             "AttackPersue : Is Target Beyond Fire Range",
		//                             (_) => {
		//                                 var distanceToTargetSquared = (quarry.transform.position - _craft.transform.position).sqrMagnitude;
		//                                 return distanceToTargetSquared > (attackingRange * attackingRange);
		//                             },
		//                             // chargeNode
		//                             new Action(
		//                                 // name
		//                                 "AttackPersue : Intercept Wrapper",
		//                                 // action
		//                                 _ => NodeState.Running,
		//                                 // start
		//                                 _ => {
		//                                     activeSubroutine = interceptSubroutine;
		//                                 }
		//                             )
		//                         )
		//                     ).AddChild(
		//                         new Action(
		//                             // name
		//                             "AttackPersue : Attack Wrapper",
		//                             // action
		//                             _ => attackSubtree.FullTick(),
		//                             // start
		//                             _ => {
		//                                 // attackSubtree.Start();
		//                                 activeSubroutine = attackSubroutine;
		//                             }
		//                         )
		//                     )
		//                 );

		//                 wrappingNode.SetChild(attackPersueTree);
		//             }

		//             return (Transform currentTransform, ref ISIS.NewtonianCraft.State currentState) => {
		//                 return activeSubroutine(currentTransform, ref currentState);
		//             };
		//         }

		//         public virtual SteeringRoutine GetAttackPersueRoutineBlackboard(
		//             ref Checker wrappingNode,
		//             MonoBehaviour quarry = null
		//         ) {
		//             /*
		//                               |
		//                           g(WhileQuarryAlive)
		//                               |
		//                            AttackPersue(Quarry) blackboard = [isAsideDirection]
		//                               |
		//                            Parallel Sequence Resume
		//                               |            |
		//                            Caclulate     Parallel Select Join
		//                            Distance        |      \
		//                                            |       \
		//                                       Guard(IsFar)  Guard(IsClose)
		//                                            |         |
		//                                          Intecept   Attack

		//                    */
		//             throw new System.NotImplementedException();
		//         }

		//         public virtual SteeringRoutine GetAttackRoutine(
		//             ref DecoratorNode wrappingNode,
		//             MonoBehaviour quarry = null
		//         ) {
		//             /*
		//       --Attack(quarry)       blackboard = Static[quarry]{targetDirecton}
		//              |
		//            ProritizedSelect
		//            /        |       \
		//           /         |        \
		//       M(isAhead)  M(isAside)  +
		//          |          |         |
		//     Line Up       Loop    Brake With Flair 
		//      Shot
		// */
		//             if (!quarry)
		//                 quarry = _target.GetComponent<MonoBehaviour>();

		//             var lineUpShotRoutine = GetLineUpShotRoutine(quarry);
		//             var interceptSubroutine = GetInterceptRoutine(quarry);
		//             var loopSubroutine = interceptSubroutine;
		//             var brakeWithFlairSubroutine = interceptSubroutine;

		//             // initially assume we're too far to attack 
		//             var activeSubroutine = lineUpShotRoutine;

		//             // var lastTargetDirection = GeneralRelativeDirection.Ahead;
		//             var targetDirection = NewtonianStatic.GeneralRelativeDirection.Ahead;

		//             var attackRoutineTree = new PrioritizedSelector(
		//                 "Attack : Core"
		//             ).AddChild(
		//                 new PrioritizedSelector(
		//                     $"Attack : Choose Craft Input Routine FSM",
		//                     new Monitored(
		//                         // name
		//                         $"Attack : Target Ahead Gurad",
		//                         // guard name
		//                         $"Attack : Is Target Ahead",
		//                         // guard callback
		//                         (_) => {
		//                             targetDirection = NewtonianStatic.GetGeneralRelativeDirectionOfTransforms(transform, quarry.transform);
		//                             return targetDirection == NewtonianStatic.GeneralRelativeDirection.Ahead;
		//                         },
		//                         // charge
		//                         new Action(
		//                             "Attack : Taget Ahead - Line Up Shot",
		//                             (_) => {
		//                                 //_basicArmsBehaviorTree.Tick();
		//                                 return NodeState.Running;
		//                             },
		//                             (_) => {
		//                                 Debug.Log("Target Is Ahead: Lining Up Shot");
		//                                 activeSubroutine = lineUpShotRoutine;
		//                             }
		//                         )
		//                     ),
		//                     new Monitored(
		//                         $"Attack : Target Aside Gurad",
		//                         $"Attack : Is Target Aside",
		//                         (_) => targetDirection == NewtonianStatic.GeneralRelativeDirection.Aside, // targetDirection was caclulated earlier
		//                         new Action(
		//                             "Attack : Target Aside",
		//                             (_) => NodeState.Running,
		//                             (_) => {
		//                                 Debug.Log("Target Is Aside");
		//                                 activeSubroutine = loopSubroutine;
		//                             }
		//                         )
		//                     ),
		//                     new Action(
		//                         "Attack : Target Behind",
		//                         (_) => {
		//                             return NodeState.Running;
		//                         },
		//                         (_) => {
		//                             Debug.Log("Target Is Behind");
		//                             activeSubroutine = loopSubroutine;
		//                         }
		//                     )
		//                 )
		//             );
		//             wrappingNode.SetChild(attackRoutineTree);

		//             return (Transform currentTransform, ref ISIS.NewtonianCraft.State currentState) => {
		//                 return activeSubroutine(currentTransform, ref currentState);
		//             };
		//         }
		//         public virtual SteeringRoutine GetLineUpShotRoutine(
		//             MonoBehaviour quarry = null
		//         ) {
		//             var averageWeaponVelocity = _craft.ArmsManager.AverageFixMountedProjectileVelocity;

		//             return (Transform currentTransform, ref ISIS.NewtonianCraft.State currentState) => {
		//                 var targetPosition = FindInterceptionPosition(
		//                     currentTransform.position,
		//                     averageWeaponVelocity,
		//                     _target.transform.position,
		//                     _targetRigidBody.velocity
		//                 );

		//                 /*  if (Vector3.Angle(currentTransform.forward, targetPosition - currentTransform.position) < _firingAngularProximity) {
		//                 foreach (var weapon in _fireableWeapons)
		//                     weapon.Fire();
		//             }
		//  */
		//                 _basicArmsBehaviorTree.Tick();
		//                 return new ISIS.NewtonianCraft.State.InputPack(
		//                     Vector3.Scale(SeekPosition(currentTransform.position, targetPosition), currentState.LinearVLimit),
		//                     AngularInputToFacePosition(targetPosition, currentTransform)
		//                 );
		//             };
		//         }
	}

}
