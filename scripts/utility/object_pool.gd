extends Reference

class_name ObjectPool

const CSharp: CSharpScript = preload("res://scripts/utility/ObjectPool.cs");

enum Policy {
	# can create more objects that the set size but doesn't cache the extra objects
	SoftLimited,
	# will never create more objects than the set size
	HardLimited
}

static func new_pool(size: int, object_generator: Object, policy: int) -> Reference:
	return (CSharp as CSharpScript).new(size, object_generator, policy);
