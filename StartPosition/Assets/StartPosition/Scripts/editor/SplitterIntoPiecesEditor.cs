// using StartPosition.Extensions;
// using UnityEditor;
// using UnityEditor.SceneManagement;
// using UnityEngine;
//
// namespace StartPosition.Scripts.editor
// {
//     [CustomEditor(typeof(SplitterIntoPieces))]
//     public class SplitterIntoPiecesEditor : Editor
//     {
//         private SplitterIntoPieces _splitter;
//
//         private void OnEnable()
//         {
//             _splitter = (SplitterIntoPieces)target;
//         }
//
//         public override void OnInspectorGUI()
//         {
//             base.OnInspectorGUI();
//
//             if (_splitter.Pieces.Length > 0)
//             {
//                 EditorGUILayout.BeginVertical("TextArea");
//                 _splitter.waitTimeForAllPieces = EditorGUILayout.FloatField("Wait Before Move", _splitter.waitTimeForAllPieces);
//                 if (GUILayout.Button("Fill in for each piece"))
//                     foreach (var piece in _splitter.Pieces)
//                         piece.waitBeforeMove = _splitter.waitTimeForAllPieces;
//                 EditorGUILayout.EndVertical();
//                 
//                 EditorGUILayout.Space();
//                 
//                 EditorGUILayout.BeginVertical("TextArea");
//                 _splitter.moveTimeForAllPieces = EditorGUILayout.FloatField("Movement Time", _splitter.moveTimeForAllPieces);
//                 if (GUILayout.Button("Fill in for each piece"))
//                     foreach (var piece in _splitter.Pieces)
//                         piece.movementTime = _splitter.moveTimeForAllPieces;
//                 EditorGUILayout.EndVertical();
//                 
//                 EditorGUILayout.Space();
//                 
//                 EditorGUILayout.BeginVertical("TextArea");
//                 _splitter.movementCurveForAllPieces = EditorGUILayout.CurveField("Movement Curve", _splitter.movementCurveForAllPieces);
//                 _splitter.movementCurveForAllPieces?.CorrectKeys(new Keyframe(0, 0), new Keyframe(1, 1));
//                 if (GUILayout.Button("Fill in for each piece"))
//                     foreach (var piece in _splitter.Pieces)
//                         piece.movementCurve = new AnimationCurve(_splitter.movementCurveForAllPieces.keys);
//                 EditorGUILayout.EndVertical();
//
//                 EditorGUILayout.Space();
//
//                 EditorGUILayout.BeginVertical("TextArea");
//                 _splitter.rotationTimeForAllPieces = EditorGUILayout.FloatField("Rotation Time", _splitter.rotationTimeForAllPieces);
//                 if (GUILayout.Button("Fill in for each piece"))
//                     foreach (var piece in _splitter.Pieces)
//                         piece.rotationTime = _splitter.rotationTimeForAllPieces;
//                 EditorGUILayout.EndVertical();
//
//                 EditorGUILayout.Space();
//
//                 EditorGUILayout.BeginVertical("TextArea");
//                 _splitter.rotationAnglesForAllPieces = EditorGUILayout.Vector3Field("Rotation Angles", _splitter.rotationAnglesForAllPieces);
//                 if (GUILayout.Button("Fill in for each piece"))
//                     foreach (var piece in _splitter.Pieces)
//                         piece.rotationAngles = _splitter.rotationAnglesForAllPieces;
//                 EditorGUILayout.EndVertical();
//
//                 EditorGUILayout.Space();
//
//                 EditorGUILayout.BeginVertical("TextArea");
//                 _splitter.rotationCurveForAllPieces = EditorGUILayout.CurveField("Rotation Curve", _splitter.rotationCurveForAllPieces);
//                 _splitter.rotationCurveForAllPieces?.CorrectKeys(new Keyframe(0, 0), new Keyframe(1, 1));
//                 if (GUILayout.Button("Fill in for each piece"))
//                     foreach (var piece in _splitter.Pieces)
//                         piece.rotationCurve = new AnimationCurve(_splitter.rotationCurveForAllPieces.keys);
//                 EditorGUILayout.EndVertical();
//
//                 if (GUI.changed) SetObjectDirty(_splitter.gameObject);
//             }
//         }
//
//         private static void SetObjectDirty(GameObject gO)
//         {
//             EditorUtility.SetDirty(gO);
//             EditorSceneManager.MarkSceneDirty(gO.scene);
//         }
//     }
// }