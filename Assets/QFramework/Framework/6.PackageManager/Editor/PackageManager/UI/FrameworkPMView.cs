﻿/****************************************************************************
 * Copyright (c) 2018.7 ~ 11 liangxie
 * 
 * http://qframework.io
 * https://github.com/liangxiegame/QFramework
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 ****************************************************************************/

using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace QFramework
{
    public class FrameworkPMView
    {
        private List<PackageData> mPackageDatas = new List<PackageData>();

        public FrameworkLocalVersion FrameworkLocalVersion;

        private static readonly string EXPORT_ROOT_DIR = Application.dataPath.CombinePath("../");

        public static string ExportPaths(string exportPackageName, params string[] paths)
        {
            if (Directory.Exists(paths[0]))
            {
                if (paths[0].EndsWith("/"))
                {
                    paths[0] = paths[0].Remove(paths[0].Length - 1);
                }

                var filePath = EXPORT_ROOT_DIR.CombinePath(exportPackageName);
                AssetDatabase.ExportPackage(paths,
                    filePath, ExportPackageOptions.Recurse);
                AssetDatabase.Refresh();

                return filePath;
            }

            return string.Empty;
        }

        public void Init(EditorWindow window)
        {
            FrameworkLocalVersion = FrameworkLocalVersion.Get();

            mPackageDatas = PackageInfosRequestCache.Get().PackageDatas;

            InstalledPackageVersions.Reload();

            EditorActionKit.ExecuteNode(new GetAllRemotePackageInfo(packageDatas => { mPackageDatas = packageDatas; }));
        }

        private Vector2 mScrollPos;

        private int mToolbarIndex = 0;
        
        private string[] mToolbarNamesLogined = {"Framework","Plugin","UIKitComponent","Shader","AppOrTemplate", "Private"};
        private string[] mToolbarNamesUnLogined = {"Framework","Plugin","UIKitComponent","Shader","AppOrTemplate"};

        public string[] ToolbarNames
        {
            get { return User.Logined ? mToolbarNamesLogined : mToolbarNamesUnLogined; }
        }

        public IEnumerable<PackageData> SelectedPackageType
        {
            get
            {
                switch (mToolbarIndex)
                {
                    case 0:
                        return mPackageDatas.Where(packageData => packageData.Type == PackageType.FrameworkModule);
                    case 1:
                        return mPackageDatas.Where(packageData => packageData.Type == PackageType.Plugin);
                    case 2:
                        return mPackageDatas.Where(packageData => packageData.Type == PackageType.UIKitComponent);
                    case 3:
                        return mPackageDatas.Where(packageData => packageData.Type == PackageType.Shader);
                    case 4:
                        return mPackageDatas.Where(packageData =>
                            packageData.Type == PackageType.AppOrGameDemoOrTemplate);
                    case 5:
                        return mPackageDatas.Where(packageData =>
                            packageData.AccessRight == PackageAccessRight.Private);
                    default:
                        return mPackageDatas.Where(packageData => packageData.Type == PackageType.FrameworkModule);
                }
            }
        }


        public void OnGUI()
        {
            GUILayout.Label("Framework:");
            GUILayout.BeginVertical("box");

            GUILayout.Label(string.Format("Current Framework Version:{0}", FrameworkLocalVersion.Version));

            if (User.Logined)
            {
                mToolbarIndex = GUILayout.Toolbar(mToolbarIndex, ToolbarNames);
            }

            // 这里开始具体的内容
            GUILayout.BeginHorizontal("box");
            GUILayout.Label("Package", GUILayout.Width(150));
            GUILayout.Label("Server", GUILayout.Width(80));
            GUILayout.Label("Local", GUILayout.Width(80));
            GUILayout.Label("Right", GUILayout.Width(120));
            GUILayout.Label("Action", GUILayout.Width(100));
            GUILayout.Label("Readme", GUILayout.Width(100));
            GUILayout.EndHorizontal();

            GUILayout.BeginVertical("box");

            mScrollPos = GUILayout.BeginScrollView(mScrollPos, false, true, GUILayout.Height(240));

            foreach (var packageData in SelectedPackageType)
            {
                GUILayout.Space(2);
                GUILayout.BeginHorizontal();
                GUILayout.Label(packageData.Name, GUILayout.Width(150));
                GUILayout.Label(packageData.Version, GUILayout.Width(80));
                var installedPackage = InstalledPackageVersions.FindVersionByName(packageData.Name);
                GUILayout.Label(installedPackage != null ? installedPackage.Version : " ", GUILayout.Width(80));
                GUILayout.Label(packageData.AccessRight.ToString(), GUILayout.Width(120));

                if (installedPackage == null)
                {
                    if (GUILayout.Button("Import", GUILayout.Width(90)))
                    {
                        EditorActionKit.ExecuteNode(new InstallPackage(packageData));
                    }
                }
                else if (installedPackage != null && packageData.VersionNumber > installedPackage.VersionNumber)
                {
                    if (GUILayout.Button("Update", GUILayout.Width(90)))
                    {
                        var path = Application.dataPath.Replace("Assets", packageData.InstallPath);

                        if (Directory.Exists(path))
                        {
                            Directory.Delete(path, true);
                        }

                        EditorActionKit.ExecuteNode(new InstallPackage(packageData));
                    }
                }
                else if (installedPackage != null)
                {
                    
                    // maybe support upload? 
                    GUILayout.Space(94);
                }

                if (GUILayout.Button("Readme", GUILayout.Width(90)))
                {
                    ReadmeWindow.Init(packageData.readme, packageData.PackageVersions.First());
                }

                GUILayout.EndHorizontal();
            }

            GUILayout.EndScrollView();

            GUILayout.Space(2);

            GUILayout.EndVertical();

            GUILayout.EndVertical();


            GUILayout.Space(50);
        }
    }
}