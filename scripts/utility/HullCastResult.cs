namespace ISIS {
    using Godot;
    public struct HullCastResult {
        /// <summary>
        ///  The object's surface normal at the intersection point.
        /// </summary>
        public Vector3 Normal { get; set; }

        /// <summary>
        /// The intersection point.
        /// </summary>
        public Vector3 Point { get; set; }

        /// <summary>
        /// The colliding object's velocity Vector3. If the object is an Area, the result is (0, 0, 0).
        /// </summary>
        public Vector3 LinearVelocity { get; set; }

        /// <summary>
        /// The shape index of the colliding shape.
        /// </summary>
        public int ShapeIndex { get; set; }

        /// <summary>
        /// The colliding object's ID.
        /// </summary>
        public ulong ColliderId { get; set; }

        /// <summary>
        /// The intersecting object's RID.
        /// </summary>
        public RID RID { get; set; }
    }
}