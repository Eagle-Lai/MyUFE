using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

/// <summary>
/// UFE 渐变效果（UFEGradient，UI/Effects 组件）。
/// <para>用途：为 UGUI 图形（Image/Text 等）提供垂直或水平方向的顶点颜色渐变效果。</para>
/// <para>继承 BaseMeshEffect，通过修改网格顶点颜色实现渐变，支持偏移（Offset）调节渐变中心。</para>
/// </summary>
[AddComponentMenu("UI/Effects/UFE Gradient")]
public class UFEGradient : BaseMeshEffect {
	/// <summary>渐变方向类型。</summary>
    public enum Type {
		/// <summary>垂直渐变。</summary>
        Vertical,
		/// <summary>水平渐变。</summary>
        Horizontal
    }

	/// <summary>渐变方向（垂直/水平）。</summary>
    [SerializeField]
    public Type GradientType = Type.Vertical;

	/// <summary>渐变偏移（-1.5~1.5，调节渐变中心位置）。</summary>
    [SerializeField]
    [Range(-1.5f, 1.5f)]
    public float Offset = 0f;

	/// <summary>起始颜色（渐变一端）。</summary>
    [SerializeField]
    private Color32 StartColor = Color.white;
	/// <summary>结束颜色（渐变另一端）。</summary>
    [SerializeField]
    private Color32 EndColor = Color.black;

	/// <summary>
	/// 修改网格顶点颜色以实现渐变（垂直/水平方向）。
	/// </summary>
	/// <param name="helper">顶点辅助器。</param>
    public override void ModifyMesh(VertexHelper helper) {
        if (!IsActive() || helper.currentVertCount == 0)
            return;

        List<UIVertex> _vertexList = new List<UIVertex>();
        helper.GetUIVertexStream(_vertexList);

        int nCount = _vertexList.Count;
        switch (GradientType) {
            case Type.Vertical: {
                    float fBottomY = _vertexList[0].position.y;
                    float fTopY = _vertexList[0].position.y;
                    float fYPos = 0f;

                    for (int i = nCount - 1; i >= 1; --i) {
                        fYPos = _vertexList[i].position.y;
                        if (fYPos > fTopY)
                            fTopY = fYPos;
                        else if (fYPos < fBottomY)
                            fBottomY = fYPos;
                    }

                    float fUIElementHeight = 1f / (fTopY - fBottomY);
                    UIVertex v = new UIVertex();

                    for (int i = 0; i < helper.currentVertCount; i++) {
                        helper.PopulateUIVertex(ref v, i);
                        v.color = Color32.Lerp(EndColor, StartColor, (v.position.y - fBottomY) * fUIElementHeight - Offset);
                        helper.SetUIVertex(v, i);
                    }
                }
                break;
            case Type.Horizontal: {
                    float fLeftX = _vertexList[0].position.x;
                    float fRightX = _vertexList[0].position.x;
                    float fXPos = 0f;

                    for (int i = nCount - 1; i >= 1; --i) {
                        fXPos = _vertexList[i].position.x;
                        if (fXPos > fRightX)
                            fRightX = fXPos;
                        else if (fXPos < fLeftX)
                            fLeftX = fXPos;
                    }

                    float fUIElementWidth = 1f / (fRightX - fLeftX);
                    UIVertex v = new UIVertex();

                    for (int i = 0; i < helper.currentVertCount; i++) {
                        helper.PopulateUIVertex(ref v, i);
                        v.color = Color32.Lerp(EndColor, StartColor, (v.position.x - fLeftX) * fUIElementWidth - Offset);
                        helper.SetUIVertex(v, i);
                    }

                }
                break;
            default:
                break;
        }
    }
}
