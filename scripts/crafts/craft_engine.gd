extends RigidBody

class_name CraftEngine

export var _config: Resource;
var _state: CraftState;
var _driver: CraftDriver;
var _motor: CraftMotor;


func _ready():
	_state = ($State as CraftState);
	_driver = ($Driver as CraftDriver);
	_motor = ($Motor as CraftMotor);
	_init_from_config(_config as CraftConfig);
	linear_damp = 0.0
	angular_damp = 0.0
	mass = _state.mass


func _init_from_config(config: CraftConfig):
	_state._init_from_config(config);
	_driver._init_from_config(config);
	_motor._init_from_config(config);


func _physics_process(delta):
	update_readings();
	_driver._update_flames(_state);
	_motor._apply_flames(_state, self);
	
	# orthonormalize to clean up inaccuracies
	#transform = transform.orthonormalized();

func update_readings():
	# rotate them to local space
	_state.linear_velocity = Utility.transform_vector_inv(global_transform,linear_velocity);
	_state.angular_velocity = Utility.transform_vector_inv(global_transform, angular_velocity);


func set_mass(mass: float):
	_state.mass = mass
	mass = mass





