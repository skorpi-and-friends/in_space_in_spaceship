extends RigidBody2D

class_name CraftMaster2D

signal moment_of_inertia_changed(inc_inertia);

onready var engine := $Engine as CraftEngine2D;

var _moment_of_inertia_inv := 0.0;

func _ready() -> void:
	engine._start_engine(self);

func get_craft_rigidbody() -> RigidBody2D:
	return self;


func _integrate_forces(state: Physics2DDirectBodyState):
	var inv_inertia = state.inverse_inertia;
	# in float lingo: if inv_inertia != _moment_of_inertia_inv
	if inv_inertia != _moment_of_inertia_inv:
		_moment_of_inertia_inv = inv_inertia;
		emit_signal("moment_of_inertia_changed", inv_inertia);