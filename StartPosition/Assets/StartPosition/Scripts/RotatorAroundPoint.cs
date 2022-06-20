using System;
using System.Collections;
using StartPosition.Extensions;
using UnityEngine;
using UnityEngine.Events;

namespace StartPosition.Scripts
{
    [ExecuteAlways]
    public class RotatorAroundPoint : MonoBehaviour
    {
        [SerializeField] private Transform pointAroundWhichToRotate;
        [SerializeField] private float rotationTime;
        [SerializeField] private Transform startingRotationPoint;
        [SerializeField] private AnimationCurve rotationCurve;
        [SerializeField] private Vector3 rotationAxis;
        [SerializeField] private bool startRotationAtStart;
        [Header("Events")]
        [SerializeField] private UnityEvent OnStartRotation;
        [SerializeField] private UnityEvent OnEndRotation;

        private Vector3 _startingPosition;
        private Quaternion _startingRotation;
        
        private void Start()
        {
            if (!Application.isPlaying) return;
            
            _startingPosition = transform.position;
            _startingRotation = transform.rotation;
            if (startRotationAtStart)
            {
                StartRotation();
                OnStartRotation?.Invoke();
            }
        }

        private void Update()
        {
            if (!Application.isPlaying)
                rotationCurve.CorrectKeys(new Keyframe(0, 0), new Keyframe(1, 1));
        }

        public void StartRotation()
        {
            StartCoroutine(RotateCoroutine());
        }
        
        public void StopRotationAndReturnToStartingPosition()
        {
            StopAllCoroutines();
            transform.position = _startingPosition;
            transform.rotation = _startingRotation;
            OnEndRotation.Invoke();
        }

        private IEnumerator RotateCoroutine()
        {
            transform.position = startingRotationPoint.position;
            
            var areaUnderRotationCurve = rotationCurve.GetAreaUnderCurve(1, 1);
            var rotationSpeed = 360 / rotationTime;

            for (var elapsedTime = rotationTime; elapsedTime > 0; elapsedTime -= Time.deltaTime)
            {
                var modifiedRotationSpeed = rotationCurve.Evaluate(elapsedTime / rotationTime) * rotationSpeed /
                                            areaUnderRotationCurve;
                var rotationStep = modifiedRotationSpeed * Time.deltaTime;
                transform.RotateAround(pointAroundWhichToRotate.position, rotationAxis, rotationStep);
                yield return null;
            }

            transform.rotation = _startingRotation;
            transform.position = _startingPosition;
        }
    }
}
