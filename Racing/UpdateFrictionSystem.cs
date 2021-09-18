using Microsoft.Xna.Framework;
using Undine.Core;
using Undine.VelcroPhysics.MonoGame;

namespace Racing
{
    public class UpdateFrictionSystem : UnifiedSystem<UpdateFrictionComponent, VelcroPhysicsComponent>
    {
        private static readonly Vector2 _right = new Vector2(1, 0);
        private static readonly Vector2 _forward = new Vector2(0, 1);
        private float _maxLateralImpulse = 0.3f;
        public override void ProcessSingleEntity(int entityId, ref UpdateFrictionComponent a, ref VelcroPhysicsComponent b)
        {
            a.CurrentRightNormal = b.Body.GetWorldVector(_right);
            a.LateralVelocity = Vector2.Dot(a.CurrentRightNormal, b.Body.LinearVelocity) * a.CurrentRightNormal;

            a.LateralImpulse = b.Body.Mass * -a.LateralVelocity;
            if (a.LateralImpulse.Length() > _maxLateralImpulse)
            {
                a.LateralImpulse *= _maxLateralImpulse / a.LateralImpulse.Length();
            }
            b.Body.ApplyLinearImpulse(a.LateralImpulse, b.Body.Position);

            b.Body.ApplyAngularImpulse(0.1f * b.Body.Inertia * -b.Body.AngularVelocity);
            a.CurrentForwardNormal = b.Body.GetWorldVector(_forward);
            //_currentForwardSpeed=_currentForwardNormal.Normalize()
            a.CurrentForwardSpeed = a.CurrentForwardNormal.Length();//Squared?
            a.DragForceMagnitude = -0.25f * a.CurrentForwardSpeed;
            a.DragForceMagnitude *= a.CurrentDrag;
            b.Body.ApplyForce(a.CurrentTraction * a.DragForceMagnitude * a.CurrentForwardNormal, b.Body.Position);
        }
    }
}