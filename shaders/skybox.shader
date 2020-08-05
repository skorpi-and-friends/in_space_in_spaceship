shader_type canvas_item;

uniform float star_percent = 0.565;
uniform vec4 star_color = vec4(1.0);
uniform vec4 back_color = vec4(0.0);
uniform sampler2D noise;

void fragment() {
	float rand = texture(noise, UV.xy).x;
	if (rand > star_percent)
		COLOR = star_color;
	else
		COLOR = back_color;
}
