using System;
using System.Collections;
using System.Collections.Generic;
using net.krej.FPSCounter;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class ControlsActivation : MonoBehaviour
{
    public GameObject controlsPanel;
    public TMP_Text framerateLabel;
    public TMP_Text qualityLabel;

    private InputMaster _controls;

    void Awake()
    {
        _controls = new InputMaster();
    }
    
    // Start is called before the first frame update
    void Start()
    {
        _controls.System.Help.performed += OnHelp;
        _controls.System.Help.canceled += OnHelpCanceled;
        
        Invoke(nameof(HideHelp), 5f);
    }

    void Update()
    {
        framerateLabel.text = $"FPS: {(int) (1 / Time.smoothDeltaTime)}";
        qualityLabel.text = $"Quality: {QualitySettings.names[QualitySettings.GetQualityLevel()]}";
    }

    void OnDestroy()
    {
        _controls.System.Help.performed -= OnHelp;
        _controls.System.Help.canceled -= OnHelpCanceled;
    }

    void OnEnable()
    {
        _controls.Enable();
    }

    void OnDisable()
    {
        _controls.Disable();
    }

    void OnHelp(InputAction.CallbackContext _)
    {
        controlsPanel.SetActive(true);
    }
    
    void OnHelpCanceled(InputAction.CallbackContext _)
    {
        HideHelp();
    }

    void HideHelp()
    {
        controlsPanel.SetActive(false);
    }
}
