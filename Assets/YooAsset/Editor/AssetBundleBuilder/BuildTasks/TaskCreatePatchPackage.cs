﻿using System.Collections;
using System.Collections.Generic;

namespace YooAsset.Editor
{
	/// <summary>
	/// 制作补丁包
	/// </summary>
	public class TaskCreatePatchPackage : IBuildTask
	{
		void IBuildTask.Run(BuildContext context)
		{
			var buildParameters = context.GetContextObject<AssetBundleBuilder.BuildParametersContext>();
			CopyPatchFiles(buildParameters);
		}

		/// <summary>
		/// 拷贝补丁文件到补丁包目录
		/// </summary>
		private void CopyPatchFiles(AssetBundleBuilder.BuildParametersContext buildParameters)
		{
			int resourceVersion = buildParameters.Parameters.BuildVersion;
			string packageDirectory = buildParameters.GetPackageDirectory();
			UnityEngine.Debug.Log($"准备开始拷贝补丁文件到补丁包目录：{packageDirectory}");

			// 拷贝Report文件
			{
				string sourcePath = $"{buildParameters.PipelineOutputDirectory}/{YooAssetSettings.ReportFileName}";
				string destPath = $"{packageDirectory}/{YooAssetSettings.ReportFileName}";
				EditorTools.CopyFile(sourcePath, destPath, true);
				UnityEngine.Debug.Log($"拷贝构建报告文件到：{destPath}");
			}

			// 拷贝补丁清单文件
			{
				string sourcePath = $"{buildParameters.PipelineOutputDirectory}/{YooAssetSettingsData.GetPatchManifestFileName(resourceVersion)}";
				string destPath = $"{packageDirectory}/{YooAssetSettingsData.GetPatchManifestFileName(resourceVersion)}";
				EditorTools.CopyFile(sourcePath, destPath, true);
				UnityEngine.Debug.Log($"拷贝补丁清单文件到：{destPath}");
			}

			// 拷贝补丁清单哈希文件
			{
				string sourcePath = $"{buildParameters.PipelineOutputDirectory}/{YooAssetSettingsData.GetPatchManifestHashFileName(resourceVersion)}";
				string destPath = $"{packageDirectory}/{YooAssetSettingsData.GetPatchManifestHashFileName(resourceVersion)}";
				EditorTools.CopyFile(sourcePath, destPath, true);
				UnityEngine.Debug.Log($"拷贝补丁清单哈希文件到：{destPath}");
			}

			// 拷贝静态版本文件
			{
				string sourcePath = $"{buildParameters.PipelineOutputDirectory}/{YooAssetSettings.VersionFileName}";
				string destPath = $"{packageDirectory}/{YooAssetSettings.VersionFileName}";
				EditorTools.CopyFile(sourcePath, destPath, true);
				UnityEngine.Debug.Log($"拷贝静态版本文件到：{destPath}");
			}

			// 拷贝UnityManifest序列化文件
			{
				string sourcePath = $"{buildParameters.PipelineOutputDirectory}/{YooAssetSettingsData.Setting.UnityManifestFileName}";
				string destPath = $"{packageDirectory}/{YooAssetSettingsData.Setting.UnityManifestFileName}";
				EditorTools.CopyFile(sourcePath, destPath, true);
				UnityEngine.Debug.Log($"拷贝UnityManifest文件到：{destPath}");
			}

			// 拷贝UnityManifest文本文件
			{
				string sourcePath = $"{buildParameters.PipelineOutputDirectory}/{YooAssetSettingsData.Setting.UnityManifestFileName}.manifest";
				string destPath = $"{packageDirectory}/{YooAssetSettingsData.Setting.UnityManifestFileName}.manifest";
				EditorTools.CopyFile(sourcePath, destPath, true);
			}

			// 拷贝所有补丁文件
			int progressValue = 0;
			PatchManifest patchManifest = AssetBundleBuilderHelper.LoadPatchManifestFile(buildParameters.PipelineOutputDirectory, buildParameters.Parameters.BuildVersion);
			int patchFileTotalCount = patchManifest.BundleList.Count;
			foreach (var patchBundle in patchManifest.BundleList)
			{
				string sourcePath = $"{buildParameters.PipelineOutputDirectory}/{patchBundle.BundleName}";
				string destPath = $"{packageDirectory}/{patchBundle.Hash}";
				EditorTools.CopyFile(sourcePath, destPath, true);
				UnityEngine.Debug.Log($"拷贝补丁文件到补丁包：{patchBundle.BundleName}");
				EditorTools.DisplayProgressBar("拷贝补丁文件", ++progressValue, patchFileTotalCount);
			}
			EditorTools.ClearProgressBar();
		}
	}
}