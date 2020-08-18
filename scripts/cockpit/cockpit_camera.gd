extends Camera

onready var Yaw = get_parent()
export var x_clamp := 5.0;
export var y_clamp := 10.0;

func _ready():
	## Tell Godot that we want to handle input
	set_process_input(false)

func look_updown_rotation(rotation = 0):
	"""
	Get the new rotation for looking up and down
	"""
	var toReturn := self.get_rotation() + Vector3(rotation, 0, 0)
	##
	## We don't want the player to be able to bend over backwards
	## neither to be able to look under their arse.
	## Here we'll clamp the vertical look to 90° up and down
	toReturn.x = clamp(toReturn.x, PI / -2, PI / 2)

	return toReturn

func look_leftright_rotation(rotation = 0):
	"""
	Get the new rotation for looking left and right
	"""
	return Yaw.get_rotation() + Vector3(
			0, 
			rotation,
			0);

func mouse(event):
	"""
	First person camera controls
	"""
	##
	## We'll use the parent node "Yaw" to set our left-right rotation
	## This prevents us from adding the x-rotation to the y-rotation
	## which would result in a kind of flight-simulator camera
	var yaw_rotation = look_leftright_rotation(event.relative.x / -200)
	yaw_rotation.y = clamp(yaw_rotation.y, -deg2rad(x_clamp), deg2rad(x_clamp))
	Yaw.set_rotation(yaw_rotation);

	##
	## Now we can simply set our y-rotation for the camera, and let godot
	## handle the transformation of both together
	var pitch_rotation = look_updown_rotation(event.relative.y / -200)
	pitch_rotation.x = clamp(pitch_rotation.x, -deg2rad(y_clamp), deg2rad(y_clamp))
	self.set_rotation(pitch_rotation)

func _input(event):
	##
	## We'll only process mouse motion events
	if event is InputEventMouseMotion:
		return mouse(event)
