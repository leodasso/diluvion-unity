using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using SpiderWeb;
using HeavyDutyInspector;


public class DUIObjectScroller : MonoBehaviour
{ 
    public RectTransform maskGroup;
    public RectTransform scrollContentParent;
    public int currentChild = 0;
    public float visibleChildBuffer = 1;
    public float bufferMoveSeconds = 0.5f;
    public float scrollSeconds = 0.5f;

    [ComplexHeader("Element Options", Style.Box, Alignment.Left, ColorEnum.White, ColorEnum.Blue)]
    public GameObject objectTemplate;

   // RectTransform firstBufferObject;
  //  RectTransform lastBufferObject;
    Rect templateRect;


    float totalElementSpacing = 0;
    float stepSize = 0;
    HorizontalOrVerticalLayoutGroup currentLayoutGroup;
    //public int visibleObjectNeighbours

    #region test variables

    [Button("Init Values", "SetGlobalValues", true)]
    public bool startbool;

    [Button("Scroll Next", "ScrollNext", true)]
    public bool scrollNextBool = false;

    [Button("Scroll Previous", "ScrollPrevious", true)]
    public bool scrollPrevBool = false;

    public Sprite test;
    [Button("Add test Object", "AddTestGO", true)]
    public bool addToScroll;

    public void AddTestGO()
    {
        AddElement(test);

    }
    #endregion

    #region init
    public void Awake()
    {
        SetGlobalValues();

    }

    //Adds an element to the scrollableList and sets it up
    public virtual GameObject AddElement(Sprite go)
    {
        GameObject newElementObject = Instantiate(objectTemplate);
        Image image = newElementObject.gameObject.AddComponent<Image>();
        image.sprite = go;

        newElementObject.GetComponent<RectTransform>().SetParent(scrollContentParent);
        return newElementObject;
    }

  

    //Set all the global values for initiation
    public void SetGlobalValues()
    {
        TotalGroupSize();
        templateRect = objectTemplate.GetComponent<RectTransform>().rect;
       
        SetMaskSize();
        SetPositionInstant(currentChild);

    }
   
    //Set the size of the parent mask based on how many children we want to show
    public void SetMaskSize()
    {
        if (maskGroup == null) return;
        float elementSum = ((visibleChildBuffer * 2) * GetElementSize()) + GetElementSize();
        float bufferSum = (visibleChildBuffer * 2) * SpacingSize();
        RectTransform.Axis axis;

        if (IsHorizontal())
            axis = RectTransform.Axis.Horizontal;
        else
            axis = RectTransform.Axis.Vertical;

        maskGroup.SetSizeWithCurrentAnchors(axis, bufferSum + elementSum);
    }

    #endregion

    #region Get functions

    float GetPositionOf(int child)
    {
      //  Debug.Log("Getting position of child:"+child+ ":  "+ (child * -TotalElementSpacing() + " / " + TotalElementSpacing() * visibleChildBuffer));
        return (child * -TotalElementSpacing()) + TotalElementSpacing() * visibleChildBuffer;
    }

    float GetElementSize()
    {
        float returnSpacing = 0;
        if (templateRect == null)
            templateRect = objectTemplate.GetComponent<RectTransform>().rect;
        if (IsHorizontal())
            returnSpacing = templateRect.width;
        else
            returnSpacing = templateRect.height;        
        return returnSpacing;
    }
    


    //Gets the element count in the scroll bar
    int ElementCount()
    {
        return scrollContentParent.childCount;
    }

    //Gets the spacing size from the scrollContentparent
    float SpacingSize()
    {
        if (currentLayoutGroup == null) currentLayoutGroup = scrollContentParent.gameObject.GetComponent<HorizontalOrVerticalLayoutGroup>();
        return currentLayoutGroup.spacing;       
    }

    /// <summary>
    /// TODO CACHE THE TEMPLATE SPACING
    /// </summary>
    /// <returns></returns>
    //Gets the total width of an element with the spacing
     float TotalElementSpacing()
    {    
        totalElementSpacing = GetElementSize()+SpacingSize();  
        return totalElementSpacing;
    }

    bool IsHorizontal()
    {
        if (currentLayoutGroup == null) currentLayoutGroup = scrollContentParent.gameObject.GetComponent<HorizontalOrVerticalLayoutGroup>();
        if (currentLayoutGroup.GetType() == typeof(HorizontalLayoutGroup))
            return true;
        else return false;
    }

    //Total Size of the element Group 
    float TotalGroupSize()
    {
        return (ElementCount() * TotalElementSpacing()); //+ GetElementSize()/2;
    }


    #endregion





    #region Operations

  

    public void RemoveAllChildren()
    {
        foreach (RectTransform rt in GetComponentInChildren<RectTransform>())
            Destroy(rt.gameObject);
    }

