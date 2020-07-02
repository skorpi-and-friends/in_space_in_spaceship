extends Object

class_name Utilities

static func clamp_vector_components(
	value: Vector3, 
	minimum: Vector3, 
	maximum: Vector3
) -> Vector3:
	return Vector3(
		clamp(value.x, minimum.x, maximum.x),
		clamp(value.y, minimum.y, maximum.y),
		clamp(value.z, minimum.z, maximum.z)
	)