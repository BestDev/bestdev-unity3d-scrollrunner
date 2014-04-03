/*
Permission is hereby granted, free of charge, to any person  obtaining a copy of this software and associated documentation  files (the "Software"), to deal in the Software without  restriction, including without limitation the rights to use,  copy, modify, merge, publish, distribute, sublicense, and/or sell  copies of the Software, and to permit persons to whom the  Software is furnished to do so, subject to the following conditions:
The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/
using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Text;
using System.Text.RegularExpressions;

/// <summary> 
/// <para>A small data structure class hold values for making Doxygen config files </para>
/// <para> Fixed: SungMin Lee (http://bestdev.net)</para>
/// <para> 2014-04-02 : Other Option Control Add</para>
/// <para> 2014-04-02 : Fixed BaseDoxyFile (Version Up : Doxyfile 1.8.6)</para>
/// </summary>
public class DoxygenConfig
{
	public string Project = PlayerSettings.productName;
	public string Synopsis = "";
	public string Version = PlayerSettings.bundleVersion;
	public string ScriptsDirectory = Application.dataPath;
	public string DocDirectory = Application.dataPath.Replace("Assets", "Docs");
	public string PathtoDoxygen = "C:/Program Files/doxygen/bin/doxygen.exe";

	// 20140402 Add(SungMin Lee:Other Option Control Add)
	// Project
	public string DoxyFileEncoding = "EUC-KR";
	public string OutputLanguage = "Korean";

	// Build
	public string ExtractAll = "YES";
	public string ExtractPrivate = "YES";
	public string ExtractStatic = "YES";

	// Input
	public string InputEncoding = "EUC-KR";

	// HTML
	public string GenerateHtmlHelp = "YES";
	public string ChmFile = "../" + PlayerSettings.productName + ".chm";
	public string HHCLocation = "C:/Program Files (x86)/HTML Help Workshop/hhc.exe";
	public string GenerateCHI = "YES";
	public string ChmIndexEncoding = "EUC-KR";

	// PreProcessor
	public string MacroExpansion = "YES";
	// 2014-04-02 PREDEFINED 입력 처리가 깔끔하지 못해서 프로젝트별 설정 파일에서 직접 수정하도록 함 (추후 추가 여부 고민)
	//public string PreDefined = "";

	// Dot
	public string ClassDiagrams = "YES";
	public string HaveDot = "YES";
	public string UmlLook = "YES";
	public string DotPath = "C:/Program Files (x86)/Graphviz2.36/bin";
}

/// <summary>
/// <para> A Editor Plugin for automatic doc generation through Doxygen</para>
/// <para> Author: Jacob Pennock (http://Jacobpennock.com)</para>
/// <para> Version: 1.0</para>	
/// <para> Fixed: SungMin Lee (http://bestdev.net)</para>
/// <para> 2014-04-02 : ETC Option Control Add</para>
/// </summary>
public class DoxygenWindow : EditorWindow 
{
	public static DoxygenWindow Instance;
	public enum WindowModes{Generate,Configuration,About}
	public string UnityProjectID = PlayerSettings.productName+":";
	public string AssestsFolder = Application.dataPath;
	public string[] Themes = new string[3] {"Default", "Dark and Colorful", "Light and Clean"};
	public int SelectedTheme = 1;
	WindowModes DisplayMode = WindowModes.Generate;
	static DoxygenConfig Config;
	static bool DoxyFileExists = false;
	StringReader reader;
	TextAsset basefile;
	float DoxyfileCreateProgress = -1.0f;
	float DoxyoutputProgress = -1.0f;
	string CreateProgressString = "Creating Doxyfile..";
	public string BaseFileString = null;
	public string DoxygenOutputString = null;
	public string CurentOutput = null;
	DoxyThreadSafeOutput DoxygenOutput = null; 
	List<string> DoxygenLog = null;
	bool ViewLog = false;
	Vector2 scroll;
	bool DocsGenerated = false;

	[MenuItem( "Window/Documentation with Doxygen" )]
	public static void Init()
	{
		Instance = (DoxygenWindow)EditorWindow.GetWindow( typeof( DoxygenWindow ), false, "Documentation" );
		Instance.minSize = new Vector2( 420, 245 );
		Instance.maxSize = new Vector2( 420, 720 );

	}

	void OnEnable()
	{
		LoadConfig();
		DoxyoutputProgress = 0;
	}

	void OnDisable()
	{
		DoxyoutputProgress = 0;
		DoxygenLog = null;
	}

	void OnGUI()
	{
		DisplayHeadingToolbar();
		switch(DisplayMode)
		{
			case WindowModes.Generate:
				GenerateGUI();
			break;

			case WindowModes.Configuration:
				ConfigGUI();
			break;

			case WindowModes.About:
				AboutGUI();
			break;
		}
	}

	void DisplayHeadingToolbar()
	{
		GUIStyle normalButton = new GUIStyle( EditorStyles.toolbarButton );
		normalButton.fixedWidth = 140;
		GUILayout.Space (5);
		EditorGUILayout.BeginHorizontal( EditorStyles.toolbar );
		{
			if( GUILayout.Toggle( DisplayMode == WindowModes.Generate, "Generate Documentation", normalButton ) )
			{
				DoxyfileCreateProgress = -1;
				DisplayMode = WindowModes.Generate;
			}
			if( GUILayout.Toggle( DisplayMode == WindowModes.Configuration, "Settings/Configuration", normalButton ) )
			{
				DisplayMode = WindowModes.Configuration;
			}
			if( GUILayout.Toggle( DisplayMode == WindowModes.About, "About", normalButton ) )
			{
				DoxyfileCreateProgress = -1;
				DisplayMode = WindowModes.About;
			}
		}
		EditorGUILayout.EndHorizontal();
	}	

	void ConfigGUI()
	{
		GUILayout.Space (10);
		if(Config.Project == "Enter your Project name (Required)" || Config.Project == "" || Config.PathtoDoxygen == "" )
			GUI.enabled = false;
		if(GUILayout.Button ("Save Configuration and Build new DoxyFile", GUILayout.Height(40)))
		{
			MakeNewDoxyFile(Config);
		}
		if(DoxyfileCreateProgress >= 0)
		{
			Rect r = EditorGUILayout.BeginVertical();
			EditorGUI.ProgressBar(r, DoxyfileCreateProgress, CreateProgressString);
			GUILayout.Space(16);
			EditorGUILayout.EndVertical();
		}
		GUI.enabled = true;

		GUILayout.Space (20);
		GUILayout.Label("Set Path to Doxygen Install",EditorStyles.boldLabel);
		GUILayout.Space (5);
		EditorGUILayout.BeginHorizontal();
		Config.PathtoDoxygen = EditorGUILayout.TextField("Doxygen.exe : ",Config.PathtoDoxygen);
		if(GUILayout.Button ("...",EditorStyles.miniButtonRight, GUILayout.Width(22)))
			 Config.PathtoDoxygen = EditorUtility.OpenFilePanel("Where is doxygen.exe installed?","", "");
		EditorGUILayout.EndHorizontal();

		// 2014-04-02 Add(SungMin Lee:Project Option)
		GUILayout.Space(20);
		Config.DoxyFileEncoding = EditorGUILayout.TextField("DOXYFILE_ENCODING : ", Config.DoxyFileEncoding);
		Config.OutputLanguage = EditorGUILayout.TextField("OUTPUT_LANGUAGE : ", Config.OutputLanguage);


		// 2014-04-02 Add(SungMin Lee:Build Option)
		GUILayout.Space (20);
		Config.ExtractAll= EditorGUILayout.TextField("EXTRACT_ALL : ", Config.ExtractAll);
		Config.ExtractPrivate = EditorGUILayout.TextField("EXTRACT_PRIVATE : ", Config.ExtractPrivate);
		Config.ExtractStatic = EditorGUILayout.TextField("EXTRACT_STATIC : ", Config.ExtractStatic);

		// 2014-04-02 Add(SungMin Lee:Input Option)
		GUILayout.Space (20);
		Config.InputEncoding = EditorGUILayout.TextField("INPUT_ENCODING : ", Config.InputEncoding);

		// 2014-04-02 Add(SungMin Lee:HTML Option)
		GUILayout.Space (20);
		Config.GenerateHtmlHelp = EditorGUILayout.TextField("GENERATE_HTMLHELP : ", Config.GenerateHtmlHelp);
		GUILayout.Label("CHM Save File Path (exactly Relative Path)");
		Config.ChmFile = EditorGUILayout.TextField("CHM_FILE : ", Config.ChmFile);
		GUILayout.Label("HTML Help Workshop Execute File Path");
		EditorGUILayout.BeginHorizontal();
		Config.HHCLocation = EditorGUILayout.TextField("HHC_LOCATION : ", Config.HHCLocation);
		if(GUILayout.Button ("...",EditorStyles.miniButtonRight, GUILayout.Width(22)))
			Config.HHCLocation = EditorUtility.OpenFilePanel("Where is hhc.exe installed?", "", "");
		EditorGUILayout.EndHorizontal();
		Config.GenerateCHI = EditorGUILayout.TextField("GENERATE_CHI : ", Config.GenerateCHI);
		Config.ChmIndexEncoding = EditorGUILayout.TextField("CHM_INDEX_ENCODING : ", Config.ChmIndexEncoding);

		// 2014-04-02 Add(SungMin Lee:PreProcessor Option)
		GUILayout.Space (20);
		Config.MacroExpansion = EditorGUILayout.TextField("MACRO_EXPANSION : ", Config.MacroExpansion);
		//Config.PreDefined = EditorGUILayout.TextField("PREDEFINED : ", Config.PreDefined);

		// 2014-04-02 Add(SungMin Lee:Dot Option)
		GUILayout.Space (20);
		Config.ClassDiagrams = EditorGUILayout.TextField("CLASS_DIAGRAMS : ", Config.ClassDiagrams);
		Config.HaveDot = EditorGUILayout.TextField("HAVE_DOT : ", Config.HaveDot);
		Config.UmlLook = EditorGUILayout.TextField("UML_LOOK : ", Config.UmlLook);
		GUILayout.Label("Graphviz Execute File Path");
		EditorGUILayout.BeginHorizontal();
		Config.DotPath = EditorGUILayout.TextField("DOT_PATH : ", Config.DotPath);
		if (GUILayout.Button("...", EditorStyles.miniButtonRight, GUILayout.Width(22)))
			Config.DotPath = EditorUtility.OpenFolderPanel("Where is gvedit.exe installed?", "", "");
		EditorGUILayout.EndHorizontal();

		
		GUILayout.Label("Provide some details about the project",EditorStyles.boldLabel);
		GUILayout.Space (5);
		Config.Project = EditorGUILayout.TextField("Project Name: ",Config.Project);
		Config.Synopsis = EditorGUILayout.TextField("Project Brief: ",Config.Synopsis);
		Config.Version = EditorGUILayout.TextField("Project Version: ",Config.Version);
		
		GUILayout.Space (15);
		GUILayout.Label("Select Theme",EditorStyles.boldLabel);
		GUILayout.Space (5);
		SelectedTheme = EditorGUILayout.Popup(SelectedTheme,Themes) ;		

		GUILayout.Space (20);
		GUILayout.Label("Setup the Directories",EditorStyles.boldLabel);
		GUILayout.Space (5);
		EditorGUILayout.BeginHorizontal();
		Config.ScriptsDirectory = EditorGUILayout.TextField("Scripts folder: ",Config.ScriptsDirectory);
		if(GUILayout.Button ("...",EditorStyles.miniButtonRight, GUILayout.Width(22)))
			 Config.ScriptsDirectory = EditorUtility.OpenFolderPanel("Select your scripts folder", Config.ScriptsDirectory, "");
		EditorGUILayout.EndHorizontal();
		GUILayout.Space (5);
		EditorGUILayout.BeginHorizontal();
		Config.DocDirectory = EditorGUILayout.TextField("Output folder: ",Config.DocDirectory);
		if(GUILayout.Button ("...",EditorStyles.miniButtonRight, GUILayout.Width(22)))
			 Config.DocDirectory = EditorUtility.OpenFolderPanel("Select your ouput Docs folder", Config.DocDirectory, "");
		EditorGUILayout.EndHorizontal();
		
		GUILayout.Space (5);
		EditorGUILayout.BeginHorizontal();
		GUILayout.Space (5);
		GUILayout.Space (30);
		GUILayout.Label("By default Doxygen will search through your whole Assets folder for C# script files to document. Then it will output the documentation it generates into a folder called \"Docs\" that is placed in your project folder next to the Assets folder. If you would like to set a specific script or output folder you can do so above. ",EditorStyles.wordWrappedMiniLabel);
		GUILayout.Space (30);
		EditorGUILayout.EndHorizontal();
	}

	void AboutGUI()
	{
		GUIStyle CenterLable = new GUIStyle(EditorStyles.largeLabel);
		GUIStyle littletext = new GUIStyle(EditorStyles.miniLabel) ;
		CenterLable.alignment = TextAnchor.MiddleCenter;
		GUILayout.Space (20);
		GUILayout.Label( "Automatic C# Documentation Generation through Doxygen",CenterLable);
		GUILayout.Label( "Version: 1.0",CenterLable);
		GUILayout.Label( "By: Jacob Pennock",CenterLable);

		// 2014-04-02 Add(SungMin Lee:About Add)
		GUILayout.Space(20);
		GUILayout.Label("Other Option Control Add", CenterLable);
		GUILayout.Label("Fixed By SungMin Lee", CenterLable);
		if(GUILayout.Button("bestdev.net"))
			Application.OpenURL("http://bestdev.net");
		GUILayout.Label("2014-04-02", CenterLable);

		GUILayout.Space (20);
		EditorGUILayout.BeginHorizontal();
		GUILayout.Space (20);
		GUILayout.Label( "Follow me for more Unity tips and tricks",littletext);
		GUILayout.Space (15);
		if(GUILayout.Button( "twitter"))
			Application.OpenURL("http://twitter.com/@JacobPennock");
		GUILayout.Space (20);
		EditorGUILayout.EndHorizontal();

		GUILayout.Space (10);
		EditorGUILayout.BeginHorizontal();
		GUILayout.Space (20);
		GUILayout.Label( "Visit my site for more plugins and tutorials",littletext);
		if(GUILayout.Button( "JacobPennock.com"))
			Application.OpenURL("http://www.jacobpennock.com/Blog/?cat=19");
		GUILayout.Space (20);
		EditorGUILayout.EndHorizontal();
	}

	void GenerateGUI()
	{
		if(DoxyFileExists)
		{
			GUILayout.Space (10);
			if(!DocsGenerated)
				GUI.enabled = false;
			if(GUILayout.Button ("Browse Documentation", GUILayout.Height(40)))
				Application.OpenURL("File://"+Config.DocDirectory+"/html/index.html");
			GUI.enabled = true;	

			if(DoxygenOutput == null)
			{
				if(GUILayout.Button ("Run Doxygen", GUILayout.Height(40)))
				{
					DocsGenerated = false;
					RunDoxygen();
				}
					
				if(DocsGenerated && DoxygenLog != null)
				{
					if(GUILayout.Button( "View Doxygen Log",EditorStyles.toolbarDropDown))
						ViewLog = !ViewLog;
					if(ViewLog)
					{
						scroll = EditorGUILayout.BeginScrollView(scroll, GUILayout.ExpandHeight(true));
						foreach(string logitem in DoxygenLog)
						{
							EditorGUILayout.SelectableLabel(logitem,EditorStyles.miniLabel,GUILayout.ExpandWidth(true));
						}
		            	EditorGUILayout.EndScrollView();
					}
				}
			}
			else
			{
				if(DoxygenOutput.isStarted() && !DoxygenOutput.isFinished())
				{
					string currentline = DoxygenOutput.ReadLine();
					DoxyoutputProgress = DoxyoutputProgress + 0.1f;
					if(DoxyoutputProgress >= 0.9f)
						DoxyoutputProgress = 0.75f;
					Rect r = EditorGUILayout.BeginVertical();
					EditorGUI.ProgressBar(r, DoxyoutputProgress,currentline );
					GUILayout.Space(40);
					EditorGUILayout.EndVertical();
				}
	        	if(DoxygenOutput.isFinished())
	        	{
	        		if (Event.current.type == EventType.Repaint)
					{
						/*
						If you look at what SetTheme is doing, I know, it seems a little scary to be 
						calling file moving operations from inside a an OnGUI call like this. And 
						maybe it would be a better choice to call SetTheme and update these other vars
						from inside of the OnDoxygenFinished callback. But since the callback is static
						that would require a little bit of messy singleton instance checking to make sure
						the call back was calling into the right functions. I did try to do it that way
						but for some reason the file operations failed every time. I'm not sure why.
						This is what I was getting from the debugger:
						
						Error in file: C:/BuildAgent/work/842f9551727e852/Runtime/Mono/MonoManager.cpp at line 2212
						UnityEditor.FileUtil:DeleteFileOrDirectory(String)
						UnityEditor.FileUtil:ReplaceFile(String, String) (at C:\BuildAgent\work\842f9557127e852\Editor\MonoGenerated\Editor\FileUtil.cs:42)

						Doing them here seems to work every time and the Repaint event check ensures that they will only be done once.
						*/
						SetTheme(SelectedTheme);
		        		DoxygenLog = DoxygenOutput.ReadFullLog();
		        		DoxyoutputProgress = -1.0f;
		        		DoxygenOutput = null;
	        			DocsGenerated = true;
	        			EditorPrefs.SetBool(UnityProjectID+"DocsGenerated",DocsGenerated);
					}
	        	}
			}
		}
		else
		{
			GUIStyle ErrorLabel = new GUIStyle(EditorStyles.largeLabel);
			ErrorLabel.alignment = TextAnchor.MiddleCenter;
			GUILayout.Space(20);
			GUI.contentColor = Color.red;
			GUILayout.Label("You must set the path to your Doxygen install and \nbuild a new Doxyfile before you can generate documentation",ErrorLabel);
		}
	}

	public void readBaseConfig()
	{
		basefile = (TextAsset)Resources.Load("BaseDoxyfile", typeof(TextAsset));
		reader = new StringReader(basefile.text);
		if ( reader == null )
		   UnityEngine.Debug.LogError("BaseDoxyfile not found or not readable");
		else
		   BaseFileString = reader.ReadToEnd();
	}

	public void MakeNewDoxyFile(DoxygenConfig config)
	{
		SaveConfigtoEditor(config);
		CreateProgressString = "Creating Output Folder";
		DoxyfileCreateProgress = 0.01f;
		System.IO.Directory.CreateDirectory(config.DocDirectory);

		DoxyfileCreateProgress = 0.02f;
		string newfile = BaseFileString.Replace("PROJECT_NAME           =", "PROJECT_NAME           = "+"\""+config.Project+"\"");
		DoxyfileCreateProgress = 0.04f;
		newfile = newfile.Replace("PROJECT_NUMBER         =", "PROJECT_NUMBER         = "+config.Version);
		DoxyfileCreateProgress = 0.06f;
		newfile = newfile.Replace("PROJECT_BRIEF          =", "PROJECT_BRIEF          = "+"\""+config.Synopsis+"\"");
		DoxyfileCreateProgress = 0.08f;
		newfile = newfile.Replace("OUTPUT_DIRECTORY       =", "OUTPUT_DIRECTORY       = "+"\""+config.DocDirectory+"\"");
		DoxyfileCreateProgress = 0.10f;
		newfile = newfile.Replace("INPUT                  =", "INPUT                  = "+"\""+config.ScriptsDirectory+"\"");
		DoxyfileCreateProgress = 0.12f;

		// 2014-04-02 Add(SungMin Lee:Other Option Add)
		newfile = newfile.Replace("DOXYFILE_ENCODING      = UTF-8", "DOXYFILE_ENCODING      = " + config.DoxyFileEncoding);
		DoxyfileCreateProgress = 0.15f;
		newfile = newfile.Replace("OUTPUT_LANGUAGE        = English", "OUTPUT_LANGUAGE        = " + config.OutputLanguage);
		DoxyfileCreateProgress = 0.20f;
		newfile = newfile.Replace("EXTRACT_ALL            = YES", "EXTRACT_ALL            = " + config.ExtractAll);
		DoxyfileCreateProgress = 0.25f;
		newfile = newfile.Replace("EXTRACT_PRIVATE        = YES", "EXTRACT_PRIVATE        = " + config.ExtractPrivate);
		DoxyfileCreateProgress = 0.30f;
		newfile = newfile.Replace("EXTRACT_STATIC         = YES", "EXTRACT_STATIC         = " + config.ExtractStatic);
		DoxyfileCreateProgress = 0.35f;
		newfile = newfile.Replace("INPUT_ENCODING         = UTF-8", "INPUT_ENCODING         = " + config.InputEncoding);
		DoxyfileCreateProgress = 0.40f;
		newfile = newfile.Replace("GENERATE_HTMLHELP      = YES", "GENERATE_HTMLHELP      = " + config.GenerateHtmlHelp);
		DoxyfileCreateProgress = 0.45f;
		newfile = newfile.Replace("CHM_FILE               =", "CHM_FILE               = " + "\"" + config.ChmFile + "\"");
		DoxyfileCreateProgress = 0.50f;
		newfile = newfile.Replace("HHC_LOCATION           =", "HHC_LOCATION           = " + "\"" + config.HHCLocation + "\"");
		DoxyfileCreateProgress = 0.55f;
		newfile = newfile.Replace("GENERATE_CHI           = YES", "GENERATE_CHI           = " + config.GenerateCHI);
		DoxyfileCreateProgress = 0.60f;
		newfile = newfile.Replace("CHM_INDEX_ENCODING     = UTF-8", "CHM_INDEX_ENCODING     = " + config.ChmIndexEncoding);
		DoxyfileCreateProgress = 0.65f;
		newfile = newfile.Replace("MACRO_EXPANSION        = YES", "MACRO_EXPANSION        = " + config.MacroExpansion);
		DoxyfileCreateProgress = 0.70f;
		//newfile = newfile.Replace("PREDEFINED             =", "PREDEFINED             = " + "\"" + config.PreDefined+ "\"");
		//DoxyfileCreateProgress = 0.75f;
		newfile = newfile.Replace("CLASS_DIAGRAMS         = YES", "CLASS_DIAGRAMS         = " + config.ClassDiagrams);
		DoxyfileCreateProgress = 0.80f;
		newfile = newfile.Replace("HAVE_DOT               = YES", "HAVE_DOT               = " + config.HaveDot);
		DoxyfileCreateProgress = 0.85f;
		newfile = newfile.Replace("UML_LOOK               = YES", "UML_LOOK               = " + config.UmlLook);
		DoxyfileCreateProgress = 0.90f;
		newfile = newfile.Replace("DOT_PATH               =", "DOT_PATH               = " + "\"" + config.DotPath + "\"");
		DoxyfileCreateProgress = 0.95f;

		switch(SelectedTheme)
		{
			case 0:
				newfile = newfile.Replace("GENERATE_TREEVIEW      = NO", "GENERATE_TREEVIEW      = YES");
			break;
			case 1:
				newfile = newfile.Replace("SEARCHENGINE           = YES", "SEARCHENGINE           = NO");
				newfile = newfile.Replace("CLASS_DIAGRAMS         = YES", "CLASS_DIAGRAMS         = NO");
			break;
		}

		CreateProgressString = "New Options Set";

		StringBuilder sb = new StringBuilder();
		sb.Append(newfile);
        StreamWriter NewDoxyfile = new StreamWriter(config.DocDirectory + @"\Doxyfile");
        
        NewDoxyfile.Write(sb.ToString());
        NewDoxyfile.Close();
        DoxyfileCreateProgress = 1.0f;
        CreateProgressString = "New Doxyfile Created!";
        DoxyFileExists = true;
        EditorPrefs.SetBool(UnityProjectID+"DoxyFileExists",DoxyFileExists);
	}

	void SaveConfigtoEditor(DoxygenConfig config)
	{
		EditorPrefs.SetString(UnityProjectID + "DoxyProjectName",config.Project);
		EditorPrefs.SetString(UnityProjectID + "DoxyProjectNumber",config.Version);
		EditorPrefs.SetString(UnityProjectID + "DoxyProjectBrief",config.Synopsis);
		EditorPrefs.SetString(UnityProjectID + "DoxyProjectFolder",config.ScriptsDirectory);
		EditorPrefs.SetString(UnityProjectID + "DoxyProjectOutput",config.DocDirectory);
		EditorPrefs.SetString("DoxyEXE", config.PathtoDoxygen);
		EditorPrefs.SetInt(UnityProjectID+"DoxyTheme", SelectedTheme);

		// 2014-04-02 Add(SungMin Lee:Other Option Add)
		EditorPrefs.SetString(UnityProjectID + "DoxyOption_DOXYFILE_ENCODING", config.DoxyFileEncoding);
		EditorPrefs.SetString(UnityProjectID + "DoxyOption_OUTPUT_LANGUAGE", config.OutputLanguage);
		EditorPrefs.SetString(UnityProjectID + "DoxyOption_EXTRACT_ALL", config.ExtractAll);
		EditorPrefs.SetString(UnityProjectID + "DoxyOption_EXTRACT_PRIVATE", config.ExtractPrivate);
		EditorPrefs.SetString(UnityProjectID + "DoxyOption_EXTRACT_STATIC", config.ExtractStatic);
		EditorPrefs.SetString(UnityProjectID + "DoxyOption_INPUT_ENCODING", config.InputEncoding);
		EditorPrefs.SetString(UnityProjectID + "DoxyOption_GENERATE_HTMLHELP", config.GenerateHtmlHelp);
		EditorPrefs.SetString(UnityProjectID + "DoxyOption_CHM_FILE", config.ChmFile);
		EditorPrefs.SetString("DoxyOption_HHC_LOCATION", config.HHCLocation);
		EditorPrefs.SetString(UnityProjectID + "DoxyOption_GENERATE_CHI", config.GenerateCHI);
		EditorPrefs.SetString(UnityProjectID + "DoxyOption_CHM_INDEX_ENCODING", config.ChmIndexEncoding);
		EditorPrefs.SetString(UnityProjectID + "DoxyOption_MACRO_EXPANSION", config.MacroExpansion);
		//EditorPrefs.SetString(UnityProjectID + "DoxyOption_PREDEFINED", config.PreDefined);
		EditorPrefs.SetString(UnityProjectID + "DoxyOption_CLASS_DIAGRAMS", config.ClassDiagrams);
		EditorPrefs.SetString(UnityProjectID + "DoxyOption_HAVE_DOT", config.HaveDot);
		EditorPrefs.SetString(UnityProjectID + "DoxyOption_UML_LOOK", config.UmlLook);
		EditorPrefs.SetString("DoxyOption_DOT_PATH", config.DotPath);

	}

	void LoadConfig()
	{
		if(BaseFileString == null)
			readBaseConfig();

		if(Config == null)
		{
			if(!LoadSavedConfig())
				Config = new DoxygenConfig();
		}

		if(EditorPrefs.HasKey(UnityProjectID + "DoxyFileExists"))
			DoxyFileExists = EditorPrefs.GetBool(UnityProjectID + "DoxyFileExists");
		if(EditorPrefs.HasKey(UnityProjectID + "DocsGenerated"))
			DocsGenerated = EditorPrefs.GetBool(UnityProjectID + "DocsGenerated");
		if(EditorPrefs.HasKey(UnityProjectID + "DoxyTheme"))
			SelectedTheme = EditorPrefs.GetInt(UnityProjectID + "DoxyTheme");
		if(EditorPrefs.HasKey("DoxyEXE"))
			Config.PathtoDoxygen = EditorPrefs.GetString("DoxyEXE");

		// 2014-04-02 Add(SungMin Lee:Other Option Add)
		if (EditorPrefs.HasKey(UnityProjectID + "DoxyOption_DOXYFILE_ENCODING"))
			Config.DoxyFileEncoding = EditorPrefs.GetString(UnityProjectID + "DoxyOption_DOXYFILE_ENCODING");
		if (EditorPrefs.HasKey(UnityProjectID + "DoxyOption_OUTPUT_LANGUAGE"))
			Config.OutputLanguage = EditorPrefs.GetString(UnityProjectID + "DoxyOption_OUTPUT_LANGUAGE");
		if (EditorPrefs.HasKey(UnityProjectID + "DoxyOption_EXTRACT_ALL"))
			Config.ExtractAll = EditorPrefs.GetString(UnityProjectID + "DoxyOption_EXTRACT_ALL");
		if (EditorPrefs.HasKey(UnityProjectID + "EXTRACT_PRIVATE"))
			Config.ExtractPrivate = EditorPrefs.GetString(UnityProjectID + "DoxyOption_EXTRACT_PRIVATE");
		if (EditorPrefs.HasKey(UnityProjectID + "DoxyOption_EXTRACT_STATIC"))
			Config.ExtractStatic = EditorPrefs.GetString(UnityProjectID + "DoxyOption_EXTRACT_STATIC");
		if (EditorPrefs.HasKey(UnityProjectID + "DoxyOption_INPUT_ENCODING"))
			Config.InputEncoding = EditorPrefs.GetString(UnityProjectID + "DoxyOption_INPUT_ENCODING");
		if (EditorPrefs.HasKey(UnityProjectID + "DoxyOption_GENERATE_HTMLHELP"))
			Config.GenerateHtmlHelp = EditorPrefs.GetString(UnityProjectID + "DoxyOption_GENERATE_HTMLHELP");
		if (EditorPrefs.HasKey(UnityProjectID + "DoxyOption_CHM_FILE"))
			Config.ChmFile = EditorPrefs.GetString(UnityProjectID + "DoxyOption_CHM_FILE");
		if (EditorPrefs.HasKey("DoxyOption_HHC_LOCATION"))
			Config.HHCLocation = EditorPrefs.GetString("DoxyOption_HHC_LOCATION");
		if (EditorPrefs.HasKey(UnityProjectID + "DoxyOption_GENERATE_CHI"))
			Config.GenerateCHI = EditorPrefs.GetString(UnityProjectID + "DoxyOption_GENERATE_CHI");
		if (EditorPrefs.HasKey(UnityProjectID + "DoxyOption_CHM_INDEX_ENCODING"))
			Config.ChmIndexEncoding = EditorPrefs.GetString(UnityProjectID + "DoxyOption_CHM_INDEX_ENCODING");
		if (EditorPrefs.HasKey(UnityProjectID + "DoxyOption_MACRO_EXPANSION"))
			Config.MacroExpansion = EditorPrefs.GetString(UnityProjectID + "DoxyOption_MACRO_EXPANSION");
		//if (EditorPrefs.HasKey(UnityProjectID + "DoxyOption_PREDEFINED"))
		//	Config.PreDefined = EditorPrefs.GetString(UnityProjectID + "DoxyOption_PREDEFINED");
		if (EditorPrefs.HasKey(UnityProjectID + "DoxyOption_CLASS_DIAGRAMS"))
			Config.ClassDiagrams = EditorPrefs.GetString(UnityProjectID + "DoxyOption_CLASS_DIAGRAMS");
		if (EditorPrefs.HasKey(UnityProjectID + "DoxyOption_HAVE_DOT"))
			Config.HaveDot = EditorPrefs.GetString(UnityProjectID + "DoxyOption_HAVE_DOT");
		if (EditorPrefs.HasKey(UnityProjectID + "DoxyOption_UML_LOOK"))
			Config.UmlLook = EditorPrefs.GetString(UnityProjectID + "DoxyOption_UML_LOOK");
		if (EditorPrefs.HasKey("DoxyOption_DOT_PATH"))
			Config.DotPath = EditorPrefs.GetString("DoxyOption_DOT_PATH"); 

	}

	bool LoadSavedConfig()
	{
		if( EditorPrefs.HasKey (UnityProjectID+"DoxyProjectName"))
		{
			Config = new DoxygenConfig();
			Config.Project = EditorPrefs.GetString(UnityProjectID + "DoxyProjectName");
			Config.Version = EditorPrefs.GetString(UnityProjectID + "DoxyProjectNumber");
			Config.Synopsis = EditorPrefs.GetString(UnityProjectID + "DoxyProjectBrief");
			Config.DocDirectory = EditorPrefs.GetString(UnityProjectID + "DoxyProjectOutput");
			Config.ScriptsDirectory = EditorPrefs.GetString(UnityProjectID + "DoxyProjectFolder");

			// 2014-04-02 Add(SungMin Lee:Other Option Add)
			Config.DoxyFileEncoding = EditorPrefs.GetString(UnityProjectID + "DoxyOption_DOXYFILE_ENCODING");
			Config.OutputLanguage = EditorPrefs.GetString(UnityProjectID + "DoxyOption_OUTPUT_LANGUAGE");
			Config.ExtractAll = EditorPrefs.GetString(UnityProjectID + "DoxyOption_EXTRACT_ALL");
			Config.ExtractPrivate = EditorPrefs.GetString(UnityProjectID + "DoxyOption_EXTRACT_PRIVATE");
			Config.ExtractStatic = EditorPrefs.GetString(UnityProjectID + "DoxyOption_EXTRACT_STATIC");
			Config.InputEncoding = EditorPrefs.GetString(UnityProjectID + "DoxyOption_INPUT_ENCODING");
			Config.GenerateHtmlHelp = EditorPrefs.GetString(UnityProjectID + "DoxyOption_GENERATE_HTMLHELP");
			Config.ChmFile = EditorPrefs.GetString(UnityProjectID + "DoxyOption_CHM_FILE");
			Config.HHCLocation = EditorPrefs.GetString("DoxyOption_HHC_LOCATION");
			Config.GenerateCHI = EditorPrefs.GetString(UnityProjectID + "DoxyOption_GENERATE_CHI");
			Config.ChmIndexEncoding = EditorPrefs.GetString(UnityProjectID + "DoxyOption_CHM_INDEX_ENCODING");
			Config.MacroExpansion = EditorPrefs.GetString(UnityProjectID + "DoxyOption_MACRO_EXPANSION");
			//Config.PreDefined = EditorPrefs.GetString(UnityProjectID + "DoxyOption_PREDEFINED");
			Config.ClassDiagrams = EditorPrefs.GetString(UnityProjectID + "DoxyOption_CLASS_DIAGRAMS");
			Config.HaveDot = EditorPrefs.GetString(UnityProjectID + "DoxyOption_HAVE_DOT");
			Config.UmlLook = EditorPrefs.GetString(UnityProjectID + "DoxyOption_UML_LOOK");
			Config.DotPath = EditorPrefs.GetString("DoxyOption_DOT_PATH");

			return true;
		}
		return false;
	}

	public static void OnDoxygenFinished(int code)
	{
		if(code != 0)
		{
			UnityEngine.Debug.LogError("Doxygen finsished with Error: return code " + code +"\nCheck the Doxgen Log for Errors.\nAlso try regenerating your Doxyfile,\nyou will new to close and reopen the\ndocumentation window before regenerating.");
		}
	}

	void SetTheme(int theme)
	{
		switch(theme)
		{
			case 1:
    			FileUtil.ReplaceFile(AssestsFolder + "/Editor/Doxygen/Resources/DarkTheme/doxygen.css", Config.DocDirectory+"/html/doxygen.css");
    			FileUtil.ReplaceFile(AssestsFolder + "/Editor/Doxygen/Resources/DarkTheme/tabs.css", Config.DocDirectory+"/html/tabs.css");
    			FileUtil.ReplaceFile(AssestsFolder + "/Editor/Doxygen/Resources/DarkTheme/img_downArrow.png", Config.DocDirectory+"/html/img_downArrow.png");
			break;
			case 2:
    			FileUtil.ReplaceFile(AssestsFolder + "/Editor/Doxygen/Resources/LightTheme/doxygen.css", Config.DocDirectory+"/html/doxygen.css");
    			FileUtil.ReplaceFile(AssestsFolder + "/Editor/Doxygen/Resources/LightTheme/tabs.css", Config.DocDirectory+"/html/tabs.css");
    			FileUtil.ReplaceFile(AssestsFolder + "/Editor/Doxygen/Resources/LightTheme/img_downArrow.png", Config.DocDirectory+"/html/img_downArrow.png");
    			FileUtil.ReplaceFile(AssestsFolder + "/Editor/Doxygen/Resources/LightTheme/background_navigation.png", Config.DocDirectory+"/html/background_navigation.png");
			break;
		}
	}

	public void RunDoxygen()
	{
		string[] Args = new string[1];
		Args[0] = Config.DocDirectory + "/Doxyfile";

      	DoxygenOutput = new DoxyThreadSafeOutput();
      	DoxygenOutput.SetStarted();

      	Action<int> setcallback = (int returnCode) => OnDoxygenFinished(returnCode);

      	DoxyRunner Doxygen = new DoxyRunner(Config.PathtoDoxygen,Args,DoxygenOutput,setcallback);

      	Thread DoxygenThread = new Thread(new ThreadStart(Doxygen.RunThreadedDoxy));
      	DoxygenThread.Start();
	}

}

