extends Control

export var _craft_path: NodePath
onready var _craft := get_node(_craft_path) as CraftMaster;
onready var _marker := $Marker as Control;
export var _marker_size := Vector2(16, 16);

func _process(delta: float) -> void:
	if !_craft:
		return;
	var viewport := get_viewport();
	var current_camera := viewport.get_camera();
	var craft_velocity := _craft.linear_velocity;
	if craft_velocity.length_squared() < 0.1:
		_marker.visible = false;
		return;
	
	var extents := _craft.engine.craft_extents;
	var projection_length := extents.x;
	if extents.y > projection_length:
		projection_length = extents.y;
	projection_length *= 2;

	var craft_position := _craft.global_transform.origin;
	var projection_world_position := craft_position + (projection_length * craft_velocity.normalized());
	
	var projection_position := current_camera.unproject_position(
		projection_world_position
	);
	_marker.rect_size = _marker_size;
	var padding = _marker.rect_size.x / 2.0;
	
	_marker.rect_position = Vector2(
		clamp(projection_position.x, padding, viewport.size.x - padding),
		clamp(projection_position.y, padding, viewport.size.y - padding)
	);
	_marker.visible = true;
