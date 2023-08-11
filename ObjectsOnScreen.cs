using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ObjectsOnScreen : MonoBehaviour
{
    CullingGroup cullingGroup;
    BoundingSphere[] bounds;
    GameObject[] selectables;

    [SerializeField] bool[] onScreenState;
    [SerializeField] bool[] visibleState;
    [SerializeField] Vector2[] onScreenPosition;
    // Start is called before the first frame update
    void Start()
    {
        cullingGroup = new CullingGroup();
        cullingGroup.targetCamera = Camera.main;

        // We only care about object on screen or not;
        selectables = GameObject.FindGameObjectsWithTag("Selectable");
        bounds = new BoundingSphere[selectables.Length];
        onScreenState = new bool[selectables.Length];
        visibleState = new bool[selectables.Length];
        onScreenPosition = new Vector2[selectables.Length];

        for (int i = 0; i < selectables.Length; i++) {
            GameObject obj = selectables[i];
            SphereCollider collider = obj.GetComponent<SphereCollider>();
            BoundingSphere bsphere = new BoundingSphere();
            bsphere.position = obj.transform.TransformPoint(collider.center);
            bsphere.radius = collider.radius;
            bounds[i] = bsphere;
            onScreenState[i] = false;
            onScreenPosition[i] = new Vector2(-1, -1);
        }

        cullingGroup.SetBoundingSpheres(bounds);
        cullingGroup.SetBoundingSphereCount(bounds.Length);
        cullingGroup.SetDistanceReferencePoint(Camera.main.transform);

        cullingGroup.onStateChanged += StateChanged;
        // StartCoroutine(LogOnScreenState(1.0f));
    }

    void StateChanged(CullingGroupEvent e) {
        onScreenState[e.index] = e.isVisible;
        visibleState[e.index] = false;
        if (e.isVisible) {
            onScreenPosition[e.index] = Camera.main.WorldToScreenPoint(selectables[e.index].transform.position);
        }
        else {
            onScreenPosition[e.index] = new Vector2(-1, -1);
        }
    }

    private void Update() {
        // Update on screen position if the object is visible and not occluded
        for (int i = 0; i < onScreenState.Length; i++) {
            //if (onScreenState[i] && !visibleState[i]) { // Update on screen position when the player is moving
            if (onScreenState[i]) {
                // When the object is in camera's frustrum but it is not visible yet
                // TODO: Cast a ray
                RaycastHit hit;
                Vector3 rayDirection = (selectables[i].transform.position - Camera.main.transform.position).normalized;
                if (Physics.Raycast(Camera.main.transform.position, rayDirection, out hit, Mathf.Infinity)) {
                    if (hit.collider.gameObject.CompareTag("Selectable")) {
                        //Debug.DrawRay(Camera.main.transform.position, rayDirection * hit.distance, Color.yellow, 30.0f);
                        //if (selectables[i].name == "Selectable_01") Debug.Log("Hit " + hit.collider.gameObject.name);
                        onScreenPosition[i] = Camera.main.WorldToScreenPoint(selectables[i].transform.position);
                        visibleState[i] = true;
                    }
                    else {
                        visibleState[i] = false;
                    }
                }
            }
        }
    }

    // cleanup
    private void OnDestroy() {
        cullingGroup.onStateChanged -= StateChanged;
        cullingGroup.Dispose();
        cullingGroup = null;
    }

    public (GameObject[], Vector2[]) GetObjectsOnScreen() {
        Vector2[] retScreenPosition = onScreenPosition.Where((position, idx) => visibleState[idx]).ToArray();
        GameObject[] retOnScreenGameobjects = selectables.Where((position, idx) => visibleState[idx]).ToArray();

        return (retOnScreenGameobjects, retScreenPosition);
    }

    private IEnumerator LogOnScreenState(float period) {
        while (true) {
            string ostr = "";
            for (int i = 0; i < selectables.Length; i++) {
                int dist = cullingGroup.GetDistance(0);
                ostr += $"{selectables[i].name}: {onScreenState[i]} {dist}";
            }
            Debug.Log(ostr);
            yield return new WaitForSeconds(period);
        }
    }
}