/// <summary>
///  This class spawns and runs Doxygen in a separate thread, and could serve as an example of how to create 
///  plugins for unity that call a command line application and then get the data back into Unity safely.	 
/// </summary>
public class DoxyRunner
{
	DoxyThreadSafeOutput SafeOutput;
	public Action<int> onCompleteCallBack;
	List<string> DoxyLog = new List<string>();
	public string EXE = null;
	public string[] Args;
	static string WorkingFolder;

	public DoxyRunner(string exepath, string[] args,DoxyThreadSafeOutput safeoutput,Action<int> callback)
	{
		EXE = exepath;
		Args = args;
		SafeOutput = safeoutput;
		onCompleteCallBack = callback;
		WorkingFolder = FileUtil.GetUniqueTempPathInProject();
		System.IO.Directory.CreateDirectory(WorkingFolder);
	}

	public void updateOuputString(string output)
	{
		SafeOutput.WriteLine(output);
		DoxyLog.Add(output);
	}

	public void RunThreadedDoxy()
	{
		Action<string> GetOutput = (string output) => updateOuputString(output);
		int ReturnCode = Run(GetOutput,null,EXE,Args);
		SafeOutput.WriteFullLog(DoxyLog);
		SafeOutput.SetFinished();
		onCompleteCallBack(ReturnCode);
	}

