extends Object

class_name Utility

static func clamp_vector_components(
	value: Vector3, 
	minimum: Vector3, 
	maximum: Vector3
) -> Vector3:
	return Vector3(
		clamp(value.x, minimum.x, maximum.x),
		clamp(value.y, minimum.y, maximum.y),
		clamp(value.z, minimum.z, maximum.z)
	);


static func transform_point(
		transform: Transform, 
		direction_vector: Vector3) -> Vector3:
	return transform.basis.xform(direction_vector) + transform.origin;


static func transform_point_inv(
		transform: Transform, 
		direction_vector: Vector3) -> Vector3:
	return transform.basis.xform_inv(direction_vector) - transform.origin;


# not affected by origin
static func transform_vector(
		transform: Transform, 
		vector: Vector3) -> Vector3:
	return transform.basis.xform(vector);

# not affected by origin
static func transform_vector_inv(
		transform: Transform, 
		vector: Vector3) -> Vector3:
	return transform.basis.xform_inv(vector);


# not affected by origin or scale
static func transform_direction(
		transform: Transform, 
		direction_vector: Vector3) -> Vector3:
	return transform.basis.orthonormalized().xform(direction_vector);


# not affected by origin or scale
static func transform_direction_inv(
		transform: Transform, 
		direction_vector: Vector3) -> Vector3:
	return transform.basis.orthonormalized().xform_inv(direction_vector);


