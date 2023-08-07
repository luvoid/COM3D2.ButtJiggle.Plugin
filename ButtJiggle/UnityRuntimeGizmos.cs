using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace UnityRuntimeGizmos
{
	internal static partial class RuntimeGizmos
	{
		static GizmoDrawer _staticDrawer;
		static GizmoDrawer StaticDrawer
		{
			get
			{
				if (_staticDrawer == null)
				{
					var go = new GameObject("StaticRuntimeGizmoDrawer");
					_staticDrawer = go.AddComponent<GizmoDrawer>();
					GameObject.DontDestroyOnLoad(go);
				}
				return _staticDrawer;
			}
		}


		static Material _coloredMaterial;
		static Material StaticMaterial
		{
			get
			{
				if (_coloredMaterial != null) return _coloredMaterial;

				Shader shader = Shader.Find("Hidden/Internal-Colored");

				_coloredMaterial = new Material(shader)
				{
					hideFlags = HideFlags.HideAndDontSave
				};
				_coloredMaterial.SetInt("_ZTest", 0);
				_coloredMaterial.SetInt("_SrcBlend", 5);
				_coloredMaterial.SetInt("_DstBlend", 10);
				_coloredMaterial.SetInt("_Cull", 0);
				_coloredMaterial.SetInt("_ZWrite", 0);
				_coloredMaterial.renderQueue = 5000;
				//material.color = this.lineColor;
				return _coloredMaterial;
			}
		}

		static Color _defaultGizmoColor = UnityEngine.Color.white;


		public static void Color(Color color)
		{
			_defaultGizmoColor = color;
		}

		public static IGizmo Ray(Vector3 start, Vector3 dir, Color color)
		{
			LineGizmo lineGizmo = new LineGizmo();
			lineGizmo.Start = start;
			lineGizmo.End = start + dir;
			lineGizmo.Color = color;
			return lineGizmo;
		}

		public static void DrawRay(Vector3 start, Vector3 dir, Color color)
		{
			StaticDrawer.Enqueue(Ray(start, dir, color));
		}

		public static void DrawRay(Vector3 start, Vector3 dir)
		{
			DrawRay(start, dir, _defaultGizmoColor);
		}

		public static IGizmo WireSphere(Vector3 center, float radius, Color color)
		{
			WireSphereGizmo wireSphereGizmo = new WireSphereGizmo();
			wireSphereGizmo.Center = center;
			wireSphereGizmo.Radius = radius;
			wireSphereGizmo.Color = color;
			return wireSphereGizmo;
		}

		public static void DrawWireSphere(Vector3 center, float radius, Color color)
		{
			StaticDrawer.Enqueue(WireSphere(center, radius, color));
		}

		public static void DrawWireSphere(Vector3 center, float radius)
		{
			DrawWireSphere(center, radius, _defaultGizmoColor);
		}
	}

	internal static partial class RuntimeGizmos // private class GizmoDrawer
	{
		private class GizmoDrawer : MonoBehaviour
		{
			private List<IGizmo> m_Gizmos = new List<IGizmo>();

			public void Enqueue(IGizmo gizmo)
			{
				m_Gizmos.Add(gizmo);
			}

			void OnRenderObject()
			{
				GL.PushMatrix();
				try
				{
					GL.MultMatrix(Matrix4x4.identity);
					foreach (var gizmo in m_Gizmos)
					{
						try { gizmo.Draw(); }
						catch (Exception ex) { Debug.LogException(ex); }
					}
				}
				finally
				{
					GL.PopMatrix();
					m_Gizmos.Clear();
				}
			}
		}
	}

	internal static partial class RuntimeGizmos // private Gizmo structs
	{

		private static class GizmoDefault
		{
			public static void Draw(IGizmo gizmo)
			{
				if (gizmo.Material != null)
				{
					for (int i = 0; i < gizmo.Material.passCount; i++)
					{
						gizmo.Material.SetPass(i);
						gizmo.DrawPass(i);
					}
				}
				else
				{
					gizmo.DrawPass(-1);
				}
			}
		}
		
		private struct LineGizmo : IGizmo
		{
			public Color Color { get; set; }
			public Material Material { get; set; } = StaticMaterial;

			public Vector3 Start;
			public Vector3 End;

			public LineGizmo()
			{ }

			public void Draw() => GizmoDefault.Draw(this);
			public void DrawPass(int materialPassIndex)
			{
				GL.Begin(2);
				try
				{
					GL.Color(Color);
					GL.Vertex(Start);
					GL.Vertex(End);
				}
				finally
				{
					GL.End();
				}
			}
		}

		private struct WireSphereGizmo : IGizmo
		{
			private const int DensityCount = 80;
			private const float CircleAngle = 2 * Mathf.PI / DensityCount;

			public Color Color { get; set; }
			public Material Material { get; set; } = StaticMaterial;

			public float Radius = 0.1f;
			public Vector3 Center = Vector3.zero;

			public WireSphereGizmo()
			{ }

			public void Draw() => GizmoDefault.Draw(this);
			public void DrawPass(int materialPassIndex)
			{
				DrawSphere(this.Radius, this.Center);
			}

			private void DrawSphere(in float radius, in Vector3 pos)
			{
				DrawCircle(Vector3.forward * radius, Vector3.up * radius, pos, Color);
				DrawCircle(Vector3.back * radius, Vector3.right * radius, pos, Color);
				DrawCircle(Vector3.down * radius, Vector3.left * radius, pos, Color);
			}

			public static void DrawCircle(in Vector3 v1, in Vector3 v2, in Vector3 pos, Color color)
			{
				const int densityCount = 80;
				const float circleAngle = 2 * Mathf.PI / densityCount;

				GL.Begin(2);
				try
				{
					GL.Color(color);
					for (float radians = -circleAngle; radians <= Mathf.PI * 2; radians += circleAngle)
					{
						Vector3 vertex = v1 * Mathf.Cos(radians) + v2 * Mathf.Sin(radians) + pos;
						GL.Vertex(vertex);
					}
				}
				finally
				{
					GL.End();
				}
			}
		}
	}

	internal interface IGizmo
	{
		Color Color { get; set; }
		Material Material { get; set; }
		void Draw();
		void DrawPass(int materialPassIndex);
	}

	internal class GizmoRenderer : MonoBehaviour
	{
		public IGizmo Gizmo;

		void OnRenderObject()
		{
			GL.PushMatrix();
			try
			{
				GL.MultMatrix(this.transform.localToWorldMatrix);
				try { Gizmo?.Draw(); }
				catch (Exception ex) { Debug.LogException(ex); }
				}
			finally
			{
				GL.PopMatrix();
			}
		}
	}

	internal class GizmoCollectionRenderer : MonoBehaviour, ICollection<IGizmo>
	{
		private List<IGizmo> m_Gizmos = new List<IGizmo>();

		public int Count => ((ICollection<IGizmo>)m_Gizmos).Count;

		public bool IsReadOnly => ((ICollection<IGizmo>)m_Gizmos).IsReadOnly;

		void OnRenderObject()
		{
			GL.PushMatrix();
			try
			{
				GL.MultMatrix(this.transform.localToWorldMatrix);
				foreach (var gizmo in m_Gizmos)
				{
					try { gizmo.Draw(); }
					catch (Exception ex) { Debug.LogException(ex); }
				}
			}
			finally
			{
				GL.PopMatrix();
			}
		}

		public void Clear() => m_Gizmos.Clear();

		public void CopyTo(IGizmo[] array, int arrayIndex) => m_Gizmos.CopyTo(array, arrayIndex);

		public void Add(IGizmo item) => m_Gizmos.Add(item);

		public bool Contains(IGizmo item) => m_Gizmos.Contains(item);

		public bool Remove(IGizmo item) => m_Gizmos.Remove(item);

		public IEnumerator<IGizmo> GetEnumerator() => m_Gizmos.GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)m_Gizmos).GetEnumerator();
	}
}
