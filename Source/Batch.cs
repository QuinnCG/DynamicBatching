using OpenTK.Graphics.OpenGL4;

namespace DynamicBatching;

class Batch : IDisposable
{
	public int Handle { get; }
	public int IndexCount { get; private set; }

	// Vertex buffer size in bytes and index buffer size in bytes.
	public event Action<int, int>? OnBufferCapacityChange;

	private bool _isGenerated;

	private int _vbo = -1, _ibo = -1;
	private int _vSize, _iSize;

	public Batch()
	{
		Handle = GL.GenVertexArray();
	}

	public void Bind()
	{
		if (_isGenerated)
		{
			GL.BindVertexArray(Handle);
			GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
			GL.BindBuffer(BufferTarget.ElementArrayBuffer, _ibo);
		}
	}

	public void Update(float[] vertices, uint[] indices)
	{
		int requiredV = sizeof(float) * vertices.Length;
		int requiredI = sizeof(uint) * indices.Length;

		RecalculateBufferCapacities(requiredV, requiredI);

		GL.BindVertexArray(Handle);

		GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
		GL.BufferSubData(BufferTarget.ArrayBuffer, 0, requiredV, vertices);

		GL.BindBuffer(BufferTarget.ElementArrayBuffer, _ibo);
		GL.BufferSubData(BufferTarget.ElementArrayBuffer, 0, requiredI, indices);

		IndexCount = indices.Length;
	}

	public void Dispose()
	{
		GL.DeleteVertexArray(Handle);

		if (GL.IsBuffer(_vbo))
		{
			GL.DeleteBuffer(_vbo);
		}

		if (GL.IsBuffer(_ibo))
		{
			GL.DeleteBuffer(_ibo);
		}
	}

	private static int CalculateSize(int current, int required)
	{
		int size = 2;

		// Grow.
		if (required > current)
		{
			while (size < required)
			{
				size *= 2;
			}

			size *= 2;
		}
		// Shrink.
		else if (required < current / 2)
		{
			size = current;

			while (size > required)
			{
				size /= 2;
			}
			 
			size = Math.Max(size, 2);
			size *= 2;
		}
		else
		{
			size = current;
		}

		return size;
	}

	private void RecalculateBufferCapacities(int requireVSize, int requiredISize)
	{
		int newVSize = CalculateSize(_vSize, requireVSize);
		int newISize = CalculateSize(_iSize, requiredISize);

		if (_vSize != newVSize || _iSize != newISize)
		{
			GenerateBuffers(newVSize, newISize);
			OnBufferCapacityChange?.Invoke(newVSize, newISize);
		}
	}

	private void GenerateBuffers(int vSize, int iSize)
	{
		if (GL.IsBuffer(_vbo))
		{
			GL.DeleteBuffer(_vbo);
		}

		if (GL.IsBuffer(_ibo))
		{
			GL.DeleteBuffer(_ibo);
		}

		_vSize = vSize;
		_iSize = iSize;

		GL.BindVertexArray(Handle);

		_vbo = GL.GenBuffer();
		GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
		GL.BufferData(BufferTarget.ArrayBuffer, vSize, 0, BufferUsageHint.DynamicDraw);

		_ibo = GL.GenBuffer();
		GL.BindBuffer(BufferTarget.ElementArrayBuffer, _ibo);
		GL.BufferData(BufferTarget.ElementArrayBuffer, iSize, 0, BufferUsageHint.DynamicDraw);

		GL.EnableVertexAttribArray(0);
		GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, sizeof(float) * 2, 0);

		_isGenerated = true;
	}
}
