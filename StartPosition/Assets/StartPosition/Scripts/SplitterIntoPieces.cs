using System.Collections;
using StartPosition.Extensions;
using UnityEngine;
using UnityEngine.Events;

namespace StartPosition.Scripts
{
    [ExecuteAlways]
    public class SplitterIntoPieces : MonoBehaviour
    {
        [Header("For each piece")]
        [SerializeField] private float circleRadius = 10f;
        [SerializeField] private float waitBeforeMove;
        [SerializeField] private float movementTime;
        [SerializeField] private AnimationCurve movementCurve;
        [SerializeField] private float rotationTime;
        [SerializeField] private Vector3 rotationAngles;
        [SerializeField] private AnimationCurve rotationCurve;
        [Header("Events")]
        [SerializeField] private UnityEvent onStartedMoving;
        [SerializeField] private UnityEvent onEndedMoving;

        private MeshRenderer _mainModelMeshRenderer;
        private Piece[] _pieces;

        private float _areaUnderMovementCurve;
        private float _areaUnderRotationCurve;

        private uint _numberOfRunningCoroutines;
        private Action _currentAction;

        private float _phi;
        
        private void Awake()
        {
            if (!Application.isPlaying) return;

            _phi = Mathf.PI * (3f - Mathf.Sqrt(5f));

            _areaUnderMovementCurve = movementCurve.GetAreaUnderCurve(1, 1);
            _areaUnderRotationCurve = rotationCurve.GetAreaUnderCurve(1, 1);
            
            _mainModelMeshRenderer = GetComponent<MeshRenderer>();
            var meshRenderersInChildren = GetComponentsInChildren<MeshRenderer>();
            _pieces = new Piece[meshRenderersInChildren.Length];
            for (var i = 0; i < _pieces.Length; i++)
                _pieces[i] = new Piece(meshRenderersInChildren[i]);
        }

        private void Start()
        {
            if (!Application.isPlaying) return;
            
            SetMainModelVisibility(true);
            SetPiecesVisibility(false);
        }

        private void Update()
        {
            if (Application.isPlaying) return;
            
            movementCurve?.CorrectKeys(new Keyframe(0, 0), new Keyframe(1, 1));
            rotationCurve?.CorrectKeys(new Keyframe(0, 0), new Keyframe(1, 1));
        }

        public void SplitIntoPieces()
        {
            if (_numberOfRunningCoroutines <= 0)
                StartCoroutine(SplitIntoPiecesCoroutine());
            _currentAction = Action.SplitIntoPieces;
        }

        public void GatherPiecesTogether()
        {
            if (_numberOfRunningCoroutines <= 0)
                StartCoroutine(GatherPiecesTogetherCoroutine());
            _currentAction = Action.GatherPiecesTogether;
        }

        private IEnumerator SplitIntoPiecesCoroutine()
        {
            OnCoroutineStart();
            
            SetMainModelVisibility(false);
            SetPiecesVisibility(true);
            
            for (var i = 0; i < _pieces.Length; i++)
            {
                var piece = _pieces[i];
                var destination = GetPositionOnSphere(i, _pieces.Length);
                yield return new WaitForSeconds(waitBeforeMove);
                StartCoroutine(MovePieceCoroutine(piece, destination));
                StartCoroutine(RotatePieceCoroutine(piece.Transform, rotationTime, rotationAngles, rotationCurve));
            }

            OnCoroutineEnd();
        }

        private IEnumerator GatherPiecesTogetherCoroutine()
        {
            OnCoroutineStart();
            
            for (var i = _pieces.Length - 1; i >= 0; i--)
            {
                var piece = _pieces[i];
                
                var destination = piece.StartingPosition;
                yield return new WaitForSeconds(waitBeforeMove);
                StartCoroutine(MovePieceCoroutine(piece, destination));
                StartCoroutine(RotatePieceCoroutine(piece.Transform, rotationTime, -rotationAngles, rotationCurve));
            }

            OnCoroutineEnd();
        }

