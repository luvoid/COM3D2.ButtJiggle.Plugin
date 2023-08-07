using Mono.Cecil;
using RenderHeads.Media.AVProVideo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using TriLib;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using UnityEngine.UI;
using UnityRuntimeGizmos;

namespace COM3D2.ButtJiggle
{
	public class JiggleBoneHelper : MonoBehaviour
	{

		[SerializeField] private TBody m_TBody;

		public Transform TargetBone;
		public jiggleBone Jiggle;
		[field: SerializeField] public MaidJiggleOverride.Slot Slot { get; private set; }
		[field: SerializeField] public bool IsChangeCheck { get; private set; }

		public Transform JiggleBone => Jiggle.transform;
		[SerializeField] private Transform m_JiggleOffsetBone;
		[SerializeField] private Transform m_JiggleDefBone;
		[SerializeField] private Transform m_JiggleSubBone;
		[SerializeField] private Transform m_JiggleOutputBone;

		[SerializeField] private Transform m_HitParent;
		[SerializeField] private Transform m_HitChild;
		[SerializeField] private SphereCollider m_HitChildCollider;


		public static bool UseGlobalOverride = false;
		public static MaidJiggleOverride GlobalOverride = MaidJiggleOverride.Default;

		public static bool DebugMode = false;
		[SerializeField] private LineRenderer[] m_LineRenderers = new LineRenderer[2];
		[SerializeField] private GizmoRenderer m_SphereRenderer;
		[SerializeField] private Material m_DebugMaterial;

		/// <summary>
		/// Converts a bone to a <see cref="jiggleBone"/>, wrapping it with a <see cref="JiggleBoneHelper"/>.
		/// </summary>
		/// <remarks>
		/// If possible, substitute references to the target bone with the returned <see cref="JiggleBoneHelper"/>,
		/// so manipulation is applied to the helper instead of the target. 
		/// Manipulations that cannot be redirected to the helper can be captured with <see cref="CheckForChanges"/>.
		/// </remarks>
		/// <param name="targetBone">The target bone to wrap.</param>
		/// <param name="tbody">The body to which the helper will be assigned.</param>
		/// <returns>The wrapping <see cref="JiggleBoneHelper"/></returns>
		public static JiggleBoneHelper WrapBone(Transform targetBone, TBody tbody, MaidJiggleOverride.Slot slot, Quaternion? localRotation = null, bool useSubBone = true)
		{
			if (localRotation == null)
				localRotation = Quaternion.identity;

			var helperBone = CopyBone(targetBone, targetBone.name + "_helper");
			var jiggleOffset = CopyBone(targetBone, targetBone.name + "_jiggleOffset");

			var jiggleBone = CopyBone(targetBone, targetBone.name + "_jiggle");
			jiggleBone.SetParent(jiggleOffset, true);
			jiggleBone.localRotation = localRotation.Value;
			
			var jiggleDef = CopyBone(jiggleBone, jiggleBone.name + "Def");

			var subBone = CopyBone(jiggleBone, jiggleBone.name + "_sub");
			subBone.SetParent(jiggleBone, true);
			Transform jiggleNub = jiggleBone.Find(jiggleBone.name + "_nub");
			subBone.position = (jiggleNub != null) ? jiggleNub.position : jiggleBone.TransformPoint(Vector3.left * 0.2f);
			subBone.localEulerAngles = new Vector3(0, 0, 180);

			var jb = jiggleBone.gameObject.AddComponent<jiggleBone>();
			jb.BlendValue = 1;
			jb.BlendValue2 = 1;
			jb.MuneL_y = 1; //bone.name.Contains("_R") ? -1 : 1;
			// jb.boneAxis = boneAxis;
			jb.MuneUpDown = 0;
			jb.MuneYori = 0;
			jb.m_fMuneYawaraka = slot switch
			{
				MaidJiggleOverride.Slot.Hip => ButtJiggle.ButtJiggle_DefaultSoftness_Hip.Value,
				MaidJiggleOverride.Slot.Pelvis => ButtJiggle.ButtJiggle_DefaultSoftness_Pelvis.Value,
				_ => throw new ArgumentOutOfRangeException(nameof(slot)),
			};

			var outputBone = CopyBone(targetBone, targetBone.name + "_output");
			outputBone.SetParent(useSubBone ? subBone : jiggleBone, true);

			var helper = helperBone.gameObject.AddComponent<JiggleBoneHelper>();
			helper.TargetBone = targetBone;
			helper.Jiggle = jb;
			helper.Slot = slot;
			helper.m_JiggleOffsetBone = jiggleOffset;
			helper.m_JiggleDefBone = jiggleDef;
			helper.m_JiggleSubBone = subBone;
			helper.m_JiggleOutputBone = outputBone;
			helper.m_TBody = tbody;

			return helper;
		}

