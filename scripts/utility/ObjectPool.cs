using Godot;

namespace ISIS {
    public class ObjectPool : Reference {
        public enum Policy {

            /// <summary>
            /// can create more objects that the set size but doesn't cache the extra objects
            /// </summary>
            SoftLimited,
            /// <summary>
            /// will never create more objects than the set size
            /// </summary>
            HardLimited
        }

        private readonly int _size;
        public int Size {
            get => _size;
            set =>
                throw new System.ApplicationException("can't resize pool after creation");
        }

        public Policy InstancePolicy { get; set; } = Policy.SoftLimited;

        public System.Collections.ICollection ActiveObjects => _activeObjects.Keys;
        public System.Collections.ICollection InactiveObjects => _inactiveObjects;

        [Export] private readonly Godot.Collections.Dictionary _activeObjects;

        // keep a count for performance improvements
        private int _activeObjectCount;
        public int ActiveObjectCount {
            get => _activeObjectCount;
            set =>
                throw new System.ApplicationException();
        }

        [Export] private readonly Godot.Collections.Array _inactiveObjects;
        public int InactiveObjectCount { get; set; }

        private readonly FuncRef _objectGeneratorFunc;

        public ObjectPool() { }

        public ObjectPool(int size, FuncRef objectGenerator, Policy policy) {
            _size = size;
            InstancePolicy = policy;
            _activeObjects = new Godot.Collections.Dictionary();
            _inactiveObjects = new Godot.Collections.Array();
            _objectGeneratorFunc = objectGenerator;
        }

        public object GetObject() {
            object @object;
            if (InactiveObjectCount > 0) {
                @object = _inactiveObjects[InactiveObjectCount - 1];
                _inactiveObjects.RemoveAt(InactiveObjectCount - 1);
                InactiveObjectCount--;
            } else {
                if (_activeObjects.Count >= _size) {
                    if (InstancePolicy == Policy.HardLimited) {
                        return null;
                    }
                }
                @object = _objectGeneratorFunc.CallFunc();
            }
            _activeObjects.Add(@object, null);
            _activeObjectCount++;
            return @object;
        }

        public void ReturnObject(object @object) {
            if (!_activeObjects.Contains(@object))
                return; // exit early if not found to preserve numbers
            _activeObjects.Remove(@object);
            _activeObjectCount--;
            var totalObjectCount = InactiveObjectCount + _activeObjectCount;
            if (totalObjectCount < _size) {
                _inactiveObjects.Add(@object);
                InactiveObjectCount++;
            }
        }
    }
}