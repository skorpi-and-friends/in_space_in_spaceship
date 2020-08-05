using System;
using Godot;

public interface IPlayerMindModule {

    RigidBody _activeCraft { get; set; }
    Godot.Node player_mind { get; set; }

    // Called when the node enters the scene tree for the first time.
    /* public override void _Ready() {
        SetMeta(IdentifierMeta, true);
    }
 */

    void _craft_changed(Godot.Object craft);

    void _update_engine_input(Godot.Object state);

}

public static class PlayerMindModuleMixin {

    public const string IdentifierMeta = "player_mind_module";

    public static void _InitModule(this IPlayerMindModule module) {
        var node = module as Node;
        if (node == null)
            throw new ApplicationException(
                $"{nameof(IPlayerMindModule)} implementers doesn't inherit from {nameof(Godot.Node)}");
        node.SetMeta(IdentifierMeta, true);
    }
}