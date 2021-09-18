using Microsoft.Xna.Framework;
using Penumbra;
using System;
using System.Collections.Generic;
using Undine.MonoGame.Penumbra;
using Undine.VelcroPhysics.MonoGame;
using VelcroPhysics.Collision.Shapes;
using VelcroPhysics.Dynamics;
using VelcroPhysics.Shared;
using VelcroPhysics.Utilities;

namespace Racing
{
    public class PenumbraHullsSystem : PenumbraLightSystem
    {
        public PenumbraHullsSystem(Microsoft.Xna.Framework.Game game, Microsoft.Xna.Framework.Color color) : base(game, color)
        {
        }
        private int _accuracy = 24;
        public Vertices GetVertices(Shape shape)
        {
            switch (shape)
            {
                case ChainShape chain:
                    return chain.Vertices;

                case CircleShape cirle:
                    return GetCircleVertices(cirle);

                case EdgeShape edge:
                    throw new System.NotImplementedException();

                case PolygonShape polygon:
                    return polygon.Vertices;
            }
            throw new System.NotImplementedException();
        }
        public Vertices GetCircleVertices(CircleShape cirle)
        {
            List<Vector2> vertices = new List<Vector2>();
            var x = 0f;
            var y = 0f;
            for (int i = 0; i < _accuracy; i++)
            {
                x = (float)(cirle.Position.X + cirle.Radius * Math.Sin(i * 2 * Math.PI / _accuracy));
                y = (float)(cirle.Position.X + cirle.Radius * Math.Cos(i * 2 * Math.PI / _accuracy));
                vertices.Add(new Vector2(x, y));
            }
            var result = new Vertices(vertices);
            return result;
        }
        public HullComponent CreateHulls(float meterInPixels, VelcroPhysicsComponent tBody)
        {
            HullComponent HullComponent = new HullComponent();//Modify this
            // Create Hulls from the fixtures of the body
            foreach (Fixture f in tBody.Body.FixtureList)
            {
                // Creating the Hull out of the Shape (respectively Vertices) of the fixtures of the physics body
                Hull h = new Hull(GetVertices(f.Shape));

                // We need to scale the Hull according to our "MetersInPixels-Simulation-Value"
                h.Scale = new Vector2(meterInPixels);

                // A Hull of Penumbra is set in Display space but the physics body is set in Simulation space
                // Thats why we need to convert the simulation units of the physics object to the display units
                // of the Hull object
                h.Position = ConvertUnits.ToDisplayUnits(tBody.Body.Position);

                // We are adding the new Hull to our physics body hull list
                // This is necessary to update the Hulls in the Update method (see below)
                HullComponent.Hulls = new System.Collections.Generic.List<Hull>();
                HullComponent.Hulls.Add(h);

                // Adding the Hull to Penumbra
                Penumbra.Hulls.Add(h);
            }
            return HullComponent;
        }
    }
}