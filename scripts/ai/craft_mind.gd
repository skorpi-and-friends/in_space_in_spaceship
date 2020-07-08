extends Node

class_name CraftMind

export var craft_master_path: NodePath
onready var craft_master := get_node(craft_master_path) as CraftMaster;
var _state: CraftState;


static func face_dir_angular_input(direction: Vector3, currentTransform: Transform) -> Vector3:
	return face_local_dir_angular_input(
				Utility.transform_direction_inv(currentTransform, direction));


static func face_local_dir_angular_input(direction: Vector3) -> Vector3:
	var temp := Utility.look_direction_basis(direction).get_euler();
	temp *= Utility.RAD2DEG;
	temp = Vector3(sign(temp.x) * Utility.delta_angle_deg(0, temp.x),
					sign(temp.y) * Utility.delta_angle_deg(0, temp.y),
					sign(temp.z) * Utility.delta_angle_deg(0, temp.z));
	return temp * Utility.DEG2RAD;