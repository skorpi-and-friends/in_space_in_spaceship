extends Spatial

class_name AttireMaster

signal damage_recieved(profile, weapon, damage)

var profiles := [];

func _ready():
	for child in get_children():
		var profile := child as AttireProfile;
		if !profile: continue;
		profile.connect("contact", self, "contact");
		profile.connect("contact", self, "contact");
		profiles.append(profile);
	if profiles.empty():
		printerr("no profiles found on attire master");


func contact(profile: AttireProfile, body):
	if !body.has_method("get_weapon"): return;
	var weapon := body.get_weapon() as Weapon;
	var remaining_damage := profile.damage(weapon.damage, weapon.damage_type);
	emit_signal("damage_recieved", profile, weapon, weapon.damage - remaining_damage);

"""

[ExecuteAlways]
public class AttireMaster : MonoBehaviour {
	public AttireProfile[] Profiles { get => _profiles; set => _profiles = value; }

	[SerializeField] private AttireProfile[] _profiles;

	private List<ParticleCollisionEvent> _particleCollisionEvents;

	private void Start() {
		_profiles = transform.GetComponentsInChildren<AttireProfile>();
		_particleCollisionEvents = new List<ParticleCollisionEvent>();
	}

	private void OnParticleCollision(GameObject other) {
		var weaponUsed = other.GetComponent<IProjectileWeapon>();
		if (weaponUsed == null) return;
		Debug.Log($"Stricken by {other.name}");
		ParticlePhysicsExtensions.GetCollisionEvents(
			other.GetComponent<ParticleSystem>(),
			gameObject,
			_particleCollisionEvents
		);
		if (other.name.Equals("Wing")) {

		}
		Debug.Log($"{name} struck by {other.name} at collider {_particleCollisionEvents[0].colliderComponent.name}");
		DamageConcenredAttire(
			weaponUsed.Damage,
			weaponUsed.DamageType,
			_particleCollisionEvents.Select(collision => collision.intersection).ToArray()
		);
		//var numCollisionEvents = bulletEmitter.GetCollisionEvents(other, collisionEvents);
		//Debug.Log($"collided with {collisionEvents[0].colliderComponent.name}");
		// for (var i = 0; i < numCollisionEvents; i++) Owner.Hit(this, other);
		//	fireCount += numCollisionEvents;
	}

	private void OnCollisionEnter(Collision other) {

		var forceVector = (other.impulse / Time.fixedDeltaTime);

		if (forceVector == Vector3.zero) return;

		var force = forceVector.magnitude;

		var collisionContactsPoints = new ContactPoint[other.contactCount];
		other.GetContacts(collisionContactsPoints);
		DamageConcenredAttire(force, DamageType.Collision, collisionContactsPoints.Select(contact => contact.point).ToArray());
	}

	private void DamageConcenredAttire(float damage, DamageType damageType, Vector3[] contactsPoints) {
		var damagedProfiles = new List<AttireProfile>();
		foreach (var point in contactsPoints) {
			foreach (var profile in _profiles) {
				// Debug.Log($"Collision point{contact.point}; ClosestPoint {collider.ClosestPoint(contact.point)}");
				if (profile.IsPointInColliders(point)) {
					damagedProfiles.Add(profile);
				}
			}
			// Debug.Log($"thisCollider: {contact.thisCollider.name} otherCollider: {contact.otherCollider.name}");
		}
		if (damagedProfiles.Count == 0) return;
		var distributeDamage = damage / damagedProfiles.Count;
		foreach (var profile in damagedProfiles) {
			var remainingDamage = profile.DamageEvent(distributeDamage, damageType);
			if (remainingDamage > 0) Debug.Log("Dead");
		}
	}

}
"""