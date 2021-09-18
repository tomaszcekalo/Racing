using Microsoft.Xna.Framework;
using System.Linq;
using Undine.Core;
using Undine.MonoGame;
using Undine.VelcroPhysics.MonoGame;

namespace Racing
{
    public class CarSteeringSystem : UnifiedSystem<KeyboardComponent, VelcroPhysicsComponent>
    {
        public GameTime GameTime { get; set; }
        public override void ProcessSingleEntity(int entityId,
            ref KeyboardComponent a, ref VelcroPhysicsComponent b)
        {
            var dt = (float)GameTime.ElapsedGameTime.TotalSeconds;
            var body = b.Body;
            body.GetTransform(out var transform);
            var dir = new Vector2(transform.q.s, -transform.q.c);
            var v = body.LinearVelocity.Length();
            var engineForce = 555;

            if (a.Bindings.Where(x => x.KeyAction == "Forward").Any(x => x.IsDown))
            {
                body.ApplyForce(engineForce * dir * dt);
            }
            if (a.Bindings.Where(x => x.KeyAction == "Back").Any(x => x.IsDown))
            {
                body.ApplyForce(-engineForce * dir * dt);
            }
        }
    }
}