using System;
using Godot;
using static PlayerMindModuleMixin;

namespace ISIS {

    public class SubMind : GroupMind, IPlayerMindModule {
        public RigidBody _activeCraft { get; set; }

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

        public void _craft_changed(Godot.Object craft) {
            // make the old guy a member
            if (_activeCraft != null)
                AddMember(_activeCraft);

            // remove the new guy from membership
            var craftMaster = (RigidBody) craft;
            RemoveMember(GenerateCraftId(craftMaster));
            _activeCraft = craftMaster;
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

    }
}