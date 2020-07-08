extends Spatial

export var _camera_path: NodePath;
onready var _camera := get_node(_camera_path) as CraftCamera;

export var direction := Vector3.LEFT;

func _process(delta):
	var position = _camera._focus_point;
	var adjusted_rotation := Utility.transform_direction(
			_camera._target_rotation, 
			_camera.facing_direction);
	var new_rotation = Utility.get_basis_facing_direction(
			adjusted_rotation,
			_camera._target_rotation.y);
#			Vector3.UP);
	global_transform = Transform(new_rotation, position);
#	global_transform = Transform().translated(position
#			).looking_at(
#					position + direction, 
#					Vector3.UP);
