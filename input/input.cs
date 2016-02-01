using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System;

//----------------------------------------------------------------
/// <summary>
/// アセットバンドルのツール
/// </summary>
//----------------------------------------------------------------
public class AssetBundleTool : MonoBehaviour {
	// ラベルが記載されたテキストファイルの名前
	private static readonly string LabelFileName = "_label.txt";
	// アセットバンドルのパス
	private static readonly string AssetPath = "Assets/AssetBundles";
	
	// バージョンファイルに記載するバージョン文字列
	private static string Version = "";
	
	// バージョンを切り分けるアセットバンドル数
	private static int PartitionAssetbundleFileCount = 2;

	// 文字列1行のデータ構造
	class RowData {
		public string AssetBundleName;
		public string Hash;
	}

	// パーティション一つのデータ構造
	class PartitionData {
		public PartitionData() {
			rowDataList = new List<RowData>();
		}
		public List<RowData> rowDataList;// このパーティションに含まれるアセットバンドルのデータリスト
		public string dataVersion; // このデータ自体のバージョン。これの差分で更新するかどうかを確認する
	}
	
	//------------------------------------------------------------
	/// <summary>
	/// テキスト出力のテスト
	/// </summary>
	//------------------------------------------------------------
	[MenuItem("Tayutau/アセット/テキスト出力テスト")]
	static void OutputTextTest() {
		using(StreamWriter w = new StreamWriter(@"text.txt")) {
			w.WriteLine("基本的に同じ");
			w.WriteLine("末尾に改行文字も足されるよ");
		}
	}
	
	//------------------------------------------------------------
	/// <summary>
	/// テキスト読み込みのテスト
	/// </summary>
	//------------------------------------------------------------
	[MenuItem("Tayutau/アセット/テキスト読込テスト")]
	static void InputTextTest() {
		using(StreamReader r = new StreamReader(@"text.txt")) {
			string line = "";
			while((line = r.ReadLine()) != null) {
				Debug.Log(line);
			}
		}
	}
	
	//------------------------------------------------------------
	/// <summary>
	/// MD5のテスト
	/// </summary>
	//------------------------------------------------------------
	[MenuItem("Tayutau/アセット/MD5生成テスト")]
	static void CreateMD5HashTest() {
		//MD5ハッシュ値を計算する文字列
		string s = "test2";
		
		string result = CreateMD5Hash(s);
		Debug.Log(result);
	}
	
	static string CreateMD5Hash(string baseString) {
		//MD5ハッシュ値を計算する文字列
		string s = baseString;
		
		//文字列をbyte型配列に変換する
		byte[] data = System.Text.Encoding.UTF8.GetBytes(s);

		//MD5CryptoServiceProviderオブジェクトを作成
		System.Security.Cryptography.MD5CryptoServiceProvider md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
		//または、次のようにもできる
		//System.Security.Cryptography.MD5 md5 =
		//	System.Security.Cryptography.MD5.Create();

		//ハッシュ値を計算する
		byte[] bs = md5.ComputeHash(data);

		//リソースを解放する
		md5.Clear();

		//byte型配列を16進数の文字列に変換
		System.Text.StringBuilder result = new System.Text.StringBuilder();
		foreach (byte b in bs) {
			result.Append(b.ToString("x2"));
		}
		//ここの部分は次のようにもできる
		//string result = BitConverter.ToString(bs).ToLower().Replace("-","");

		return result.ToString();
	}
	
	//------------------------------------------------------------
	/// <summary>
	/// アセットバンドルの一括ビルド
	/// </summary>
	//------------------------------------------------------------
	//[MenuItem("Tayutau/アセット/アセットバンドルのビルド")]
	//static void BuildAssetBundles()
	//{
	//	Debug.Log("BuildAssetBundles : Target=" + EditorUserBuildSettings.activeBuildTarget.ToString());
	//	
	//	// PlatForm名をそのまま出力フォルダとして使用する。
	//	//string outputPath = Application.streamingAssetsPath + "/" + EditorUserBuildSettings.activeBuildTarget.ToString();
	//	string outputPath = "OutputAssetbundle/" + EditorUserBuildSettings.activeBuildTarget.ToString();
	//	
	//	// 指定フォルダ存在チェック
	//	if (Directory.Exists(outputPath) == false) {
	//		Directory.CreateDirectory(outputPath);
	//	}

