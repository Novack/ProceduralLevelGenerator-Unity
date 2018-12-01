﻿namespace Assets.Editor.RoomsEditor.EditorNodes
{
	using System;
	using System.Linq;
	using NodeBasedEditor;
	using Scripts.Data2;
	using UnityEditor;
	using UnityEngine;
	using UnityEngine.Tilemaps;

	public class RoomNode : IEditorNode
	{
		public readonly Room Data;
		public Rect Rect;
		private readonly GUIStyle style;

		public Action<RoomNode> OnDeleted;

		public RoomNode(Room data, GUIStyle style)
		{
			Data = data;
			this.style = style;
		}

		public bool ProcessEvents(Event e)
		{
			switch (e.type)
			{
				case EventType.MouseDown:
					if (e.button == 0)
					{
						if (Rect.Contains(e.mousePosition))
						{
							Selection.activeObject = Data;
							e.Use();
						}
					}

					if (e.button == 1)
					{
						if (Rect.Contains(e.mousePosition))
						{
							ProcessContextMenu();
							e.Use();
						}
					}

					break;

				case EventType.DragUpdated:
				case EventType.DragPerform:

					if (!Rect.Contains(e.mousePosition))
					{
						break;
					}

					DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

					if (e.type == EventType.DragPerform)
					{
						DragAndDrop.AcceptDrag();
						var draggedObject = DragAndDrop.objectReferences.FirstOrDefault();

						if (draggedObject != null && draggedObject is GameObject)
						{
							Data.Tilemap = (GameObject) draggedObject;
						}
					}

					break;
			}

			return false;
		}

		private void ProcessContextMenu()
		{
			var genericMenu = new GenericMenu();
			genericMenu.AddItem(new GUIContent("Delete room"), false, OnClickDelete);
			genericMenu.ShowAsContext();
		}

		private void OnClickDelete()
		{
			OnDeleted?.Invoke(this);
		}

		public void Draw()
		{
			GUI.Box(Rect, Data.Tilemap?.name, style);

			if (Data.Tilemap != null)
			{
				var bounds = Data.Tilemap.GetComponentInChildren<Tilemap>().cellBounds;
				var correction = 20;
				var yOffset = 10;
				var xPerTile = (Rect.width - correction) / bounds.size.x;
				var yPerTile = (Rect.height - correction - yOffset) / bounds.size.y;
				var sizePerTile = (int)Math.Min(xPerTile, yPerTile);
				var width = sizePerTile * bounds.size.x;
				var height = sizePerTile * bounds.size.y;

				foreach (var tilemap in Data.Tilemap.GetComponentsInChildren<Tilemap>())
				{
					var allTiles = tilemap.GetTilesBlock(bounds);
					var startingPoint = Rect.center - new Vector2(width / 2, height / 2 - yOffset / 2);

					for (int x = 0; x < bounds.size.x; x++)
					{
						for (int y = 0; y < bounds.size.y; y++)
						{
							var tile = (Tile)allTiles[x + (bounds.size.y - y - 1) * bounds.size.x];
							if (tile != null)
							{
								DrawTexturePreview(new Rect(startingPoint.x + x * sizePerTile, startingPoint.y + y * sizePerTile, sizePerTile, sizePerTile), tile.sprite);
							}
						}
					}
				}
			}
		}

		private void DrawTexturePreview(Rect position, Sprite sprite)
		{
			Vector2 fullSize = new Vector2(sprite.texture.width, sprite.texture.height);
			Vector2 size = new Vector2(sprite.textureRect.width, sprite.textureRect.height);

			Rect coords = sprite.textureRect;
			coords.x /= fullSize.x;
			coords.width /= fullSize.x;
			coords.y /= fullSize.y;
			coords.height /= fullSize.y;

			Vector2 ratio;
			ratio.x = position.width / size.x;
			ratio.y = position.height / size.y;
			float minRatio = Mathf.Min(ratio.x, ratio.y);

			Vector2 center = position.center;
			position.width = size.x * minRatio;
			position.height = size.y * minRatio;
			position.center = center;

			GUI.DrawTextureWithTexCoords(position, sprite.texture, coords);
		}

		public void Drag(Vector2 delta)
		{
			Rect.position += delta;
		}
	}
}