    Vector2 targetPos = Vector3.zero;
    Vector2 startPos;
    Vector2 currentPos;
    public virtual void SetPositionTo(int childPos)
    {
        currentChild = childPos;
        targetPos = Vector3.zero;
        if (IsHorizontal())
            targetPos.x = GetPositionOf(childPos);
        else
            targetPos.y = GetPositionOf(childPos);

       // Debug.Log("Moving to " + targetPos + " from " + startPos);
        StopCoroutine("ScrollTo");
        StartCoroutine("ScrollTo",targetPos);
    }

    public void SetPositionInstant(int childPos)
    {
        currentChild = childPos;
        targetPos = Vector3.zero;
        if (IsHorizontal())
            targetPos.x = GetPositionOf(childPos);
        else
            targetPos.y = GetPositionOf(childPos);
        currentPos = targetPos;
        scrollContentParent.anchoredPosition = currentPos;
    }

    //ScrollTime
    float time = 0;
    bool scrollLerping;
    IEnumerator ScrollTo(Vector2 targetPos)
    {      
        //Debug.Log("startingCoroutine" + targetPos);
        time = 0;
        startPos = scrollContentParent.anchoredPosition;
        currentPos = startPos;
      
        while (time<scrollSeconds)        {
           
            currentPos = Vector2.Lerp(currentPos, targetPos, time / scrollSeconds);
          //  Debug.Log("CurrentPos is:  " + currentPos + " at " + time + "/" + scrollSeconds);
            scrollContentParent.anchoredPosition = currentPos;
            yield return new WaitForEndOfFrame();
            time += Time.deltaTime;
        }
        
    }

    public void ScrollNext()
    {
        if (ElementCount() < 2)
            return;
        if (currentChild < ElementCount() - Mathf.CeilToInt(visibleChildBuffer))
            currentChild++;
        else
            currentChild = 0;

        SetPositionTo(currentChild);

    } 

    public void ScrollPrevious()
    {
        if (ElementCount() < 2)
            return;
        if (currentChild > 0)
            currentChild--;
        else
            currentChild = ElementCount() - Mathf.CeilToInt(visibleChildBuffer);

        SetPositionTo(currentChild);

    }
    #endregion




    #region size lerping
    float showTime;
    public void ShowBufferTemp(int targetBuffer, float sTime)
    {
        showTime = sTime;
      
        StopCoroutine("TempShowBuffer");
        StartCoroutine("TempShowBuffer", targetBuffer);
    }


   int previousBufferAmount;
   float waitTime = 0;
   protected IEnumerator TempShowBuffer(int targetBuffer)
   {
       previousBufferAmount = 0;
       waitTime = 0;
    //SetTemporaryVisibleBuffer(targetBuffer, bufferMoveSeconds);
       visibleChildBuffer = targetBuffer;
       SetMaskSize();      
       while (waitTime<showTime)
       {
           yield return new WaitForEndOfFrame();
           waitTime += Time.deltaTime;
       }
        visibleChildBuffer = previousBufferAmount;
        SetMaskSize();
        SetPositionInstant(currentChild);
        // SetTemporaryVisibleBuffer(previousBufferAmount, bufferMoveSeconds);
    }


   float bufferTime;
    float targetBufferTime;
   public void SetTemporaryVisibleBuffer(int targetBuffer, float time)
   {
        targetBufferTime = time;
       StopCoroutine("LerptoBuffer");
       StartCoroutine("LerptoBuffer", targetBuffer);
   }

  
   float oldBufferAmount;
   IEnumerator LerptoBuffer(float targetBufferAmount)
   {
       bufferTime = 0;
       oldBufferAmount = visibleChildBuffer;

       while (bufferTime < targetBufferTime)
       {
           visibleChildBuffer = Calc.EaseInLerp(oldBufferAmount, targetBufferAmount, bufferTime / time);
           yield return new WaitForEndOfFrame();
           bufferTime += Time.deltaTime;
       }
   }
    #endregion

    #region extra code



    //Function for adding buffer elements to either side of the scroller
    /*  void ElementBuffer(DUIScrollObject newElementObject)
      {

          if (ElementCount() < 1)
          {
              firstBufferObject = CopyObject(null);//Creates an empty buffer object
              firstBufferObject.SetParent(scrollContentParent);          
              newElementObject.GetComponent<RectTransform>().SetParent(scrollContentParent);
          }
          else 
          {
              GameObject.DestroyImmediate(firstBufferObject.gameObject);
              firstBufferObject = CopyObject(newElementObject);//Creates an empty buffer object
              firstBufferObject.SetParent(scrollContentParent);          
              lastBufferObject = CopyObject(GetLastObject());
              lastBufferObject.SetParent(scrollContentParent);
              newElementObject.GetComponent<RectTransform>().SetParent(scrollContentParent);
          }
          realObjects.Add(newElementObject);


      }*/

    //Copies the object properties and returns its transform
    /*RectTransform CopyObject(DUIScrollObject newObject)
    {
        DUIScrollObject toCopy = objectTemplate;
        if (newObject != null) toCopy = newObject;

        DUIScrollObject newElementObject = Instantiate<DUIScrollObject>(toCopy);
        RectTransform newRectTrans = newElementObject.GetComponent<RectTransform>();
        return newRectTrans;
    }*/

    #endregion
}
