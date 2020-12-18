using System.Collections;
using System.Collections.Generic;
using Godot;
using ISIS.Minds;
using static ISIS.Static;

#if GODOT_Real_IS_DOUBLE
using Real = System.Double;
#else
using Real = System.Single;
#endif

namespace ISIS.Minds.SteeringBehaviors.Boids {
    public class Flock : Spatial, IEnumerable<CraftWrapper>, IList<CraftWrapper> {
        public int Count => _boids.Count;
        public Vector3 HeadingSum { get; protected set; }
        public Vector3 AverageHeading { get; protected set; }
        public Vector3 Center { get; protected set; }
        public Vector3 CenterSum { get; protected set; }
        private List<CraftWrapper> _boids { get; } = new List<CraftWrapper>();

        public override void _Ready() {
            foreach (var item in GetChildren()) {
                var(isMindfulCraft, craft) = CraftMind.IsCraftMaster(item);
                if (!isMindfulCraft) continue;
                _boids.Add(new CraftWrapper(craft));
            }
        }

        public override void _PhysicsProcess(Real delta) {
            CenterSum = Center = HeadingSum = AverageHeading = Vector3.Zero;
            foreach (var craft in _boids) {
                CenterSum += craft.CraftActual.GlobalTranslation();
                HeadingSum += craft.CraftActual.GlobalTransform.basis.z;
            }
            var memberCount = _boids.Count;
            if (memberCount > 0) {
                AverageHeading = HeadingSum / memberCount;
                Center = CenterSum / memberCount;
            }
            /* GD.Print(this);
            GD.Print(Name);
            GD.Print(CenterSum);
            GD.Print(Center); */
            // this.DebugDraw().Call("draw_box", Center, Vector3.One * 10);
        }

        #region IEnumerable Implementation

        public IEnumerator<CraftWrapper> GetEnumerator() {
            return ((IEnumerable<CraftWrapper>) _boids).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return ((IEnumerable) _boids).GetEnumerator();
        }
        #endregion
        #region IList Implementation
        public bool IsReadOnly => ((ICollection<CraftWrapper>) _boids).IsReadOnly;

        public CraftWrapper this [int index] { get => ((IList<CraftWrapper>) _boids) [index]; set => ((IList<CraftWrapper>) _boids) [index] = value; }
        public int IndexOf(CraftWrapper item) {
            return _boids.IndexOf(item);
        }

        public void Insert(int index, RigidBody item) {
            _boids.Insert(index, new CraftWrapper(item));
        }
        public void Insert(int index, CraftWrapper item) {
            _boids.Insert(index, item);
        }

        public void RemoveAt(int index) {
            _boids.RemoveAt(index);
        }

        public void Add(CraftWrapper item) {
            _boids.Add(item);
        }

        public void Add(RigidBody item) {
            _boids.Add(new CraftWrapper(item));
        }
        public void Add(CraftMind item) {
            _boids.Add(new CraftWrapper(item));
        }

        public void Clear() {
            _boids.Clear();
        }

        public bool Contains(CraftWrapper item) {
            return _boids.Contains(item);
        }

        public void CopyTo(CraftWrapper[] array, int arrayIndex) {
            _boids.CopyTo(array, arrayIndex);
        }

        public bool Remove(CraftWrapper item) {
            return _boids.Remove(item);
        }
        #endregion
    }
}