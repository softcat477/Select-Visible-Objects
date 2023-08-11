using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System; // Tuple
using System.Linq;

public class SelectObjectOnScreen : MonoBehaviour
{
    public PlayerInputActions playerInputAction;
    private InputAction focus;
    private InputAction aim;

    bool isFocusing = false;

    [SerializeField] ObjectsOnScreen objOnScreenManager;
    [SerializeField] GameObject[] objsOnScreen;
    [SerializeField] Vector2[] positionsOnScreen;
    [SerializeField] GameObject selected;

    // (index to access the gameobject by objsOnScreen[index], x/y value) should be sorted using x/y value in ascending order.
    [SerializeField] (int, float)[] l2rIndices;
    [SerializeField] (int, float)[] b2tIndices;

    [SerializeField] int selectedInstanceID = -1;

    public delegate void UpdateSelectedEventDelegate(GameObject sel);
    public event UpdateSelectedEventDelegate UpdateSelectedEvent;

    private void Awake() {
        playerInputAction = new PlayerInputActions();
        focus = playerInputAction.Player.Strafe;
        aim = playerInputAction.Player.Select;
        selectedInstanceID = gameObject.GetInstanceID();
    }

    private void OnEnable() {
        focus.Enable();
        focus.performed += OnfocusPerformed;
        focus.canceled += OnfocusCanceled;

        aim.Enable();
        aim.performed += OnAimPerformed;
    }

    private void OnDisable() {
        focus.Disable();
        aim.Disable();
    }

    // Update is called once per frame
    void Update()
    {
        if (isFocusing) {
            (objsOnScreen, positionsOnScreen) = objOnScreenManager.GetObjectsOnScreen();

            l2rIndices = positionsOnScreen.Select((vec, idx) => (idx, vec.x)).ToArray();
            b2tIndices = positionsOnScreen.Select((vec, idx) => (idx, vec.y)).ToArray();

            l2rIndices = l2rIndices.OrderBy(e => e.Item2).ToArray();
            b2tIndices = b2tIndices.OrderBy(e => e.Item2).ToArray();

            // Select the initial object is we haven't selected one
            if (objsOnScreen.Length != 0) {
                selected = objsOnScreen.FirstOrDefault(obj => obj.GetInstanceID() == selectedInstanceID);

                // selected object is out of screen
                if (selected == null) selected = objsOnScreen[0];

                selectedInstanceID = selected.GetInstanceID();
                int idx = Array.IndexOf(objsOnScreen, selected);
                selected.GetComponent<Renderer>().material.EnableKeyword("_EMISSION");
                selected.GetComponent<Renderer>().material.SetColor("_EmissionColor", Color.grey);
            }
            else {
                selected = null;
            }
            UpdateSelectedEvent?.Invoke(selected);
        }
    }

    void OnfocusPerformed(InputAction.CallbackContext ctx) {
        isFocusing = true; 
    }

    void OnfocusCanceled(InputAction.CallbackContext ctx) {
        isFocusing = false;
        selected = objsOnScreen.FirstOrDefault(obj => obj.GetInstanceID() == selectedInstanceID);
        if (selected) {
            selected.GetComponent<Renderer>().material.DisableKeyword("_EMISSION");
            selected.GetComponent<Renderer>().material.SetColor("_EmissionColor", Color.black);
        }
        selected = null;
        UpdateSelectedEvent?.Invoke(selected);
    }

    void OnAimPerformed(InputAction.CallbackContext ctx) {
        if (!isFocusing) return;
        if (objsOnScreen.Length == 0) return;

        Vector2 mouse = aim.ReadValue<Vector2>();

        // Get the index of selected gameobject
        selected = objsOnScreen.FirstOrDefault(obj => obj.GetInstanceID() == selectedInstanceID);
        int idx_selected = Array.IndexOf(objsOnScreen, selected);

        // find its index in l2r and u2t
        int idx_l2r = Array.FindIndex(l2rIndices, (tuple) => tuple.Item1 == idx_selected);
        int idx_b2t = Array.FindIndex(b2tIndices, (tuple) => tuple.Item1 == idx_selected);

        if (mouse.x != 0){
            int diff = mouse.x > 0 ? 1 : -1;
            idx_l2r = Math.Clamp(idx_l2r + diff, 0, l2rIndices.Length-1);
            selected.GetComponent<Renderer>().material.DisableKeyword("_EMISSION");
            selected.GetComponent<Renderer>().material.SetColor("_EmissionColor", Color.black);
            idx_selected = l2rIndices[idx_l2r].Item1;
            selected = objsOnScreen[idx_selected];
            selected.GetComponent<Renderer>().material.EnableKeyword("_EMISSION");
            selected.GetComponent<Renderer>().material.SetColor("_EmissionColor", Color.grey);
            selectedInstanceID = selected.GetInstanceID();
        }
        else if (mouse.y != 0) {
            int diff = mouse.y > 0 ? 1 : -1;
            idx_b2t = Math.Clamp(idx_b2t+diff, 0, b2tIndices.Length-1);
            selected.GetComponent<Renderer>().material.DisableKeyword("_EMISSION");
            selected.GetComponent<Renderer>().material.SetColor("_EmissionColor", Color.black);
            idx_selected = b2tIndices[idx_b2t].Item1;
            selected = objsOnScreen[idx_selected];
            selected.GetComponent<Renderer>().material.EnableKeyword("_EMISSION");
            selected.GetComponent<Renderer>().material.SetColor("_EmissionColor", Color.grey);
            selectedInstanceID = selected.GetInstanceID();
        }

        UpdateSelectedEvent?.Invoke(selected);
    }
}