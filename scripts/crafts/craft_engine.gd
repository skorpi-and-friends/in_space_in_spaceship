extends Node

class_name CraftEngine

#export var _root_rigidbody: NodePath;
export var _ship_extents: Vector3;

var _rigidbody: RigidBody

onready var state:= $State as CraftState;
onready var driver := $Driver as CraftDriver;
onready var motor := $Motor as CraftMotor;
	
# duck type craft_master to avoid cyclic dependecy issues
func _init_from_config(config: CraftConfig, craft_master):
	craft_master.connect("moment_of_inertia_changed", self, "moi_changed");
	
	_rigidbody = craft_master.get_craft_rigidbody();
	_rigidbody.linear_damp = 0.0
	_rigidbody.angular_damp = 0.0
	_rigidbody.mass = state.mass * Globals.MASS_MODIFIER;

	state._init_from_config(config);
	driver._init_from_config(config);
	motor._init_from_config(config);
	caclulate_torque();


func _physics_process(delta):
	update_readings();
	driver._update_flames(state);
	motor._apply_flames(state, _rigidbody);
	
	# orthonormalize to clean up inaccuracies
	#transform = transform.orthonormalized();


func update_readings():
	# rotate them to local space
	state.linear_velocity = Utility.transform_vector_inv(
			_rigidbody.global_transform, _rigidbody.linear_velocity);
	state.angular_velocity = Utility.transform_vector_inv(
			_rigidbody.global_transform, _rigidbody.angular_velocity);


# FIXME: this horrible coupling
func moi_changed(inertia_inv: Vector3):
	# We need to un-apply the inertia value is directly from
	# Godot's physics which is working with the modifed value.
	# also, use the un-inversed inertia for clarity's sake
	state.moment_of_inertia = Vector3.ONE / inertia_inv;
	driver._moi_changed(state);
	

func caclulate_torque():
	var thruster_force = state.angular_thruster_force;
	var accel_limit = state.acceleration_limit * state.acceleration_multiplier;
	if !_ship_extents:
		printerr("_ship_extents not set on craft engine");
		state.angular_thrusters_torque = thruster_force;
		state.angular_acceleration_limit = accel_limit;
		return;
	
	var axes_diameter := Vector3(
		Vector2(_ship_extents.y, _ship_extents.z).length(),
		Vector2(_ship_extents.x, _ship_extents.z).length(),
		Vector2(_ship_extents.x, _ship_extents.y).length()
	);
	
#	state.angular_thruster_torque = axes_diameter.cross(thruster_force);
	# FIXME: is this correct?
	state.angular_thruster_torque = thruster_force * axes_diameter;
	
#	accel_of_a_point  = derivative(angular_vel * r)
#	
#	var axes_circimferences := axes_diameter * PI;
#	var radian_to_meter := axes_circimferences / TAU;
#	state.angular_acceleration_limit = accel_limit / radian_to_meter;
	state.angular_acceleration_limit = accel_limit * axes_diameter * .5;

	state.angular_thruster_torque = state.angular_acceleration_limit * state.moment_of_inertia;


func set_mass(mass: float):
	state.mass = mass
	_rigidbody.mass = mass * Globals.MASS_MODIFIER;