	//	// とりあえず、旧バージョンファイルを読み込む
	//	string versionFilePath = outputPath + "/" + "version.txt";
	//	string prevVersion = "";
	//	string prevAllAssetHashString = "";
	//	if (File.Exists(versionFilePath) == true) {
	//		using(StreamReader r = new StreamReader(versionFilePath)) {
	//			string line = "";
	//			while((line = r.ReadLine()) != null)
	//			{
	//				string[] parameters = line.Split(","[0]);
	//				if (parameters[0] == "version") {
	//					// version情報を保持しておく
	//					prevVersion = parameters[1];
	//				} else {
	//					// 生成したアセットバンドルの差分チェック用に、hash連結文字列作成
	//					prevAllAssetHashString += parameters[1];
	//				}
	//			}
	//		}

	//	}

	//	// プラットフォーム名のマニフェストのアセットバンドルが作られる。
	//	// ハッシュ値の取得を試してみる
	//	AssetBundleManifest assetBundleManifest = BuildPipeline.BuildAssetBundles(outputPath, 0, EditorUserBuildSettings.activeBuildTarget);

	//	// データをソートする為のディクショナリー
	//	string[] allAssetBundles = assetBundleManifest.GetAllAssetBundles();
	//	SortedDictionary<string, string> forSortList = new SortedDictionary<string, string>();
	//	for( int i = 0; i < allAssetBundles.Length ; i++ ) {
	//		string hash = assetBundleManifest.GetAssetBundleHash( allAssetBundles[i] ).ToString();

	//		forSortList.Add(allAssetBundles[i], hash);
	//	}

	//	// 様々な結果を確認して、アセットバンドルバージョンファイル(version.txt)を再生成する必要があるか確認する
	//	bool isUpdate = false;
	//	if (string.IsNullOrEmpty(prevVersion) == true) {
	//		// そもそもバージョンファイルが存在しなかったので、初版として生成する
	//		isUpdate = true;
	//	} else {
	//		// 生成されたアセットバンドル差分チェック用ハッシュ生成
	//		string allAssetHashString = "";
	//		for (int i = 0; i < forSortList.Count; i++) {
	//			var obj = forSortList.ElementAt(i);
	//			allAssetHashString += obj.Value;
	//		}
	//		string versionCheckHash = CreateMD5Hash(allAssetHashString);
	//		//Debug.Log("versionCheckHash : " + versionCheckHash);
	//		string prevVersionCheckHash = CreateMD5Hash(prevAllAssetHashString);
	//		//Debug.Log("prevVersionCheckHash : " + prevVersionCheckHash);

	//		if (versionCheckHash != prevVersionCheckHash) {
	//			// ハッシュ値に差が見られるので、再生成する
	//			isUpdate = true;
	//		} else {
	//			Debug.LogWarning("アセットハッシュ値に差が見られないので、versionファイルの更新はされません");
	//		}
	//	}

	//	if (isUpdate == true) {
	//		if (string.IsNullOrEmpty(prevVersion) == true) {
	//			Version = "start";
	//		} else if (prevVersion == "start") {
	//			Version = "1";
	//		} else {
	//			int versionValue = int.Parse(prevVersion);
	//			versionValue++;
	//			Version = versionValue.ToString();
	//		}

	//		string versionPath = outputPath + "/version.txt";
	//		using(StreamWriter w = new StreamWriter(versionPath)) {
	//			string versionLine = "version," + Version;
	//			w.WriteLine(versionLine);
	//			
	//			for (int i = 0; i < forSortList.Count; i++) {
	//				var obj = forSortList.ElementAt(i);
	//				//Debug.Log ("name : " + obj.Key);
	//				//Debug.Log("hash : " + obj.Value);
	//				
	//				string line = "";
	//				line = string.Format("{0},{1}", obj.Key, obj.Value);
	//				w.WriteLine(line);
	//			}
	//		}
	//	}

