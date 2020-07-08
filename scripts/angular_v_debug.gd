extends RigidBody
export var acceleration := Vector3();
export var length := 20;

export var inertia_display := Vector3();
export var calculated_inertia_display := Vector3();

func _ready():
	calculated_inertia_display = Vector3(
		(1.0/12.0) * (length * length),
		1,
		1
	);

func _physics_process(delta):
	if Input.is_action_pressed("Fire Primary"):
		add_torque(acceleration * inertia_display);

func _integrate_forces(state: PhysicsDirectBodyState):
	inertia_display = Vector3.ONE / state.inverse_inertia;
	