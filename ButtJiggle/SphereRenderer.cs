using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace COM3D2.ButtJiggle
{
	internal class SphereRenderer : MonoBehaviour
	{
		private const int DensityCount = 80;
		private const float CircleAngle = 2 * Mathf.PI / DensityCount;

		public float Radius = 0.1f;
		public Vector3 Center = Vector3.zero;

		protected Color m_Color = Color.white;
		protected Material m_Material;


		public Color Color
		{
			get => m_Color;
			set
			{
				if (m_Color == value) return;
				m_Color = value;
				Refresh();
			}
		}

		public Material Material
		{
			get => m_Material;
			set
			{
				m_Material = value;
				Refresh();
			}
		}

		protected void Refresh()
		{
			if (m_Material != null)
			{
				//m_Material.color = m_Color;
			}
		}

		void OnRenderObject()
		{
			GL.PushMatrix();
			try
			{
				GL.MultMatrix(this.transform.localToWorldMatrix);
				if (m_Material != null)
				{
					for (int i = 0; i < m_Material.passCount; i++)
					{
						m_Material.SetPass(i);
						DrawSphere(this.Radius, this.Center);
					}
				}
				else
				{
					DrawSphere(this.Radius, this.Center);
				}
			}
			finally
			{
				GL.PopMatrix();
			}
		}

		private void DrawSphere(in float radius, in Vector3 pos)
		{
			DrawCircle(Vector3.forward * radius, Vector3.up    * radius, pos);
			DrawCircle(Vector3.back    * radius, Vector3.right * radius, pos);
			DrawCircle(Vector3.down    * radius, Vector3.left  * radius, pos);
		}

		private void DrawCircle(in Vector3 v1, in Vector3 v2, in Vector3 pos)
		{
			GL.Begin(2);
			try
			{
				GL.Color(m_Color);
				for (float radians = -CircleAngle; radians <= Mathf.PI * 2; radians += CircleAngle)
				{
					Vector3 vertex = v1 * Mathf.Cos(radians) + v2 * Mathf.Sin(radians) + pos;
					GL.Vertex3(vertex.x, vertex.y, vertex.z);
				}
			}
			finally
			{
				GL.End();
			}
		}
	}
}