	//	//string[] allAssetBundles = assetBundleManifest.GetAllAssetBundles();
	//	//string versionPath = outputPath + "/version.txt";
	//	//using(StreamWriter w = new StreamWriter(versionPath)) {
	//	//	string versionLine = "version," + Version;
	//	//	w.WriteLine(versionLine);
	//	//	
	//	//	string line = "";
	//	//	for( int i = 0; i < allAssetBundles.Length ; i++ ) {
	//	//		Debug.Log ("name : " + allAssetBundles[i]);
	//	//		string hash = assetBundleManifest.GetAssetBundleHash( allAssetBundles[i] ).ToString();
	//	//		Debug.Log("hash : " + hash);

	//	//		line = string.Format("{0},{1}", allAssetBundles[i], hash);
	//	//		w.WriteLine(line);
	//	//	}
	//	//}
	//
	//	Debug.Log("BuildAssetBundles : done");

	//	if (Version == "start") {
	//		Debug.LogWarning("バージョンファイルのバージョンが初期に戻っています。意図した挙動かご確認ください");
	//	}
	//}
	
	//------------------------------------------------------------
	/// <summary>
	/// アセットバンドルラベル付与
	/// </summary>
	//------------------------------------------------------------
	[MenuItem("Tayutau/アセット/アセットバンドルラベル付与")]
	public static void AssetLabelAutoSet() {

		// 指定されたフォルダが存在するかチェック
		if (!Directory.Exists(AssetPath)) {
			Debug.LogError("指定されたアセットバンドルフォルダが存在しません。パスを確認してください。");
			return;
		}
		 
		// 初期呼び出し
		Debug.Log("AssetLabelAutoSet start.");
		LabelSet(AssetPath,"");
		Debug.Log("AssetLabelAutoSet done.");
	}

	//------------------------------------------------------------
	/// <summary>
	/// 指定されたフォルダに存在するアセットにラベルを付与する
	/// </summary>
	//------------------------------------------------------------
	private static void LabelSet(string path,string assetLabel) {

		// ラベル名
		string label = assetLabel;
		// ファイルを取得(metaファイルを除く)
		List<string> files = Directory.GetFiles(path).Where(o => !o.EndsWith(".meta")).ToList();
		// ディレクトリを取得(metaファイルを除く)
		List<string> dirs = Directory.GetDirectories(path).Where(o => !o.EndsWith(".meta")).ToList();

		// ファイルもディレクトリも存在しなかったらリターン
		if ((files.Count == 0) && (dirs.Count == 0)) {
			return;
		}

		// 今のディレクトリにラベルファイルが存在してたら
		if (File.Exists(path + "/" + LabelFileName)) {
			// 読み込む
			StreamReader reader = new StreamReader(path + "/" + LabelFileName);
			label = reader.ReadToEnd();
			label = label.Replace("\r\n", "");		// 改行コードが入っている場合は除去
			label = label.Replace(((char)0x0A).ToString(), ""); 		// 0x0A(\r)のみが入っている場合も除去
			reader.Close();
		}
		
		// 再帰で子階層チェック
		foreach (string dir in dirs) {
			LabelSet(dir,label);
		}

		// 各ファイルのラベル変更
		if (string.IsNullOrEmpty(label) == false) {		// １回もラベル設定されないうちはラベルを書き換えない
			foreach (string file in files) {
				// Unity5.1.1f1 + Windows7(64bit)だと何故かGetAttributesがArchiveになる
				
				// 隠しファイルはスキップ
				if (File.GetAttributes(file) == FileAttributes.Hidden) {
					continue;
				}
				
				// ファイルサイズ0のファイルはスキップ
				FileInfo fi = new FileInfo(file);
				if (fi.Length == 0) {
					continue;
				}
				
				// ラベルファイルを除く
				if (Path.GetFileName(file) == LabelFileName) {
					continue;
				}
				
				// 通常ファイルのみ対象で、更に
				AssetImporter.GetAtPath(file).assetBundleName = label;
//				Debug.Log(file + "=>" + label);
			}
		}
	}
	