		public void SetCollider(Vector3 center)
		{
			if (m_HitParent == null)
			{
				Transform hitParent = GameObject.Instantiate<GameObject>(Resources.Load<GameObject>("OVR/SphereParent"))?.transform;
				Transform hitChild = GameObject.Instantiate<GameObject>(Resources.Load<GameObject>("OVR/SphereChild"))?.transform;
				hitChild.GetComponent<SpringJoint>().connectedBody = hitParent.GetComponent<Rigidbody>();
				hitParent.parent = JiggleBone;
				hitChild.parent = JiggleBone;
				hitParent.localPosition = m_JiggleSubBone.localPosition;
				hitChild.localPosition = m_JiggleSubBone.localPosition;

				m_HitParent = hitParent;
				m_HitChild = hitChild;
				m_HitChildCollider = hitChild.GetComponent<SphereCollider>();
			}

			m_HitChildCollider.center = center;
		}


		void Start()
		{
			for (int i = 0; i < m_LineRenderers.Length; i++)
			{
				m_LineRenderers[i] = CreateLineRenderer();
			}

			if (m_HitChild != null)
			{
				m_SphereRenderer = m_HitChild.gameObject.AddComponent<GizmoRenderer>();
			}
		}

		public void Update()
		{
			float limitDistance = m_TBody.m_fHitLimitDistanceMin + (m_TBody.m_fHitLimitDistanceMax - m_TBody.m_fHitLimitDistanceMin) * (Jiggle.BlendValue / 1.3f);
			if (m_JiggleSubBone != null && m_HitChild != null)
			{
				Vector3 hitChildLocalPos = JiggleBone.InverseTransformPoint(m_HitChild.position);
				if ((hitChildLocalPos - m_HitParent.localPosition).magnitude < limitDistance)
				{
					m_JiggleSubBone.localPosition = hitChildLocalPos;
				}
				else
				{
					m_JiggleSubBone.localPosition = m_HitParent.localPosition + (hitChildLocalPos - m_HitParent.localPosition).normalized * limitDistance;
					m_HitChild.position = m_JiggleSubBone.position;
				}
			}
		}

		public void LateUpdateSelf()
		{
			if (!this.isActiveAndEnabled) return;

			if (IsChangeCheck) ApplyChanges();

			CopyLocalTransform(this.transform, m_JiggleOffsetBone);
			Jiggle.defQtn = m_JiggleDefBone.localRotation;

			if (UseGlobalOverride)
			{
				//ButtJiggle.Logger.LogInfo("Use Global Override!");
				var origValues = JiggleBoneOverride.From(Jiggle);
				GlobalOverride[Slot].ApplyTo(Jiggle);
				Jiggle.LateUpdateSelf();
				origValues.ApplyTo(Jiggle);
			}
			else
			{
				Jiggle.LateUpdateSelf();
			}
			CopyTransform(m_JiggleOutputBone.transform, TargetBone);
			TargetBone.localScale = Vector3.Scale(Jiggle.transform.localScale, this.transform.localScale);

			DebugDraw();
		}
		


		private static Transform CopyBone(Transform bone, string name)
		{
			Transform newBone = new GameObject(name).transform;
			newBone.gameObject.layer = bone.gameObject.layer;
			newBone.parent = bone.parent;
			CopyLocalTransform(bone, newBone);

			// Also copy the nub if it exists
			Transform nub = bone.Find(bone.name + "_nub")?.transform;
			if (nub != null)
			{
				Transform newNub = CopyBone(nub, name + "_nub");
				newNub.SetParent(newBone, false);
			}

			return newBone;
		}


		public static void CopyLocalTransform(Transform sourceBone, Transform targetBone)
		{
			targetBone.localPosition = sourceBone.localPosition;
			targetBone.localRotation = sourceBone.localRotation;
			targetBone.localScale    = sourceBone.localScale;
		}

		public static void CopyTransform(Transform sourceBone, Transform targetBone)
		{
			targetBone.position   = sourceBone.position  ;
			targetBone.rotation   = sourceBone.rotation  ;
			targetBone.localScale = sourceBone.localScale;
		}

		/// <summary>
		/// Check if any changes are made to the wrapped target bone 
		/// between now and the next <see cref="ApplyChanges"/> call,
		/// or until the next <see cref="LateUpdateSelf"> call.
		/// </summary>
		public void CheckForChanges()
		{
			CopyLocalTransformToBone();
			IsChangeCheck = true;
		}

		/// </summary>
		/// Apply any changes made to the wrapped target bone back to this source bone.
		/// </summary>
		/// <exception cref="InvalidOperationException">If changes are not currently being checked for.</exception>
		public void ApplyChanges()
		{
			if (!IsChangeCheck) throw new InvalidOperationException("Not currently checking for changes");
			CopyLocalTransformFromBone();
			IsChangeCheck = false;
		}

