using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Penumbra;
using System;
using System.Collections.Generic;
using Undine.Core;
using Undine.LeopotamEcs;
using Undine.LilyPath;
using Undine.MonoGame;
using Undine.MonoGame.Penumbra;
using Undine.VelcroPhysics.MonoGame;
using VelcroPhysics.Utilities;

namespace Racing
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private PenumbraHullsSystem _penumbraLightSystem;
        private ISystem _lilyPathSystem;
        private VelcroPhysicsSystem _velcroPhysicsSystem;
        private WheelTurningSystem _wheelTurningSystem;
        private CarSteeringSystem _carSteering;

        private Dictionary<int, int> draws = new Dictionary<int, int>();
        private Dictionary<int, int> updates = new Dictionary<int, int>();

        private (IUnifiedEntity, VelcroPhysics.Dynamics.Body)[] _wheels;// = new Entity[4];
        private VelcroPhysics.Dynamics.Body _carBody;
        private EcsContainer _ecsContainer;

        public Game1()
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
            System.Threading.Thread.CurrentThread.CurrentUICulture = System.Globalization.CultureInfo.InvariantCulture;
            _graphics = new GraphicsDeviceManager(this);
            _graphics.PreferredBackBufferWidth = 1024;  // set this value to the desired width of your window
            _graphics.PreferredBackBufferHeight = 768;   // set this value to the desired height of your window
            //_graphics.SynchronizeWithVerticalRetrace = false;
            //IsFixedTimeStep = false;

            _graphics.ApplyChanges();

            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            var _screenCenter = new Vector2(_graphics.GraphicsDevice.Viewport.Width / 2f, _graphics.GraphicsDevice.Viewport.Height / 2f);
            float meterInPixels = 64;
            ConvertUnits.SetDisplayUnitToSimUnitRatio(meterInPixels);

            //_ecsContainer = new DefaultEcsContainer();
            //_ecsContainer = new MinEcsContainer();
            _ecsContainer = new LeopotamEcsContainer();

            _ecsContainer.AddSystem(new RotationFromJointSystem());
            _wheelTurningSystem = new WheelTurningSystem();
            _ecsContainer.AddSystem(_wheelTurningSystem);
            _ecsContainer.AddSystem(new KeyboardSystem());
            _carSteering = new CarSteeringSystem();
            _ecsContainer.AddSystem(_carSteering);
            _ecsContainer.AddSystem(new UpdateFrictionSystem());
            _ecsContainer.AddSystem(new LightPositionSystem());
            _velcroPhysicsSystem = new VelcroPhysicsSystem();
            _ecsContainer.AddSystem(_velcroPhysicsSystem);
            _ecsContainer.AddSystem(new VelcroPhysicsTransformSystem());
            _ecsContainer.AddSystem(new HullSystem());
            _penumbraLightSystem = new PenumbraHullsSystem(this, Color.Gray);
            _lilyPathSystem = _ecsContainer.GetSystem(new LilyPathSystem(GraphicsDevice));
            _ecsContainer.Init();
            var physicsEntity = _ecsContainer.CreateNewEntity();
            var physicsWorld = new VelcroPhysics.Dynamics.World(Vector2.Zero);
            physicsEntity.AddComponent(new VelcroWorldComponent()
            {
                World = physicsWorld
            });

            var factory = new RacingFactory(
                _ecsContainer,
                _penumbraLightSystem,
                physicsWorld);
            var ground = factory.GetPhysicalRectangle(_screenCenter, 64, 64, LilyPath.Brush.Red, VelcroPhysics.Dynamics.BodyType.Dynamic);
            var car = factory.GetPhysicalRectangle(_screenCenter / 2f, 64, 64, LilyPath.Brush.DarkBlue, VelcroPhysics.Dynamics.BodyType.Dynamic);

            float wheelWidth = 16;
            float wheelHeight = 32;
            float distanceFromMiddle = 0.5f;
            var vectors = new Vector2[4]
            {
                - Vector2.One * distanceFromMiddle,
                Vector2.One * distanceFromMiddle,
                new Vector2(-distanceFromMiddle, distanceFromMiddle),
                new Vector2(distanceFromMiddle, -distanceFromMiddle)
            };
            _wheels = new (IUnifiedEntity, VelcroPhysics.Dynamics.Body)[4];
            for (int i = 0; i < _wheels.Length; i++)
            {
                _wheels[i] = factory.GetPhysicalRectangle(_screenCenter / 2f + vectors[i],
                    wheelWidth, wheelHeight, LilyPath.Brush.Black,
                    VelcroPhysics.Dynamics.BodyType.Dynamic);
            }
            _carBody = car.Item2;
            for (int i = 0; i < _wheels.Length; i++)
            {
                var wheelBody = _wheels[i].Item2;
                var joint = new VelcroPhysics.Dynamics.Joints.RevoluteJoint(_carBody, wheelBody, vectors[i], Vector2.Zero, false);
                physicsWorld.AddJoint(joint);
                _wheels[i].Item1.AddComponent(new JointComponent() { Joint = joint });
                _wheels[i].Item1.AddComponent(new UpdateFrictionComponent()
                {
                    CurrentDrag = 1
                });
            }

            var carKB = new KeyboardComponent()
            {
                Bindings = new List<KeyBinding>()
            };
            carKB.Bindings.Add(new KeyBinding() { Key = Microsoft.Xna.Framework.Input.Keys.W, KeyAction = "Forward" });
            carKB.Bindings.Add(new KeyBinding() { Key = Microsoft.Xna.Framework.Input.Keys.S, KeyAction = "Back" });
            car.Item1.AddComponent(carKB);

            var frontWheelKBC = new KeyboardComponent()
            {
                Bindings = new List<KeyBinding>()
            };
            frontWheelKBC.Bindings.Add(new KeyBinding() { Key = Microsoft.Xna.Framework.Input.Keys.A, KeyAction = "Left" });
            frontWheelKBC.Bindings.Add(new KeyBinding() { Key = Microsoft.Xna.Framework.Input.Keys.D, KeyAction = "Right" });
            //frontWheelKBC.Bindings.Add(new KeyBinding() { Key = Microsoft.Xna.Framework.Input.Keys.W, KeyAction = KeyAction.Forward });
            _wheels[0].Item1.AddComponent(frontWheelKBC);
            var wc1 = new WheelComponent()
            {
                MaxTurn = 0.85f,
                WheelRotation = 0,
                WheelRotatingSpeed = 2,
            };
            _wheels[0].Item1.AddComponent(wc1);

            frontWheelKBC = new KeyboardComponent()
            {
                Bindings = new List<KeyBinding>()
            };
            frontWheelKBC.Bindings.Add(new KeyBinding() { Key = Microsoft.Xna.Framework.Input.Keys.A, KeyAction = "Left" });
            frontWheelKBC.Bindings.Add(new KeyBinding() { Key = Microsoft.Xna.Framework.Input.Keys.D, KeyAction = "Right" });
            //frontWheelKBC.Bindings.Add(new KeyBinding() { Key = Microsoft.Xna.Framework.Input.Keys.W, KeyAction = KeyAction.Forward });
            _wheels[3].Item1.AddComponent(frontWheelKBC);
            var wc3 = new WheelComponent()
            {
                MaxTurn = 0.85f,
                WheelRotation = 0,
                WheelRotatingSpeed = 2,
            };
            _wheels[3].Item1.AddComponent(wc3);

            var lightSource = _ecsContainer.CreateNewEntity();
            lightSource.AddComponent(new TransformComponent() { Position = Vector2.Zero });
            //lightSource.AddComponent(new HarmonicComponent()
            //{
            //    Entity = lightSource,
            //    HarmonicType = HarmonicType.Rotating,
            //    Origin = new Vector2(512, 384),
            //    //Offset = new Vector2(384, 384),
            //    Range = new Vector2(384, 384)
            //});
            lightSource.AddComponent(new PenumbraLightComponent()
            {
                Light = new PointLight
                {
                    Position = new Vector2(1, 1),
                    Color = new Color(255, 255, 255),
                    Scale = new Vector2(1300),
                    ShadowType = ShadowType.Occluded
                }
            });
            _penumbraLightSystem.Penumbra.Lights.Add(
                new PointLight
                {
                    Position = new Vector2(1, 1),
                    Color = new Color(255, 255, 255),
                    Scale = new Vector2(1300),
                    ShadowType = ShadowType.Occluded
                });

            base.Initialize();
        }
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            _spriteBatch = new SpriteBatch(GraphicsDevice);
        }
        protected override void Update(GameTime gameTime)
        {
            var gt = (int)gameTime.TotalGameTime.TotalSeconds;
            if (!updates.ContainsKey(gt))
            {
                if (gt > 0)
                {
                    Console.WriteLine("Updates: " + updates[gt - 1]);
                }
                updates.Add(gt, 0);
            }
            updates[gt]++;

            //foreach (var system in _updateables)
            //    system.Update(gameTime);

            //weightTransfer = _oldSpeed - _carBody.LinearVelocity;
            //acceleration = _carBody.LinearVelocity - _oldSpeed;
            //_oldSpeed = _carBody.LinearVelocity;

            _velcroPhysicsSystem.ElapsedGameTimeTotalSeconds = (float)gameTime.ElapsedGameTime.TotalSeconds;
            _wheelTurningSystem.GameTime = gameTime;
            _carSteering.GameTime = gameTime;
            _ecsContainer.Run();

            base.Update(gameTime);
        }
        protected override void Draw(GameTime gameTime)
        {
            var gt = (int)gameTime.TotalGameTime.TotalSeconds;
            if (!draws.ContainsKey(gt))
            {
                if (gt > 0)
                {
                    Console.WriteLine("Draws: " + draws[gt - 1]);
                }
                draws.Add(gt, 0);
            }
            draws[gt]++;

            GraphicsDevice.Clear(Color.CornflowerBlue);
            _penumbraLightSystem.PenumbraBeginDraw();
            GraphicsDevice.Clear(Color.LightGray);

            //_lilyPathSystem._drawBatch.FillCircle(LilyPath.Brush.Red,
            //    VelcroPhysics.Utilities.ConvertUnits.ToDisplayUnits(weightTransfer * 10f + _carBody.Position), 10);
            //_lilyPathSystem._drawBatch.FillCircle(LilyPath.Brush.Green,
            //    VelcroPhysics.Utilities.ConvertUnits.ToDisplayUnits(acceleration * 10f + _carBody.Position), 10);
            _lilyPathSystem.ProcessAll();
            _penumbraLightSystem.PenumbraDraw(gameTime);
            base.Draw(gameTime);
        }
    }
}