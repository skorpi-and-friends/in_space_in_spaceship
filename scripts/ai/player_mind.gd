extends Node

class_name PlayerMind

export var _craft: NodePath
var _state: CraftState


func _process(delta):
	update_craft_input(delta);


func update_craft_input(delta):
	var state := get_craft_state();
	var linear_input := Vector3();
	if Input.is_action_pressed("Thrust"):
		linear_input.z += 1
	if Input.is_action_pressed("Thrust Reverse"):
		linear_input.z -= 1
	if Input.is_action_pressed("Starfe Left"):
		linear_input.x += 1
	if Input.is_action_pressed("Starfe Right"):
		linear_input.x -= 1
	if Input.is_action_pressed("Altitude Raise"):
		linear_input.y += 1
	if Input.is_action_pressed("Altitude Lower"):
		linear_input.y -= 1
	
	linear_input *= state.linear_v_limit;
	
	if !linear_input.x:
		linear_input.x = state.set_speed.x;
		
	if !linear_input.y:
		linear_input.y = state.set_speed.y;
		
	if !linear_input.z:
		linear_input.z = state.set_speed.z;
	
	state.linear_input = linear_input;
	
	var angular_input := Vector3();
	if Input.is_action_pressed("Roll Right"):
		angular_input.z += 1
	if Input.is_action_pressed("Roll Left"):
		angular_input.z -= 1
	if Input.is_action_pressed("Pitch Up"):
		angular_input.x -= 1
	if Input.is_action_pressed("Pitch Down"):
		angular_input.x += 1
	if Input.is_action_pressed("Yaw Left"):
		angular_input.y += 1
	if Input.is_action_pressed("Yaw Right"):
		angular_input.y -= 1
	
	angular_input *= state.angular_v_limit;
	
	state.angular_input = angular_input;


func get_craft_state() -> CraftState:
	if !_state:
		_state = get_node(_craft).get_node("State") as CraftState;
		# TODO: remove this assert
		assert(_state != null);
	return _state;
