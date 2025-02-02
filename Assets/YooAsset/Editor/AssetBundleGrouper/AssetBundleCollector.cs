﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace YooAsset.Editor
{
	[Serializable]
	public class AssetBundleCollector
	{
		/// <summary>
		/// 收集路径
		/// 注意：支持文件夹或单个资源文件
		/// </summary>
		public string CollectPath = string.Empty;

		/// <summary>
		/// 打包规则类名
		/// </summary>
		public string PackRuleName = string.Empty;

		/// <summary>
		/// 过滤规则类名
		/// </summary>
		public string FilterRuleName = string.Empty;

		/// <summary>
		/// 不写入资源列表
		/// </summary>
		public bool NotWriteToAssetList = false;

		/// <summary>
		/// 资源分类标签
		/// </summary>
		public string AssetTags = string.Empty;


		/// <summary>
		/// 检测配置错误
		/// </summary>
		public void CheckConfigError()
		{
			if (AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(CollectPath) == null)
				throw new Exception($"Invalid collect path : {CollectPath}");

			if (AssetBundleGrouperSettingData.HasPackRuleName(PackRuleName) == false)
				throw new Exception($"Invalid {nameof(IPackRule)} class type : {PackRuleName}");

			if (AssetBundleGrouperSettingData.HasFilterRuleName(FilterRuleName) == false)
				throw new Exception($"Invalid {nameof(IFilterRule)} class type : {FilterRuleName}");
		}

		/// <summary>
		/// 获取打包收集的资源文件
		/// </summary>
		public List<CollectAssetInfo> GetAllCollectAssets(AssetBundleGrouper grouper)
		{
			Dictionary<string, CollectAssetInfo> result = new Dictionary<string, CollectAssetInfo>(1000);
			bool isRawAsset = PackRuleName == nameof(PackRawFile);

			// 如果是文件夹
			if (AssetDatabase.IsValidFolder(CollectPath))
			{
				string collectDirectory = CollectPath;
				string[] findAssets = EditorTools.FindAssets(EAssetSearchType.All, collectDirectory);
				foreach (string assetPath in findAssets)
				{
					if (IsValidateAsset(assetPath) == false)
						continue;
					if (IsCollectAsset(assetPath) == false)
						continue;
					if (result.ContainsKey(assetPath) == false)
					{
						string bundleName = GetBundleName(grouper, assetPath, isRawAsset);
						List<string> assetTags = GetAssetTags(grouper);
						var collectAssetInfo = new CollectAssetInfo(bundleName, assetPath, assetTags, isRawAsset, NotWriteToAssetList);
						collectAssetInfo.DependAssets = GetAllDependencies(assetPath);
						result.Add(assetPath, collectAssetInfo);
					}
					else
					{
						throw new Exception($"The collecting asset file is existed : {assetPath} in collector : {CollectPath}");
					}
				}
			}
			else
			{
				string assetPath = CollectPath;
				if (result.ContainsKey(assetPath) == false)
				{
					if (isRawAsset && NotWriteToAssetList)
						UnityEngine.Debug.LogWarning($"Are you sure raw file are not write to asset list : {assetPath}");

					string bundleName = GetBundleName(grouper, assetPath, isRawAsset);
					List<string> assetTags = GetAssetTags(grouper);
					var collectAssetInfo = new CollectAssetInfo(bundleName, assetPath, assetTags, isRawAsset, NotWriteToAssetList);
					result.Add(assetPath, collectAssetInfo);
				}
				else
				{
					throw new Exception($"The collecting asset file is existed : {assetPath} in collector : {CollectPath}");
				}
			}

			// 返回列表
			return result.Values.ToList();
		}

		private bool IsValidateAsset(string assetPath)
		{
			if (assetPath.StartsWith("Assets/") == false && assetPath.StartsWith("Packages/") == false)
				return false;

			if (AssetDatabase.IsValidFolder(assetPath))
				return false;

			// 注意：忽略编辑器下的类型资源
			Type type = AssetDatabase.GetMainAssetTypeAtPath(assetPath);
			if (type == typeof(LightingDataAsset))
				return false;

			string ext = System.IO.Path.GetExtension(assetPath);
			if (ext == "" || ext == ".dll" || ext == ".cs" || ext == ".js" || ext == ".boo" || ext == ".meta")
				return false;

			return true;
		}
		private bool IsCollectAsset(string assetPath)
		{
			// 如果收集全路径着色器
			if (AssetBundleGrouperSettingData.Setting.AutoCollectShaders)
			{
				Type assetType = AssetDatabase.GetMainAssetTypeAtPath(assetPath);
				if (assetType == typeof(UnityEngine.Shader))
					return true;
			}

			// 根据规则设置过滤资源文件
			IFilterRule filterRuleInstance = AssetBundleGrouperSettingData.GetFilterRuleInstance(FilterRuleName);
			return filterRuleInstance.IsCollectAsset(new FilterRuleData(assetPath));
		}
		private string GetBundleName(AssetBundleGrouper grouper, string assetPath, bool isRawAsset)
		{
			// 如果收集全路径着色器
			if (AssetBundleGrouperSettingData.Setting.AutoCollectShaders)
			{
				System.Type assetType = AssetDatabase.GetMainAssetTypeAtPath(assetPath);
				if (assetType == typeof(UnityEngine.Shader))
				{
					string bundleName = AssetBundleGrouperSettingData.Setting.ShadersBundleName;
					return RevisedBundleName(bundleName, false);
				}
			}

			// 根据规则设置获取资源包名称
			{
				IPackRule packRuleInstance = AssetBundleGrouperSettingData.GetPackRuleInstance(PackRuleName);
				string bundleName = packRuleInstance.GetBundleName(new PackRuleData(assetPath, CollectPath, grouper.GrouperName));
				return RevisedBundleName(bundleName, isRawAsset);
			}
		}
		private List<string> GetAssetTags(AssetBundleGrouper grouper)
		{
			List<string> tags = StringUtility.StringToStringList(grouper.AssetTags, ';');
			List<string> temper = StringUtility.StringToStringList(AssetTags, ';');
			tags.AddRange(temper);
			return tags;
		}
		private List<string> GetAllDependencies(string mainAssetPath)
		{
			List<string> result = new List<string>();
			string[] depends = AssetDatabase.GetDependencies(mainAssetPath, true);
			foreach (string assetPath in depends)
			{
				if (IsValidateAsset(assetPath))
				{
					// 注意：排除主资源对象
					if (assetPath != mainAssetPath)
						result.Add(assetPath);
				}
			}
			return result;
		}


		/// <summary>
		/// 修正资源包名
		/// </summary>
		public static string RevisedBundleName(string bundleName, bool isRawBundle)
		{
			if (isRawBundle)
			{
				string fullName = $"{bundleName}.{YooAssetSettingsData.Setting.RawFileVariant}";
				return EditorTools.GetRegularPath(fullName).ToLower();
			}
			else
			{
				string fullName = $"{bundleName}.{YooAssetSettingsData.Setting.AssetBundleFileVariant}";
				return EditorTools.GetRegularPath(fullName).ToLower(); ;
			}
		}
	}
}