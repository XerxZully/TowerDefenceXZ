using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using UnityEngine.Tilemaps;

[CustomEditor( typeof( LevelEditor ) )]
[CanEditMultipleObjects]
public class EditorMod : Editor
{
    [HideInInspector] public bool IsInit = false;
    

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        LevelEditor levelEditor = ( LevelEditor )serializedObject.targetObject;

        if( levelEditor != null ) {
            if( levelEditor.level == null ) {
                levelEditor.Init();
                levelEditor.CreateLevel();
            }

            if( GUILayout.Button( "Create new level" ) ) {
                levelEditor.stringFileName = "Default.txt";
                levelEditor.CreateLevel();
            }

            levelEditor.stringFileName = EditorGUILayout.TextField( "File name: ", levelEditor.stringFileName );
            levelEditor.level.data.levelName = EditorGUILayout.TextField( "Level name: ", levelEditor.level.data.levelName );
            levelEditor.level.data.width = EditorGUILayout.IntField( "Map Width:", levelEditor.level.data.width );
            levelEditor.level.data.height = EditorGUILayout.IntField( "Map Height:", levelEditor.level.data.height );
            levelEditor.level.data.x = EditorGUILayout.IntField( "Base X:", levelEditor.level.data.x );
            levelEditor.level.data.y = EditorGUILayout.IntField( "Base Y:", levelEditor.level.data.y );
            //levelEditor.level.data.spawnsCount = EditorGUILayout.IntField( "Enemies count:", levelEditor.level.data.spawnsCount );
            EditorGUILayout.IntField( "Enemies count:", levelEditor.level.data.spawnsCount );
            levelEditor.level.data.towersCount = EditorGUILayout.IntField( "Towers count:", levelEditor.level.data.towersCount );
            levelEditor.level.data.towerMaxLevel = EditorGUILayout.IntField( "Tower max level:", levelEditor.level.data.towerMaxLevel );
            levelEditor.level.data.startScore = EditorGUILayout.IntField( "Start money:", levelEditor.level.data.startScore );

            if( GUI.changed ) {
                levelEditor.Base.transform.position = new Vector3( levelEditor.level.data.x + 0.5f, levelEditor.level.data.y + 0.5f, -0.01f );
            }

            if( GUILayout.Button( "Save level" ) ) {
                var path = EditorUtility.SaveFilePanel( "Save level as TXT", Application.dataPath + "/Resources/Levels/", levelEditor.stringFileName, "txt" );

                if( path.Length != 0 ) {
                    levelEditor.SaveLevel( Path.GetFileName( path ) );
                    levelEditor.stringFileName = Path.GetFileName( path );
                }
            }

            if( GUILayout.Button( "Load level" ) ) {
                string path = EditorUtility.OpenFilePanel( "Overwrite with txt", Application.dataPath + "/Resources/Levels/", "txt" );

                if( path.Length != 0 ) {
                    levelEditor.LoadLevel( Path.GetFileName( path ) );
                    levelEditor.stringFileName = Path.GetFileName( path );
                }
            }
        }
    }
}
