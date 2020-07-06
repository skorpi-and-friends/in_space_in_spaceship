extends Object

class_name Utility

const DEG2RAD := 57.29578;
const RAD2DEG := 1.0/DEG2RAD;

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


static func face_direction(tform:Transform, direction: Vector3, up: Vector3) -> Transform:
	return tform.looking_at((tform.origin + direction), up);
	
# FIXME: this horrible hack
static func look_direction_basis(forward: Vector3, upward: Vector3 = Vector3.UP) -> Basis:
	return Transform().looking_at(forward, upward).basis;

#static func look_direction_basis(forward: Vector3, upward: Vector3 = Vector3.UP) -> Basis:
#	var z := forward.normalized();
#	var x := upward.cross(z).normalized();
#	var y := z.cross(x).normalized();
#	return Basis(x,y,z);


static func format_vector(
		format_string: String,
		vector: Vector3) -> String:
		return "({x}, {y}, {z})".format({
					"x" : format_string % vector.x,
					"y" : format_string % vector.y,
					"z" : format_string % vector.z});


static func format_vector_std(vector: Vector3) -> String:
#	return format_vector("%+003.5f",vector);
	return format_vector("%+03.f",vector);


static func deg2rad_vec3(vector: Vector3) -> Vector3:
	return Vector3(deg2rad(vector.x), deg2rad(vector.y), deg2rad(vector.z));


static func deg2rad_ve2c(vector: Vector2) -> Vector2:
	return Vector2(deg2rad(vector.x), deg2rad(vector.y));


static func delta_angle_deg(a: float, b: float) -> float:
	var lpea_a := lowest_pos_equivalent_angle(a);
	var lpea_b := lowest_pos_equivalent_angle(b);
	var result := abs(lpea_a - lpea_b);
	if result < 0.00001 && (lpea_a == 180 || lpea_b == 180):
		return 180.0;
	return result;

static func delta_angle_rad(a: float, b: float) -> float:
	return delta_angle_deg(rad2deg(a), rad2deg(b)) * DEG2RAD;

static func lowest_equivalent_angle(angle: float) -> float:
	angle = fmod(angle, 360.0);
	if angle > 180:
		angle -= 360;
	elif angle < -180:
		angle += 360;
	return angle;


static func lowest_pos_equivalent_angle(angle: float) -> float:
	angle = fmod(angle, 360.0);
	if angle < 0:
		return 360 + angle;
	return angle;


static func lerp_angle(from: float, to: float, weight: float) -> float:
		return lerp(fmod(to, 360.0), fmod(from, 360.0), weight);


static func move_towards_angle(from: float, to: float, max_delta: float) -> float:
	return to if abs(max_delta) > delta_angle(from, to) else from + max_delta;