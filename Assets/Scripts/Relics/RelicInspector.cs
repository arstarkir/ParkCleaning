using UnityEngine;

public class RelicInspector : MonoBehaviour
{
    public GameObject inspectedObject;
    public float distance = 1000f;
    public float sensitivity = 5.0f;

    private void Start()
    {
        inspectedObject = this.gameObject;
    }

    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");
        if (Input.GetMouseButton(0))
        {
            inspectedObject.transform.Rotate(new Vector3(0, mouseX * sensitivity, 0));
            inspectedObject.transform.Rotate(new Vector3(-mouseY * sensitivity, 0, 0));
        }
    }
}
