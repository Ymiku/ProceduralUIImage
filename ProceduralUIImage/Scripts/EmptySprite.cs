using UnityEngine;
using System.Collections;

public static class EmptySprite {
	static Sprite instance;

	///<summary>
	/// Returns the instance of a (1 x 1) white Spprite
	/// </summary>	
	public static Sprite Get(){
		if (instance == null) {
			instance = OnePixelWhiteSprite();
		}
		return instance;
	}
	/// <summary>
	/// Generates a white sprite (1 x 1).
	/// </summary>
	/// <returns>A white sprite (1 x 1).</returns>
	static Sprite OnePixelWhiteSprite(){
		Texture2D tex = new Texture2D (1,1);
		tex.SetPixel (0,0,Color.white);
		tex.Apply ();
		return Sprite.Create(tex,new Rect(0,0,1,1),Vector2.zero);
	}
}
