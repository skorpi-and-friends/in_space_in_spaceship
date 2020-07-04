extends Node

class_name CraftEngine

#export var _root_rigidbody: NodePath;
export var _config: Resource;

var _rigidbody: RigidBody

var state: CraftState;
var driver: CraftDriver;
var motor: CraftMotor;


func _ready():
	state = ($State as CraftState);
	driver = ($Driver as CraftDriver);
	motor = ($Motor as CraftMotor);
	_init_from_config(_config as CraftConfig);

#	_rigidbody = $_root_rigidbody as RigidBody;
	_rigidbody = get_parent() as RigidBody;
	assert(_rigidbody);
	_rigidbody.linear_damp = 0.0
	_rigidbody.angular_damp = 0.0
	_rigidbody.mass = state.mass
	

func _init_from_config(config: CraftConfig):
	state._init_from_config(config);
	driver._init_from_config(config);
	motor._init_from_config(config);


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


func set_mass(mass: float):
	state.mass = mass
	_rigidbody.mass = mass





