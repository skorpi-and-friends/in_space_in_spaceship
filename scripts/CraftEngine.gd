extends RigidBody

export var _state: Resource# CraftState
var _driver: CraftDriver
var _motor: CraftMotor


func _ready():
	_driver = ($Driver as CraftDriver);
	_motor = ($Motor as CraftMotor);
	linear_damp = 0.0
	angular_damp = 0.0
	mass = _state.mass


func _process(delta):
	var input_v := Vector3();
	if Input.is_action_pressed("Thrust"):
		input_v.z += 1
	if Input.is_action_pressed("Thrust Reverse"):
		input_v.z -= 1
	if Input.is_action_pressed("Starfe Right"):
		input_v.x += 1
	if Input.is_action_pressed("Starfe Left"):
		input_v.x -= 1
	if Input.is_action_pressed("Altitude Raise"):
		input_v.y += 1
	if Input.is_action_pressed("Altitude Lower"):
		input_v.y -= 1
	
	input_v *= _state.linear_thruster_force;
	
	_state.linear_input = input_v;


func _physics_process(delta):
	update_readings();
	_driver._update_flames(_state);
	_motor._apply_flames(_state, self);


func update_readings():
	_state.linear_velocity = linear_velocity
	_state.angular_velocity = angular_velocity


func set_mass(mass: float):
	_state.mass = mass
	mass = mass





