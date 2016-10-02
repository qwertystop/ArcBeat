using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using System.IO;

public class Browsing : MonoBehaviour {
    private FileBrowser fb;
    public Canvas buttonCanvas;
    public Canvas loadingCanvas;
    public Text loadingText;

	// Use this for initialization
	void Start () {
        enabled = false;
        loadingCanvas.enabled = false;
        fb = new FileBrowser();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    // start the game
    public void startGame() {
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainScene");
    }

    public void setAuto(int player) {
        Utilities.Globals.autos[player] = !Utilities.Globals.autos[player];
    }

    // OnGui is called once per frame
    void OnGUI() {
        if (fb.draw())
        {
            if (fb.outputFile != null)
            {
                // file selected, work with it
                loadingCanvas.enabled = true;
                loadAudio(Utilities.Preprocessor.processOsuFile(fb.outputFile, loadingText));
            }
            enabled = false; // disable after
        }
    }
    
    private void loadAudio(FileInfo audiofile) {
        loadingText.text = "Loading audio file.";
        WWW www = new WWW("file:///" + audiofile.FullName);
        if (www.error != null)
        {// something wrong
            Debug.Log(www.error);
        }

        AudioClip ac = www.GetAudioClip(false, false);
        ac.LoadAudioData();
        ac.name = Path.GetFileNameWithoutExtension(audiofile.Name);
        while (ac.loadState == AudioDataLoadState.Loading || ac.loadState == AudioDataLoadState.Unloaded)
        {
        }
        if (ac.loadState == AudioDataLoadState.Failed)
        {
            Debug.LogError("audio load failed");
        }
        Utilities.Globals.audioClip = ac;
        buttonCanvas.GetComponentInChildren<Text>().text = fb.outputFile.Name; // button shows selected file
        buttonCanvas.enabled = true;// enable buttons after loading done
        loadingText.text = "";
    }
}
