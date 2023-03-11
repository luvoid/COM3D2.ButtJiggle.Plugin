using Mono.Cecil;
using RenderHeads.Media.AVProVideo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TriLib;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using static MathCM;
using static RootMotion.FinalIK.IKSolver;

namespace COM3D2.ButtJiggle
{
	public class JiggleBoneHelper : MonoBehaviour
	{
		public bool IsChangeCheck = false;

		[SerializeField] private TBody m_TBody;

		public Transform TargetBone;
		public jiggleBone Jiggle;

		public Transform JiggleBone => Jiggle.transform;
		[SerializeField] private Transform m_JiggleSubBone;
		[SerializeField] private Transform m_JiggleOutputBone;

		[SerializeField] private Transform m_HitParent;
		[SerializeField] private Transform m_HitChild;
		[SerializeField] private SphereCollider m_HitChildCollider;


		public static bool UseGlobalOverride = false;
		public static JiggleBoneOverride GlobalOverride = JiggleBoneOverride.None;

		public static bool DebugMode = false;
		[SerializeField] private LineRenderer[] m_LineRenderers = new LineRenderer[5];
		[SerializeField] private SphereRenderer m_SphereRenderer;
		private static Material m_DebugMaterial;

		public static JiggleBoneHelper CreateFromBone(Transform targetBone, TBody tbody)
		{

			var helperBone = CopyBone(targetBone, targetBone.name + "_helper");
			var jiggleBone = CopyBone(targetBone, targetBone.name + "_jiggle");
			var subBone    = CopyBone(jiggleBone, jiggleBone.name + "_sub"   );
			subBone.SetParent(jiggleBone, true);
			Transform jiggleNub = jiggleBone.Find(jiggleBone.name + "_nub");
			subBone.position = (jiggleNub != null) ? jiggleNub.position : jiggleBone.TransformPoint(Vector3.right * 0.2f);
			subBone.localEulerAngles = new Vector3(0, 0, 180);
			
			var outputBone = CopyBone(targetBone, targetBone.name + "_copy");
			outputBone.SetParent(subBone, true);

			var jb = jiggleBone.gameObject.AddComponent<jiggleBone>();
			jb.BlendValue = 1;
			jb.BlendValue2 = 1;
			jb.MuneL_y = 1; //bone.name.Contains("_R") ? -1 : 1;
			jb.boneAxis = new Vector3(-1, -1, 0).normalized;
			jb.MuneUpDown = 30;
			jb.MuneYori = 0;
			jb.m_fMuneYawaraka = ButtJiggle.ButtJiggle_DefaultSoftness.Value;


			var helper = helperBone.gameObject.AddComponent<JiggleBoneHelper>();
			helper.TargetBone = targetBone;
			helper.Jiggle = jb;
			helper.m_JiggleSubBone = subBone;
			helper.m_JiggleOutputBone = outputBone;
			helper.m_TBody = tbody;

			if (GameMain.Instance.VRMode || true)
			{
				Transform hitParent = GameObject.Instantiate<GameObject>(Resources.Load<GameObject>("OVR/SphereParent"))?.transform;
				Transform hitChild  = GameObject.Instantiate<GameObject>(Resources.Load<GameObject>("OVR/SphereChild" ))?.transform;
				hitChild.GetComponent<SpringJoint>().connectedBody = hitParent.GetComponent<Rigidbody>();
				hitParent.parent = helper.JiggleBone;
				hitChild .parent = helper.JiggleBone;
				hitParent.localPosition = helper.m_JiggleSubBone.localPosition;
				hitChild .localPosition = helper.m_JiggleSubBone.localPosition;

				helper.m_HitParent = hitParent;
				helper.m_HitChild  = hitChild ;
				helper.m_HitChildCollider = hitChild.GetComponent<SphereCollider>();
				var center = new Vector3(0.3f, 0.1f, -0.7f);
				if (targetBone.name.Contains("_R"))
				{
					center.x *= -1;
				}
				helper.m_HitChildCollider.center = center;
			}

			return helper;
		}

		void Start()
		{
			for (int i = 0; i < m_LineRenderers.Length; i++)
			{
				m_LineRenderers[i] = CreateLineRenderer();
			}

			if (m_HitChild != null)
			{
				m_SphereRenderer = m_HitChild.gameObject.AddComponent<SphereRenderer>();
				m_SphereRenderer.Material = m_DebugMaterial;
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

			CopyLocalTransform(this.transform, Jiggle.transform);
			Jiggle.defQtn = this.transform.localRotation;

			if (UseGlobalOverride)
			{
				//ButtJiggle.Logger.LogInfo("Use Global Override!");
				var origValues = JiggleBoneOverride.From(Jiggle);
				GlobalOverride.ApplyTo(Jiggle);
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

		public void ApplyChanges()
		{
			CopyLocalTransformFromBone();
			IsChangeCheck = false;
		}

		public void CopyLocalTransformToBone()
		{
			CopyLocalTransform(this.transform, TargetBone.transform);
			//JiggleBone.defQtn = this.transform.localRotation;
		}

		public void CopyLocalTransformFromBone()
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
			Vector3 lookTarget = Jiggle.transform.position + boneForward;

			Jiggle.transform.localRotation = origRotation;
			
			DrawRay(0, Jiggle.transform.position, boneForward, Color.blue);
			DrawRay(1, Jiggle.transform.position, boneUp * 0.2f, Color.green);
			DrawRay(2, lookTarget, Vector3.up * 0.2f, Color.yellow);
			DrawRay(3, Jiggle.dynamicPos, Vector3.up * 0.2f, Color.red);

			DrawBone(4, TargetBone, Color.white);

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
			Vector3 offset = (nub != null) ? nub.position - bone.position : bone.TransformDirection(Vector3.right) * 0.2f;
			DrawRay(lineIndex, bone.position, offset, color);
		}
		
		private void DrawRay(int lineIndex, Vector3 start, Vector3 dir, Color color)
		{
			var line = m_LineRenderers[lineIndex];
			if (line == null)
			{
				ButtJiggle.Logger.LogError($"{this}.m_LineRenderers[{lineIndex}] == null");
				return;
			}

			line.enabled = true;

			line.startColor = color * 0.5f;
			line.endColor = color;

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
			m_SphereRenderer.Radius = sphereCollider.radius;
			m_SphereRenderer.Center = sphereCollider.center;
			m_SphereRenderer.Color = color;
		}
	}
}
