﻿using System;
using LeagueSharp.CommonEx.Core.Extensions.SharpDX;
using SharpDX;
using SharpDX.Direct3D9;

namespace LeagueSharp.CommonEx.Core.Render.RenderObjects
{
    /// <summary>
    ///     RenderObject class used to render a Circle
    /// </summary>
    public class Circle : Render.RenderObject
    {
        private static VertexBuffer _vertices;
        private static VertexElement[] _vertexElements;
        private static VertexDeclaration _vertexDeclaration;
        private static Effect _effect;
        private static EffectHandle _technique;
        private static bool _initialized;
        private static Vector3 _offset = new Vector3(0, 0, 0);

        /// <summary>
        ///     Draws a Circle
        /// </summary>
        /// <param name="unit">GameObject Position</param>
        /// <param name="radius">Radius of the Circle</param>
        /// <param name="color">Color of the circle</param>
        /// <param name="width">Width of the circle</param>
        /// <param name="zDeep">Value of zDeep</param>
        public Circle(GameObject unit, float radius, Color color, int width = 1, bool zDeep = false)
        {
            Color = color;
            Unit = unit;
            Radius = radius;
            Width = width;
            ZDeep = zDeep;
        }

        /// <summary>
        ///     Draws a Circle
        /// </summary>
        /// <param name="unit">GameObject Position</param>
        /// <param name="offset">Vector3 Offset</param>
        /// <param name="radius">Radius of the Circle</param>
        /// <param name="color">Color of the circle</param>
        /// <param name="width">Width of the circle</param>
        /// <param name="zDeep">Value of zDeep</param>
        public Circle(GameObject unit, Vector3 offset, float radius, Color color, int width = 1, bool zDeep = false)
        {
            Color = color;
            Unit = unit;
            Radius = radius;
            Width = width;
            ZDeep = zDeep;
            Offset = offset;
        }

        /// <summary>
        ///     Draws a Circle
        /// </summary>
        /// <param name="position">Position of the Circle as Vector3</param>
        /// <param name="offset">Offset of the Circle as Vector3</param>
        /// <param name="radius">Radius of radius</param>
        /// <param name="color">Color of the Circle</param>
        /// <param name="width">Width of the Circle</param>
        /// <param name="zDeep">Value of zDeep</param>
        public Circle(Vector3 position, Vector3 offset, float radius, Color color, int width = 1, bool zDeep = false)
        {
            Color = color;
            Position = position;
            Radius = radius;
            Width = width;
            ZDeep = zDeep;
            Offset = offset;
        }

        /// <summary>
        ///     Draws a Circle
        /// </summary>
        /// <param name="position">Position of the Circle as Vector3</param>
        /// <param name="radius">Radius of radius</param>
        /// <param name="color">Color of the Circle</param>
        /// <param name="width">Width of the Circle</param>
        /// <param name="zDeep">Value of zDeep</param>
        public Circle(Vector3 position, float radius, Color color, int width = 1, bool zDeep = false)
        {
            Color = color;
            Position = position;
            Radius = radius;
            Width = width;
            ZDeep = zDeep;
        }

        /// <summary>
        ///     Position of Circle as Vector3
        /// </summary>
        public Vector3 Position { get; set; }

        /// <summary>
        ///     Unit of Circle
        /// </summary>
        public GameObject Unit { get; set; }

        /// <summary>
        ///     Radius of Circle
        /// </summary>
        public float Radius { get; set; }

        /// <summary>
        ///     Color of Circle
        /// </summary>
        public Color Color { get; set; }

        /// <summary>
        ///     Width of Circle
        /// </summary>
        public int Width { get; set; }

        /// <summary>
        ///     Boolean ZDeep
        /// </summary>
        public bool ZDeep { get; set; }

        /// <summary>
        ///     Circle Offset
        /// </summary>
        public Vector3 Offset
        {
            get { return _offset; }
            set { _offset = value; }
        }

