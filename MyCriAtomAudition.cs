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
	private string nameFilter = "";
	private bool viewId = true;
	private bool viewCueSheet = false;
	private bool viewLength = false;
	private bool viewPriorty = false;
	private bool viewType = false;
	private bool viewUserData = false;
	private bool viewNumLimits = false;
	private bool viewNumBlocks = false;
	private bool viewCategories = false;
	private bool view3dInfo = false;
	private bool viewGameVariable = false;
	private Vector2 cueScrollPosition;
	// Public
	public string dspBusSetting = "DspBusSetting_0";
	#endregion
	[MenuItem("CRI/My/Open CRI Atom Audition ...")]
	static void OpenWindow()
	{
		EditorWindow.GetWindow<MyCriAtomAudition>(false, "CRI Atom Audition");
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
				progressBackground = new Texture2D(1,1);
				progressForground = new Texture2D(1,1);
				
				progressBackground.SetPixel(0, 0, new Color(1,1,1,0.2f));
				progressForground.SetPixel(0, 0, new Color(1,1,1,0.1f));
				Reload();
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

	private void Reload()
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

	bool monoFlag = true;
	private void GUIAudition()
	{
		//this.acfPath = EditorGUILayout.TextField("ACF File Path", this.acfPath0, EditorStyles.label);
		this.nameFilter = EditorGUILayout.TextField("Search", this.nameFilter);//, EditorStyles.label);
		
		//EditorGUILayout.Space();

		//EditorGUILayout.BeginVertical();
		//GUILayout.Space(32.0f);
		//EditorGUILayout.EndVertical();
		this.GetSource();

		EditorGUILayout.BeginHorizontal();
		GUI.color = Color.green;
		if(GUILayout.Button("Reload"))
		{
			Reload ();
		}


		if(GUILayout.Button("Stop"))
		{
			this.source.Stop();
		}
		this.monoFlag = GUILayout.Toggle(this.monoFlag,"Mono mode");

		EditorGUILayout.EndHorizontal();

		EditorGUILayout.BeginHorizontal();
		this.viewId = GUILayout.Toggle(this.viewId,"id");
		
		this.viewType = GUILayout.Toggle(this.viewType,"type");
		this.viewUserData = GUILayout.Toggle(this.viewUserData,"user");
		this.viewLength = GUILayout.Toggle(this.viewLength,"length");
		this.viewPriorty = GUILayout.Toggle(this.viewPriorty,"priority");
		this.viewNumLimits = GUILayout.Toggle(this.viewNumLimits,"limits");
		EditorGUILayout.EndHorizontal();
		EditorGUILayout.BeginHorizontal();
		this.viewNumBlocks = GUILayout.Toggle(this.viewNumBlocks,"blocks");
		this.viewCategories = GUILayout.Toggle(this.viewCategories,"categ");
		this.view3dInfo = GUILayout.Toggle(this.view3dInfo,"3dInfo");
		this.viewGameVariable = GUILayout.Toggle(this.viewGameVariable,"variable");
		this.viewCueSheet = GUILayout.Toggle(this.viewCueSheet,"cuesheet");
		EditorGUILayout.EndHorizontal();


		cueScrollPosition = EditorGUILayout.BeginScrollView(cueScrollPosition);
		EditorGUILayout.PrefixLabel("");
		foreach (MyCueInfo inf in cueInfoList) {
			if(viewLength)
			{
				Rect r = GUILayoutUtility.GetLastRect();
				if(inf.cueInfo.length < 0){
					DrawProgress(new Vector2(18,r.y+18),
					             new Vector2(Screen.width/2,r.height),
					             2,
					             2,
					             "");
				} else {
					DrawProgress(new Vector2(18,r.y+18),
					             new Vector2(Screen.width/2,r.height),
					             inf.cueInfo.length/12000f,
					             inf.cueInfo.length/12000f,
					             "");
				}
			}

			if(this.nameFilter !=""){
				if(inf.cueInfo.name.ToLower().IndexOf(this.nameFilter.ToLower()) < 0) continue;
			}
			EditorGUILayout.BeginHorizontal();
			
			if(this.source.cueSheet == "")
			{
				GUI.color = new Color(0.9f,0.9f,0.9f);
			} else if(inf.cueSheet != this.source.cueSheet)
			{
				GUI.color = Color.gray; 
			}else {
				GUI.color = Color.yellow;
			}

			if (GUILayout.Button(inf.cueInfo.name, EditorStyles.radioButton)) {

				Event e = Event.current;
				//this.selectedCueId = inf.id;
				if (monoFlag) {
					if(e.shift == false){
						this.source.Stop();
					}
				} 
				this.source.cueName = inf.cueInfo.name;

				this.source.Play();
			}
			if(viewId)GUILayout.Label(inf.cueInfo.id.ToString(),GUILayout.ExpandWidth(false));
			if(viewType)GUILayout.Label(inf.cueInfo.type.ToString(), GUILayout.Width(65));
			if(viewUserData)GUILayout.Label(inf.cueInfo.userData.ToString(), GUILayout.Width(40));
			if(viewLength)
			{
				if(inf.cueInfo.length < 0){
					GUILayout.Label("loop", GUILayout.Width(40));
				} else {
					GUILayout.Label(string.Format("{0:F3}", inf.cueInfo.length/1000f), GUILayout.Width(40));
				}
			}
			if(viewPriorty)GUILayout.Label(inf.cueInfo.priority.ToString(), GUILayout.Width(40));
			if(viewNumLimits)
			{
				if(inf.cueInfo.numLimits < 0){
					GUILayout.Label("noLimit", GUILayout.Width(45));
				} else {
					GUILayout.Label(inf.cueInfo.numLimits.ToString(), GUILayout.Width(45));
				}
			}
			if(viewNumBlocks)
			{
				if(inf.cueInfo.numBlocks < 1){
					GUILayout.Label("noBlk", GUILayout.Width(40));
				} else {
					GUILayout.Label(inf.cueInfo.numBlocks.ToString(), GUILayout.Width(40));
				}
			}
			if(viewCategories)
			{
				int id = 0;
				foreach(ushort categoryId in inf.cueInfo.categories){
					if(!(categoryId == 65535 || categoryId == 0)){
						GUILayout.Label(categoryId.ToString(), GUILayout.Width(65));				
					} else {
						GUILayout.Label("no Category",
					                GUILayout.Width(65));
					}
					if(id == 3)break;
					id++;
				}
			}
			if(view3dInfo)
			{
				
				GUILayout.Label(string.Format("doppler {0}", inf.cueInfo.pos3dInfo.dopplerFactor), GUILayout.Width(30));
				GUILayout.Label(string.Format("({0}-{1})", inf.cueInfo.pos3dInfo.minDistance,inf.cueInfo.pos3dInfo.maxDistance), GUILayout.Width(60));

				if(!(inf.cueInfo.pos3dInfo.distanceAisacControl == 65535 && 
				     inf.cueInfo.pos3dInfo.listenerBaseAngleAisacControl == 65535 && 
				     inf.cueInfo.pos3dInfo.sourceBaseAngleAisacControl == 65535)){
					GUILayout.Label(string.Format("(distance:{0}listenrAngle:{1}sourceAngle:{2})", 
					                              inf.cueInfo.pos3dInfo.distanceAisacControl, 
					                              inf.cueInfo.pos3dInfo.listenerBaseAngleAisacControl,
					                              inf.cueInfo.pos3dInfo.sourceBaseAngleAisacControl), GUILayout.Width(70));	

//				if(!(inf.cueInfo.pos3dInfo.distanceAisacControl == 65535 && inf.cueInfo.pos3dInfo.angleAisacControl == 65535)){
//					GUILayout.Label(string.Format("(d:{0}a:{1})", inf.cueInfo.pos3dInfo.distanceAisacControl, inf.cueInfo.pos3dInfo.angleAisacControl), GUILayout.Width(70));			
				}else {
					GUILayout.Label("no AISAC",
					GUILayout.Width(70));
				}
			}
			if(this.viewGameVariable )
			{
				if(!(inf.cueInfo.gameVariableInfo.id == 65535)){
					GUILayout.Label(string.Format("({0}\"{1}\":{2})", inf.cueInfo.gameVariableInfo.id,
				                              inf.cueInfo.gameVariableInfo.name,
												inf.cueInfo.gameVariableInfo.gameValue),
				                GUILayout.Width(160));
				} else {
					GUILayout.Label("no Variable",
					                GUILayout.Width(160));
				}
			}
			if(viewCueSheet)GUILayout.Label(inf.cueSheet.ToString(), GUILayout.Width(140));
			EditorGUILayout.EndHorizontal();
		}
		EditorGUILayout.EndScrollView();
		GUI.color = Color.white;
	}

	private void DrawProgress(Vector2 location ,Vector2 size,float progress,float progressHold,string valueString)
	{
		Color tmpColor = GUI.color;
		//GUI.color = Color.gray;
		//GUI.DrawTexture(new Rect(location.x, location.y, size.x, size.y), progressBackground);
		if(progress > 1){
			GUI.color = new Color(0.5f,1,0.5f,0.2f);
		} else {
			GUI.color = new Color(0.8f,0.5f,0.3f,0.2f);
		}
		EditorGUI.DrawTextureAlpha(new Rect(location.x, location.y, size.x * progress, size.y), progressForground); 
		//EditorGUI.DrawTextureAlpha(new Rect(size.x * progressHold-1f, location.y, 2f, size.y), progressForground); 
		//EditorGUI.DrawTextureAlpha
		//GUI.color = Color.white;
		//EditorGUI.DropShadowLabel(new Rect(location.x, location.y, size.x, size.y), valueString); 
		GUI.color = tmpColor;
	}
}