	//------------------------------------------------------------
	/// <summary>
	/// アセットバンドルラベル削除
	/// </summary>
	//------------------------------------------------------------
	[MenuItem("Tayutau/アセット/アセットバンドルラベル削除")]
	public static void AssetLabelRemove() {

		// 指定されたフォルダが存在するかチェック
		if (!Directory.Exists(AssetPath)) {
			Debug.LogError("指定されたアセットバンドルフォルダが存在しません。パスを確認してください。");
			return;
		}
		 
		// 初期呼び出し
		Debug.Log("AssetLabelAutoSet start.");
		LabelSetForRemove(AssetPath,"");
		Debug.Log("AssetLabelAutoSet done.");
	}

	//------------------------------------------------------------
	/// <summary>
	/// 指定されたフォルダに存在するアセットのラベルを削除する
	/// </summary>
	//------------------------------------------------------------
	private static void LabelSetForRemove(string path,string assetLabel) {

		// ラベル名
		string label = assetLabel;
		// ファイルを取得(metaファイルを除く)
		List<string> files = Directory.GetFiles(path).Where(o => !o.EndsWith(".meta")).ToList();
		// ディレクトリを取得(metaファイルを除く)
		List<string> dirs = Directory.GetDirectories(path).Where(o => !o.EndsWith(".meta")).ToList();

		// ファイルもディレクトリも存在しなかったらリターン
		if ((files.Count == 0) && (dirs.Count == 0)) {
			return;
		}

		//// 今のディレクトリにラベルファイルが存在してたら
		//if (File.Exists(path + "/" + LabelFileName)) {
		//	// 読み込む
		//	StreamReader reader = new StreamReader(path + "/" + LabelFileName);
		//	label = reader.ReadToEnd();
		//	label = label.Replace("\r\n", "");		// 改行コードが入っている場合は除去
		//	label = label.Replace(((char)0x0A).ToString(), ""); 		// 0x0A(\r)のみが入っている場合も除去
		//	reader.Close();
		//}
		
		// 再帰で子階層チェック
		foreach (string dir in dirs) {
			LabelSetForRemove(dir,label);
		}

		// 各ファイルのラベル変更
		//if (string.IsNullOrEmpty(label) == false) {		// １回もラベル設定されないうちはラベルを書き換えない
			foreach (string file in files) {
				// Unity5.1.1f1 + Windows7(64bit)だと何故かGetAttributesがArchiveになる
				
				// 隠しファイルはスキップ
				if (File.GetAttributes(file) == FileAttributes.Hidden) {
					continue;
				}
				
				// ファイルサイズ0のファイルはスキップ
				FileInfo fi = new FileInfo(file);
				if (fi.Length == 0) {
					continue;
				}
				
				// ラベルファイルを除く
				if (Path.GetFileName(file) == LabelFileName) {
					continue;
				}
				
				// 通常ファイルのみ対象で、更に
				AssetImporter.GetAtPath(file).assetBundleName = label;
//				Debug.Log(file + "=>" + label);
			}
		//}
	}
	
