[System.Serializable]
public struct TabbedMenuIDs
{
    // UXML selector for a clickable tab
    public string tabClassName;// = "tab";

    // UXML selector for currently selected tab 
    public string selectedTabClassName; //= "selected-tab";

    // UXML selector for content to hide
    public string unselectedContentClassName; // = "unselected-content";

    // use a basename to pair a tab with its content, e.g. 'name1-tab' matches 'name1-content'

    // suffix naming convention for tabs
    public string tabNameSuffix;// = "-tab";

    // suffix naming convention for content
    public string contentNameSuffix;// = "-content";

}