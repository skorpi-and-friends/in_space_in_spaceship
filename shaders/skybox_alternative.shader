shader_type canvas_item;
render_mode blend_premul_alpha;

uniform float star_percent = 0.565;
uniform vec4 star_color = vec4(1.0);
uniform vec4 back_color = vec4(0.0);

vec3 hash(vec3 p) {
	p = vec3(dot(p, vec3(127.1, 311.7, 74.7)),
	dot(p, vec3(269.5, 183.3, 246.1)),
	dot(p, vec3(113.5, 271.9, 124.6)));
	return -1.0 + 2.0 * fract(sin(p) * 43758.5453123);
}

float noise(vec3 p) {
	vec3 i = floor(p);
	vec3 f = fract(p);
	vec3 u = f * f * (3.0 - 2.0 * f);
	return mix(
		mix(mix(dot(hash(i+ vec3(0.0,0.0,0.0)), 
			f - vec3(0.0, 0.0, 0.0)), 
			dot(hash(i + vec3(1.0, 0.0, 0.0)), 
				f - vec3(1.0, 0.0, 0.0)), u.x),  
				mix(dot(hash(i + vec3(0.0, 1.0, 0.0)), f - vec3(0.0, 1.0, 0.0)),                     dot(hash(i + vec3(1.0, 1.0, 0.0)), f - vec3(1.0, 1.0, 0.0)), u.x), u.y),             mix(mix(dot(hash(i + vec3(0.0, 0.0, 1.0)), f - vec3(0.0, 0.0, 1.0)),                     dot(hash(i + vec3(1.0, 0.0, 1.0)), f - vec3(1.0, 0.0, 1.0)), u.x),                 mix(dot(hash(i + vec3(0.0, 1.0, 1.0)), f - vec3(0.0, 1.0, 1.0)),                     dot(hash(i + vec3(1.0, 1.0, 1.0)), f - vec3(1.0, 1.0, 1.0)), u.x), u.y), u.z);
}

void fragment() {
	float theta = UV.y * 3.14159;
	float phi = UV.x * 3.14159 * 2.0;
	vec3 unit = vec3(0.0, 0.0, 0.0);
	unit.x = sin(phi) * sin(theta);
	unit.y = cos(theta) * -1.0;
	unit.z = cos(phi) * sin(theta);
	unit = normalize(unit);
	float n = noise(unit);
	float a = 200.0;
	float b = 0.05;
	for (float index = 1.0; index < 17.0; index++){
		n+= noise(unit * a) * b;
		a += a;
		b *= b;
	}
	if (n > star_percent)
		COLOR = star_color;
	else
		COLOR = back_color;
}