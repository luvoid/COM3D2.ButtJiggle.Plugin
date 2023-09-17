using BepInEx;
using CM3D2.UGUI;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UniverseLib.UI;
using UniverseLib.UI.Panels;
using COM3D2.ButtJiggle.UI;
using UniverseLib.UI.Styles;
using CM3D2.UGUI.Resources;
using CM3D2.UGUI.Panels;

namespace COM3D2.ButtJiggle
{
	public sealed partial class ButtJiggle : BaseUnityPlugin
	{
		public static UIBase UIBase => Instance?.m_UIBase;
		private UIBase m_UIBase;
		private PluginPanel m_PluginPanel;

		void Start()
		{
			CM3D2Universe.Init(OnUIStart);
		}

		void Update()
		{
			if (UIHotkey.Value.IsDown())
			{
				ToggleUI();
			}
		}

		void OnUIStart()
		{
			// Create a UIBase and specify update callback
			m_UIBase = CM3D2UniversalUI.RegisterUI(PluginInfo.PLUGIN_GUID);
			
			// Is the panel displayed by default?
			m_UIBase.Enabled = false;

			// Create the main panel
			m_PluginPanel = new PluginPanel(m_UIBase);
		}

		internal void EnableUI()
		{

			if (m_UIBase != null)m_UIBase.Enabled = true;
			if (m_PluginPanel != null)
			{
				m_PluginPanel.Enabled = true;
				m_PluginPanel.EnsureValidPosition();
				m_PluginPanel.EnsureValidSize();
			}
		}

		internal void DisableUI()
		{
			if (m_UIBase != null) m_UIBase.Enabled = false;
			if (m_PluginPanel != null) m_PluginPanel.Enabled = false;
		}

		internal void ToggleUI()
		{
			if (m_UIBase == null) return;

			if (m_UIBase.Enabled)
			{
				DisableUI();
			}
			else
			{
				EnableUI();
			}
		}
	}

	internal class PluginPanel : CM3D2Panel
	{
		public PluginPanel(UIBase owner) : base(owner) { }

		public override string Name => PluginInfo.PLUGIN_NAME;
		public override Vector2 PreferredSize => base.PreferredSize + new Vector2(10, 10);
		public override Vector2 DefaultAnchorMin => new(1f, 0.5f);
		public override Vector2 DefaultAnchorMax => DefaultAnchorMin;
		public override Vector2 DefaultPosition => new Vector2(-PreferredSize.x - 50, PreferredSize.y / 2);
		public override bool CanDragAndResize => true;

		private CanvasGroup globalOverrideCanvasGroup;

		protected override void OnClosePanelClicked()
		{
			ButtJiggle.Instance.DisableUI();
		}

		protected override void ConstructPanelContent()
		{
			try
			{
				SafeConstructPanelContent();
			}
			catch (System.Exception ex)
			{
				ButtJiggle.Logger.LogError(ex);
			}
		}

