using HarmonyLib;
using UnityEngine;

namespace COM3D2.ButtJiggle
{
	[HarmonyPatch(typeof(BoneMorph_))]
	internal static class BoneMorph_Patch
	{
		[HarmonyPrefix, HarmonyPatch(nameof(BoneMorph_.Blend))]
		static void Blend_Prefix(BoneMorph_ __instance)
		{
			if (!__instance.m_tbSkin.body.TryGetHipHelpers(out var jbhHipL, out var jbhHipR)) return;

			// Copy helper's transform so we can see if it get's changed
			jbhHipL.CopyLocalTransformToBone();
			jbhHipR.CopyLocalTransformToBone();

			jbhHipL.IsChangeCheck = true;
			jbhHipR.IsChangeCheck = true;
		}

		[HarmonyPostfix, HarmonyPatch(nameof(BoneMorph_.Blend))]
		static void Blend_Postfix(BoneMorph_ __instance)
		{
			if (!__instance.m_tbSkin.body.TryGetHipHelpers(out var jbhHipL, out var jbhHipR)) return;

			// Copy the bones's transform and any changes back to the helper
			jbhHipL.CopyLocalTransformFromBone();
			jbhHipR.CopyLocalTransformFromBone();
		}
	}
}
