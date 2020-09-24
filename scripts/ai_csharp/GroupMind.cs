using System.Collections.Generic;
using System.Linq;
using Godot;
using GreenBehaviors;
using GreenBehaviors.Composite;
using GreenBehaviors.Decorator;
using GreenBehaviors.LeafLambda;
using MemberId = System.Int32;

using static ISIS.Static;

namespace ISIS.Minds {
	public partial class GroupMind : Godot.Node {
		protected MasterMind _masterMind;
		protected readonly List<ScanPresence> _hostileContacts = new List<ScanPresence>();

		protected readonly Dictionary<MemberId, CraftMind> _members = new Dictionary<MemberId, CraftMind>();
		protected GreenBehaviors.Node _mainBehaviorTree;

		private bool _quiedMainBehaviorCancel;

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
			Think();
		}

		protected virtual void CollectMembers() {
			foreach (var item in GetChildren()) {
				var(isMindfulCraft, craftMind) = CraftMind.IsMindfulCraft(item);
				if (!isMindfulCraft)
					continue;
				AddMember(craftMind);
			}
		}

		protected virtual void SetupBehavior() {
			_mainBehaviorTree = FollowThePath();
		}
		protected virtual void Think() {
			if (_quiedMainBehaviorCancel) {
				_mainBehaviorTree?.Cancel();
				_quiedMainBehaviorCancel = false;
			}
			_mainBehaviorTree.FullTick();
		}

		public virtual Relationship AssessRelationship(ScanPresence contact) {
			if (IsHostileContact(contact))
				return Relationship.Hostile;
			if (IsFreindlyContact(contact))
				return Relationship.Freindly;
			return Relationship.Neutral;
		}

		protected virtual bool IsHostileContact(ScanPresence contact) {
			return contact is Boid && !this.IsAParentOf(contact);
		}

		protected virtual bool IsFreindlyContact(ScanPresence contact) {
			return contact is Boid boid &&
				_members.ContainsKey(GenerateCraftId((RigidBody) boid.GetBody()));
		}

		protected virtual void ContactMade(ScanPresence contact) {
			if (IsHostileContact(contact))
				_hostileContacts.Add(contact);
		}

		protected virtual void ContactLost(ScanPresence contact) {
			_hostileContacts.Remove(contact);
		}

		protected virtual void AddMember(CraftMind craftMind) {
			var id = GenerateCraftId(craftMind.Craft);
			if (_members.ContainsKey(id))
				return;
			_members[id] = craftMind;
			CancelAllBehaviors();
		}

		protected virtual void RemoveMember(MemberId id) {
			if (_members.Remove(id))
				CancelAllBehaviors();
		}

		protected virtual void RemoveAllMembers() {
			_members.Clear();
			CancelAllBehaviors();
		}
		protected static MemberId GenerateCraftId(RigidBody craft) => craft.GetRid().GetId();

		private void CancelAllBehaviors() {
			_quiedMainBehaviorCancel = true;
		}
	}

	#region BEHAVIORS

	public partial class GroupMind {

		protected virtual GreenBehaviors.Node FollowThePath() {
			var path = GetNodeOrNull<Path>("The Path");
			if (path == null)
				return DestroyAllHostiles();

			return new Action(
				"follow the path",
				tick : _ => NodeState.Running,
				start : _ => {
					foreach (var member in _members.Values) {
						member.FollowPath(path);
					}
				}
			);
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
					conditional : _ => _hostileContacts.Count != 0
				)
				.SetChild(
					new Sequence("Eliminate Hostiles")
					// action
					.AddChild(
						"Assign Targets To Members",
						tick : _ => {
							var hostileCount = _hostileContacts.Count;
							var ii = 0;
							foreach (var member in _members.Values) {
								member.Target = _hostileContacts[ii % hostileCount];
								ii++;
							}
							return NodeState.Success;
						}
					)
					.AddChild(
						"Set Members To Attack Pursue",
						tick : _ => memberDrivers.FullTick(),
						start : _ => {
							memberDrivers = EliminateTargets(_members.Keys.ToArray());
							memberDrivers.Start();
						}
					)
				);
		}

		protected virtual GreenBehaviors.Node InterceptTargets(params MemberId[] members) {
			return new Action(
				"Intercept Targets Group",
				tick : _ => NodeState.Running,
				start : _ => {
					foreach (var id in members) {
						_members[id].InterceptSetTarget();
					}
				}
			);
		}

		protected virtual GreenBehaviors.Node EliminateTargets(params MemberId[] members) {
			var childrenRoutines = new PrioritizedSelector("");
			foreach (var memberId in members) {
				DecoratorNode craftInputRoutineWrapper = new SimpleInclude($"{memberId} AttackPursue");
				_members[memberId].EliminateSetTarget(in craftInputRoutineWrapper);
				childrenRoutines.AddChild(craftInputRoutineWrapper);
			}
			return childrenRoutines;
		}
	}
	#endregion
}