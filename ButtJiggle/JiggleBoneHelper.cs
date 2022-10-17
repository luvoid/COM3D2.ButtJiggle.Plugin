using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.SocialPlatforms;

namespace COM3D2.ButtJiggle
{
	public class JiggleBoneHelper : MonoBehaviour
	{
		public jiggleBone JiggleBone;
		public bool IsChangeCheck = false;

		public static bool UseGlobalOverride = false;
		public static JiggleBoneOverride GlobalOverride = JiggleBoneOverride.None;

		public static JiggleBoneHelper CreateFromBone(Transform bone)
		{
			var newBone = CopyBone(bone, bone.name + "_helper");
			var subBone = CopyBone(bone, bone.name + "_sub").transform;
			subBone.SetParent(bone, true);

			var jb = bone.gameObject.AddComponent<jiggleBone>();
			jb.BlendValue = 1;
			jb.BlendValue2 = 1;
			jb.MuneL_y = 1; //bone.name.Contains("_R") ? -1 : 1;
			jb.boneAxis = new Vector3(-1, -1, 0).normalized;
			jb.MuneUpDown = 30;
			jb.MuneYori = 0;
			jb.m_fMuneYawaraka = ButtJiggle.ButtJiggle_DefaultSoftness.Value;

			var helper = newBone.gameObject.AddComponent<JiggleBoneHelper>();
			helper.JiggleBone = jb;

			return helper;
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
			targetBone.localScale = sourceBone.localScale;
		}

		public void LateUpdateSelf()
		{
			if (!this.isActiveAndEnabled) return;

			if (IsChangeCheck) ApplyChanges();

			CopyLocalTransformToBone();

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
			JiggleBone.transform.localScale = Vector3.Scale(JiggleBone.transform.localScale, this.transform.localScale);
		}

		public void ApplyChanges()
		{
			CopyLocalTransformFromBone();
			IsChangeCheck = false;
		}

		public void CopyLocalTransformToBone()
		{
			CopyLocalTransform(this.transform, JiggleBone.transform);
			JiggleBone.defQtn = this.transform.localRotation;
		}

		public void CopyLocalTransformFromBone()
		{
			CopyLocalTransform(JiggleBone.transform, this.transform);
		}
	}
}
