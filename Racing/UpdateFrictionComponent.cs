using Microsoft.Xna.Framework;

namespace Racing
{
    public struct UpdateFrictionComponent
    {
        public float CurrentForwardSpeed;
        public float DragForceMagnitude;
        public float CurrentDrag;
        public float CurrentTraction;
        public Vector2 LateralVelocity;

        public Vector2 CurrentRightNormal;
        public Vector2 CurrentForwardNormal;
        public Vector2 LateralImpulse;
    }
}