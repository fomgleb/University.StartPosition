using System;
using System.Collections;
using StartPosition.Extensions;
using UnityEngine;
using UnityEngine.Events;

namespace StartPosition.Scripts
{
    
    [ExecuteAlways]
    public class SplitterIntoPieces : MonoBehaviour
    {
        [SerializeField] private float circleRadius = 10f;
        [SerializeField][Range(0, 360)] private float startingAngle;
        [Header("Events")]
        [SerializeField] private UnityEvent onStartedMoving;
        [SerializeField] private UnityEvent onEndedMoving;
        [Header("Model")]
        [SerializeField] private Transform main;
        [SerializeField] private Piece[] pieces;

        private uint _numberOfRunningCoroutines;
        private bool _coroutineIsRunning;
        
        public Piece[] Pieces => pieces;

        [HideInInspector] public float waitTimeForAllPieces;
        [HideInInspector] public float moveTimeForAllPieces;
        [HideInInspector] public AnimationCurve movementCurveForAllPieces;
        [HideInInspector] public float rotationTimeForAllPieces;
        [HideInInspector] public Vector3 rotationAnglesForAllPieces;
        [HideInInspector] public AnimationCurve rotationCurveForAllPieces;
        
        private void Update()
        {
            if (!Application.isPlaying)
                foreach (var piece in pieces)
                {
                    piece.movementCurve.CorrectKeys(new Keyframe(0, 0), new Keyframe(1, 1));
                    piece.rotationCurve.CorrectKeys(new Keyframe(0, 0), new Keyframe(1, 1));
                }
            else
            {
                if (_numberOfRunningCoroutines > 0)
                {
                    if (_coroutineIsRunning) return;
                    _coroutineIsRunning = true;
                    onStartedMoving?.Invoke();
                    return;
                }

                if (_coroutineIsRunning)
                {
                    _coroutineIsRunning = false;
                    onEndedMoving?.Invoke();
                }
            }
        }

        private void Start()
        {
            if (!Application.isPlaying) return;
            
            SetActiveMainAndPieces(true, false);
            foreach (var piece in pieces)
                piece.StartingPosition = piece.Transform.position;
        }
        
        public void SplitIntoPieces()
        {
            if (_numberOfRunningCoroutines <= 0)
                StartCoroutine(SplitIntoPiecesCoroutine());
        }

        public void GatherPiecesTogether()
        {
            if (_numberOfRunningCoroutines <= 0)
                StartCoroutine(GatherPiecesTogetherCoroutine());
        }
        
        private IEnumerator SplitIntoPiecesCoroutine()
        {
            _numberOfRunningCoroutines++;
            
            SetActiveMainAndPieces(false, true);

            var angleBetweenTwoPieces = 360f / pieces.Length;
            var currentAngle = startingAngle;

            foreach (var piece in pieces)
            {
                var destination = GetPositionOnCircumference(currentAngle);
                yield return new WaitForSeconds(piece.waitBeforeMove);
                StartCoroutine(MovePieceCoroutine(piece, destination));
                StartCoroutine(RotatePieceCoroutine(piece.Transform, piece.rotationTime, piece.rotationAngles,
                    piece.rotationCurve));

                currentAngle += angleBetweenTwoPieces;
            }

            _numberOfRunningCoroutines--;
        }
        
        private IEnumerator GatherPiecesTogetherCoroutine()
        {
            _numberOfRunningCoroutines++;
            
            for (var i = pieces.Length - 1; i >= 0; i--)
            {
                var piece = pieces[i];
                
                var destination = piece.StartingPosition;
                var pieceRotationAngles = -piece.rotationAngles;
                yield return new WaitForSeconds(piece.waitBeforeMove);
                StartCoroutine(MovePieceCoroutine(piece, destination));
                StartCoroutine(RotatePieceCoroutine(piece.Transform, piece.rotationTime, pieceRotationAngles,
                    piece.rotationCurve));
            }

            yield return new WaitForSeconds(Math.Max(pieces[0].movementTime, pieces[0].rotationTime));
            
            SetActiveMainAndPieces(true, false);

            _numberOfRunningCoroutines--;
        }
        
