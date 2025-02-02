﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YooAsset
{
	[Serializable]
	internal sealed class PatchCache
	{
		/// <summary>
		/// 缓存的APP内置版本
		/// </summary>
		public string CacheAppVersion = string.Empty;

		/// <summary>
		/// 读取缓存文件
		/// 注意：如果文件不存在则创建新的缓存文件
		/// </summary>
		public static PatchCache LoadCache()
		{
			if (SandboxHelper.CheckSandboxCacheFileExist())
			{				
				string filePath = SandboxHelper.GetSandboxCacheFilePath();
				string jsonData = FileUtility.ReadFile(filePath);
				var patchCache = JsonUtility.FromJson<PatchCache>(jsonData);
				YooLogger.Log($"Load cache file : {patchCache.CacheAppVersion}");
				return patchCache;
			}
			else
			{
				YooLogger.Log($"Create cache file : {Application.version}");
				PatchCache cache = new PatchCache();
				cache.CacheAppVersion = Application.version;
				string filePath = SandboxHelper.GetSandboxCacheFilePath();
				string jsonData = JsonUtility.ToJson(cache);
				FileUtility.CreateFile(filePath, jsonData);
				return cache;
			}
		}

		/// <summary>
		/// 更新缓存文件
		/// </summary>
		public static void UpdateCache()
		{
			YooLogger.Log($"Update patch cache to disk : {Application.version}");
			PatchCache cache = new PatchCache();
			cache.CacheAppVersion = Application.version;
			string filePath = SandboxHelper.GetSandboxCacheFilePath();
			string jsonData = JsonUtility.ToJson(cache);
			FileUtility.CreateFile(filePath, jsonData);
		}
	}
}