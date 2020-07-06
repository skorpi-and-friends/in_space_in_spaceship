extends Camera

class_name CraftCameraAngle

const DEFAULT_ANGLES := Vector2(30.0, 180.0);

export var target_path: NodePath;

export var distance := 15.0;

export var _orbit_angles := Vector2(30.0, 0.0);

#export(float, -90, 90, 1) var max_pitch_angle := 90.0;
#export(float, -90, 90, 1) var min_pitch_angle := -90.0;

#export(float, -180, 180, 1) var max_angle_y := 180.0;
#export(float, -180, 180, 1) var min_angle_y := -180.0;

export var rotation_speed := 90.0;
export(float, 0, 90, 0.1) var align_delay := 3.0;
export(float, 0, 90, 0.1) var align_smooth_range := 45.0;

var _last_manual_rotation_time: float;
var _focus_point: Vector3;
var _previous_focus_point: Vector3;

var _rotation_changed_manually := false;
var _target: Spatial;

func _process(delta: float):
	_target = get_node(target_path) as Spatial;
	_last_manual_rotation_time += delta;
	update_focus_point();
	
	if _rotation_changed_manually || automatic_rotation(delta):
		constrain_angles();
		
	var euler_vector := Vector3(_orbit_angles.x, _orbit_angles.y, 0.0);
	euler_vector = -euler_vector;
	var orbital_rotation := Basis(Utility.deg2rad_vec3(euler_vector));
	
	# TODO: align agains velocity
	# TODO; ignore rapid rotation
#	var new_rotation := orbital_rotation.slerp(rolled_rotation, rotation_speed * delta);
		
	var new_rotation := _target.global_transform.basis * orbital_rotation;
	
	var look_direction := new_rotation * Vector3.FORWARD;
	var new_position := _focus_point - look_direction * distance;
	global_transform = Transform(new_rotation, new_position);
	
	
	_rotation_changed_manually = false;
#	var current_basis := global_transform.basis
#	var target_basis := _target.global_transform.basis;
#
#	var look_rotation := current_basis;
#	if _rotation_changed_manually || automatic_rotation():
#		constrain_angles();
#		var euler_vector := Vector3(_orbit_angles.x, _orbit_angles.y, -_target.rotation.z);
#		euler_vector = -euler_vector;
#		look_rotation = Basis(Utility.deg2rad_vec3(euler_vector));
#
#	var look_direction := look_rotation * Vector3.FORWARD;
#	var look_position := _focus_point - look_direction * distance;
#
#	global_transform = Transform(look_rotation, look_position);


func update_focus_point():
	_previous_focus_point = _focus_point;
	_focus_point = _target.global_transform.origin;


func constrain_angles():
	if !_rotation_changed_manually && abs(_orbit_angles.x) > 90:
		pass
#		_orbit_angles.y -= 180;
	_orbit_angles.x = clamp(_orbit_angles.x, -90, 90);

	if _orbit_angles.y > 180:
		_orbit_angles.y -= 360;
	elif _orbit_angles.y < -180:
		_orbit_angles.y += 360;


func _input(event):
	var mouse_event := event as InputEventMouseMotion;
	if !mouse_event: return;
	var input := Vector2(
		# notice the axis switch
		mouse_event.relative.y,
		mouse_event.relative.x
	);
	_orbit_angles += (rotation_speed * get_process_delta_time()) * input;
	_rotation_changed_manually = true;
	_last_manual_rotation_time = 0;


func automatic_rotation(delta: float) -> bool:
	if _last_manual_rotation_time < align_delay:
		return false;
	
	var pitch_change = get_rotation_change(
			Utility.delta_angle_deg(_orbit_angles.x, DEFAULT_ANGLES.x));
	var yaw_change = get_rotation_change(
			Utility.delta_angle_deg(_orbit_angles.y, DEFAULT_ANGLES.y));
	
	_orbit_angles = Vector2(
		Utility.move_towards_angle(_orbit_angles.x, DEFAULT_ANGLES.x, pitch_change),
		Utility.move_towards_angle(_orbit_angles.y, DEFAULT_ANGLES.y, yaw_change)
#		Utility.move_towards_angle(0, heading_angle, yaw_rotation_change)
	);
	
	return true;


func get_rotation_change(angle_delta: float) -> float:
	var rotation_change := rotation_speed * get_process_delta_time();

	var delta_abs := abs(angle_delta);
	
	if delta_abs < align_smooth_range:
		rotation_change *= delta_abs / align_smooth_range;
		
#	elif 180.0 - delta_abs < align_smooth_range:
#		rotation_change *= (180.0 - delta_abs) / align_smooth_range;
	return rotation_change;