    /// <summary>
    /// Runs the specified executable with the provided arguments and returns the process' exit code.
    /// </summary>
    /// <param name="output">Recieves the output of either std/err or std/out</param>
    /// <param name="input">Provides the line-by-line input that will be written to std/in, null for empty</param>
    /// <param name="exe">The executable to run, may be unqualified or contain environment variables</param>
    /// <param name="args">The list of unescaped arguments to provide to the executable</param>
    /// <returns>Returns process' exit code after the program exits</returns>
    /// <exception cref="System.IO.FileNotFoundException">Raised when the exe was not found</exception>
    /// <exception cref="System.ArgumentNullException">Raised when one of the arguments is null</exception>
    /// <exception cref="System.ArgumentOutOfRangeException">Raised if an argument contains '\0', '\r', or '\n'
    public static int Run(Action<string> output, TextReader input, string exe, params string[] args)
    {
        if (String.IsNullOrEmpty(exe))
            throw new FileNotFoundException();
        if (output == null)
            throw new ArgumentNullException("output");

        ProcessStartInfo psi = new ProcessStartInfo();
        psi.UseShellExecute = false;
        psi.RedirectStandardError = true;
        psi.RedirectStandardOutput = true;
        psi.RedirectStandardInput = true;
        psi.WindowStyle = ProcessWindowStyle.Hidden;
        psi.CreateNoWindow = true;
        psi.ErrorDialog = false;
        psi.WorkingDirectory = WorkingFolder;
        psi.FileName = FindExePath(exe); 
        psi.Arguments = EscapeArguments(args); 

        using (Process process = Process.Start(psi))
        using (ManualResetEvent mreOut = new ManualResetEvent(false),
               mreErr = new ManualResetEvent(false))
        {
            process.OutputDataReceived += (o, e) => { if (e.Data == null) mreOut.Set(); else output(e.Data); };
        process.BeginOutputReadLine();
            process.ErrorDataReceived += (o, e) => { if (e.Data == null) mreErr.Set(); else output(e.Data); };
        process.BeginErrorReadLine();

            string line;
            while (input != null && null != (line = input.ReadLine()))
                process.StandardInput.WriteLine(line);

            process.StandardInput.Close();
            process.WaitForExit();

            mreOut.WaitOne();
            mreErr.WaitOne();
            return process.ExitCode;
        }
    }

