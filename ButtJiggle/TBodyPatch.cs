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

		public static bool TryGetPelvisHelper(this TBody __instance, out JiggleBoneHelper jbhPelvis)
		{
			jbhPelvis = null;

			if (!__instance.isLoadedBody) return false;
			if (__instance.boMAN) return false;

			var pelvis = __instance.Pelvis;
			var pelvisSclHelper = pelvis?.Find($"{pelvis.name}_SCL__helper");
			if (pelvisSclHelper == null) return false;

			jbhPelvis = pelvisSclHelper.GetComponent<JiggleBoneHelper>();

			return jbhPelvis != null;
		}

		[HarmonyPostfix, HarmonyPatch(nameof(TBody.LoadBody_R))]
		static void LoadBody_R(TBody __instance)
		{
			if (!__instance.isLoadedBody) return;
			if (__instance.boMAN) return;

			if (!ButtJiggle.ButtJiggle_Enabled.Value) return;

			var hipL = __instance.Hip_L;
			var hipR = __instance.Hip_R;
			if (hipL != null && hipR != null)
			{
				var jbhHipL = JiggleBoneHelper.WrapBone(hipL, __instance, MaidJiggleOverride.Slot.Hip, Quaternion.Euler(0, 180, 90));
				var jbhHipR = JiggleBoneHelper.WrapBone(hipR, __instance, MaidJiggleOverride.Slot.Hip, Quaternion.Euler(0, 180, 90));

				jbhHipL.SetCollider(new Vector3(0.0f, 0.4f, 0.1f));
				jbhHipR.SetCollider(new Vector3(0.0f, 0.4f, 0.1f));

				__instance.Hip_L = jbhHipL.transform;
				__instance.Hip_R = jbhHipR.transform;
			}

			var pelvis = __instance.Pelvis;
			var pelvisScl = pelvis?.Find($"{pelvis.name}_SCL_");
			if (pelvisScl != null)// && ButtJiggle.Experimental_PelvisEnabled.Value)
			{
				var jbhPelvis = JiggleBoneHelper.WrapBone(pelvisScl, __instance, MaidJiggleOverride.Slot.Pelvis, Quaternion.Euler(0, 0, 60), useSubBone: false);
			}
		}

		[HarmonyPrefix, HarmonyPatch(nameof(TBody.LateUpdate))]
		static void LateUpdate(TBody __instance)
		{
			if (__instance.TryGetHipHelpers(out var jbhHipL, out var jbhHipR))
			{
				jbhHipL.LateUpdateSelf();
				jbhHipR.LateUpdateSelf();
			}

			if (__instance.TryGetPelvisHelper(out var jbhPelvis))
			{
				jbhPelvis.LateUpdateSelf();
			}
		}


		[HarmonyPrefix, HarmonyPatch(nameof(TBody.BoneMorph_FromProcItem))]
		static void BoneMorph_FromProcItem(TBody __instance, string tag, float f)
		{
			if (__instance.boMAN) return;

			if (__instance.TryGetHipHelpers(out var jbhHipL, out var jbhHipR))
			{
				if (tag == "HipYawaraka")
				{
					jbhHipL.Jiggle.m_fMuneYawaraka = f;
					jbhHipR.Jiggle.m_fMuneYawaraka = f;
				}
				if (tag == "koshi")
				{
					ButtJiggle.Logger.LogDebug($"koshi = {f}");
					jbhHipL.Jiggle.BlendValue = Mathf.Clamp(f + .5f, 0f, 1.5f);
					jbhHipR.Jiggle.BlendValue = Mathf.Clamp(f + .5f, 0f, 1.5f);
				}
			}

			if (__instance.TryGetPelvisHelper(out var jbhPelvis))
			{
				if (tag == "HipYawaraka")
				{
					jbhPelvis.Jiggle.m_fMuneYawaraka = f;
				}
				if (tag == "koshi")
				{
					jbhPelvis.Jiggle.BlendValue = Mathf.Clamp(f + .5f, 0f, 1.5f);
				}
			}
		}

		[HarmonyPrefix, HarmonyPatch(nameof(TBody.Update))]
		static void Update(TBody __instance)
		{
			if (__instance.TryGetHipHelpers(out var jbhHipL, out var jbhHipR))
			{
				jbhHipL.Jiggle.boBRA = !__instance.boVisible_XXX;
				jbhHipR.Jiggle.boBRA = !__instance.boVisible_XXX;
			}

			if (__instance.TryGetPelvisHelper(out var jbhPelvis))
			{
				jbhPelvis.Jiggle.boBRA = !__instance.boVisible_XXX;
			}
		}

		[HarmonyPrefix, HarmonyPatch(nameof(TBody.WarpInit))]
		static void WarpInit(TBody __instance)
		{
			if (__instance.TryGetHipHelpers(out var jbhHipL, out var jbhHipR))
			{
				jbhHipL.Jiggle.boWarpInit = true;
				jbhHipR.Jiggle.boWarpInit = true;
			}

			if (__instance.TryGetPelvisHelper(out var jbhPelvis))
			{
				jbhPelvis.Jiggle.boWarpInit = true;
			}
		}
	}
}