		protected void SafeConstructPanelContent()
		{
			//Text myText = UIFactory.CreateLabel(ContentRoot, "myText", "Hello world");
			//UIFactory.SetLayoutElement(myText.gameObject, minWidth: 200, minHeight: 25);

			Create.BoolControl(ContentRoot, "DebugMode", "Debug Mode", refGet: () => ref JiggleBoneHelper.DebugMode);

			var verticalFrame = Create.VerticalFrame(ContentRoot, "Overrides");
			var overrideColumn = verticalFrame.ContentRoot;
			//UIFactory.SetLayoutGroup<VerticalLayoutGroup>(overrideColumn, false, false, true, true);
			UIFactory.SetLayoutElement(overrideColumn, minWidth: 300, minHeight: 300);

			Create.BoolControl(overrideColumn, "UseGlobalOverride", "Use Global Override",
				refGet: () =>
				{
					ref var value = ref JiggleBoneHelper.UseGlobalOverride;
					globalOverrideCanvasGroup.interactable = value;
					return ref value;
				},
				onSet: OnGlobalOverrideUIUpdated,
				listenForUpdate: ButtJiggle.Instance.OnGlobalOverrideUpdated
			);


			var globalOverrideGroup = Create.VerticalGroup(overrideColumn, "GlobalOverrideCanvasGroup");
			globalOverrideCanvasGroup = UIFactory.SetCanvasGroup(globalOverrideGroup, interactable: JiggleBoneHelper.UseGlobalOverride);
			using (Create.LayoutContext(flexibleWidth: 1))
			{
				List<GameObject> slotColumns = null;

				var slotDropdown = Create.Dropdown(globalOverrideGroup, "SlotSelectDropdown");
				slotDropdown.OnValueChanged += (slotIndex) =>
				{
					for (int i = 0; i < slotColumns.Count; i++)
					{
						slotColumns[i].SetActive(i == slotIndex);
					}
				};

				slotColumns = new List<GameObject>() {
					CreateJiggleBoneOverrideColumn(globalOverrideGroup, "Hips"  , () => ref JiggleBoneHelper.GlobalOverride.HipOverride   , OnGlobalOverrideUIUpdated, ButtJiggle.Instance.OnGlobalOverrideUpdated),
					CreateJiggleBoneOverrideColumn(globalOverrideGroup, "Pelvis", () => ref JiggleBoneHelper.GlobalOverride.PelvisOverride, OnGlobalOverrideUIUpdated, ButtJiggle.Instance.OnGlobalOverrideUpdated),
				};

				int i = 0;
				slotDropdown.Component.AddOptions(
					slotColumns.Select((go) =>
					{
						ButtJiggle.Logger.LogDebug($"Add slotSelect {go.name}");
						go.SetActive(i++ == 0);
						return go.name;
					}).ToList()
				);
				slotDropdown.Component.RefreshShownValue();
				slotDropdown.Component.Show();
			}
		}

		delegate ref JiggleBoneOverride OverrideRefGetter();

		private GameObject CreateJiggleBoneOverrideColumn(
			GameObject parent,
			string name,
			OverrideRefGetter refGet,
			System.Action onSet,
			UnityEvent listen = null)
		{
			var column = Create.VerticalGroup(parent, name);
			using (Create.LayoutContext(flexibleWidth: 1))
			{
				Create.OverrideProperty(column, "Blend Value (auto)", refGet: () => ref refGet().BlendValue      ,  onSet, listen);
				Create.OverrideProperty(column, "Blend Value 2"     , refGet: () => ref refGet().BlendValue2     ,  onSet, listen);
				Create.OverrideProperty(column, "Gravity"           , refGet: () => ref refGet().Gravity         ,  onSet, listen);
				Create.OverrideProperty(column, "Clothed Stiffness" , refGet: () => ref refGet().ClothedStiffness,  onSet, listen);
				Create.OverrideProperty(column, "Naked Stiffness"   , refGet: () => ref refGet().NakedStiffness  ,  onSet, listen);
				Create.OverrideProperty(column, "Softness"          , refGet: () => ref refGet().Softness        ,  onSet, listen);
				Create.OverrideProperty(column, "Up & Down"         , refGet: () => ref refGet().UpDown          ,  onSet, listen);
				Create.OverrideProperty(column, "Yori"              , refGet: () => ref refGet().Yori            ,  onSet, listen);
				Create.OverrideProperty(column, "Squash & Stretch"  , refGet: () => ref refGet().SquashAndStretch,  onSet, listen);
				Create.OverrideProperty(column, "Front Stretch"     , refGet: () => ref refGet().FrontStretch    ,  onSet, listen);
				Create.OverrideProperty(column, "Side Stretch"      , refGet: () => ref refGet().SideStretch     ,  onSet, listen);
				Create.OverrideProperty(column, "Enable Scale X"    , refGet: () => ref refGet().EnableScaleX    ,  onSet, listen);
				Create.OverrideProperty(column, "Limit Rotation"    , refGet: () => ref refGet().LimitRotation   ,  onSet, listen);
				Create.OverrideProperty(column, "Limit Rot Decay"   , refGet: () => ref refGet().LimitRotDecay   ,  onSet, listen);
			}
			return column;
		}

		private void OnGlobalOverrideUIUpdated()
		{
			ButtJiggle.ConfigSaveGlobalOverride();
			globalOverrideCanvasGroup.interactable = JiggleBoneHelper.UseGlobalOverride;
		}

		// override other methods as desired
	}
}
