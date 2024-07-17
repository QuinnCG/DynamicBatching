using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace DynamicBatching;

class Shader : IDisposable
{
	public int Handle { get; }

	public Shader(string vertexShader, string fragmentShader)
	{
		int vs = CreateModule(ShaderType.VertexShader, vertexShader);
		int fs = CreateModule(ShaderType.FragmentShader, fragmentShader);

		Handle = GL.CreateProgram();
		GL.AttachShader(Handle, vs);
		GL.AttachShader(Handle, fs);
		GL.LinkProgram(Handle);
		GL.ValidateProgram(Handle);

		GL.DeleteShader(vs);
		GL.DeleteShader(fs);

		string info = GL.GetProgramInfoLog(Handle);
		if (!string.IsNullOrEmpty(info))
		{
			Console.WriteLine(info);
		}
	}

	public void Bind()
	{
		GL.UseProgram(Handle);
	}

	public void Dispose()
	{
		GL.DeleteProgram(Handle);
	}

	public void SetUniform(string name, Matrix4 matrix)
	{
		GL.UniformMatrix4(GL.GetUniformLocation(Handle, name), true, ref matrix);
	}

	private static int CreateModule(ShaderType type, string source)
	{
		int handle = GL.CreateShader(type);
		GL.ShaderSource(handle, source);
		GL.CompileShader(handle);

		string info = GL.GetShaderInfoLog(handle);
		if (!string.IsNullOrEmpty(info))
		{
			Console.WriteLine(info);
		}

		return handle;
	}
}