	//------------------------------------------------------------
	/// <summary>
	/// アセットバンドルの一括ビルド(改良版)
	/// </summary>
	//------------------------------------------------------------
	[MenuItem("Tayutau/アセット/アセットバンドルのビルド")]
	static void BuildAssetBundles() {
		Debug.Log("BuildAssetBundles : Target=" + EditorUserBuildSettings.activeBuildTarget.ToString());
		
		// PlatForm名をそのまま出力フォルダとして使用する。
		//string outputPath = Application.streamingAssetsPath + "/" + EditorUserBuildSettings.activeBuildTarget.ToString();
		string outputPath = "OutputAssetbundle/" + EditorUserBuildSettings.activeBuildTarget.ToString();
		
		// 指定フォルダ存在チェック
		if (Directory.Exists(outputPath) == false) {
			Directory.CreateDirectory(outputPath);
		}
		
		// プラットフォーム名のマニフェストのアセットバンドルが作られる。
		// ハッシュ値の取得を試してみる
		AssetBundleManifest assetBundleManifest = BuildPipeline.BuildAssetBundles(outputPath, 0, EditorUserBuildSettings.activeBuildTarget);

		// データをソートする為のディクショナリー
		SortedDictionary<string, string> forSortList = CreateAssetbundleSortedDictionary(assetBundleManifest);

		// パーティション分割チェック用のデータ生成
		Dictionary<int, List<RowData>>partitionRowDataListFromAssetbundleData = PartitionRowDataListFromSortedDictionary(forSortList);

		// 以前のビルド成果物のバージョンファイルを読み込む
		string versionFilePath = outputPath + "/" + "version.txt";
		bool isVersionFileExist = CheckVersionFileExist(versionFilePath);

		// バージョンファイルの存在成否で、まず処理が別れる
		if (isVersionFileExist == false) {
			// 完全に新規ファイル作成。もしかしたら何かの間違いかもしれないので、警告を出す
			Debug.LogWarning("以前のバージョンファイルが確認できませんでした。本当に意図した新規作成かご確認ください");
			
			string outputString = "";
			int partitionName = 1;
			foreach(var rowDataList in partitionRowDataListFromAssetbundleData) {
				string temp = "";
				int dataCount = rowDataList.Value.Count;
				string rowDataDetail = string.Format("{0},{1},{2},{3}", 
												"version",
												"start",
												dataCount,
												partitionName);
				outputString += rowDataDetail;
				outputString += Environment.NewLine;

				foreach(var rowData in rowDataList.Value) {
					temp += rowData.AssetBundleName + "," + rowData.Hash;
					temp += Environment.NewLine;
				}

				outputString += temp;
				partitionName++;
			}

			
			// 末尾の改行コードを消す。データ数＝行数にしたい為
			outputString = outputString.Substring(0, (outputString.Length-1));
			
			// バージョンファイル保存
			SaveVersionFile(versionFilePath, outputString);
		} else {
			// バージョンファイルが存在するので、差分があるかの確認に入る
			
			// 既存のバージョンファイル読み込み
			string versionSource = LoadVersionFile(versionFilePath);

			// 読み込んだファイルから参照用データを作る
			// キーがパーティション番号
			Dictionary<int, PartitionData> partitionDataDictFromVersionFile = CreateCheckDifferenceDictionaryFromPrevVersionFile(versionSource);

			// 作成したアセットバンドルのデータから、比較用データを作成する
//Dictionary<int, List<RowData>>partitionRowDataList = PartitionRowDataListFromSortedDictionary(forSortList);
			string outputString = "";
			int partitionNumber = 1;
			foreach(var partitionRowDataList in partitionRowDataListFromAssetbundleData){
				// ひとまず、バージョンファイルから作ったディクショナリーから、該当のパーティション情報があるか確認する
				PartitionData partitionData;
				partitionDataDictFromVersionFile.TryGetValue(partitionNumber, out partitionData);
				if (partitionData == null) {
					// 既存リストに無い、新規パーティション情報
					outputString += CreateNewPartitionString(partitionRowDataList);
				} else {
					// 既存リストに存在するので、ハッシュチェックをして更新する必要があるか確認する
					// TODO 明日はここの妥当性を検証するところから始める
					string newHashString = CreateHashStringFromAssetBundlePartitionData(partitionRowDataList);
					string prevHashString = CreateHashStringFromVersionPartitionData(partitionData);

					if (prevHashString == newHashString) {
						// 差がないので、バージョンファイルの情報をそのまま書き込む
						outputString += CreateCurrentPartitionString(partitionData, partitionNumber);
					} else {
						// 差があるので、バージョンを更新した上で、新しい情報を書き込む
						string updatedVersionString = GetUpdatedVersionString(partitionData);
						outputString += CreateUpdatePartitionString(partitionRowDataList, updatedVersionString);
					}
				}

				partitionNumber++;
			}

			// 末尾の改行コードを消す。データ数＝行数にしたい為
			outputString = outputString.Substring(0, (outputString.Length-1));
			
			// バージョンファイル保存
			SaveVersionFile(versionFilePath, outputString);

			Debug.Log("outputstring : " + outputString);
		}
		Debug.Log("=====END=====");
	}
	
