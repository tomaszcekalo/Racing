using Microsoft.Xna.Framework;
using System;
using System.Linq;
using Undine.Core;
using Undine.MonoGame;
using Undine.VelcroPhysics.MonoGame;

namespace Racing
{
    public class WheelTurningSystem : UnifiedSystem<WheelComponent, VelcroPhysicsComponent, KeyboardComponent>
    {
        public GameTime GameTime { get; set; }

        public override void ProcessSingleEntity(int entityId,
            ref WheelComponent a,
            ref VelcroPhysicsComponent b,
            ref KeyboardComponent c)
        {
            if (c.Bindings.Any(x => x.IsDown && x.KeyAction == "Left"))
            {
                a.WheelRotation -= (float)GameTime.ElapsedGameTime.TotalSeconds * a.WheelRotatingSpeed;
                //row.ComponentB.Body.AngularVelocity = -1;
                //row.ComponentB.Body.ApplyTorque(-1);
                if (a.WheelRotation < -a.MaxTurn)
                {
                    a.WheelRotation = -a.MaxTurn;
                }
            }
            else if (c.Bindings.Any(x => x.IsDown && x.KeyAction == "Right"))
            {
                a.WheelRotation += (float)GameTime.ElapsedGameTime.TotalSeconds * a.WheelRotatingSpeed;
                //row.ComponentB.Body.AngularVelocity = 1;
                //row.ComponentB.Body.ApplyTorque(1);
                if (a.WheelRotation > a.MaxTurn)
                {
                    a.WheelRotation = a.MaxTurn;
                }
            }
            else
            {
                a.WheelRotation += (float)GameTime.ElapsedGameTime.TotalSeconds * Math.Sign(-a.WheelRotation) * a.WheelRotatingSpeed;
            }

            b.Body.Rotation += a.WheelRotation;
        }
    }
}