    /// <summary>
    /// Quotes all arguments that contain whitespace, or begin with a quote and returns a single
    /// argument string for use with Process.Start().
    /// </summary>
    /// <param name="args">A list of strings for arguments, may not contain null, '\0', '\r', or '\n'</param>
    /// <returns>The combined list of escaped/quoted strings</returns>
    /// <exception cref="System.ArgumentNullException">Raised when one of the arguments is null</exception>
    /// <exception cref="System.ArgumentOutOfRangeException">Raised if an argument contains '\0', '\r', or '\n'</exception>
    public static string EscapeArguments(params string[] args)
    {
        StringBuilder arguments = new StringBuilder();
        Regex invalidChar = new Regex("[\x00\x0a\x0d]");//  these can not be escaped
        Regex needsQuotes = new Regex(@"\s|""");//          contains whitespace or two quote characters
        Regex escapeQuote = new Regex(@"(\\*)(""|$)");//    one or more '\' followed with a quote or end of string
        for (int carg = 0; args != null && carg < args.Length; carg++)
        {
            if (args[carg] == null)
            {
                throw new ArgumentNullException("args[" + carg + "]");
            }
            if (invalidChar.IsMatch(args[carg]))
            {
                throw new ArgumentOutOfRangeException("args[" + carg + "]");
            }
            if (args[carg] == String.Empty)
            {
                arguments.Append("\"\"");
            }
            else if (!needsQuotes.IsMatch(args[carg]))
            {
                arguments.Append(args[carg]);
            }
            else
            {
                arguments.Append('"');
                arguments.Append(escapeQuote.Replace(args[carg], m =>
                                                     m.Groups[1].Value + m.Groups[1].Value +
                                                     (m.Groups[2].Value == "\"" ? "\\\"" : "")
                                                    ));
                arguments.Append('"');
            }
            if (carg + 1 < args.Length)
                arguments.Append(' ');
        }
        return arguments.ToString();
    }


