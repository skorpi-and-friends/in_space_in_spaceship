extends Control


export var _target_path: NodePath
onready var _target := get_node(_target_path) as RigidBody;

export var _weapon_path: NodePath
onready var weapon :=get_node(_weapon_path) as RangedWeapon;

onready var _marker := $Marker as Control;
export var _marker_max_size := Vector2(32, 32);
export var _marker_min_size := Vector2(8, 8);


func _process(delta: float) -> void:
	if !_target || !weapon:
		_marker.visible = false;
		return;
	
	var target_world_position := _target.global_transform.origin;
	var target_velocity := _target.linear_velocity;
	if target_velocity.length_squared() < 0.1:
		_marker.visible = false;
		return;

	var weapon_position := weapon.global_transform.origin;
	var weapon_range := weapon._get_range();
	
	var target_distance := (target_world_position - weapon_position).length_squared();
	if weapon_range * weapon_range < target_distance:
		_marker.visible = false;
		return;
	target_distance = sqrt(target_distance);
	
	var viewport := get_viewport();
	var current_camera := viewport.get_camera();
	
	if current_camera.is_position_behind(target_world_position):
		_marker.visible = false;
		return;
	
	var projected_position := current_camera.unproject_position(
				target_world_position);
	
	var is_on_screen := projected_position.x > 0 && projected_position.x < viewport.size.x;
	is_on_screen = is_on_screen && projected_position.y > 0 && projected_position.y < viewport.size.y;
	
	if !is_on_screen:
		_marker.visible = false;
		return;
		
	_marker.visible = true;
	var leading_world_position := SteeringBehaviors.find_intercept_position(
		weapon_position,
		weapon._get_projectile_velocity(),
		target_world_position,
		target_velocity
	);
	projected_position = current_camera.unproject_position(
		leading_world_position
	);
	var weight := 1.0 - (target_distance/weapon_range);
	var result = lerp(
		_marker_min_size, 
		_marker_max_size, 
		weight);
	_marker.rect_size = result;
	projected_position -= _marker.rect_size * .5;
	_marker.rect_position = projected_position;


func set_target(target: RigidBody) -> void:
	_target = target;
