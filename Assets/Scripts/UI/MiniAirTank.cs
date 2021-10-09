using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DUI
{
    [RequireComponent(typeof(Image))]
    public class MiniAirTank : MonoBehaviour
    {

        public Color fullColor = Color.blue;
        public Color emptyColor = Color.grey;
        
        public bool filled = false;

        Image image;

        // Use this for initialization
        void Start ()
        {
            image = GetComponent<Image>();
        }

        // Update is called once per frame
        void Update ()
        {
            if (filled) image.color = fullColor;
            else image.color = emptyColor;
        }
    }
}