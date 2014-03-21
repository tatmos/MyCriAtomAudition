using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

public class MyCriAtomAudition : EditorWindow {
	#region Variables
	class MyCueInfo
	{
		public MyCueInfo(string name,CriAtomEx.CueInfo info)
		{
			cueSheet = name;
			cueInfo = info;
		}
		public string cueSheet;
		public CriAtomEx.CueInfo cueInfo;
	}

	private CriAtomSource source = null;
	private Vector2 scrollPos;
	private Vector2 scrollPos_Window;
	private Rect windowRect = new Rect(10, 10, 100, 100);
	private bool scaling = true;
	private Texture2D progressBackground;
	private Texture2D progressForground;
	private List<MyCueInfo> cueInfoList = new List<MyCueInfo>();
	// Public
	public string dspBusSetting = "DspBusSetting_0";
	#endregion
	[MenuItem("CRI/My/Open CRI Atom AtomAudition ...")]
	static void OpenWindow()
	{
		EditorWindow.GetWindow<MyCriAtomAudition>(false, "CRI Atom AtomAudition");
	}

	static MyCriAtomAudition()
	{
		//EditorApplication.update += Update;
	}

	void GetSource()
	{
		if (this.source == null) {
			// ref : http://qiita.com/shin5734/items/fcf02aa84516dfad5d9c
			// Project & Sceneにある GameObject を持つ全オブジェクトを取得
			foreach(GameObject obj in Resources.FindObjectsOfTypeAll(typeof(GameObject)))
			{
				string path = AssetDatabase.GetAssetOrScenePath(obj);
				
				string sceneExtension = ".unity";
				bool isExistInScene = Path.GetExtension(path).Equals(sceneExtension);
				if(isExistInScene){ 
					CriAtomSource atomSource = obj.GetComponent<CriAtomSource>();
					if(atomSource != null)
					{
						this.source = atomSource;//シーン上のAtomSourceを借りる（再生用）
						break;
					}
					// シーンのオブジェクトを格納するリストに登録 
				}else{ 
					// プロジェクトのオブジェクトを格納するリストに登録 
				}
			}
		}
	}

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
		if (/*EditorApplication.isCompiling && */EditorApplication.isPlaying)
		{
			if(progressBackground == null){
				progressBackground = new Texture2D(16,16);
				progressForground = new Texture2D(16,16);
			}

			Repaint ();
		}
	}
	private void ScalingWindow(int windowID)
	{
		GUILayout.Box("", GUILayout.Width(20), GUILayout.Height(20));
		if (Event.current.type == EventType.MouseUp)
			this.scaling = false;
		else if (Event.current.type == EventType.MouseDown && GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition))
			this.scaling = true;
		
		if (this.scaling)
			this.windowRect = new Rect(windowRect.x, windowRect.y, windowRect.width + Event.current.delta.x, windowRect.height + Event.current.delta.y);
		
	}

	private void OnGUI()
	{
		this.windowRect = GUILayout.Window(0, windowRect, ScalingWindow, "resizeable", GUILayout.MinHeight(80), GUILayout.MaxHeight(200));

		this.scrollPos_Window = GUILayout.BeginScrollView(this.scrollPos_Window);
		{
			if (/*EditorApplication.isCompiling && */EditorApplication.isPlaying)
			{
				GUIAudition();
			}
		}
		GUILayout.EndScrollView();
	}

	private void GUIAudition()
	{
		//this.acfPath = EditorGUILayout.TextField("ACF File Path", this.acfPath, EditorStyles.label);
		//this.dspBusSetting = EditorGUILayout.TextField("DSP Bus Setting", this.dspBusSetting, EditorStyles.label);
		
		//EditorGUILayout.Space();

		//EditorGUILayout.BeginVertical();
		//GUILayout.Space(32.0f);
		//EditorGUILayout.EndVertical();
		GUI.color = Color.green;
		if(GUILayout.Button("Reload"))
		{
			string[] files = System.IO.Directory.GetFiles(Application.streamingAssetsPath);
			int acbIndex = 0;

			foreach (string file in files) {
				if (System.IO.Path.GetExtension(file.Replace("\\","/")) == ".acb") {
					
					Debug.Log(file.ToString());
					//CriAtomAcfInfo.AcbInfo acbInfo = new CriAtomAcfInfo.AcbInfo(System.IO.Path.GetFileNameWithoutExtension(file),
					//                                                            acbIndex,"",System.IO.Path.GetFileName(file),"","");
					
					//Debug.LogWarning("ADX2 acb " + file.ToString());	
					
					/* 指定したACBファイル名(キューシート名)を指定してキュー情報を取得 */
					CriAtomExAcb acb = CriAtomExAcb.LoadAcbFile(null, file.Replace("\\","/"), "");
					if(acb != null){
						/* キュー名リストの作成 */
						int cueIndex = 0;
						CriAtomEx.CueInfo[] cueInfoListArray = acb.GetCueInfoList();
						foreach(CriAtomEx.CueInfo cueInfo in cueInfoListArray){
							//CriAtomAcfInfo.CueInfo tmpCueInfo = new CriAtomAcfInfo.CueInfo(cueInfo.name,cueInfo.id,"");
							//acbInfo.cueInfoList.Add(cueInfo.id,tmpCueInfo);
							
							//Debug.LogWarning("ADX2 cue " + cueInfo.name.ToString());
							Debug.Log(cueInfo.name);
							cueInfoList.Add(new MyCueInfo(Path.GetFileNameWithoutExtension(file),cueInfo));

							cueIndex++;
						}
						//CriAtomAcfInfo.acfInfo.acbInfoList.Add(acbInfo);
						acbIndex++;
						acb.Dispose();
						acb = null;
					} else {
						Debug.LogWarning("ADX2 acb null" + file.ToString());	
					}
				}
			}
		}

		this.GetSource();
		foreach (MyCueInfo inf in cueInfoList) {
			EditorGUILayout.BeginHorizontal();
			
			if(inf.cueSheet != this.source.cueSheet)GUI.color = Color.gray; else GUI.color = Color.yellow;
			if (GUILayout.Button(inf.cueInfo.name, EditorStyles.radioButton)) {
				//this.selectedCueId = inf.id;

				this.source.Stop();
				this.source.cueName = inf.cueInfo.name;

				this.source.Play();
			}
			GUILayout.Label(inf.cueInfo.id.ToString(), GUILayout.Width(40));
			EditorGUILayout.EndHorizontal();
		}
		GUI.color = Color.white;


	}

}