        /// <summary>
        ///     Direct3DDevice
        /// </summary>
        public static Device Device
        {
            get { return Drawing.Direct3DDevice; }
        }

        /// <summary>
        ///     OnDrawEvent
        /// </summary>
        public override void OnDraw()
        {
            try
            {
                if (Unit != null && Unit.IsValid)
                {
                    DrawCircle(Unit.Position + _offset, Radius, Color, Width, ZDeep);
                }
                else if (Vector3Extensions.ToVector2(Position + _offset).IsValid())
                {
                    DrawCircle(Position + _offset, Radius, Color, Width, ZDeep);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(@"Common.Render.Circle.OnEndScene: " + e);
            }
        }

        /// <summary>
        ///     Creates the Vertexes
        /// </summary>
        public static void CreateVertexes()
        {
            const float x = 6000f;
            _vertices = new VertexBuffer(
                Device, Utilities.SizeOf<Vector4>() * 2 * 6, Usage.WriteOnly, VertexFormat.None, Pool.Managed);

            _vertices.Lock(0, 0, LockFlags.None).WriteRange(
                new[]
                {
                    //T1
                    new Vector4(-x, 0f, -x, 1.0f), new Vector4(), new Vector4(-x, 0f, x, 1.0f), new Vector4(),
                    new Vector4(x, 0f, -x, 1.0f), new Vector4(),

                    //T2
                    new Vector4(x, 0f, x, 1.0f), new Vector4(), new Vector4(-x, 0f, x, 1.0f), new Vector4(),
                    new Vector4(x, 0f, -x, 1.0f), new Vector4()
                });
            _vertices.Unlock();

            _vertexElements = new[]
            {
                new VertexElement(
                    0, 0, DeclarationType.Float4, DeclarationMethod.Default, DeclarationUsage.Position, 0),
                new VertexElement(
                    0, 16, DeclarationType.Float4, DeclarationMethod.Default, DeclarationUsage.Color, 0),
                VertexElement.VertexDeclarationEnd
            };

            _vertexDeclaration = new VertexDeclaration(Device, _vertexElements);

            #region Effect

            /*  Source:
             *      _effect = Effect.FromString(Device, @"
                    struct VS_S
                     {
                         float4 Position : POSITION;
                         float4 Color : COLOR0;
                         float4 Position3D : TEXCOORD0;
                     };
                     float4x4 ProjectionMatrix;
                     float4 CircleColor;
                     float Radius;
                     float Border;
                     bool zEnabled;
                     VS_S VS( VS_S input )
                     {
                         VS_S output = (VS_S)0;
	
                         output.Position = mul(input.Position, ProjectionMatrix);
                         output.Color = input.Color;
                         output.Position3D = input.Position;
                         return output;
                     }
                     float4 PS( VS_S input ) : COLOR
                     {
                         VS_S output = (VS_S)0;
                         output = input;
                         float4 v = output.Position3D; 
                         float distance = Radius - sqrt(v.x * v.x + v.z*v.z); // Distance to the circle arc.
    
                         output.Color.x = CircleColor.x;
                         output.Color.y = CircleColor.y;
                         output.Color.z = CircleColor.z;
                            
                         if(distance < Border && distance > -Border)
                         {
                             output.Color.w = (CircleColor.w - CircleColor.w * abs(distance * 1.75 / Border));
                         }
                         else
                         {
                             output.Color.w = 0;
                         }
                            
                         if(Border < 1 && distance >= 0)
                         {
                             output.Color.w = CircleColor.w;
                         }
                         return output.Color;
                     }
                     technique Main {
                         pass P0 {
                             ZEnable = zEnabled;
                             AlphaBlendEnable = TRUE;
                             DestBlend = INVSRCALPHA;
                             SrcBlend = SRCALPHA;
                             VertexShader = compile vs_2_0 VS();
                             PixelShader  = compile ps_2_0 PS();
                         }
                     }", ShaderFlags.None);
             * */

            try
            {
                var compiledEffect = new byte[]
                {
                    0x01, 0x09, 0xFF, 0xFE, 0x00, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x03, 0x00, 0x00, 0x00,
                    0x02, 0x00, 0x00, 0x00, 0x60, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                    0x04, 0x00, 0x00, 0x00, 0x04, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x11, 0x00, 0x00, 0x00, 0x50, 0x72, 0x6F, 0x6A,
                    0x65, 0x63, 0x74, 0x69, 0x6F, 0x6E, 0x4D, 0x61, 0x74, 0x72, 0x69, 0x78, 0x00, 0x00, 0x00, 0x00,
                    0x03, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0xA4, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                    0x00, 0x00, 0x00, 0x00, 0x04, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x0C, 0x00, 0x00, 0x00,
                    0x43, 0x69, 0x72, 0x63, 0x6C, 0x65, 0x43, 0x6F, 0x6C, 0x6F, 0x72, 0x00, 0x03, 0x00, 0x00, 0x00,
                    0x00, 0x00, 0x00, 0x00, 0xD4, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                    0x01, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x07, 0x00, 0x00, 0x00,
                    0x52, 0x61, 0x64, 0x69, 0x75, 0x73, 0x00, 0x00, 0x03, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                    0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00,
                    0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x07, 0x00, 0x00, 0x00, 0x42, 0x6F, 0x72, 0x64,
                    0x65, 0x72, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x2C, 0x01, 0x00, 0x00,
                    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00,
                    0x00, 0x00, 0x00, 0x00, 0x09, 0x00, 0x00, 0x00, 0x7A, 0x45, 0x6E, 0x61, 0x62, 0x6C, 0x65, 0x64,
                    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00,
                    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00,
                    0x01, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00,
                    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00,
                    0x01, 0x00, 0x00, 0x00, 0x06, 0x00, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00,
                    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00,
                    0x01, 0x00, 0x00, 0x00, 0x05, 0x00, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00,
                    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00,
                    0x01, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x10, 0x00, 0x00, 0x00, 0x04, 0x00, 0x00, 0x00,
                    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00,
                    0x0F, 0x00, 0x00, 0x00, 0x04, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                    0x00, 0x00, 0x00, 0x00, 0x03, 0x00, 0x00, 0x00, 0x50, 0x30, 0x00, 0x00, 0x05, 0x00, 0x00, 0x00,
                    0x4D, 0x61, 0x69, 0x6E, 0x00, 0x00, 0x00, 0x00, 0x05, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00,
                    0x03, 0x00, 0x00, 0x00, 0x03, 0x00, 0x00, 0x00, 0x04, 0x00, 0x00, 0x00, 0x20, 0x00, 0x00, 0x00,
                    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x78, 0x00, 0x00, 0x00, 0x94, 0x00, 0x00, 0x00,
                    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xB4, 0x00, 0x00, 0x00, 0xD0, 0x00, 0x00, 0x00,
                    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xE0, 0x00, 0x00, 0x00, 0xFC, 0x00, 0x00, 0x00,
                    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x0C, 0x01, 0x00, 0x00, 0x28, 0x01, 0x00, 0x00,
                    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xF4, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                    0x01, 0x00, 0x00, 0x00, 0xEC, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x06, 0x00, 0x00, 0x00,
                    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x40, 0x01, 0x00, 0x00, 0x3C, 0x01, 0x00, 0x00,
                    0x0D, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x60, 0x01, 0x00, 0x00, 0x5C, 0x01, 0x00, 0x00,
                    0x07, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x80, 0x01, 0x00, 0x00, 0x7C, 0x01, 0x00, 0x00,
                    0x06, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xA0, 0x01, 0x00, 0x00, 0x9C, 0x01, 0x00, 0x00,
                    0x92, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xC0, 0x01, 0x00, 0x00, 0xBC, 0x01, 0x00, 0x00,
                    0x93, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xD8, 0x01, 0x00, 0x00, 0xD4, 0x01, 0x00, 0x00,
                    0x00, 0x00, 0x00, 0x00, 0x03, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                    0xFF, 0xFF, 0xFF, 0xFF, 0x05, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x4C, 0x04, 0x00, 0x00,
                    0x00, 0x02, 0xFF, 0xFF, 0xFE, 0xFF, 0x38, 0x00, 0x43, 0x54, 0x41, 0x42, 0x1C, 0x00, 0x00, 0x00,
                    0xAA, 0x00, 0x00, 0x00, 0x00, 0x02, 0xFF, 0xFF, 0x03, 0x00, 0x00, 0x00, 0x1C, 0x00, 0x00, 0x00,
                    0x00, 0x00, 0x00, 0x20, 0xA3, 0x00, 0x00, 0x00, 0x58, 0x00, 0x00, 0x00, 0x02, 0x00, 0x05, 0x00,
                    0x01, 0x00, 0x00, 0x00, 0x60, 0x00, 0x00, 0x00, 0x70, 0x00, 0x00, 0x00, 0x80, 0x00, 0x00, 0x00,
                    0x02, 0x00, 0x03, 0x00, 0x01, 0x00, 0x00, 0x00, 0x8C, 0x00, 0x00, 0x00, 0x70, 0x00, 0x00, 0x00,
                    0x9C, 0x00, 0x00, 0x00, 0x02, 0x00, 0x04, 0x00, 0x01, 0x00, 0x00, 0x00, 0x60, 0x00, 0x00, 0x00,
                    0x70, 0x00, 0x00, 0x00, 0x42, 0x6F, 0x72, 0x64, 0x65, 0x72, 0x00, 0xAB, 0x00, 0x00, 0x03, 0x00,
                    0x01, 0x00, 0x01, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x43, 0x69, 0x72, 0x63,
                    0x6C, 0x65, 0x43, 0x6F, 0x6C, 0x6F, 0x72, 0x00, 0x01, 0x00, 0x03, 0x00, 0x01, 0x00, 0x04, 0x00,
                    0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x52, 0x61, 0x64, 0x69, 0x75, 0x73, 0x00, 0x70,
                    0x73, 0x5F, 0x32, 0x5F, 0x30, 0x00, 0x4D, 0x69, 0x63, 0x72, 0x6F, 0x73, 0x6F, 0x66, 0x74, 0x20,
                    0x28, 0x52, 0x29, 0x20, 0x48, 0x4C, 0x53, 0x4C, 0x20, 0x53, 0x68, 0x61, 0x64, 0x65, 0x72, 0x20,
                    0x43, 0x6F, 0x6D, 0x70, 0x69, 0x6C, 0x65, 0x72, 0x20, 0x39, 0x2E, 0x32, 0x39, 0x2E, 0x39, 0x35,
                    0x32, 0x2E, 0x33, 0x31, 0x31, 0x31, 0x00, 0xAB, 0xFE, 0xFF, 0x7C, 0x00, 0x50, 0x52, 0x45, 0x53,
                    0x01, 0x02, 0x58, 0x46, 0xFE, 0xFF, 0x30, 0x00, 0x43, 0x54, 0x41, 0x42, 0x1C, 0x00, 0x00, 0x00,
                    0x8B, 0x00, 0x00, 0x00, 0x01, 0x02, 0x58, 0x46, 0x02, 0x00, 0x00, 0x00, 0x1C, 0x00, 0x00, 0x00,
                    0x00, 0x01, 0x00, 0x20, 0x88, 0x00, 0x00, 0x00, 0x44, 0x00, 0x00, 0x00, 0x02, 0x00, 0x01, 0x00,
                    0x01, 0x00, 0x00, 0x00, 0x4C, 0x00, 0x00, 0x00, 0x5C, 0x00, 0x00, 0x00, 0x6C, 0x00, 0x00, 0x00,
                    0x02, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x78, 0x00, 0x00, 0x00, 0x5C, 0x00, 0x00, 0x00,
                    0x42, 0x6F, 0x72, 0x64, 0x65, 0x72, 0x00, 0xAB, 0x00, 0x00, 0x03, 0x00, 0x01, 0x00, 0x01, 0x00,
                    0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x43, 0x69, 0x72, 0x63, 0x6C, 0x65, 0x43, 0x6F,
                    0x6C, 0x6F, 0x72, 0x00, 0x01, 0x00, 0x03, 0x00, 0x01, 0x00, 0x04, 0x00, 0x01, 0x00, 0x00, 0x00,
                    0x00, 0x00, 0x00, 0x00, 0x74, 0x78, 0x00, 0x4D, 0x69, 0x63, 0x72, 0x6F, 0x73, 0x6F, 0x66, 0x74,
                    0x20, 0x28, 0x52, 0x29, 0x20, 0x48, 0x4C, 0x53, 0x4C, 0x20, 0x53, 0x68, 0x61, 0x64, 0x65, 0x72,
                    0x20, 0x43, 0x6F, 0x6D, 0x70, 0x69, 0x6C, 0x65, 0x72, 0x20, 0x39, 0x2E, 0x32, 0x39, 0x2E, 0x39,
                    0x35, 0x32, 0x2E, 0x33, 0x31, 0x31, 0x31, 0x00, 0xFE, 0xFF, 0x0C, 0x00, 0x50, 0x52, 0x53, 0x49,
                    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x03, 0x00, 0x00, 0x00,
                    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                    0x03, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xFE, 0xFF, 0x1A, 0x00,
                    0x43, 0x4C, 0x49, 0x54, 0x0C, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xF0, 0xBF,
                    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xFE, 0xFF, 0x1F, 0x00, 0x46, 0x58, 0x4C, 0x43,
                    0x03, 0x00, 0x00, 0x00, 0x01, 0x00, 0x30, 0x10, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                    0x02, 0x00, 0x00, 0x00, 0x04, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x04, 0x00, 0x00, 0x00,
                    0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x40, 0xA0, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                    0x02, 0x00, 0x00, 0x00, 0x04, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00,
                    0x08, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x04, 0x00, 0x00, 0x00, 0x04, 0x00, 0x00, 0x00,
                    0x03, 0x00, 0x00, 0x10, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00,
                    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x04, 0x00, 0x00, 0x00, 0x08, 0x00, 0x00, 0x00,
                    0xF0, 0xF0, 0xF0, 0xF0, 0x0F, 0x0F, 0x0F, 0x0F, 0xFF, 0xFF, 0x00, 0x00, 0x51, 0x00, 0x00, 0x05,
                    0x06, 0x00, 0x0F, 0xA0, 0x00, 0x00, 0xE0, 0x3F, 0x00, 0x00, 0x00, 0x80, 0x00, 0x00, 0x80, 0xBF,
                    0x00, 0x00, 0x00, 0x00, 0x1F, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00, 0x80, 0x00, 0x00, 0x07, 0xB0,
                    0x05, 0x00, 0x00, 0x03, 0x00, 0x00, 0x08, 0x80, 0x00, 0x00, 0xAA, 0xB0, 0x00, 0x00, 0xAA, 0xB0,
                    0x04, 0x00, 0x00, 0x04, 0x00, 0x00, 0x01, 0x80, 0x00, 0x00, 0x00, 0xB0, 0x00, 0x00, 0x00, 0xB0,
                    0x00, 0x00, 0xFF, 0x80, 0x07, 0x00, 0x00, 0x02, 0x00, 0x00, 0x01, 0x80, 0x00, 0x00, 0x00, 0x80,
                    0x06, 0x00, 0x00, 0x02, 0x00, 0x00, 0x01, 0x80, 0x00, 0x00, 0x00, 0x80, 0x02, 0x00, 0x00, 0x03,
                    0x00, 0x00, 0x01, 0x80, 0x00, 0x00, 0x00, 0x81, 0x04, 0x00, 0x00, 0xA0, 0x02, 0x00, 0x00, 0x03,
                    0x00, 0x00, 0x02, 0x80, 0x00, 0x00, 0x00, 0x81, 0x05, 0x00, 0x00, 0xA1, 0x58, 0x00, 0x00, 0x04,
                    0x00, 0x00, 0x02, 0x80, 0x00, 0x00, 0x55, 0x80, 0x06, 0x00, 0x55, 0xA0, 0x06, 0x00, 0xAA, 0xA0,
                    0x02, 0x00, 0x00, 0x03, 0x00, 0x00, 0x04, 0x80, 0x00, 0x00, 0x00, 0x80, 0x05, 0x00, 0x00, 0xA1,
                    0x58, 0x00, 0x00, 0x04, 0x00, 0x00, 0x02, 0x80, 0x00, 0x00, 0xAA, 0x80, 0x06, 0x00, 0x55, 0xA0,
                    0x00, 0x00, 0x55, 0x80, 0x05, 0x00, 0x00, 0x03, 0x00, 0x00, 0x04, 0x80, 0x00, 0x00, 0x00, 0x80,
                    0x06, 0x00, 0x00, 0xA0, 0x58, 0x00, 0x00, 0x04, 0x00, 0x00, 0x01, 0x80, 0x00, 0x00, 0x00, 0x80,
                    0x06, 0x00, 0xAA, 0xA0, 0x06, 0x00, 0x55, 0xA0, 0x01, 0x00, 0x00, 0x02, 0x00, 0x00, 0x08, 0x80,
                    0x06, 0x00, 0x55, 0xA0, 0x58, 0x00, 0x00, 0x04, 0x00, 0x00, 0x01, 0x80, 0x01, 0x00, 0x00, 0xA0,
                    0x00, 0x00, 0xFF, 0x80, 0x00, 0x00, 0x00, 0x80, 0x05, 0x00, 0x00, 0x03, 0x00, 0x00, 0x04, 0x80,
                    0x00, 0x00, 0xAA, 0x80, 0x00, 0x00, 0x00, 0xA0, 0x23, 0x00, 0x00, 0x02, 0x00, 0x00, 0x04, 0x80,
                    0x00, 0x00, 0xAA, 0x80, 0x04, 0x00, 0x00, 0x04, 0x00, 0x00, 0x04, 0x80, 0x03, 0x00, 0xFF, 0xA0,
                    0x00, 0x00, 0xAA, 0x81, 0x03, 0x00, 0xFF, 0xA0, 0x58, 0x00, 0x00, 0x04, 0x00, 0x00, 0x02, 0x80,
                    0x00, 0x00, 0x55, 0x80, 0x06, 0x00, 0xFF, 0xA0, 0x00, 0x00, 0xAA, 0x80, 0x58, 0x00, 0x00, 0x04,
                    0x00, 0x00, 0x08, 0x80, 0x00, 0x00, 0x00, 0x80, 0x00, 0x00, 0x55, 0x80, 0x03, 0x00, 0xFF, 0xA0,
                    0x01, 0x00, 0x00, 0x02, 0x00, 0x00, 0x07, 0x80, 0x02, 0x00, 0xE4, 0xA0, 0x01, 0x00, 0x00, 0x02,
                    0x00, 0x08, 0x0F, 0x80, 0x00, 0x00, 0xE4, 0x80, 0xFF, 0xFF, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                    0x00, 0x00, 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0x04, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                    0x4C, 0x01, 0x00, 0x00, 0x00, 0x02, 0xFE, 0xFF, 0xFE, 0xFF, 0x34, 0x00, 0x43, 0x54, 0x41, 0x42,
                    0x1C, 0x00, 0x00, 0x00, 0x9B, 0x00, 0x00, 0x00, 0x00, 0x02, 0xFE, 0xFF, 0x01, 0x00, 0x00, 0x00,
                    0x1C, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x20, 0x94, 0x00, 0x00, 0x00, 0x30, 0x00, 0x00, 0x00,
                    0x02, 0x00, 0x00, 0x00, 0x04, 0x00, 0x00, 0x00, 0x44, 0x00, 0x00, 0x00, 0x54, 0x00, 0x00, 0x00,
                    0x50, 0x72, 0x6F, 0x6A, 0x65, 0x63, 0x74, 0x69, 0x6F, 0x6E, 0x4D, 0x61, 0x74, 0x72, 0x69, 0x78,
                    0x00, 0xAB, 0xAB, 0xAB, 0x03, 0x00, 0x03, 0x00, 0x04, 0x00, 0x04, 0x00, 0x01, 0x00, 0x00, 0x00,
                    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                    0x00, 0x00, 0x00, 0x00, 0x76, 0x73, 0x5F, 0x32, 0x5F, 0x30, 0x00, 0x4D, 0x69, 0x63, 0x72, 0x6F,
                    0x73, 0x6F, 0x66, 0x74, 0x20, 0x28, 0x52, 0x29, 0x20, 0x48, 0x4C, 0x53, 0x4C, 0x20, 0x53, 0x68,
                    0x61, 0x64, 0x65, 0x72, 0x20, 0x43, 0x6F, 0x6D, 0x70, 0x69, 0x6C, 0x65, 0x72, 0x20, 0x39, 0x2E,
                    0x32, 0x39, 0x2E, 0x39, 0x35, 0x32, 0x2E, 0x33, 0x31, 0x31, 0x31, 0x00, 0x1F, 0x00, 0x00, 0x02,
                    0x00, 0x00, 0x00, 0x80, 0x00, 0x00, 0x0F, 0x90, 0x1F, 0x00, 0x00, 0x02, 0x0A, 0x00, 0x00, 0x80,
                    0x01, 0x00, 0x0F, 0x90, 0x09, 0x00, 0x00, 0x03, 0x00, 0x00, 0x01, 0xC0, 0x00, 0x00, 0xE4, 0x90,
                    0x00, 0x00, 0xE4, 0xA0, 0x09, 0x00, 0x00, 0x03, 0x00, 0x00, 0x02, 0xC0, 0x00, 0x00, 0xE4, 0x90,
                    0x01, 0x00, 0xE4, 0xA0, 0x09, 0x00, 0x00, 0x03, 0x00, 0x00, 0x04, 0xC0, 0x00, 0x00, 0xE4, 0x90,
                    0x02, 0x00, 0xE4, 0xA0, 0x09, 0x00, 0x00, 0x03, 0x00, 0x00, 0x08, 0xC0, 0x00, 0x00, 0xE4, 0x90,
                    0x03, 0x00, 0xE4, 0xA0, 0x01, 0x00, 0x00, 0x02, 0x00, 0x00, 0x0F, 0xD0, 0x01, 0x00, 0xE4, 0x90,
                    0x01, 0x00, 0x00, 0x02, 0x00, 0x00, 0x0F, 0xE0, 0x00, 0x00, 0xE4, 0x90, 0xFF, 0xFF, 0x00, 0x00,
                    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0x00, 0x00, 0x00,
                    0x00, 0x00, 0x00, 0x00, 0xE0, 0x00, 0x00, 0x00, 0x00, 0x02, 0x58, 0x46, 0xFE, 0xFF, 0x25, 0x00,
                    0x43, 0x54, 0x41, 0x42, 0x1C, 0x00, 0x00, 0x00, 0x5F, 0x00, 0x00, 0x00, 0x00, 0x02, 0x58, 0x46,
                    0x01, 0x00, 0x00, 0x00, 0x1C, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x20, 0x5C, 0x00, 0x00, 0x00,
                    0x30, 0x00, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x3C, 0x00, 0x00, 0x00,
                    0x4C, 0x00, 0x00, 0x00, 0x7A, 0x45, 0x6E, 0x61, 0x62, 0x6C, 0x65, 0x64, 0x00, 0xAB, 0xAB, 0xAB,
                    0x00, 0x00, 0x01, 0x00, 0x01, 0x00, 0x01, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                    0x74, 0x78, 0x00, 0x4D, 0x69, 0x63, 0x72, 0x6F, 0x73, 0x6F, 0x66, 0x74, 0x20, 0x28, 0x52, 0x29,
                    0x20, 0x48, 0x4C, 0x53, 0x4C, 0x20, 0x53, 0x68, 0x61, 0x64, 0x65, 0x72, 0x20, 0x43, 0x6F, 0x6D,
                    0x70, 0x69, 0x6C, 0x65, 0x72, 0x20, 0x39, 0x2E, 0x32, 0x39, 0x2E, 0x39, 0x35, 0x32, 0x2E, 0x33,
                    0x31, 0x31, 0x31, 0x00, 0xFE, 0xFF, 0x02, 0x00, 0x43, 0x4C, 0x49, 0x54, 0x00, 0x00, 0x00, 0x00,
                    0xFE, 0xFF, 0x0C, 0x00, 0x46, 0x58, 0x4C, 0x43, 0x01, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x10,
                    0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                    0x00, 0x00, 0x00, 0x00, 0x04, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xF0, 0xF0, 0xF0, 0xF0,
                    0x0F, 0x0F, 0x0F, 0x0F, 0xFF, 0xFF, 0x00, 0x00
                };
                _effect = Effect.FromMemory(Device, compiledEffect, ShaderFlags.None);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return;
            }

            #endregion

            _technique = _effect.GetTechnique(0);

            if (!_initialized)
            {
                _initialized = true;
                Drawing.OnPreReset += OnPreReset;
                Drawing.OnPreReset += OnPostReset;
                AppDomain.CurrentDomain.DomainUnload += Dispose;
            }
        }

        private static void OnPreReset(EventArgs args)
        {
            if (_effect != null && !_effect.IsDisposed)
            {
                _effect.OnLostDevice();
            }
        }

        private static void OnPostReset(EventArgs args)
        {
            if (_effect != null && !_effect.IsDisposed)
            {
                _effect.OnResetDevice();
            }
        }

        private static void Dispose(object sender, EventArgs e)
        {
            if (_effect != null && !_effect.IsDisposed)
            {
                _effect.Dispose();
            }

            if (_vertices != null && !_vertices.IsDisposed)
            {
                _vertices.Dispose();
            }

            if (_vertexDeclaration != null && !_vertexDeclaration.IsDisposed)
            {
                _vertexDeclaration.Dispose();
            }
        }

        private static void DrawCircle(Vector3 position, float radius, Color color, int width = 5, bool zDeep = false)
        {
            try
            {
                if (Device == null || Device.IsDisposed)
                {
                    return;
                }

                if (_vertices == null)
                {
                    CreateVertexes();
                }

                if (_vertices == null || _vertices.IsDisposed || _vertexDeclaration.IsDisposed || _effect.IsDisposed ||
                    _technique.IsDisposed)
                {
                    return;
                }

                var olddec = Device.VertexDeclaration;

                _effect.Technique = _technique;

                _effect.Begin();
                _effect.BeginPass(0);
                _effect.SetValue(
                    "ProjectionMatrix",
                    Matrix.Translation(Vector3Extensions.SwitchYz(position)) * Drawing.View * Drawing.Projection);
                _effect.SetValue(
                    "CircleColor", new Vector4(color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255f));
                _effect.SetValue("Radius", radius);
                _effect.SetValue("Border", 2f + width);
                _effect.SetValue("zEnabled", zDeep);

                Device.SetStreamSource(0, _vertices, 0, Utilities.SizeOf<Vector4>() * 2);
                Device.VertexDeclaration = _vertexDeclaration;

                Device.DrawPrimitives(PrimitiveType.TriangleList, 0, 2);

                _effect.EndPass();
                _effect.End();

                Device.VertexDeclaration = olddec;
            }
            catch (Exception e)
            {
                _vertices = null;
                Console.WriteLine(@"DrawCircle: " + e);
            }
        }
    }
}