using Microsoft.Xna.Framework;
using Undine.Core;
using Undine.LilyPath;
using Undine.MonoGame;
using Undine.VelcroPhysics.MonoGame;
using VelcroPhysics.Dynamics;

namespace Racing
{
    public class RacingFactory
    {
        private EcsContainer _ecsContainer;
        private World _world;
        private PenumbraHullsSystem _penumbraLightSystem;
        public RacingFactory(
            EcsContainer ecsContainer,
            PenumbraHullsSystem penumbraLightSystem,
            World world)
        {
            _ecsContainer = ecsContainer;
            _world = world;
            _penumbraLightSystem = penumbraLightSystem;
        }
        public (IUnifiedEntity, Body) GetPhysicalRectangle(Vector2 position,
            float width,
            float height,
            LilyPath.Brush brush,
            VelcroPhysics.Dynamics.BodyType bodyType
            )
        {
            var result = _ecsContainer.CreateNewEntity();
            result.AddComponent(new TransformComponent()
            {
                Position = position
            });
            var lilyPathComponent = new LilyPathComponent()
            {
                Brush = brush,
                Height = height,
                Width = width,
                LilyPathDrawType = LilyPathDrawType.FillRectangleCentered
            };
            result.AddComponent(lilyPathComponent);
            var body = VelcroPhysics.Factories.BodyFactory.CreateRectangle(
                    _world,
                    VelcroPhysics.Utilities.ConvertUnits.ToSimUnits(lilyPathComponent.Width),
                    VelcroPhysics.Utilities.ConvertUnits.ToSimUnits(lilyPathComponent.Height),
                    1f,
                    VelcroPhysics.Utilities.ConvertUnits.ToSimUnits(position),
                    0,
                    bodyType);
            var velcroPhysicsComponent = new VelcroPhysicsComponent()
            {
                Body = body
            };
            //velcroPhysicsComponent.Body.LinearDamping = 0.9f;
            //velcroPhysicsComponent.Body.AngularDamping = 0.9f;
            result.AddComponent(velcroPhysicsComponent);
            result.AddComponent(_penumbraLightSystem.CreateHulls(64, velcroPhysicsComponent));
            result.AddComponent(lilyPathComponent);
            return (result, body);
        }
    }
}