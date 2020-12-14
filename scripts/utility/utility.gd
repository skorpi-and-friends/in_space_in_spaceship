extends Object

class_name Utility

# FIXME: these are inveted wtf
const RAD2DEG := 57.29578;
const DEG2RAD := 1.0/RAD2DEG;

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


static func clamp_vector_components_2D(
	value: Vector2, 
	minimum: Vector2, 
	maximum: Vector2
) -> Vector2:
	return Vector2(
		clamp(value.x, minimum.x, maximum.x),
		clamp(value.y, minimum.y, maximum.y)
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
	# use negeative direction since looking_at align the -Z axis
	return tform.looking_at((tform.origin - direction), up);
	
# FIXME: this horrible hack
static func get_basis_facing_direction(forward: Vector3,
		upward: Vector3 = Vector3.UP) -> Basis:
	# use negeative direction since looking_at align the -Z axis
	return Transform().looking_at(-forward, upward).basis;


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
#	return format_vector("%+03.3f",vector);
	return _format_vector_std_special(vector);


static func _format_vector_std_special(vector: Vector3) -> String:
	var x_split :=("%0.3f" % vector.x).split(".");
	var y_split := ("%0.3f" % vector.y).split(".");
	var z_split := ("%0.3f" % vector.z).split(".")
	return "({xw}.{xf}, {yw}.{yf}, {zw}.{zf})".format({
				"xw" : "%+03d" % int(x_split[0]),
				"xf" : x_split[1],
				"yw" : "%+03d" % int(y_split[0]),
				"yf" : y_split[1],
				"zw" : "%+03d" % int(z_split[0]),
				"zf" : z_split[1]
		});


static func format_vector_rich(vector: Vector3, 
		postive_color = Color.coral,
		negative_color = Color.firebrick) -> String:
	return "%s, %s, %s" % [
			float_str_sign_colored(vector.x, postive_color, negative_color),
			float_str_sign_colored(vector.y, postive_color, negative_color),
			float_str_sign_colored(vector.z, postive_color, negative_color)
		];

static func float_str_sign_colored(number: float, 
		postive_color: Color = Color.coral,
		negative_color: Color = Color.firebrick) -> String:
	var color := negative_color if sign(number) < 0 else postive_color;
	return "[color=#{c}]{w}.{f}[/color]".format({
			"c" : color.to_html(),
			# we must format again on the int
			# to enable zero padding as the float formatter
			# doesn't support that
			# also use abs to remove any negative signs
			"w" : "%03d" % abs(number),
			"f" : ("%+0.3f" % number).split(".")[1]
	});

static func deg2rad_vec3(vector: Vector3) -> Vector3:
	return Vector3(deg2rad(vector.x), deg2rad(vector.y), deg2rad(vector.z));


static func deg2rad_ve2c(vector: Vector2) -> Vector2:
	return Vector2(deg2rad(vector.x), deg2rad(vector.y));


static func delta_angle_deg(a: float, b: float) -> float:
	var lpea_a := smallest_postive_equivalent_angle_deg(a);
	var lpea_b := smallest_postive_equivalent_angle_deg(b);
	var result := abs(lpea_a - lpea_b);
	return result - 360 if result > 180 else result;
#	return abs(result - 360) if result > 180 else result;


static func delta_angle_rad(a: float, b: float) -> float:
	var lpea_a := smallest_postive_equivalent_angle_rad(a);
	var lpea_b := smallest_postive_equivalent_angle_rad(b);
	var result := abs(lpea_a - lpea_b);
	return abs(result - TAU) if result > PI else result;


static func smallest_equivalent_angle_deg(angle: float) -> float:
	angle = fmod(angle, 360.0);
	if angle > 180:
		angle -= 360;
	elif angle < -180:
		angle += 360;
	return angle;


static func smallest_equivalent_angle_rad(angle: float) -> float:
	angle = fmod(angle, TAU);
	if angle <= -PI:
		angle += TAU;
	elif angle > PI:
		angle -= TAU;
	return angle;


static func smallest_postive_equivalent_angle_deg(angle: float) -> float:
	angle = fmod(angle, 360.0);
	if angle < 0:
		return 360 + angle;
	return angle;


static func smallest_postive_equivalent_angle_rad(angle: float) -> float:
	angle = fmod(angle, TAU);
	if angle < 0:
		return angle + TAU;
	return angle;


static func lerp_angle(from: float, to: float, weight: float) -> float:
		return lerp(fmod(to, 360.0), fmod(from, 360.0), weight);


static func move_towards_angle(from: float, to: float, max_delta: float) -> float:
	return to if abs(max_delta) > delta_angle_rad(from, to) else from + max_delta;


static func min_vec_componentwise(a: Vector3, b: Vector3) -> Vector3:
	return Vector3(
			min(a.x, b.x),
			min(a.y, b.y),
			min(a.z, b.z)
	);


static func max_vec_componentwise(a: Vector3, b: Vector3) -> Vector3:
	return Vector3(
			max(a.x, b.x),
			max(a.y, b.y),
			max(a.z, b.z)
	);

static func materialFromViewportTexture(viewport:Viewport) -> SpatialMaterial:
	var material = SpatialMaterial.new();
	applyViewportTexture(material,viewport);
	return material;

static func applyViewportTexture(material:SpatialMaterial, viewport:Viewport) -> void:
	material.flags_unshaded = true;
	material.flags_transparent = true;
	material.flags_do_not_receive_shadows = true;
	
	material.albedo_color = Color.white;
	material.albedo_texture = viewport.get_texture();
	material.albedo_texture.flags |= Texture.FLAG_VIDEO_SURFACE;
