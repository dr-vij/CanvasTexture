using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class TextureGraphGroup : Group
{
    public string ID { get; set; }

    public TextureGraphGroup(string groupTitle, Vector2 position)
    {
        ID = Guid.NewGuid().ToString();
        title = groupTitle;

        SetPosition(new Rect(position, Vector2.zero));
    }
}
