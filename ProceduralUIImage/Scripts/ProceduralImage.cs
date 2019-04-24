using UnityEngine;
using UnityEngine.UI;

/* Author: Josh H.
 * Procedural UI Image
 * assetstore.joshh@gmail.com for feedback or questions
 */

namespace UnityEngine.UI {

    [ExecuteInEditMode]
    [AddComponentMenu("UI/Procedural Image")]
	public class ProceduralImage : ImageProxy {
        public Sprite sprite//procedualSprite
        {
            get { return m_sprite; }
            set { m_sprite = value;Init(); }
        }
        public Sprite m_sprite;
		[SerializeField]private float borderWidth;
		private ProceduralImageModifier modifier;
		private Material materialInstance;
		private Material materialInstanceClip;
		[SerializeField]private float falloffDistance = 1;
        public bool needClipping = false;
        public float BorderWidth {
			get {
				return borderWidth;
			}
			set {
				borderWidth = value;
				this.SetMaterialDirty();
			}
		}

		public float FalloffDistance {
			get {
				return falloffDistance;
			}
			set {
				falloffDistance = value;
				this.SetMaterialDirty();
			}
		}

		protected ProceduralImageModifier Modifier {
			get {
				if (modifier == null) {
					//try to get the modifier on the object.
					modifier = this.GetComponent<ProceduralImageModifier>();
					//if we did not find any modifier
					if(modifier == null){
						//Add free modifier
						ModifierType = typeof(FreeModifier);
					}
				}
				return modifier;
			}
			set{
				modifier = value;
			}
		}

		/// <summary>
		/// Gets or sets the type of the modifier. Adds a modifier of that type.
		/// </summary>
		/// <value>The type of the modifier.</value>
		public System.Type ModifierType {
			get {
				return Modifier.GetType();
			}
			set {
				if(this.GetComponent<ProceduralImageModifier>()!=null){
					Destroy(this.GetComponent<ProceduralImageModifier>());
				}
				this.gameObject.AddComponent(value);
				Modifier = this.GetComponent<ProceduralImageModifier>();
				this.SetAllDirty();
			}
		}

		override protected void OnEnable(){
			base.OnEnable ();
			this.Init ();
		}

		/// <summary>
		/// Initializes this instance.
		/// </summary>
		public void Init (){
            if (m_sprite != null)
				base.sprite = m_sprite;
            else
			    base.sprite = EmptySprite.Get();
			if (materialInstance == null) {
				materialInstance = new Material (Shader.Find("UI/Procedural UI Image"));
			}
			if (materialInstanceClip == null) {
				materialInstanceClip = new Material (Shader.Find("UI/Procedural UI Image Clip"));
			}
			this.material = materialInstance;
		}
		public void Update(){
			if (needClipping) {
				this.material = materialInstanceClip;
			} else {
				this.material = materialInstance;
			}
			this.UpdateMaterial ();
		}
		/// <summary>
		/// Prevents radius to get bigger than rect size
		/// </summary>
		/// <returns>The fixed radius.</returns>
		/// <param name="vec">border-radius as Vector4 (starting upper-left, clockwise)</param>
		private Vector4 FixRadius(Vector4 vec){
			Rect r = this.rectTransform.rect;
			vec = new Vector4 (Mathf.Max(vec.x,0),Mathf.Max(vec.y,0),Mathf.Max(vec.z,0),Mathf.Max(vec.w,0));
			//float maxRadiusSums = Mathf.Max (vec.x,vec.z) + Mathf.Max (vec.y,vec.w);
			float scaleFactor = Mathf.Min(r.width/(vec.x+vec.y),r.width/(vec.z+vec.w),r.height/(vec.x+vec.w),r.height/(vec.z+vec.y),1);
			return vec*scaleFactor;
		}
		protected override void OnPopulateMesh(VertexHelper toFill)
		{
			//note: Sliced and Tiled have no effect to this currently.

			if (overrideSprite == null)
			{
				base.OnPopulateMesh(toFill);
				return;
			}
			
			switch (type)
			{
			case Type.Simple:
				GenerateSimpleSprite(toFill);
				break;
			case Type.Sliced:
				GenerateSimpleSprite(toFill);
				break;
			case Type.Tiled:
				GenerateSimpleSprite(toFill);
				break;
			case Type.Filled:
				base.OnPopulateMesh(toFill);
				break;
			}
		}
		#if UNITY_EDITOR
		protected override void Reset (){
			base.Reset ();
			OnEnable ();
		}
		#endif
		/// <summary>
		/// Generates the Verticies needed.
		/// </summary>
		/// <param name="vh">vertex helper</param>
		void GenerateSimpleSprite(VertexHelper vh){
			var r = GetPixelAdjustedRect();
			var v = new Vector4(r.x, r.y, r.x + r.width, r.y + r.height);
			var uv = new Vector4 (0,0,1,1);
            if (needClipping)
            {
				uv *= 10000;
                float a = r.x / r.y;
                float b = base.sprite.bounds.size.x / base.sprite.bounds.size.y;
                float c = 0.0f;
                if (a < b)
                {
					c = ( b - a ) / b * 0.5f;//(r.y * b - r.x) / (r.y * b * 2.0f);
                    uv += new Vector4(c, 0, 1.0f - c, 1.0f);
                }
                else
                {
					c = (a - b) / a * 0.5f;//(r.x / b - r.y) / (r.x * 2.0f / b);
                    uv += new Vector4(0, c, 1.0f, 1.0f - c);
                }
            }
			float aa = falloffDistance/2f;
			var color32 = this.color;
			vh.Clear();
			vh.AddVert(new Vector3(v.x-aa, v.y-aa), color32, new Vector2(uv.x, uv.y));
			vh.AddVert(new Vector3(v.x-aa, v.w+aa), color32, new Vector2(uv.x, uv.w));
			vh.AddVert(new Vector3(v.z+aa, v.w+aa), color32, new Vector2(uv.z, uv.w));
			vh.AddVert(new Vector3(v.z+aa, v.y-aa), color32, new Vector2(uv.z, uv.y));

			vh.AddTriangle(0, 1, 2);
			vh.AddTriangle(2, 3, 0);
		}
		/// <summary>
		/// Sets the material values of shader.
		/// Implementation of IMaterialModifier
		/// </summary>
		public override Material GetModifiedMaterial (Material baseMaterial){
			Rect rect = this.GetComponent<RectTransform> ().rect;
			//get world-space corners of rect
			Vector3[] corners = new Vector3[4];
			rectTransform.GetWorldCorners (corners);
			float pixelSize = Vector3.Distance (corners [1], corners [2]) / rect.width;
			pixelSize = pixelSize/falloffDistance;

			Vector4 radius = FixRadius (Modifier.CalculateRadius (rect));
			Material m = MaterialHelper.SetMaterialValues (new ProceduralImageMaterialInfo(rect.width+falloffDistance,rect.height+falloffDistance,Mathf.Max(pixelSize,0),radius,Mathf.Max(borderWidth,0)),baseMaterial);
			return base.GetModifiedMaterial (m);
		}
	}
}
