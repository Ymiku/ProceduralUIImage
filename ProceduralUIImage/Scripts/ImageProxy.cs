using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class ImageProxy : Image {
    public Color renderColor {
        get { return canvasRenderer.GetColor(); }
        set { canvasRenderer.SetColor(value); }
    }

}
