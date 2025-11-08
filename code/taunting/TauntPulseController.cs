using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

public class TauntPulseController : MonoBehaviour
{
    [SerializeField] private KeyCode tauntKey = KeyCode.T;
    [Space]
    [SerializeField] private AnimationCurve fadeCurve;
    [SerializeField] private float pulseDuration = 1f;
    [SerializeField] private float pulseScale = 5f;
    [Space]
    [SerializeField] private Color pulseColor = Color.red;
    private Color fadeColor;
    [SerializeField] private Collider _collider;

    Material _material;

    private void Start()
    {
        transform.localScale = Vector3.zero;
        _material = GetComponent<PostProcessingScan>().material;

        GetComponent<PostProcessingScan>().material = _material;
        _material.SetColor("_Colour", pulseColor);
        
    }

    private void Update()
    {
        if (Input.GetKey(tauntKey))
        {
            StartScan();
        }
    }

    Tween tween;
    [Button("Play Scale FX")]
    public void StartScan()
    {
        if(tween == null)
        {
            tween = transform.DOScale(Vector3.one * pulseScale, pulseDuration).OnComplete(OnScanComplete);
            _collider.enabled = true;

            PlayMaterialFx();

            return;
        }

        if (tween.IsPlaying())
        {
            return;
        }

        tween = transform.DOScale(Vector3.one * pulseScale, pulseDuration).OnComplete(OnScanComplete);
        _collider.enabled = true;


        //tween _material color so that it fades in and out based on the pulse duration
        PlayMaterialFx();
    }

    void PlayMaterialFx()
    {
        float elapsed = 0f;

        DOTween.To(() => elapsed, x => {
            elapsed = x;
            float t = fadeCurve.Evaluate(elapsed / pulseDuration);
            Color currentColor = Color.Lerp(pulseColor, fadeColor, t);
            _material.SetColor("_Colour", currentColor);
        }, pulseDuration, pulseDuration).SetEase(Ease.Linear);

    }

    void OnScanComplete()
    {
        transform.localScale = Vector3.zero;
        _collider.enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {

        if (other.GetComponent<NPC_Collision>())
        {
            Passenger passenger = other.GetComponent<NPC_Collision>().GetInteractible().GetComponent<Passenger>();
            if (passenger)
            {
                if(passenger.passengerStatus != PassengerStatusEnum.PassengerStatus.WAITING_FOR_INTERACTION)
                {
                    return;
                }

                if (other.GetComponent<NPC_Collision>().isPrimaryCollider)
                {
                    Transform passengerTransform = other.GetComponent<NPC_Collision>().GetInteractible().transform;
                    passengerTransform.GetComponent<NPC_VehicleBoardingHandler>().HandleTauntResponse();
                    passengerTransform.GetComponent<NPCharacter>().proceduralAnimController.DisableBoneSimulators();
                    passengerTransform.GetComponent<NPCharacter>().proceduralAnimController.DisableLookAnimator(true);
                }

            }
        }
    }
}
