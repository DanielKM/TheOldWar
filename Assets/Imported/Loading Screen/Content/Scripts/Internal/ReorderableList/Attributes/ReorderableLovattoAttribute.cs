using UnityEngine;

namespace Lovatto.SceneLoader
{

	public class ReorderableLovattoAttribute : PropertyAttribute {

		public bool add;
		public bool remove;
		public bool draggable;
		public bool singleLine;
		public bool paginate;
		public int pageSize;
		public string elementNameProperty;
		public string elementNameOverride;
		public string elementIconPath;

		public ReorderableLovattoAttribute()
			: this(null) {
		}

		public ReorderableLovattoAttribute(string elementNameProperty)
			: this(true, true, true, elementNameProperty, null, null) {
		}

		public ReorderableLovattoAttribute(string elementNameProperty, string elementIconPath)
			: this(true, true, true, elementNameProperty, null, elementIconPath) {
		}

		public ReorderableLovattoAttribute(string elementNameProperty, string elementNameOverride, string elementIconPath)
			: this(true, true, true, elementNameProperty, elementNameOverride, elementIconPath) {
		}

		public ReorderableLovattoAttribute(bool add, bool remove, bool draggable, string elementNameProperty = null, string elementIconPath = null) 
			: this(add, remove, draggable, elementNameProperty, null, elementIconPath) {
		}

		public ReorderableLovattoAttribute(bool add, bool remove, bool draggable, string elementNameProperty = null, string elementNameOverride = null, string elementIconPath = null) {

			this.add = add;
			this.remove = remove;
			this.draggable = draggable;
			this.elementNameProperty = elementNameProperty;
			this.elementNameOverride = elementNameOverride;
			this.elementIconPath = elementIconPath;
		}
	}
}