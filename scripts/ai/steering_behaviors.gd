extends Object

class_name SteeringBehaviors

static func find_intercept_position(
		currentPosition: Vector3, 
		travelSpeed:float, 
		targetPosition: Vector3, 
		targetVelocity: Vector3) -> Vector3:
	# FIXME improve
	var toTarget = targetPosition - currentPosition;
	var distanceToTarget = toTarget.length();
	var timeToTargetPosition = distanceToTarget / travelSpeed;
	# time to target intercept position

	return (targetPosition + (timeToTargetPosition * targetVelocity));
