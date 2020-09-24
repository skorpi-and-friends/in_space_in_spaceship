using System;
using Godot;
using MemberId = System.Int32;
using static ISIS.PlayerMindModuleMixin;

namespace ISIS.Minds {
    public class SubMind : GroupMind, IPlayerMindModule {
        private RigidBody _activeCraft;
        public RigidBody active_craft {
            get => _activeCraft;
            set {
                // make the old guy a member
                if (_activeCraft != null) {
                    var(isOldGuyMindful, oldGuyMind) = CraftMind.IsMindful(_activeCraft);
                    if (isOldGuyMindful)
                        AddMember(oldGuyMind);
                }

                // deactivate the autopilot of the new guy
                var(isNewGuyMindful, newGuyMind) = CraftMind.IsMindful(value);
                if (isNewGuyMindful) {
                    newGuyMind.DisableAutoPilot();
                    // remove the new guy from membership
                    RemoveMember(GenerateCraftId(value));
                }

                _activeCraft = value;
            }
        }

        protected Node _playerMind;
        public Node player_mind {
            get => _playerMind;
            set {
                _playerMind = value;
                if (!_isReady) return;
                RemoveAllMembers();
                CollectMembers();
            }
        }

        private bool _isReady;

        public SubMind() {
            this._InitModule();
        }

        public void _update_engine_input(Godot.Object state) {
            // do notin
        }

        public override void _Ready() {
            base._Ready();
            _isReady = true;
        }

        protected override void CollectMembers() {
            if (_playerMind == null)
                return;
            foreach (var item in _playerMind.GetChildren()) {
                if (item == active_craft)
                    continue;

                var(isMindfulCraft, craftMind) = CraftMind.IsMindfulCraft(item);
                if (!isMindfulCraft)
                    continue;
                AddMember(craftMind);
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