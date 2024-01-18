using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IconMaker : MonoBehaviour
{
	public Camera cam;
	public Dictionary<string, RenderTexture> icons = new Dictionary<string, RenderTexture>();
	
	void Start() {
		foreach (KeyValuePair<string, IItem> kvp in Database.items) {
			RenderTexture texture = new RenderTexture(64, 64, 16);

			texture.Create();

			string modelName = "";

			if (kvp.Value is Gun) {
				modelName = ((Gun) kvp.Value).model[0];
			} else if (kvp.Value is Tool) {
				modelName = ((Tool) kvp.Value).model;
			} else if (kvp.Value is Food) {
				modelName = ((Food) kvp.Value).model;
			} else if (kvp.Value is Throwable) {
				modelName = ((Throwable) kvp.Value).model;
			}

			if (Resources.Load("ItemModels/" + modelName)) {
				GameObject model = Instantiate(Resources.Load("ItemModels/" + modelName) as GameObject, transform);
				
				model.transform.localPosition = Vector3.zero;

				cam.targetTexture = texture;
				cam.Render();

				model.transform.localPosition = Vector3.up * -20;

				Destroy(model);
			}

			icons.Add(kvp.Key, texture);
		}
	}
}
