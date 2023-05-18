[![CodeFactor](https://www.codefactor.io/repository/github/bugrahanakbulut/unity-infinite-scroll/badge/main)](https://www.codefactor.io/repository/github/bugrahanakbulut/unity-infinite-scroll/overview/main)

# Dynamic Scroll View
Unity UI Optimized Scroll Rect to represent large number of entities with less rect transform. 

DynamicScrollRect does not rely any layout group element to keep entities organized. You can use vertical, horizontal or grid orientation without using any built-in layout element.

![Alt Text](https://github.com/bugrahanakbulut/Unity-Infinite-Scroll/blob/main/Assets/Resources/scroll_infinite.gif)
![Alt Text](https://github.com/bugrahanakbulut/Unity-Infinite-Scroll/blob/main/Assets/Resources/scroll_jumpback.gif)
![Alt Text](https://github.com/bugrahanakbulut/Unity-Infinite-Scroll/blob/main/Assets/Resources/horizontal_scroll.gif)


## For Custom Usage

By inheriting from DynamicScrollItemData and DynamicScrollItem<T> you can create your custom scroll entities. You should be aware of DynamicScrollItem inherited from monobehaviour so its' file name and class name must be identical to attach component to game object.

```cs

public class CustomDynamicScrollItemData : DynamicScrollItemData
{
    // Some arbitrary fields and properties
    public CustomDynamicScrollItemData(int index) : base(index)
    {
        
    }
}

public class CustomDynamicScrollItem : DynamicScrollItem<CustomDynamicScrollItemData> 
{
    protected override void OnInit(CustomScrollItemData data)
    {
        base.OnInit(data);
    }

    protected override void OnActivate()
    {
        base.OnActivate();
    }

    protected override void OnDeactivate()
    {
        base.OnDeactivate();
    }
}
```
    
## Focusing Items 
    
Dynamic scroll rect can make focus easily when entities outside of the viewport. You can add offset to focus or determine focus duration from focus settings on DynamicScrollRect component.
    
<p align="center">
    <img src="https://github.com/bugrahanakbulut/Unity-Infinite-Scroll/blob/main/Assets/Resources/scroll_focus.gif" alt="animated" />
</p>