using System.Collections.Generic;
using Godot;

namespace ISIS {

	public class GroupMind : CraftMind {

		private List<ScanPresence> _hostileContacts = new List<ScanPresence>();
		private Dictionary<string, RigidBody> _crafts = new Dictionary<string, RigidBody>();
		private MasterMind _masterMind;

		public override void _Ready() {
			base._Ready();
			foreach (var item in GetChildren()) {
				var(isCraftMaster, craftMaster) = IsCraftMaster(item);
				if (!isCraftMaster)
					continue;
				_crafts[craftMaster.Name] = craftMaster;
			}

			_masterMind = (MasterMind) GetTree().GetNodesInGroup("MasterMind") [0];
			foreach (var contact in _masterMind.MasterContactList) {
				if (IsHostileContact(contact))
					_hostileContacts.Add(contact);
			}
			_masterMind.Connect(nameof(MasterMind.ContactMade), this, nameof(NewContact));
			_masterMind.Connect(nameof(MasterMind.ContactLost), this, nameof(ContactLost));
		}

		public override void _Process(float delta) {
			base._Process(delta);
			if (_hostileContacts.Count == 0)
				return;
			var enemy = ((Boid) _hostileContacts[0]).GetBody();
			foreach (var craft in _crafts.Values) {
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
	}
}
