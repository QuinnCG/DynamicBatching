using OpenTK.Mathematics;

namespace DynamicBatching;

class MeshBuilder
{
	private readonly List<float> _vertices = [];
	private readonly List<uint> _indices = [];

	private uint _highestIndex;

	private MeshBuilder() { }

	public static MeshBuilder Create() => new();

	public uint GetOffset()
	{
		return _highestIndex;
	}

	public MeshBuilder Quad(Vector2 center, Vector2 size)
	{
		Vector2 half = size / 2f;
		Vector2 lower = center - half;
		Vector2 upper = center + half;

		var vertices = new float[]
		{
			lower.X, lower.Y,
			lower.X, upper.Y,
			upper.X, upper.Y,
			upper.X, lower.Y
		};

		uint offset = _highestIndex;
		var indices = new uint[]
		{
			0 + offset,
			1 + offset,
			2 + offset,
			3 + offset,
			0 + offset,
			2 + offset
		};

		foreach (uint index in indices)
		{
			if (index > _highestIndex)
			{
				_highestIndex = index + 1;
			}
		}

		_vertices.AddRange(vertices);
		_indices.AddRange(indices);

		return this;
	}

	public void Build(out float[] vertices, out uint[] indices)
	{
		vertices = [.. _vertices];
		indices = [.. _indices];
	}
}
