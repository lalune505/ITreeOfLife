using System.Collections;

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class PageManager : MonoBehaviour
{
    public string name;

    public RawImage nodeImage;
    // Start is called before the first frame update
    void Start()
    {
        GetPage();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private async void GetPage()
    {
       var texture =  await NetworkManager.GetNodeImage(name);
       nodeImage.texture = texture;
       
       nodeImage.SetNativeSize();
    }
}