	// =====ここからメソッド群=====
	
	// 作成したアセットバンドル情報を、ソートしたディクショナリー形式にして返す
	static private SortedDictionary<string, string> CreateAssetbundleSortedDictionary(AssetBundleManifest assetBundleManifest) {
		string[] allAssetBundles = assetBundleManifest.GetAllAssetBundles();
		SortedDictionary<string, string> forSortList = new SortedDictionary<string, string>();
		for( int i = 0; i < allAssetBundles.Length ; i++ ) {
			string hash = assetBundleManifest.GetAssetBundleHash( allAssetBundles[i] ).ToString();

			forSortList.Add(allAssetBundles[i], hash);
		}

		// 結果出力
		Debug.Log("=====SortedDictionary:OutputStart=====");
		foreach(var data in forSortList) {
			Debug.Log("key : " + data.Key + "/" + "Value : " + data.Value);
		}
		Debug.Log("=====SortedDictionary:OutputEnd=====");

		return forSortList;
	}
	
	// アセットバンドルディクショナリーから、パーティション分割したListを作成する
	static private Dictionary<int, List<RowData>> PartitionRowDataListFromSortedDictionary(SortedDictionary<string, string> sortedDict) {
		// ソートされたデータを、パーティション分割して扱えるようにする
		Dictionary<int, List<RowData>> dic = new Dictionary<int, List<RowData>>();
		
		int dataCounter = 0;
		int partitionNumber = 1;
		
		List<RowData> tempList = new List<RowData>();
		foreach(var sortedDictData in sortedDict) {
			RowData rowData = new RowData();
			rowData.AssetBundleName	= sortedDictData.Key;
			rowData.Hash			= sortedDictData.Value;
			
			tempList.Add(rowData);

			dataCounter++;
			// データ収納数越えてたら、パーティション分割する
			if (dataCounter >= PartitionAssetbundleFileCount) {
				dic.Add(partitionNumber, tempList);
				tempList = new List<RowData>();
				partitionNumber++;
				dataCounter = 0;
			}
		}

		// 端数分のデータ格納
		if (dataCounter != 0) {
			dic.Add(partitionNumber, tempList);
		}

		// 結果出力
		Debug.Log("=====PartitionRowDataListFromSortedDictionary:OutputStart=====");
		foreach(var dicdata in dic) {
			Debug.Log("key : " + dicdata.Key);

			var list = dicdata.Value;
			foreach(var listdata in list) {
				Debug.Log("rowdata : " + listdata.AssetBundleName + "," + listdata.Hash);
			}
		}
		Debug.Log("=====PartitionRowDataListFromSortedDictionary:OutputEnd=====");

		return dic;
	}
	 
	// バージョンファイルの存在確認
	static private bool CheckVersionFileExist(string path_filename) {
		bool result = System.IO.File.Exists(path_filename);
		
		// 結果出力
		Debug.Log("=====CheckVersionFileExist:OutputStart=====");
		Debug.Log("path : " + path_filename);
		Debug.Log("result : " + result);
		Debug.Log("=====CheckVersionFileExist:OutputEnd=====");

		return result;
	}
	
	// バージョンファイルの保存
	static private void SaveVersionFile(string path_filename, string source) {
		using(StreamWriter w = new StreamWriter(path_filename)) {
			//w.WriteLine(source);
			w.Write(source);
		}
		
		// 結果出力
		Debug.Log("=====SaveVersionFile:OutputStart=====");
		Debug.Log("path : " + path_filename);
		Debug.Log("source : ↓");
		Debug.Log(source);
		Debug.Log("read : ↓");
		Debug.Log(LoadVersionFile(path_filename));
		Debug.Log("=====SaveVersionFile:OutputEnd=====");
	}
		
	// バージョンファイルの読み込み
	static private string LoadVersionFile(string path_filename) {
		string output = "";
		using(StreamReader r = new StreamReader(path_filename)) {
			string line = "";
			while((line = r.ReadLine()) != null) {
				output += line;
				output += Environment.NewLine;
			}
		}
		
		// 結果出力
		Debug.Log("=====LoadVersionFile:OutputStart=====");
		Debug.Log("path : " + path_filename);
		Debug.Log("output : ↓");
		Debug.Log(output);
		Debug.Log("=====LoadVersionFile:OutputEnd=====");

		return output;
	}
	
