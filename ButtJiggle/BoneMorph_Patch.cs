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
			// Copy helper's transform so we can see if it gets changed

			if (__instance.m_tbSkin.body.TryGetHipHelpers(out var jbhHipL, out var jbhHipR))
			{
				jbhHipL.CheckForChanges();
				jbhHipR.CheckForChanges();
			}

			if (__instance.m_tbSkin.body.TryGetPelvisHelper(out var jbhPelvis))
			{
				jbhPelvis.CheckForChanges();
			}
		}

		[HarmonyPostfix, HarmonyPatch(nameof(BoneMorph_.Blend))]
		static void Blend_Postfix(BoneMorph_ __instance)
		{
			// Copy the bones's transform and any changes back to the helper

			if (__instance.m_tbSkin.body.TryGetHipHelpers(out var jbhHipL, out var jbhHipR))
			{
				jbhHipL.ApplyChanges();
				jbhHipR.ApplyChanges();
			}

			if (__instance.m_tbSkin.body.TryGetPelvisHelper(out var jbhPelvis))
			{
				jbhPelvis.ApplyChanges();
			}
		}
	}
}