    /// <summary>
    /// Expands environment variables and, if unqualified, locates the exe in the working directory
    /// or the evironment's path.
    /// </summary>
    /// <param name="exe">The name of the executable file</param>
    /// <returns>The fully-qualified path to the file</returns>
    /// <exception cref="System.IO.FileNotFoundException">Raised when the exe was not found</exception>
    public static string FindExePath(string exe)
    {
        exe = Environment.ExpandEnvironmentVariables(exe);
        if (!File.Exists(exe))
        {
            if (Path.GetDirectoryName(exe) == String.Empty)
            {
                foreach (string test in (Environment.GetEnvironmentVariable("PATH") ?? "").Split(';'))
                {
                    string path = test.Trim();
                    if (!String.IsNullOrEmpty(path) && File.Exists(path = Path.Combine(path, exe)))
                        return Path.GetFullPath(path);
                }
            }
            throw new FileNotFoundException(new FileNotFoundException().Message, exe);
        }
        return Path.GetFullPath(exe);
    }	
}	


/// <summary>
///  This class encapsulates the data output by Doxygen so it can be shared with Unity in a thread share way.	 
/// </summary>
public class DoxyThreadSafeOutput
{
   private ReaderWriterLockSlim outputLock = new ReaderWriterLockSlim();
   private string CurrentOutput = "";  
   private List<string> FullLog = new List<string>();
   private bool Finished = false;
   private bool Started = false;

