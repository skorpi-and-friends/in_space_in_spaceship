extends Node

class_name CraftMind


static func face_dir_angular_input(direction: Vector3, currentTransform: Transform) -> Vector3:
	return face_local_dir_angular_input(
				Utility.transform_direction_inv(currentTransform, direction));


static func face_local_dir_angular_input(direction: Vector3) -> Vector3:
	var temp := Utility.get_basis_facing_direction(direction).get_euler();
	temp *= Utility.RAD2DEG;
	temp = Vector3(sign(temp.x) * abs(Utility.delta_angle_deg(0, temp.x)),
					sign(temp.y) * abs(Utility.delta_angle_deg(0, temp.y)),
					sign(temp.z) * abs(Utility.delta_angle_deg(0, temp.z)));
	return temp * Utility.DEG2RAD;