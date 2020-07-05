extends Node

class_name PlayerMind

export var _craft_path: NodePath
onready var _craft := get_node(_craft_path) as CraftMaster;
var _state: CraftState;

func _process(delta):
	update_craft_input(delta);
	
	if Input.is_action_pressed("Fire Primary"):
		_craft.arms.primary_weapon._activate();
	if Input.is_action_pressed("Fire Secondary"):
		_craft.arms.secondary_weapon._activate();


func update_craft_input(delta):
	var state := _craft.engine.state;
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