	// 既存バージョンファイルから、差分確認用ディクショナリーを作成
	static private Dictionary<int, PartitionData> CreateCheckDifferenceDictionaryFromPrevVersionFile(string versionFileSource) {
		Dictionary<int, PartitionData> partitionDataDict = new Dictionary<int, PartitionData>();

		// 改行で分けた文字列
		string[] tempList = versionFileSource.Split(Environment.NewLine[0]);
		ArrayList lineList = new ArrayList();

		// 空データの削除
		for (int i = 0; i < tempList.Length; i++) {
			if (string.IsNullOrEmpty(tempList[i]) != true) {
				lineList.Add(tempList[i]);
			}
		}

		int index = 0;
		while (index < lineList.Count) {
			string line = lineList[index] as string;
			string[] rowDetail = line.Split(","[0]);
			string version		 = rowDetail[1];
			string dataCount	   = rowDetail[2];
			string partitionNumber = rowDetail[3];

			PartitionData partitionData = new PartitionData();
			partitionData.dataVersion = version;
			for (int i = 0; i < int.Parse(dataCount); i++) {
				line = lineList[index+1+i] as string;// 詳細情報の次のデータから参照する為の+1
				string[] parameter = line.Split(","[0]);

				RowData rowData = new RowData();
				rowData.AssetBundleName =   parameter[0];
				rowData.Hash			=   parameter[1];

				partitionData.rowDataList.Add(rowData);
			}
			partitionDataDict.Add(int.Parse(partitionNumber), partitionData);
			index += (int.Parse(dataCount) + 1);// バージョン詳細の分
		}

		// 結果出力
		Debug.Log("=====CreateCheckDifferenceDictionaryFromPrevVersionFile:OutputStart=====");
		Debug.Log("versionFileSource : ↓");
		Debug.Log(versionFileSource);
		Debug.Log("partitionDataDict : ↓");
		foreach(var partitionData in partitionDataDict) {
			Debug.Log("KEY : " + partitionData.Key);
			Debug.Log("VALUE.dataVersion : " + partitionData.Value.dataVersion);
			Debug.Log("VALUE.rowDataList : ↓");
			foreach(var rowData in partitionData.Value.rowDataList) {
				Debug.Log("rowData.AssetBundleName : " + rowData.AssetBundleName);
				Debug.Log("rowData.Hash : " + rowData.Hash);
			}
		}
		Debug.Log("=====CreateCheckDifferenceDictionaryFromPrevVersionFile:OutputEnd=====");

		return partitionDataDict;
	}
	
	// アセットバンドルビルド情報から、新規パーティションバージョンストリング作成
	static private string CreateNewPartitionString(KeyValuePair<int, List<RowData>> rowDataListDict) {
		string outputString = "";
			
		// 既存情報にパーティションがないので、完全に新規のデータ
		// versionをstartにして作成する
		string temp = "";
		int dataCount = rowDataListDict.Value.Count;
		string rowDataDetail = String.Format("{0},{1},{2},{3}", 
										"version",
										"start",
										dataCount,
										rowDataListDict.Key);
		outputString += rowDataDetail;
		outputString += Environment.NewLine;

		foreach(var rowData in rowDataListDict.Value) {
			temp += String.Format("{0},{1}", rowData.AssetBundleName, rowData.Hash);
			temp += Environment.NewLine;
		}

		outputString += temp;
			
		// 結果出力
		Debug.Log("=====CreateNewPartitionString:OutputStart=====");
		Debug.Log("outputString : ↓");
		Debug.Log(outputString);
		Debug.Log("=====CreateNewPartitionString:OutputEnd=====");

		return outputString;
	}
	
