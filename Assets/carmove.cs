using UnityEditor.ShaderGraph.Internal;
using UnityEngine;

public class carmove : MonoBehaviour
{   
    public float speed =10f;
    public float x=0;
    public float y=0;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

        if(Input.GetKey(KeyCode.A)){
            transform.Translate(-speed*Time.deltaTime, 0, 0);
        }
        if(Input.GetKey(KeyCode.D)){
            transform.Translate(speed*Time.deltaTime, 0, 0);
        }
    }
}