		/// <summary>
		/// Copies the transform of this helper bone to the target bone that it is wrapping.
		/// </summary>
		protected void CopyLocalTransformToBone()
		{
			CopyLocalTransform(this.transform, TargetBone.transform);
			//JiggleBone.defQtn = this.transform.localRotation;
		}

		/// <summary>
		/// Copies the transform of the wrapped target bone to this helper bone.
		/// </summary>
		protected void CopyLocalTransformFromBone()
		{
			CopyLocalTransform(TargetBone.transform, this.transform);
		}

		private void DebugDraw()
		{
			if (!DebugMode && !Jiggle.debugMode)
			{
				foreach (var lineRenderer in m_LineRenderers)
				{
					lineRenderer.enabled = false;
				}
				if (m_SphereRenderer != null)
				{
					m_SphereRenderer.enabled = false;
				}
				return;
			}

			Quaternion origRotation = Jiggle.transform.localRotation;
			Jiggle.transform.localRotation = Jiggle.defQtn;

			Vector3 localForward = Jiggle.boneAxis * Jiggle.targetDistance;
			Vector3 boneForward = Jiggle.transform.TransformDirection(localForward);
			Vector3 boneUp = Jiggle.transform.TransformDirection(Vector3.up);
			Vector3 lookTarget = Jiggle.transform.position + boneForward * 0.2f;
			Vector3 dynamicForward = Jiggle.dynamicPos - Jiggle.transform.position;
			Vector3 dynamicTarget = dynamicForward * 0.2f + Jiggle.transform.position;

			Jiggle.transform.localRotation = origRotation;
			
			RuntimeGizmos.DrawRay(Jiggle.transform.position, boneUp * 0.1f, Color.green);
			RuntimeGizmos.DrawRay(Jiggle.transform.position, boneForward * 0.2f, Color.blue);
			RuntimeGizmos.DrawRay(Jiggle.transform.position, dynamicForward * 0.2f, Color.red);
			//RuntimeGizmos.DrawRay(dynamicTarget, Vector3.up * 0.1f, Color.red);

			DrawBone(0, JiggleBone, Color.cyan);
			DrawBone(1, TargetBone, Color.white);

			if (m_HitChildCollider != null)
			{
				DrawSphereCollider(m_HitChildCollider, Color.green);
			}
		}

		private LineRenderer CreateLineRenderer()
		{
			var go = new GameObject("LineRenderer");
			go.transform.parent = this.transform;

			var line = go.AddComponent<LineRenderer>();
			
			line.enabled = true;
			line.useWorldSpace = true;
			line.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
			line.receiveShadows = false;
			line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
			line.sortingOrder = 20000;
			line.widthMultiplier = 0.01f;

			if (m_DebugMaterial == null)
			{
				m_DebugMaterial = CreateMaterial();
			}
			line.material = m_DebugMaterial;

			return line;
		}

		private static Material CreateMaterial()
		{
			Shader shader = Shader.Find("Hidden/Internal-Colored");
			
			Material material = new Material(shader)
			{
				hideFlags = HideFlags.HideAndDontSave
			};
			material.SetInt("_ZTest"   ,  0);
			material.SetInt("_SrcBlend",  5);
			material.SetInt("_DstBlend", 10);
			material.SetInt("_Cull"    ,  0);
			material.SetInt("_ZWrite"  ,  0);
			material.renderQueue = 5000;
			//material.color = this.lineColor;
			return material;
		}

		private void DrawBone(int lineIndex, Transform bone, Color color)
		{
			Transform nub = bone.Find(bone.name + "_nub")?.transform;

			Vector3 start = bone.position;
			Vector3 dir = (nub != null) ? nub.position - bone.position : bone.TransformDirection(Vector3.left) * 0.2f;

			var line = m_LineRenderers[lineIndex];
			if (line == null)
			{
				ButtJiggle.Logger.LogError($"{this}.m_LineRenderers[{lineIndex}] == null");
				return;
			}

			line.enabled = true;

			line.startColor = color;
			line.endColor = color;
			line.startWidth = 1f * line.widthMultiplier;
			line.endWidth = line.startWidth / 5f * line.widthMultiplier;

			line.SetPositions(new Vector3[] { start, start + dir });
		}

		private void DrawSphereCollider(SphereCollider sphereCollider, Color color)
		{
			if (m_SphereRenderer == null)
			{
				ButtJiggle.Logger.LogError($"{this}.m_SphereRenderer == null");
				return;
			}

			m_SphereRenderer.enabled = true;
			m_SphereRenderer.Gizmo = RuntimeGizmos.WireSphere(sphereCollider.center, sphereCollider.radius, color);
		}
	}
}