        private Vector3 GetPositionOnCircumference(float angle)
        {
            var circleCenter = Vector3.zero;
            if (main != null)
                circleCenter = main.position;
            else
            {
                foreach (var piece in pieces)
                    circleCenter += piece.StartingPosition;
                circleCenter /= pieces.Length;
            }
                
            var position = new Vector3
            {
                x = circleCenter.x + circleRadius * Mathf.Cos(angle * Mathf.Deg2Rad),
                y = circleCenter.y,
                z = circleCenter.z + circleRadius * Mathf.Sin(angle * Mathf.Deg2Rad)
            };

            return position;
        }

        private IEnumerator MovePieceCoroutine(Piece piece, Vector3 destination)
        {
            _numberOfRunningCoroutines++;
            
            var areaUnderMovementCurve = piece.movementCurve.GetAreaUnderCurve(1, 1);
            var distanceToTarget = Vector3.Distance(piece.Transform.position, destination);
            var pieceSpeed = distanceToTarget / piece.movementTime;
            
            for (var elapsedTime = 0f; elapsedTime <= piece.movementTime; elapsedTime += Time.deltaTime)
            {
                var modifiedSpeed = piece.movementCurve.Evaluate(elapsedTime / piece.movementTime) * pieceSpeed /
                                    areaUnderMovementCurve;
                var step = Time.deltaTime * modifiedSpeed;
                piece.Transform.position = Vector3.MoveTowards(piece.Transform.position, destination, step);
                yield return null;
            }
            piece.Transform.position = destination;

            _numberOfRunningCoroutines--;
        }

        private IEnumerator RotatePieceCoroutine(Transform pieceTransform, float rotationTime, Vector3 rotationAngles,
            AnimationCurve rotationCurve)
        {
            _numberOfRunningCoroutines++;

            var startingRotation = pieceTransform.rotation;
            
            var areaUnderRotationCurve = rotationCurve.GetAreaUnderCurve(1, 1);
            var rotationSpeed = rotationAngles / rotationTime;

            for (var elapsedTime = 0f; elapsedTime <= rotationTime; elapsedTime += Time.deltaTime)
            {
                var modifiedSpeed = rotationCurve.Evaluate(elapsedTime / rotationTime) * rotationSpeed /
                                    areaUnderRotationCurve;
                var rotationStep = Time.deltaTime * modifiedSpeed;
                pieceTransform.Rotate(rotationStep);
                //pieceTransform.Rotate(new Vector3(0, 1, 0), rotationStep.y);
                //pieceTransform.rotation = Quaternion.Euler(rotationStep + pieceTransform.rotation.eulerAngles);
                // pieceTransform.rotation = Quaternion.RotateTowards(pieceTransform.rotation,
                //      Quaternion.Euler(rotationAngles), 1);
                //pieceTransform.rotation = Quaternion.Euler(rotationStep + pieceTransform.rotation.eulerAngles);
                yield return null;
            }

            pieceTransform.rotation = startingRotation;
            pieceTransform.Rotate(rotationAngles);
            //pieceTransform.rotation = Quaternion.Euler(rotationAngles + startingRotation.eulerAngles); 

            _numberOfRunningCoroutines--;
        }

        private void SetActiveMainAndPieces(bool activeMain, bool activePieces)
        {
            if (main == null)
            {
                foreach (var piece in pieces)
                    piece.Transform.gameObject.SetActive(true);
                return;
            }

            main.gameObject.SetActive(activeMain);
            foreach (var piece in pieces)
                piece.Transform.gameObject.SetActive(activePieces);
        }
    }

    [Serializable]
    public class Piece
    {
        [SerializeField] private Transform transform;
        public float waitBeforeMove;
        public float movementTime;
        public AnimationCurve movementCurve;
        public float rotationTime;
        public Vector3 rotationAngles;
        public AnimationCurve rotationCurve;
        
        public Transform Transform => transform;
        public Vector3 StartingPosition { get; set; }
    }
}
