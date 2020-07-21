extends CraftMind

class_name PlayerMind2D

onready var craft_master: CraftMaster2D;


func _ready():
	for child in get_children():
		var craft := child as CraftMaster2D;
		if craft:
			craft_master = craft;
			break;


func _process(delta):
	updatecraft_master_input(delta);


func _input(event: InputEvent):
	if event.is_action_pressed("Toggle Mouse Capture"):
		var new_mouse_mode := Input.MOUSE_MODE_CAPTURED;
		if Input.get_mouse_mode() == new_mouse_mode:
			new_mouse_mode = Input.MOUSE_MODE_VISIBLE 
		Input.set_mouse_mode(new_mouse_mode);
		

# we handle craft input separte from the event pipeline
# to simulate idle craft behavior
func updatecraft_master_input(delta):
	var state := craft_master.engine.state;
	
	var linear_input := Vector2();
	if Input.is_action_pressed("Thrust"):
		linear_input.y += 1;
	if Input.is_action_pressed("Thrust Reverse"):
		linear_input.y -= 1;
	if Input.is_action_pressed("Starfe Left"):
		linear_input.x += 1;
	if Input.is_action_pressed("Starfe Right"):
		linear_input.x -= 1;
	
	linear_input *= state.linear_v_limit;
	
	# use set speed if input is idle (i.e. 0) in given axis
	if !linear_input.x:
		linear_input.x = state.idle_velocity.x;
	# and if there's an input but no velocity limit in direction
	elif !state.limit_strafe_v:
		# set the input to inf. Since input is in the range of 0-1,
		# keyobard input won't take us past the v_limit otherwise
		linear_input.x = INF;
		
	if !linear_input.y:
		linear_input.y = state.idle_velocity.y;
	elif !state.limit_strafe_v:
		linear_input.x = INF;
	
	state.linear_input = linear_input;
	var angular_input := 0.0;
	if Input.is_action_pressed("Yaw Left"):
		angular_input -= 1
	if Input.is_action_pressed("Yaw Right"):
		angular_input += 1
	angular_input *= state.angular_v_limit;
	
#	angular_input *= state.angular_v_limit;
#	angular_input *= delta;
	state.angular_input = angular_input;


