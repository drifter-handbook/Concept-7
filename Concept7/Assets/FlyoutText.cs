using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class FlyoutText : MonoBehaviour
{

    public float moveSpeed = 0.005f;
    public float fadeMult = 2; 

    private bool fadeIn = true;
    private bool fadeOut = false;
    private float fadeTime = 0;
    private TextMeshPro text;
    // Start is called before the first frame update
    private string word = "";

   public void Initialize(string word){
        this.word = word;
   }

    void Update()
    {
        if(text == null){
            text = GetComponent<TextMeshPro>();
            text.text = word;
        }
         
        //move
        transform.position = new Vector3(transform.position.x, transform.position.y+moveSpeed, transform.position.z);

        //dissolve in/out
        if(text != null){
             Material mat = text.fontMaterial;
            mat.SetFloat("_FaceDilate" , Mathf.Lerp(-1, 0, fadeTime));
        } else {
            UnityEngine.Debug.Log("Can't get the material, text was null...");
        }

        if(fadeIn){
            fadeTime += (Time.deltaTime) * fadeMult;
            if(fadeTime >=1 ){
                fadeIn = false;
                fadeOut = true;
            }
        }

        if(fadeOut){
            fadeTime -= (Time.deltaTime/2) * fadeMult;
            if(fadeTime <= 0 ){
                Destroy(gameObject);
            }
        }
    }
}
