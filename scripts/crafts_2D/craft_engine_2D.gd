extends Node

class_name CraftEngine2D

#export var _root_rigidbody: NodePath;
export var _craft_extents: Rect2;

var _rigidbody: RigidBody2D

onready var state:= $State as CraftState2D;
onready var driver := $Driver as CraftDriver2D;
onready var motor := $Motor as CraftMotor2D;

# duck type craft_master to avoid cyclic dependecy issues
func _start_engine(craft_master):
	craft_master.connect("moment_of_inertia_changed", self, "moi_changed");

	_rigidbody = craft_master.get_craft_rigidbody();
	_rigidbody.linear_damp = 0.0
	_rigidbody.angular_damp = 0.0
	_rigidbody.mass = state.mass * Globals.MASS_MODIFIER;

	caclulate_torque();


func _physics_process(delta):
	update_readings();
	driver._update_flames(state);
	motor._apply_flames(state, _rigidbody);

	# orthonormalize to clean up inaccuracies
	#transform = transform.orthonormalized();


func update_readings():
	# rotate them to local space
	state.linear_velocity = _rigidbody.global_transform.basis_xform_inv(
			_rigidbody.linear_velocity * Globals.PIXLE2METER);
	state.angular_velocity = _rigidbody.angular_velocity;


# FIXME: this horrible coupling
func moi_changed(inertia_inv: float):
	state.moment_of_inertia = 1 / inertia_inv;
	driver._moi_changed(state);


func caclulate_torque():
	var thruster_force = state.angular_thruster_force;
	var accel_limit = state.acceleration_limit * state.acceleration_multiplier;
	if !_craft_extents:
		printerr("_craft_extents not set on craft engine");
		state.angular_thruster_torque = thruster_force;
		state.angular_acceleration_limit = accel_limit.length();
		return;

	state.angular_thruster_torque = thruster_force * _craft_extents.size.length();

	state.angular_acceleration_limit = INF;


func set_mass(mass: float):
	state.mass = mass
	_rigidbody.mass = mass * Globals.MASS_MODIFIER;


