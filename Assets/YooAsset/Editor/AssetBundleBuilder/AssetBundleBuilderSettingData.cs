﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace YooAsset.Editor
{
	public class AssetBundleBuilderSettingData
	{
		private static AssetBundleBuilderSetting _setting = null;
		public static AssetBundleBuilderSetting Setting
		{
			get
			{
				if (_setting == null)
					LoadSettingData();
				return _setting;
			}
		}

		/// <summary>
		/// 加载配置文件
		/// </summary>
		private static void LoadSettingData()
		{
			// 加载配置文件
			_setting = AssetDatabase.LoadAssetAtPath<AssetBundleBuilderSetting>(EditorDefine.AssetBundleBuilderSettingFilePath);
			if (_setting == null)
			{
				Debug.LogWarning($"Create new {nameof(AssetBundleBuilderSetting)}.asset : {EditorDefine.AssetBundleBuilderSettingFilePath}");
				_setting = ScriptableObject.CreateInstance<AssetBundleBuilderSetting>();
				EditorTools.CreateFileDirectory(EditorDefine.AssetBundleBuilderSettingFilePath);
				AssetDatabase.CreateAsset(Setting, EditorDefine.AssetBundleBuilderSettingFilePath);
				AssetDatabase.SaveAssets();
				AssetDatabase.Refresh();
			}
			else
			{
				Debug.Log($"Load {nameof(AssetBundleBuilderSetting)}.asset ok");
			}
		}

		/// <summary>
		/// 存储文件
		/// </summary>
		public static void SaveFile()
		{
			if (Setting != null)
			{
				EditorUtility.SetDirty(Setting);
				AssetDatabase.SaveAssets();
				Debug.Log($"{nameof(AssetBundleBuilderSetting)}.asset is saved!");
			}
		}
	}
}