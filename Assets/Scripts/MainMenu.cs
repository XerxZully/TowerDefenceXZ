using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private Text textTitle = null;
    [SerializeField] private GameObject menuMain = null;
    [SerializeField] private GameObject menuSelectLevel = null;
    [SerializeField] private GameObject buttonPrefab = null;

    private float tiker = 0f;
    private float alpha = 0f;

    [HideInInspector] public List<GameObject> ButtonsList = new List<GameObject>();
    FileInfo[] files;

    // Start is called before the first frame update
    private void Awake()
    {
        menuMain.SetActive( true );
        menuSelectLevel.SetActive( false );

        DirectoryInfo dir = new DirectoryInfo( Application.dataPath + "/Resources/Levels/" );
        files = dir.GetFiles( "*.txt" );
        GameObject newButton = null;
        int count = 0;
        int columns = 10;
        int col = 0;
        int row = 0;
        float BtnSize = buttonPrefab.GetComponent<RectTransform>().rect.width;
        float left = -Screen.width / 2f + BtnSize / 2f + 50f;
        float step = (Screen.width - 100f) / columns;

        foreach( FileInfo f in files ) {
            newButton = Instantiate( buttonPrefab );
            newButton.transform.GetChild( 0 ).GetComponent<Text>().text = Path.GetFileNameWithoutExtension( f.Name );
            newButton.transform.GetChild( 1 ).GetComponent<Text>().text = Path.GetFileNameWithoutExtension( f.Name );
            newButton.transform.SetParent( menuSelectLevel.gameObject.transform );
            newButton.transform.localPosition = new Vector3( left + col * step, 150f + row * step, 0f );
            newButton.transform.localScale = new Vector3( 1f, 1f, 1f );
            newButton.name = "BtnLevel_" + count;
            int param = count;
            newButton.GetComponent<Button>().onClick.AddListener( delegate { OnClickPlayLevel( param ); } );
            ButtonsList.Add( newButton );
            count++;
            col++;
            if ( col >= columns ) {
                col = 0;
                row++;
            }
            if( count >= 30 ) break;
        }

        if ( (PlayerPrefs.GetInt( "OpenMenu", 0 ) == 1) ) {
            OnPlayClick();
        }
    }

    // Update is called once per frame
    void Update()
    {
        tiker += Time.deltaTime;
        //if( tiker >= 100f ) tiker = 0f;
        alpha = (Mathf.Sin( tiker * 6f ) + 1f) * 0.5f * ( 1f - 0.5f ) + 0.5f;
        textTitle.GetComponent<Text>().color = new Color( 16f / 255f, 59f / 255f, 111f / 255f, alpha );
    }

    public void OnPlayClick()
    {
        menuMain.SetActive( false );
        menuSelectLevel.SetActive( true );
    }

    public void OnBackClick()
    {
        menuMain.SetActive( true );
        menuSelectLevel.SetActive( false );
    }

    public void OnClickPlayLevel( int index )
    {

        if( ButtonsList.Count > 0 ) {
            PlayerPrefs.SetString( "LevelFileName", files[index].Name );
            SceneManager.LoadScene( "GameScene" );
        }
    }

    void OnApplicationQuit()
    {
        PlayerPrefs.SetInt( "OpenMenu", 0 );
    }

    public void OnExitClick()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
		Application.Quit();
#endif
    }
}
