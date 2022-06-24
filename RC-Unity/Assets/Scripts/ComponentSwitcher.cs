using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComponentSwitcher : MonoBehaviour
{
    public List<GameObject> objects;
    public List<KeyCode> keys;
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        for(int i = 0; i < objects.Count; i++)
        {
            if (Input.GetKeyDown(keys[i])) objects[i].SetActive(!objects[i].active);
        }
    }

    void OnGUI()
    {
        for (int i = 0; i < objects.Count; i++)
        {
            GUI.color = objects[i].active ? Color.green : Color.grey;
            GUI.Label(new Rect(Screen.width-100, 40+i*20, 100, 25), objects[i].name);
        }
    }
}


