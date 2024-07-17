using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Mathematics;

namespace DynamicBatching;

unsafe class Application
{
	private int _debugOutputID = -1;

	private void DebugOutput(DebugSource source, DebugType type, int id, DebugSeverity severity, int length, nint message, nint userParam)
	{
		if (id == _debugOutputID) return;
		if (severity is not (DebugSeverity.DebugSeverityHigh or DebugSeverity.DebugSeverityMedium)) return;

		_debugOutputID = id;

		Console.ForegroundColor = severity is DebugSeverity.DebugSeverityHigh ? ConsoleColor.Red : ConsoleColor.Yellow;
		Console.WriteLine(System.Text.Encoding.Default.GetString((byte*)message, length));
		Console.ResetColor();
	}

	public void RunWithoutBatching()
	{
		GLFW.Init();

		GLFW.WindowHint(WindowHintOpenGlProfile.OpenGlProfile, OpenGlProfile.Core);
		GLFW.WindowHint(WindowHintInt.ContextVersionMajor, 4);
		GLFW.WindowHint(WindowHintInt.ContextVersionMinor, 3);
		GLFW.WindowHint(WindowHintBool.Resizable, false);
		GLFW.WindowHint(WindowHintInt.Samples, 4);

		var window = GLFW.CreateWindow(1200, 1000, "Dynamic Batching", null, null);
		GLFW.MakeContextCurrent(window);

		GL.LoadBindings(new GLFWBindingsContext());
		GL.Enable(EnableCap.DebugOutput);
		GL.DebugMessageCallback(DebugOutput, 0);

		var shader = new Shader("""
			#version 330 core

			layout (location = 0) in vec2 a_position;
			uniform mat4 u_mvp;

			void main()
			{
				gl_Position = vec4(a_position, 0.0, 1.0) * u_mvp;
			}
			""", """
			#version 330 core
			
			out vec4 f_color;
			
			void main()
			{
				f_color = vec4(1.0, 0.0, 0.0, 1.0);
			}
			""");

		int xCount = 0;
		int yCount = 0;

		float size = 0.01f;
		float gap = 0.012f;

		bool _canGrow = true;

		int vao = GL.GenVertexArray();
		GL.BindVertexArray(vao);

		int vbo = GL.GenBuffer();
		GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
		GL.BufferData(BufferTarget.ArrayBuffer, 4 * 2 * 4, [-0.5f, -0.5f, -0.5f, 0.5f, 0.5f, 0.5f, 0.5f, -0.5f], BufferUsageHint.StaticDraw);

		int ibo = GL.GenBuffer();
		GL.BindBuffer(BufferTarget.ElementArrayBuffer, ibo);
		GL.BufferData(BufferTarget.ElementArrayBuffer, 4 * 6, [0, 1, 2, 3, 0, 2], BufferUsageHint.StaticDraw);

		GL.EnableVertexAttribArray(0);
		GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, sizeof(float) * 2, 0);

		while (!GLFW.WindowShouldClose(window))
		{
			GL.ClearColor(0f, 0f, 0f, 1f);
			GL.Clear(ClearBufferMask.ColorBufferBit);

			shader.Bind();

			for (int x = -(xCount / 2); x < xCount / 2; x++)
			{
				for (int y = -(yCount / 2); y < yCount / 2; y++)
				{
					GL.BindVertexArray(vao);

					GLFW.GetWindowSize(window, out int width, out int height);
					float scale = 2f;

					var mvp = Matrix4.Identity;
					mvp *= Matrix4.CreateScale(size);
					mvp *= Matrix4.CreateTranslation(new Vector3(x, y, 0f) * gap);
					mvp *= Matrix4.CreateOrthographic((float)width / height * scale, scale, 0f, 1f);

					shader.SetUniform("u_mvp", mvp);
					GL.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, 0);
				}
			}

			GLFW.SwapBuffers(window);
			GLFW.PollEvents();

			if (GLFW.GetKey(window, Keys.Space) == InputAction.Press && _canGrow)
			{
				_canGrow = false;

				xCount += 2;
				yCount += 2;
			}
			else if (GLFW.GetKey(window, Keys.Space) == InputAction.Release && !_canGrow)
			{
				_canGrow = true;
			}
		}

		shader.Dispose();

		GL.DeleteVertexArray(vao);
		GL.DeleteBuffer(vbo);
		GL.DeleteBuffer(ibo);

		GLFW.Terminate();
	}

	public void RunWithBatching()
	{
		GLFW.Init();

		GLFW.WindowHint(WindowHintOpenGlProfile.OpenGlProfile, OpenGlProfile.Core);
		GLFW.WindowHint(WindowHintInt.ContextVersionMajor, 4);
		GLFW.WindowHint(WindowHintInt.ContextVersionMinor, 3);
		GLFW.WindowHint(WindowHintBool.Resizable, false);
		GLFW.WindowHint(WindowHintInt.Samples, 4);

		var window = GLFW.CreateWindow(1200, 1000, "Dynamic Batching", null, null);
		GLFW.MakeContextCurrent(window);

		GL.LoadBindings(new GLFWBindingsContext());
		GL.Enable(EnableCap.DebugOutput);
		GL.DebugMessageCallback(DebugOutput, 0);

		var shader = new Shader("""
			#version 330 core

			layout (location = 0) in vec2 a_position;
			uniform mat4 u_mvp;

			void main()
			{
				gl_Position = vec4(a_position, 0.0, 1.0) * u_mvp;
			}
			""", """
			#version 330 core
			
			out vec4 f_color;
			
			void main()
			{
				f_color = vec4(1.0, 0.0, 0.0, 1.0);
			}
			""");
		var batch = new Batch();
		batch.OnBufferCapacityChange += (vSize, iSize) =>
		{
			Console.WriteLine($"Vertex Size: {vSize / 1024f:0.00}KB, Index Size: {iSize / 1024f:0.00}KB");
		};

		var builder = MeshBuilder.Create();

		int xCount = 0;
		int yCount = 0;

		float size = 0.01f;
		float gap = 0.012f;

		bool _canGrow = true;

		while (!GLFW.WindowShouldClose(window))
		{
			GL.ClearColor(0f, 0f, 0f, 1f);
			GL.Clear(ClearBufferMask.ColorBufferBit);

			shader.Bind();
			batch.Bind();

			if (batch.IndexCount > 0)
			{
				GLFW.GetWindowSize(window, out int width, out int height);
				float scale = 2f;

				shader.SetUniform("u_mvp", Matrix4.CreateOrthographic((float)width / height * scale, scale, 0f, 1f));
				GL.DrawElements(PrimitiveType.Triangles, batch.IndexCount, DrawElementsType.UnsignedInt, 0);
			}

			GLFW.SwapBuffers(window);
			GLFW.PollEvents();

			if (GLFW.GetKey(window, Keys.Space) == InputAction.Press && _canGrow)
			{
				_canGrow = false;

				xCount += 2;
				yCount += 2;

				for (int x = -(xCount / 2); x < xCount / 2; x++)
				{
					for (int y = -(yCount / 2); y < yCount / 2; y++)
					{
						builder.Quad(new Vector2(x, y) * gap, new(size));
					}
				}

				builder.Build(out var vertices, out var indices);
				batch.Update(vertices, indices);
			}
			else if (GLFW.GetKey(window, Keys.Space) == InputAction.Release && !_canGrow)
			{
				_canGrow = true;
			}
		}

		shader.Dispose();
		batch.Dispose();

		GLFW.Terminate();
	}
}
