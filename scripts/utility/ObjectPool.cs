using Godot;

public class ObjectPool : Reference {

    public enum Policy {

        /// can create more objects that the set size but doesn't cache the extra objects
        SoftLimited,
        /// will never create more objects than the set size
        HardLimited
    }

    private int _size;
    public int Size {
        get => _size;
        set =>
            throw new System.ApplicationException("can't resize pool after creation");
    }

    public Policy InstancePolicy { get; set; } = Policy.SoftLimited;

    public System.Collections.ICollection ActiveObjects => _activeObjects.Keys;
    public System.Collections.ICollection InactiveObjects => _inactiveObjects;

    [Export] private Godot.Collections.Dictionary _activeObjects;

    // keep a count for performance improvements
    private int _activeObjectCount;
    public int ActiveObjectCount {
        get => _activeObjectCount;
        set =>
            throw new System.ApplicationException();
    }

    [Export] private Godot.Collections.Array _inactiveObjects;

    private int _inactiveObjectCount;
    public int InactiveObjectCount {
        get { return _inactiveObjectCount; }
        set { _inactiveObjectCount = value; }
    }

    private FuncRef _objectGeneratorFunc;

    public ObjectPool() {

    }

    public ObjectPool(int size, FuncRef objectGenerator, Policy policy) {
        _size = size;
        InstancePolicy = policy;
        _activeObjects = new Godot.Collections.Dictionary();
        _inactiveObjects = new Godot.Collections.Array();
        _objectGeneratorFunc = objectGenerator;
    }

    public object GetObject() {
        object @object;
        if (_inactiveObjectCount > 0) {
            @object = _inactiveObjects[_inactiveObjectCount - 1];
            _inactiveObjects.RemoveAt(_inactiveObjectCount - 1);
            _inactiveObjectCount--;
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
        _activeObjects.Remove(@object);
        _activeObjectCount--;
        var totalObjectCount = _inactiveObjectCount + _activeObjectCount;
        if (totalObjectCount < _size) {
            _inactiveObjects.Add(@object);
            _inactiveObjectCount++;
        }
    }

}