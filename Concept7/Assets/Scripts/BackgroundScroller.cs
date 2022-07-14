using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundScroller : MonoBehaviour
{
    public UnityEngine.Experimental.Rendering.Universal.PixelPerfectCamera pixelCamera;
    public Camera standardCamera;
    public float mainScrollingSpeed = 2;
    public float secondaryScrollingSpeed = 2;

    [System.Serializable]
    public class ScrollingBackgroundData        //Stores the sprite used and repeat data.
    {
        public GameObject mainBackground;
        public GameObject secondaryBackground;
        public int repeatCount;
        public bool shouldrepeatNonstop;
        public bool shouldStopScrolling;
    }
    
    public ScrollingBackgroundData[] scrollingBackgrounds;

    public class ActiveBackground               //Stores created instantiated background prefab and associated data.
    {
        public ScrollingBackgroundData data;
        public GameObject gameObject;
        public bool isNewBackground; //HACK: Keeps track of whether or not this background is the one most recently made. Can't think of a better place to put this right now
    }
    private List<ActiveBackground> currentMainBackgrounds = new List<ActiveBackground>();
    private List<ActiveBackground> currentSecondaryBackgrounds = new List<ActiveBackground>();

    private int currentListEntry = 0;           //index of next scrollingBackgrounds entry to instantiate.
    private float cameraWidth;
    private float leftSide;
    private float rightSide;
    private bool stopScrollingAnim = false;
    
    //TODO (maybe?) In BackgroundData you assign a sprite, change to a gameobject instead so we can have more complex backgrounds
    void Start()
    {
        leftSide = standardCamera.ViewportToWorldPoint(new Vector3(0, 0.5f, standardCamera.nearClipPlane)).x;
        rightSide = standardCamera.ViewportToWorldPoint(new Vector3(1, 0.5f, standardCamera.nearClipPlane)).x;
        cameraWidth = standardCamera.orthographicSize * standardCamera.aspect;
        CreateNewBackground(scrollingBackgrounds[currentListEntry].mainBackground, currentMainBackgrounds);
        CreateNewBackground(scrollingBackgrounds[currentListEntry].secondaryBackground, currentSecondaryBackgrounds);
    }

    void FixedUpdate()
    {
        //loop over the currently instantiated background objects in the scene
        for(int index = 0; index < currentMainBackgrounds.Count; index++)
        {
            //Initialize vars
            ActiveBackground activeBackground = currentMainBackgrounds[index];
            GameObject backgroundObject = activeBackground.gameObject;
            float offsetXval = backgroundObject.transform.position.x - mainScrollingSpeed;
            float objectWidth = backgroundObject.GetComponent<SpriteRenderer>().bounds.size.x;

            if(offsetXval < (leftSide - (objectWidth / 2)))
            {
                //Object has passed viewport, get rid of it
                currentMainBackgrounds.Remove(activeBackground);
                Destroy(backgroundObject);

                if(currentSecondaryBackgrounds.Count > index)
                {
                    ActiveBackground secondaryBackground = currentSecondaryBackgrounds[index];
                    currentSecondaryBackgrounds.Remove(secondaryBackground);
                    Destroy(secondaryBackground.gameObject);
                }
            }
            else if(offsetXval < (rightSide - (objectWidth/2)) && activeBackground.isNewBackground)
            {
                //Current background is filling up viewport, should start up new one if applicable
                TryStartNewBackground(activeBackground, false);
            }
            
            if(!stopScrollingAnim)
            {
                //Update background position, and secondary background position if applicable.
                backgroundObject.transform.position = new Vector3(offsetXval, backgroundObject.transform.position.y, backgroundObject.transform.position.z);
                if(currentSecondaryBackgrounds.Count > index)
                {
                    GameObject secondaryBackgroundObject = currentSecondaryBackgrounds[index].gameObject;
                    float offset = secondaryBackgroundObject.transform.position.x - secondaryScrollingSpeed;
                    secondaryBackgroundObject.transform.position = new Vector3(offset, secondaryBackgroundObject.transform.position.y, secondaryBackgroundObject.transform.position.z);
                }
            }
        }
    }


    public void ForceNextBackground()
    {
        //Function call to be used in other scripts, forces next background gameObject to be next background in data list
        stopScrollingAnim = false;
        TryStartNewBackground(currentMainBackgrounds[currentMainBackgrounds.Count - 1], true);
    }

    public void TryStartNewBackground(ActiveBackground activeBackground, bool forceStart)
    {
        //Current background is filling up viewport, should start up new one if applicable
        UpdateScrollingValues(activeBackground, forceStart);

        if(currentListEntry < scrollingBackgrounds.Length && !stopScrollingAnim)
        {
            activeBackground.isNewBackground = false;
            CreateNewBackground(scrollingBackgrounds[currentListEntry].mainBackground, currentMainBackgrounds);
            CreateNewBackground(scrollingBackgrounds[currentListEntry].secondaryBackground, currentSecondaryBackgrounds);
        }
    }

    public void UpdateScrollingValues(ActiveBackground activeBackground, bool forceStart)
    {
        if(!forceStart && activeBackground.data.shouldStopScrolling)
        {
            stopScrollingAnim = true;
        }
        else if ((forceStart || !activeBackground.data.shouldrepeatNonstop) && activeBackground.data.repeatCount <= 0)
        {
            currentListEntry++;
        }
        else if(activeBackground.data.repeatCount > 0)
        {
            activeBackground.data.repeatCount--;
        }
    }

    void CreateNewBackground(GameObject newBackground, List<ActiveBackground> backgrounds)
    {
        if(newBackground != null)
        {
            float backgroundWidth = newBackground.GetComponent<Renderer>().bounds.size.x;

            //Instantiate background game object and assign current list entry data to it
            Vector3 startingPosition = standardCamera.ViewportToWorldPoint(new Vector3(backgrounds.Count > 0 ? 1 : 0, 0.5f, standardCamera.farClipPlane));
            startingPosition.x = startingPosition.x + (backgroundWidth / 2);

            GameObject initialObject = Instantiate(newBackground, startingPosition, Quaternion.identity);
            initialObject.GetComponent<SpriteRenderer>().sprite = sprite;
            initialObject.transform.position = new Vector3(initialObject.transform.position.x, initialObject.transform.position.y, transform.position.z);

            //add newly instantiated gameobject to currentBackgrounds list
            ActiveBackground background = new ActiveBackground();
            background.data = scrollingBackgrounds[currentListEntry];
            background.gameObject = initialObject;
            background.isNewBackground = true;
            backgrounds.Add(background);
        }
    }
}
