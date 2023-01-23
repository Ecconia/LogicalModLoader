using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LogicalModLoader.Client.PrimitiveDebuggingOverlay
{
	public class PrimitiveDebuggingScreen
	{
		private readonly StringBuilder textBuilder;
		private readonly TextMeshProUGUI textMesh;

		public PrimitiveDebuggingScreen()
		{
			textBuilder = new StringBuilder();

			GameObject root;
			{
				root = new GameObject("LogicalModManager: Debug screen");
				root.SetActive(false);
				root.AddComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
				CanvasScaler scaler = root.AddComponent<CanvasScaler>();
				scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
				scaler.referenceResolution = new Vector2(3840, 2160);
				scaler.matchWidthOrHeight = 1;

				//Needs a raycaster, to prevent underlying canvases from being clickable:
				root.AddComponent<GraphicRaycaster>(); //TODO: Make it work...
			}
			GameObject content;
			{
				content = new GameObject();
				content.transform.SetParent(root.transform);

				RectTransform rect = content.AddComponent<RectTransform>();
				rect.anchorMin = new Vector2(0.1f, 0.1f);
				rect.anchorMax = new Vector2(0.9f, 0.9f);
				rect.pivot = new Vector2(.5f, .5f);
				rect.sizeDelta = new Vector2(0, 0);
				rect.localPosition = new Vector3(0, 0, 0);
			}
			{
				GameObject background = new GameObject();
				background.transform.SetParent(content.transform);
				RectTransform rect2 = background.AddComponent<RectTransform>();
				rect2.anchorMin = new Vector2(0f, 0f);
				rect2.anchorMax = new Vector2(1f, 1f);
				rect2.pivot = new Vector2(.5f, .5f);
				rect2.sizeDelta = new Vector2(0, 0);
				rect2.localPosition = new Vector3(0, 0, 0);

				background.AddComponent<Image>().color = new Color(0f, 1f, 0f, 0.4f);
			}
			{
				// GameObject center = new GameObject();
				// center.transform.SetParent(content.transform);
				// RectTransform rect = center.AddComponent<RectTransform>();
				// rect.anchorMin = new Vector2(0.5f, 0.5f);
				// rect.anchorMax = new Vector2(.5f, .5f);
				// rect.pivot = new Vector2(.5f, .5f);
				// rect.sizeDelta = new Vector2(30, 30);
				// rect.localPosition = new Vector3(0, 0, 0);
				//
				// center.AddComponent<Image>().color = new Color(1f, 0f, 0f, 0.4f);
			}
			{
				GameObject go = new GameObject();
				go.transform.SetParent(content.transform);

				RectTransform rect = go.AddComponent<RectTransform>();
				rect.anchorMin = new Vector2(0f, 0f);
				rect.anchorMax = new Vector2(1f, 1f);
				rect.pivot = new Vector2(0.5f, 0.5f);
				rect.sizeDelta = new Vector2(0, 0);
				rect.localPosition = new Vector3(0, 0, 0);

				textMesh = go.AddComponent<TextMeshProUGUI>();
				textMesh.fontSize = 100;
				textMesh.verticalAlignment = VerticalAlignmentOptions.Top;
				textMesh.horizontalAlignment = HorizontalAlignmentOptions.Left;
				textMesh.autoSizeTextContainer = false;
				textMesh.enableWordWrapping = false;
				textMesh.text = "<i>Successfully hijacked LogicWorld!</i>\n";
				textMesh.color = Color.white;
			}
			root.SetActive(true);
		}

		public void append(string text)
		{
			textBuilder.Append(text).Append('\n');
			textMesh.text = textBuilder.ToString();
		}
	}
}
