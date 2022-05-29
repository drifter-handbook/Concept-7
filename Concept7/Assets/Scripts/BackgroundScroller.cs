using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundScroller : MonoBehaviour
{
    public UnityEngine.Experimental.Rendering.Universal.PixelPerfectCamera pixelCamera;
    public Camera standardCamera;
    public GameObject spriteGameObjectPrefab;   //Background Prefab
    public float scrollingSpeed = 2;            //Scrolling speed

    [System.Serializable]
    public class ScrollingBackgroundData        //Background data from component, stores the sprite used and repeat data.
    {
        public Sprite sprite;
        public int repeatCount;
        public bool shouldrepeatNonstop;
    }
    public ScrollingBackgroundData[] scrollingBackgrounds;
    
    public class ActiveBackground               //Stores created instantiated background prefab and associated data.
    {
        public ScrollingBackgroundData data;
        public GameObject gameObject;
        public bool isNewBackground; //HACK: Keeps track of whether or not this background is the one most recently made. Can't think of a better place to put this right now
    }
    private List<ActiveBackground> currentBackgrounds = new List<ActiveBackground>();

    private int currentListEntry = 0;           //index of next scrollingBackgrounds entry to instantiate.
    private float cameraWidth;
    private float leftSide;
    private float rightSide;

    private bool forceNewBackground = false;

    void Start()
    {
        leftSide = standardCamera.ViewportToWorldPoint(new Vector3(0, 0.5f, standardCamera.nearClipPlane)).x;
        rightSide = standardCamera.ViewportToWorldPoint(new Vector3(1, 0.5f, standardCamera.nearClipPlane)).x;
        cameraWidth = standardCamera.orthographicSize * standardCamera.aspect;
        StartNewBackground();
    }

    //TODO:
    //1. Currently only supports a single background layer, should also support additional layers for parallax purposes
    //2. In BackgroundData you assign a sprite, change to a gameobject instead so we can have more complex backgrounds
    //3. MoveToNextBackground function call we can call in the codebase to force next background (think I handled this? Need to test)

    void FixedUpdate()
    {
        //loop over the currently instantiated background objects in the scene
        for(int index = 0; index < currentBackgrounds.Count; index++)
        {
            //Initialize vars
            ActiveBackground activeBackground = currentBackgrounds[index];
            GameObject backgroundObject = activeBackground.gameObject;
            float offsetXval = backgroundObject.transform.position.x + scrollingSpeed;
            float objectWidth = backgroundObject.GetComponent<SpriteRenderer>().bounds.size.x;
            Vector3 viewportPosition = standardCamera.ViewportToWorldPoint(backgroundObject.transform.position);
            
            if(offsetXval < (leftSide - (objectWidth / 2)))
            {
                //object has passed viewport, get rid of it
                bool success = currentBackgrounds.Remove(activeBackground);
                Destroy(backgroundObject);
            }
            else if(offsetXval < (rightSide - (objectWidth/2) + 3f) && currentBackgrounds[index].isNewBackground)
            {
                //Current background is filling up viewport, should start up new one
                //Note: 3 is an arbitrary magic number so we aren't creating the new game object exactly right before it needs to appear, gives us a little bit of loading time.
                //It literally means we are creating this gameObject 10 units before the camera view frame.
                
                //decrement repeat count or increment list entry if applicable
                if (forceNewBackground || !scrollingBackgrounds[currentListEntry].shouldrepeatNonstop && scrollingBackgrounds[currentListEntry].repeatCount <= 0)
                {
                    currentListEntry++;
                }
                else if(scrollingBackgrounds[currentListEntry].repeatCount > 0)
                {
                    scrollingBackgrounds[currentListEntry].repeatCount--;
                }

                //Start new background assuming we have the data for it
                if(currentListEntry < scrollingBackgrounds.Length)
                {
                    currentBackgrounds[index].isNewBackground = false;
                    StartNewBackground();
                }
                else
                {
                    Debug.Log("OUT OF BACKGROUNDS!");
                }
            }
            else
            {
                //update background position
                backgroundObject.transform.position = new Vector3(offsetXval, backgroundObject.transform.position.y, backgroundObject.transform.position.z);
            }
        }
    }

    void StartNewBackground()
    {
        Sprite newSprite = scrollingBackgrounds[currentListEntry].sprite;
        float spriteWidth = newSprite.bounds.size.x;

        //Instantiate background game object and assign current list entry data to it
        Vector3 startingPosition = standardCamera.ViewportToWorldPoint(new Vector3(currentBackgrounds.Count > 0 ? 1 : 0, 0.5f, standardCamera.farClipPlane));
        startingPosition.x = startingPosition.x + (spriteWidth / 2);

        if(currentBackgrounds.Count > 0)
        {
            startingPosition.x = startingPosition.x + 3f; //arbitrary magic number so we aren't creating the new game object exactly right before it needs to appear, gives us a little bit of loading time.
        }

        GameObject initialObject = Instantiate(spriteGameObjectPrefab, startingPosition, Quaternion.identity);
        initialObject.GetComponent<SpriteRenderer>().sprite = scrollingBackgrounds[currentListEntry].sprite;

        //add newly instantiated gameobject to currentBackgrounds list
        ActiveBackground background = new ActiveBackground();
        background.data = scrollingBackgrounds[currentListEntry];
        background.gameObject = initialObject;
        background.isNewBackground = true;
        currentBackgrounds.Add(background);
        forceNewBackground = false;
    }

    //Function call to be used in other scripts, forces next background gameObject to be next background in data list
    public void ForceNextBackground()
    {
        forceNewBackground = true;
    }
}
