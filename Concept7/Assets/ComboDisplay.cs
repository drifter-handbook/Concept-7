using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ComboDisplay : MonoBehaviour
{

    public TextMeshProUGUI comboTitle;
    public TextMeshProUGUI comboDesc;
    public Image[] comboPieces;
    public Image comboIcon;


    public Sprite rColor;
    public Sprite yColor;
    public Sprite bColor;

    public void Initialize(WeaponData weaponData)
    {
        comboTitle.text = weaponData.attackName;
        comboDesc.text = weaponData.attackDesc;
        comboIcon.sprite = weaponData.icon;

        int rLeft = weaponData.r;
        int bLeft = weaponData.b;
        int yLeft = weaponData.y;
        for(int i = 0; i<comboPieces.Length; i++){

            if(rLeft > 0){
                comboPieces[i].sprite = rColor;
                rLeft--;
                continue;
            }

            if(bLeft > 0){
                comboPieces[i].sprite = bColor;
                bLeft--;
                continue;
            }

           if(yLeft > 0){
                comboPieces[i].sprite = yColor;
                yLeft--;
                continue;
            }
        }

    }
}