        private IEnumerator MovePieceCoroutine(Piece piece, Vector3 destination)
        {
            OnCoroutineStart();
            
            var distanceToTarget = Vector3.Distance(piece.Transform.position, destination);
            var pieceSpeed = movementTime != 0 ? distanceToTarget / movementTime : float.MaxValue;
            
            for (var elapsedTime = 0f; elapsedTime <= movementTime; elapsedTime += Time.deltaTime)
            {
                var modifiedSpeed = movementTime != 0
                    ? movementCurve.Evaluate(elapsedTime / movementTime) * pieceSpeed / _areaUnderMovementCurve
                    : pieceSpeed;
                var step = Time.deltaTime * modifiedSpeed;
                piece.Transform.position = Vector3.MoveTowards(piece.Transform.position, destination, step);
                yield return null;
            }
            piece.Transform.position = destination;

            OnCoroutineEnd();
        }

        private IEnumerator RotatePieceCoroutine(Transform pieceTransform, float rotationTime, Vector3 rotationAngles,
            AnimationCurve rotationCurve)
        {
            OnCoroutineStart();

            var startingRotation = pieceTransform.rotation;
            
            var rotationSpeed = rotationAngles / rotationTime;

            for (var elapsedTime = 0f; elapsedTime <= rotationTime; elapsedTime += Time.deltaTime)
            {
                var modifiedSpeed = rotationCurve.Evaluate(elapsedTime / rotationTime) * rotationSpeed /
                                    _areaUnderRotationCurve;
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

            OnCoroutineEnd();
        }

        private Vector3 GetPositionOnCircumference(float angle)
        {
            var circleCenter = _mainModelMeshRenderer.transform.position;
                
            var position = new Vector3
            {
                x = circleCenter.x + circleRadius * Mathf.Cos(angle * Mathf.Deg2Rad),
                y = circleCenter.y,
                z = circleCenter.z + circleRadius * Mathf.Sin(angle * Mathf.Deg2Rad)
            };

            return position;
        }

        private Vector3 GetPositionOnSphere(int currentPointIndex, int pointsCount)
        {
            var y = 1 - currentPointIndex / (float)(pointsCount - 1) * 2;
            var radius = Mathf.Sqrt(1 - y * y);

            var theta = _phi * currentPointIndex;

            var x = Mathf.Cos(theta) * radius;
            var z = Mathf.Sin(theta) * radius;

            return new Vector3(x, y, z) * circleRadius;
        }

        private void OnCoroutineStart()
        {
            if (_numberOfRunningCoroutines == 0)
                onStartedMoving?.Invoke();

            _numberOfRunningCoroutines++;
        }

        private void OnCoroutineEnd()
        {
            _numberOfRunningCoroutines--;

            if (_numberOfRunningCoroutines != 0) return;

            if (_currentAction == Action.GatherPiecesTogether)
            {
                SetMainModelVisibility(true);
                SetPiecesVisibility(false);
            }
            
            onEndedMoving?.Invoke();
        }

        private void SetMainModelVisibility(bool isVisible)
        {
            if (_mainModelMeshRenderer != null)
                _mainModelMeshRenderer.enabled = isVisible;
        }

        private void SetPiecesVisibility(bool isVisible)
        {
            if (_mainModelMeshRenderer == null)
            {
                foreach (var piecesMeshRenderer in _pieces)
                    piecesMeshRenderer.MeshRenderer.enabled = true;
                return;
            }

            foreach (var piecesMeshRenderer in _pieces)
                piecesMeshRenderer.MeshRenderer.enabled = isVisible;
        }

        private class Piece
        {
            public MeshRenderer MeshRenderer { get; }
            public Transform Transform { get; }
            public Vector3 StartingPosition { get; }

            public Piece(MeshRenderer meshRenderer)
            {
                MeshRenderer = meshRenderer;
                Transform = meshRenderer.transform;
                StartingPosition = meshRenderer.transform.position;
            }
        }

        private enum Action
        {
            SplitIntoPieces,
            GatherPiecesTogether
        }
    }
}
