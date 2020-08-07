using System;
using Godot;
using static PlayerMindModuleMixin;

namespace ISIS {

    public class SubMind : GroupMind, IPlayerMindModule {

        private RigidBody _activeCraft;
        public RigidBody active_craft {
            get => _activeCraft;
            set {
                // make the old guy a member
                if (_activeCraft != null)
                    AddMember(_activeCraft);

                // remove the new guy from membership
                RemoveMember(GenerateCraftId(value));

                _activeCraft = value;
            }
        }

        protected Node _playerMind;

        public Node player_mind {
            get => _playerMind;
            set {
                _playerMind = value;
                RemoveAllMembers();
                CollectMembers();
            }
        }
        public SubMind() {
            this._InitModule();
        }

        public void _update_engine_input(Godot.Object state) {
            // do notin
        }

        public override void _Ready() {
            base._Ready();
        }

        protected override void CollectMembers() {
            if (_playerMind == null)
                return;
            foreach (var item in _playerMind.GetChildren()) {
                var(isCraftMaster, craftMaster) = IsCraftMaster(item);
                if (!isCraftMaster)
                    continue;
                AddMember(craftMaster);
            }
        }

        protected override bool IsHostileContact(ScanPresence contact) {
            return contact is Boid && (!_playerMind?.IsAParentOf(contact)).GetValueOrDefault();
        }

        protected override void ContactMade(ScanPresence contact) {
            if (IsHostileContact(contact)) {
                _hostileContacts.Add(contact);
            }
        }

        protected override void ContactLost(ScanPresence contact) {
            _hostileContacts.Remove(contact);
        }
    }
}