   public string ReadLine( )
   {
        outputLock.EnterReadLock();
        try
        {
            return CurrentOutput;
        }
        finally
        {
            outputLock.ExitReadLock();
        }
    }

   public void SetFinished( )
   {
        outputLock.EnterWriteLock();
        try
        {
            Finished = true;
        }
        finally
        {
            outputLock.ExitWriteLock();
        }
    }

   public void SetStarted( )
   {
        outputLock.EnterWriteLock();
        try
        {
            Started = true;
        }
        finally
        {
            outputLock.ExitWriteLock();
        }
    }

   public bool isStarted( )
   {
        outputLock.EnterReadLock();
        try
        {
            return Started;
        }
        finally
        {
            outputLock.ExitReadLock();
        }
    }

   public bool isFinished( )
   {
        outputLock.EnterReadLock();
        try
        {
            return Finished;
        }
        finally
        {
            outputLock.ExitReadLock();
        }
    }
   
   public List<string> ReadFullLog()
   {
        outputLock.EnterReadLock();
        try
        {
            return FullLog;
        }
        finally
        {
            outputLock.ExitReadLock();
        } 
   }

   public void WriteFullLog(List<string> newLog)
   {
        outputLock.EnterWriteLock();
        try
        {
           FullLog = newLog;
        }
        finally
        {
            outputLock.ExitWriteLock();
        } 
   }

   public void WriteLine(string newOutput)
    {
        outputLock.EnterWriteLock();
        try
        {
            CurrentOutput = newOutput;
        }
        finally
        {
            outputLock.ExitWriteLock();
        }
    }
}


