using System;
using UnityEngine;
using UnityEditor.Experimental.GraphView;

#region DS_Group
public class DS_Group : Group
{
    public Guid GroupId { get; private set; }

    private Color defaultBorderColor;
    private float defaultBorderWidth;

    public DS_Group(string gTitle, Vector2 position)
    {
        GroupId = Guid.NewGuid();

        title = gTitle;

        SetPosition(new Rect(position, Vector2.zero));

        defaultBorderColor = contentContainer.style.borderBottomColor.value;
        defaultBorderWidth = contentContainer.style.borderBottomWidth.value;
    }

    #region ERROR HANDLING
    public void SetErrorStyle()
    {
        contentContainer.style.borderBottomColor = new Color(255f / 255f, 51f / 255f, 51f / 255f);
        contentContainer.style.borderBottomWidth = 3f;
    }

    public void ResetErrorStyle()
    {
        contentContainer.style.borderBottomColor = defaultBorderColor;
        contentContainer.style.borderBottomWidth = defaultBorderWidth;
    }
    #endregion
}
#endregion