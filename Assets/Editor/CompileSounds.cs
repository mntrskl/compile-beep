using System;
using System.Reflection;
using Cysharp.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace Editor
{
    [InitializeOnLoad]
    public static class CompileSounds
    {
        private const string StartClipURL = "https://github.com/mntrskl/compile-beep/blob/25b036bc55588d55c777d1f94378bcf79bfa9d08/Assets/Sounds/elevator.mp3?raw=true";
        private const string StartAudioLocal = "Assets/Sounds/elevator.mp3";

        private const string CompileStatePrefsKey = "CompileIndicator.WasCompiling";
        private static AudioClip _startClip;

        static CompileSounds()
        {
            EditorApplication.update += OnUpdate;
            TryToGetWebClips();
        }

        private static async void TryToGetWebClips()
        {
            try
            {
                _startClip = await GetClipAsync(UnityWebRequestMultimedia.GetAudioClip(
                    StartClipURL,
                    AudioType.MPEG));
            }
            catch (Exception e)
            {
                _startClip = AssetDatabase.LoadAssetAtPath<AudioClip>(StartAudioLocal);
                Debug.Log(e);
            }
        }

        static async UniTask<AudioClip> GetClipAsync(UnityWebRequest req)
        {
            var op = await req.SendWebRequest();
            return DownloadHandlerAudioClip.GetContent(req);
        }

        private static void OnUpdate()
        {
            var wasCompiling = EditorPrefs.GetBool(CompileStatePrefsKey);
            var isCompiling = EditorApplication.isCompiling;

            if (wasCompiling == isCompiling) return;

            if (isCompiling) OnStartCompiling();
            else OnEndCompiling();

            EditorPrefs.SetBool(CompileStatePrefsKey, isCompiling);
        }

        private static void OnStartCompiling() => PlayClip(_startClip);
        private static void OnEndCompiling() => StopAllClips();
        
        //Should def unity editor (working 2022) version
        private static void PlayClip(AudioClip clip, int startSample = 0, bool loop = false)
        {
            var unityEditorAssembly = typeof(AudioImporter).Assembly;
            var audioUtilClass = unityEditorAssembly.GetType("UnityEditor.AudioUtil");
            var method = audioUtilClass.GetMethod("PlayPreviewClip",
                BindingFlags.Static | BindingFlags.Public, null,
                new[] { typeof(AudioClip), typeof(int), typeof(bool) }, null);
            method?.Invoke(null, new object[] { clip, startSample, loop });
        }

        private static void StopAllClips()
        {
            var unityEditorAssembly = typeof(AudioImporter).Assembly;
            var audioUtilClass = unityEditorAssembly.GetType("UnityEditor.AudioUtil");
            var method = audioUtilClass.GetMethod("StopAllPreviewClips",
                BindingFlags.Static | BindingFlags.Public, null, new Type[] { }, null);
            method?.Invoke(null, new object[] { });
        }
    }
}