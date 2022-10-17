using HarmonyLib;
using UnityEngine;

namespace COM3D2.ButtJiggle
{
	[HarmonyPatch(typeof(TBody))]
	internal static class TBodyPatch
	{
		public static bool TryGetHipHelpers(this TBody __instance, out JiggleBoneHelper jbhHipL, out JiggleBoneHelper jbhHipR)
		{
			jbhHipL = null;
			jbhHipR = null;

			if (!__instance.isLoadedBody) return false;
			if (__instance.boMAN) return false;

			var hipL = __instance.Hip_L;
			var hipR = __instance.Hip_R;

			if (hipL == null || hipR == null) return false;

			jbhHipL = hipL.GetComponent<JiggleBoneHelper>();
			jbhHipR = hipR.GetComponent<JiggleBoneHelper>();

			return (jbhHipL != null) && (jbhHipR != null);
		}

		[HarmonyPostfix, HarmonyPatch(nameof(TBody.LoadBody_R))]
		static void LoadBody_R(TBody __instance)
		{
			if (!__instance.isLoadedBody) return;
			if (__instance.boMAN) return;

			if (!ButtJiggle.ButtJiggle_Enabled.Value) return;

			var hipL = __instance.Hip_L;
			var hipR = __instance.Hip_R;

			if (hipL == null || hipR == null) return;

			var jbhHipL = JiggleBoneHelper.CreateFromBone(hipL);
			var jbhHipR = JiggleBoneHelper.CreateFromBone(hipR);

			__instance.Hip_L = jbhHipL.transform;
			__instance.Hip_R = jbhHipR.transform;
		}

		[HarmonyPrefix, HarmonyPatch(nameof(TBody.LateUpdate))]
		static void LateUpdate(TBody __instance)
		{
			if (!__instance.TryGetHipHelpers(out var jbhHipL, out var jbhHipR)) return;

			jbhHipL.LateUpdateSelf();
			jbhHipR.LateUpdateSelf();
		}


		[HarmonyPrefix, HarmonyPatch(nameof(TBody.BoneMorph_FromProcItem))]
		static void BoneMorph_FromProcItem(TBody __instance, string tag, float f)
		{
			if (__instance.boMAN) return;

			if (!__instance.TryGetHipHelpers(out var jbhHipL, out var jbhHipR)) return;

			if (tag == "HipYawaraka")
			{
				jbhHipL.JiggleBone.m_fMuneYawaraka = f;
				jbhHipR.JiggleBone.m_fMuneYawaraka = f;
			}
			if (tag == "koshi")
			{
				ButtJiggle.Logger.LogInfo($"koshi = {f}");
				jbhHipL.JiggleBone.BlendValue = Mathf.Clamp(f + .5f, 0f, 1.5f);
				jbhHipR.JiggleBone.BlendValue = Mathf.Clamp(f + .5f, 0f, 1.5f);
			}
		}

		[HarmonyPrefix, HarmonyPatch(nameof(TBody.Update))]
		static void Update(TBody __instance)
		{
			if (!__instance.TryGetHipHelpers(out var jbhHipL, out var jbhHipR)) return;

			jbhHipL.JiggleBone.boBRA = !__instance.boVisible_XXX;
			jbhHipR.JiggleBone.boBRA = !__instance.boVisible_XXX;
		}

		[HarmonyPrefix, HarmonyPatch(nameof(TBody.WarpInit))]
		static void WarpInit(TBody __instance)
		{
			if (!__instance.TryGetHipHelpers(out var jbhHipL, out var jbhHipR)) return;

			jbhHipL.JiggleBone.boWarpInit = true;
			jbhHipR.JiggleBone.boWarpInit = true;
		}
	}
}