	// アセットバンドルビルド情報から作成したデータから、比較用ハッシュ作成
	static private string CreateHashStringFromAssetBundlePartitionData(KeyValuePair<int, List<RowData>> partitionRowDataList) {
		
		string assetbundleHashString = "";
		foreach (var partitionRowData in partitionRowDataList.Value) {
			assetbundleHashString += partitionRowData.Hash;
		}
		string hashString = CreateMD5Hash(assetbundleHashString);

		// 結果出力
		Debug.Log("=====CreateHashStringFromAssetBundlePartitionData:OutputStart=====");
		Debug.Log("hashString : ↓");
		Debug.Log(hashString);
		Debug.Log("=====CreateHashStringFromAssetBundlePartitionData:OutputEnd=====");

		return hashString;
	}
	
	// 既存バージョン情報から作成したデータから、比較用ハッシュ作成
	static private string CreateHashStringFromVersionPartitionData(PartitionData partitionData) {
		string versionHashString = "";
		foreach (var rowData in partitionData.rowDataList) {
			versionHashString += rowData.Hash;
		}
		string hashString = CreateMD5Hash(versionHashString);

		// 結果出力
		Debug.Log("=====CreateHashStringFromVersionPartitionData:OutputStart=====");
		Debug.Log("hashString : ↓");
		Debug.Log(hashString);
		Debug.Log("=====CreateHashStringFromVersionPartitionData:OutputEnd=====");

		return hashString;
	}
	
	// アセットバンドルビルド情報から、更新されたパーティションバージョンストリング作成
	static private string CreateUpdatePartitionString(KeyValuePair<int, List<RowData>> rowDataListDict, string updatedVersionString) {
		
		string outputString = "";
						
		string dataCount = rowDataListDict.Value.Count.ToString();
		string rowDataDetail = String.Format("{0},{1},{2},{3}", 
										"version",
										updatedVersionString,
										dataCount,
										rowDataListDict.Key.ToString());

		outputString += rowDataDetail;
		outputString += Environment.NewLine;

		string temp = "";
		foreach (var rowData in rowDataListDict.Value) {
			temp += String.Format("{0},{1}", rowData.AssetBundleName, rowData.Hash);
			temp += Environment.NewLine;
		}

		outputString += temp;
		
		// 結果出力
		Debug.Log("=====CreateUpdatePartitionString:OutputStart=====");
		Debug.Log("outputString : ↓");
		Debug.Log(outputString);
		Debug.Log("=====CreateUpdatePartitionString:OutputEnd=====");

		return outputString;
	}
	
	// バージョンファイルのデータを見て、新しいバージョン情報を取得する
	static private string GetUpdatedVersionString(PartitionData partitionData) {
		string versionString = "";
		if (partitionData.dataVersion == "start") {
			versionString = "1";
		} else {
			int versionNumber = int.Parse(partitionData.dataVersion);
			versionNumber++;
			versionString = versionNumber.ToString();
		}

		// 結果出力
		Debug.Log("=====GetUpdatedVersionString:OutputStart=====");
		Debug.Log("versionString : ↓");
		Debug.Log(versionString);
		Debug.Log("=====GetUpdatedVersionString:OutputEnd=====");
		
		return versionString;
	}
	
	// バージョン情報から、その状態のバージョン文字列を作成
	static private string CreateCurrentPartitionString(PartitionData partitionData, int partitionNumber) {
		
		string outputString = "";
						
		string dataCount = partitionData.rowDataList.Count.ToString();
		string rowDataDetail = String.Format("{0},{1},{2},{3}", 
										"version",
										partitionData.dataVersion,
										dataCount,
										partitionNumber.ToString());

		outputString += rowDataDetail;
		outputString += Environment.NewLine;

		string temp = "";
		foreach (var rowData in partitionData.rowDataList) {
			temp += String.Format("{0},{1}", rowData.AssetBundleName, rowData.Hash);
			temp += Environment.NewLine;
		}

		outputString += temp;
		
		// 結果出力
		Debug.Log("=====CreateCurrentPartitionString:OutputStart=====");
		Debug.Log("outputString : ↓");
		Debug.Log(outputString);
		Debug.Log("=====CreateCurrentPartitionString:OutputEnd=====");

		return outputString;
	}

}

