using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace RayTracing
{
    class PointManager
    {
        public static void RotateX(ref Vector3 point, float angle)
        {
            Vector4 point4 = new Vector4(point, 1.0f);
            point4 *= Matrix4.CreateRotationX(MathHelper.DegreesToRadians(angle));
            point = point4.Xyz;
        }
        public static void RotateY(ref Vector3 point, float angle)
        {
            Vector4 point4 = new Vector4(point, 1.0f);
            point4 *= Matrix4.CreateRotationY(MathHelper.DegreesToRadians(angle));
            point = point4.Xyz;
        }
        public static void RotateZ(ref Vector3 point, float angle)
        {
            Vector4 point4 = new Vector4(point, 1.0f);
            point4 *= Matrix4.CreateRotationX(MathHelper.DegreesToRadians(angle));
            point = point4.Xyz;
        }
        public static void RotateAxis(Vector3 axis, ref Vector3 point, float angle)
        {
            Vector4 point4 = new Vector4(point, 1.0f);
            point4 *= Matrix4.CreateFromAxisAngle(axis, angle);
            point = point4.Xyz;
        }
        public static void Translation(ref Vector3 point, Vector3 offset)
        {
            point += offset;
        }
        public static void Scale(ref Vector3 point, float factor)
        {
            point *= factor;
        }
    }
    class Game : GameWindow
    {
        private int BasicProgramID;
        private int BasicVertexShader;
        private int BasicFragmentShader;
        private int vbo_position;
        private int vao_position;
        private int camPosLocation;
        Vector3 cameraPos;

        public Game(string title = "RT", int width = 1280, int height = 720) : base(GameWindowSettings.Default, new NativeWindowSettings()
        {
            Title = title,
            Size = new Vector2i(width, height),
            WindowBorder = WindowBorder.Resizable,
            StartVisible = false,
            StartFocused = true,
            API = ContextAPI.OpenGL,
            Profile = ContextProfile.Core,
            APIVersion = new Version(3, 3)
        })
        {
            this.CenterWindow();
            cameraPos = new Vector3(0.0f, 1.0f, -8.0f);
        }

        private void loadShader(String filename, ShaderType type, int program, out int address)
        {
            address = GL.CreateShader(type);
            GL.ShaderSource(address, File.ReadAllText(filename));
            GL.CompileShader(address);
            GL.AttachShader(program, address);
            Console.WriteLine(GL.GetShaderInfoLog(address));
        }

        protected override void OnLoad()
        {
            IsVisible = true;
            BasicProgramID = GL.CreateProgram(); // создание объекта программы
            loadShader("../../../raytracing.vert", ShaderType.VertexShader, BasicProgramID,
            out BasicVertexShader);
            loadShader("../../../raytracing.frag", ShaderType.FragmentShader, BasicProgramID,
            out BasicFragmentShader);
            GL.LinkProgram(BasicProgramID);
            // Проверяем успех компоновки
            int status = 0;
            GL.GetProgram(BasicProgramID, GetProgramParameterName.LinkStatus, out status);
            Console.WriteLine(GL.GetProgramInfoLog(BasicProgramID));
            camPosLocation = GL.GetUniformLocation(BasicProgramID, "camPos");
            Vector3[] vertdata = new Vector3[] {
                new Vector3(-1f, -1f, 0f),
                new Vector3( 1f, -1f, 0f),
                new Vector3( 1f, 1f, 0f),
                new Vector3(-1f, 1f, 0f),
                new Vector3(-1f,-1f, 0f),
                new Vector3( 1f, 1f, 0f)};
            GL.GenBuffers(1, out vbo_position);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo_position);

            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(vertdata.Length *
             Vector3.SizeInBytes), vertdata, BufferUsageHint.StaticDraw);
            GL.GenVertexArrays(1, out vao_position);
            GL.BindVertexArray(vao_position);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo_position);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 0, 0);
            GL.EnableVertexAttribArray(0);
            base.OnLoad();
        }

        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            var input = KeyboardState;
            if (input.IsKeyDown(Keys.Equal))
            {
                PointManager.Translation(ref cameraPos, new Vector3(0.0f, (float)args.Time, 0.0f));
            }
            if (input.IsKeyDown(Keys.Minus))
            {
                PointManager.Translation(ref cameraPos, new Vector3(0.0f, -(float)args.Time, 0.0f));
            }
            if (input.IsKeyDown(Keys.W))
            {
                PointManager.Scale(ref cameraPos, 1.0f - (float)args.Time);
            }
            if (input.IsKeyDown(Keys.S))
            {
                PointManager.Scale(ref cameraPos, 1.0f + (float)args.Time);
            }
            if (input.IsKeyDown(Keys.A))
            {
                PointManager.RotateY(ref cameraPos, 60 * (float)args.Time);
            }
            if (input.IsKeyDown(Keys.D))
            {
                PointManager.RotateY(ref cameraPos, -60 * (float)args.Time);
            }
            base.OnUpdateFrame(args);
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit);
            GL.UseProgram(BasicProgramID);
            GL.Uniform3(camPosLocation, cameraPos);
            GL.BindVertexArray(vao_position);
            GL.DrawArrays(PrimitiveType.Triangles, 0, 6);
            Context.SwapBuffers();
            base.OnRenderFrame(args);
        }
    }
}
