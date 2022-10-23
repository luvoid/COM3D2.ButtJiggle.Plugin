using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TriLib;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using static RootMotion.FinalIK.IKSolver;

namespace COM3D2.ButtJiggle
{
	public class JiggleBoneHelper : MonoBehaviour
	{
		public Transform TargetBone;
		public Transform OutputBone;
		public jiggleBone JiggleBone;
		public bool IsChangeCheck = false;
		
		public static bool UseGlobalOverride = false;
		public static JiggleBoneOverride GlobalOverride = JiggleBoneOverride.None;

		public static bool DebugMode = false;
		private LineRenderer[] m_LineRenderers = new LineRenderer[5];
		private static Material m_DebugMaterial;

		public static JiggleBoneHelper CreateFromBone(Transform targetBone)
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
			helper.OutputBone = outputBone;
			helper.JiggleBone = jb;

			return helper;
		}

		void Start()
		{
			for (int i = 0; i < m_LineRenderers.Length; i++)
			{
				m_LineRenderers[i] = CreateLineRenderer();
			}
		}

		public void LateUpdateSelf()
		{
			if (!this.isActiveAndEnabled) return;

			if (IsChangeCheck) ApplyChanges();

			CopyLocalTransform(this.transform, JiggleBone.transform);
			JiggleBone.defQtn = this.transform.localRotation;

			if (UseGlobalOverride)
			{
				//ButtJiggle.Logger.LogInfo("Use Global Override!");
				var origValues = JiggleBoneOverride.From(JiggleBone);
				GlobalOverride.ApplyTo(JiggleBone);
				JiggleBone.LateUpdateSelf();
				origValues.ApplyTo(JiggleBone);
			}
			else
			{
				JiggleBone.LateUpdateSelf();
			}
			CopyTransform(OutputBone.transform, TargetBone);
			TargetBone.localScale = Vector3.Scale(JiggleBone.transform.localScale, this.transform.localScale);

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
			if (!DebugMode && !JiggleBone.debugMode)
			{
				foreach (var lineRenderer in m_LineRenderers)
				{
					lineRenderer.enabled = false;
				}
				return;
			}

			Quaternion origRotation = JiggleBone.transform.localRotation;
			JiggleBone.transform.localRotation = JiggleBone.defQtn;

			Vector3 localForward = JiggleBone.boneAxis * JiggleBone.targetDistance;
			Vector3 boneForward = JiggleBone.transform.TransformDirection(localForward);
			Vector3 boneUp = JiggleBone.transform.TransformDirection(Vector3.up);
			Vector3 lookTarget = JiggleBone.transform.position + boneForward;

			JiggleBone.transform.localRotation = origRotation;
			
			DrawRay(0, JiggleBone.transform.position, boneForward, Color.blue);
			DrawRay(1, JiggleBone.transform.position, boneUp * 0.2f, Color.green);
			DrawRay(2, lookTarget, Vector3.up * 0.2f, Color.yellow);
			DrawRay(3, JiggleBone.dynamicPos, Vector3.up * 0.2f, Color.red);

			DrawBone(4, TargetBone, Color.white);
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
			

			var lineTexture = Resources.Load<Texture>("SteamVR/ArcTeleport");
			//var grayTexture = Resources.Load<Shader>("SteamVR/ArcTeleport_Gray");

			Material material = new Material(shader)
			{
				hideFlags = HideFlags.HideAndDontSave
			};
			material.SetInt("_ZTest", 0);
			material.SetInt("_SrcBlend", 5);
			material.SetInt("_DstBlend", 10);
			material.SetInt("_Cull", 0);
			material.SetInt("_ZWrite", 0);
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
	}
}
