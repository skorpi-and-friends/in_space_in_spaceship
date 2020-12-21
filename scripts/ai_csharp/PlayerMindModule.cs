using System;
using Godot;

namespace ISIS.Minds {
    public interface IPlayerMindModule {
        RigidBody active_craft { get; set; }
        Godot.Node player_mind { get; set; }

        void _update_engine_input(Godot.Object state);
    }

    public static class PlayerMindModuleMixin {
        public const string IdentifierMeta = "player_mind_module";

        public static void _InitModule(this IPlayerMindModule module) {
            if (!(module is Node node)) {
                throw new ApplicationException(
                    $"{nameof(IPlayerMindModule)} implementers doesn't inherit from {nameof(Godot.Node)}");
            }

            node.SetMeta(IdentifierMeta, true);
        }
    }
}