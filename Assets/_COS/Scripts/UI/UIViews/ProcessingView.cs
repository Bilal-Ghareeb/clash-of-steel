using UnityEngine;
using UnityEngine.UIElements;

public class ProcessingView : UIView
{
    private VisualElement m_processingPanel;

    public ProcessingView(VisualElement topElement, bool hideOnAwake = true) : base(topElement, hideOnAwake)
    {

    }

    protected override void SetVisualElements()
    {
        base.SetVisualElements();
        m_processingPanel = m_TopElement.Q<VisualElement>("Panel");
    }

    public override void Show()
    {
        base.Show();
        m_processingPanel.transform.scale = new Vector3(0.1f, 0.1f, 0.1f);
        m_processingPanel.experimental.animation.Scale(1f, 200);
    }
}
